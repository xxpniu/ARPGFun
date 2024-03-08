using System;
using System.Collections.Generic;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using Google.Protobuf.WellKnownTypes;
using Proto;
using UnityEngine;
using XNet.Libs.Utility;

namespace Server
{
    public class BindPlayer
    {
        public BattlePlayer Player;
        public string Account;
    }

    public class BattlePlayer
    {

        #region Property

        private readonly DHero _hero;
        public BattlePackage Package { private set; get; }
        public BattleCharacter HeroCharacter { set; get; }
         

        public int Gold
        {
            get => _baseGold + DiffGold;
            private set => DiffGold = value - _baseGold;
        }

        #endregion


        public StreamBuffer<Any> PushChannel { set; get; }

        public StreamBuffer<Any> RequestChannel { set; get; }

        public DHero GetHero() { return _hero; }

        public string AccountId { private set; get; }

        public L2S_CheckSession GateServer { set; get; }
        
        private readonly int _baseGold = 0;

        public BattlePlayer(string account,   PlayerPackage package, DHero hero, int gold,
            L2S_CheckSession info,
            StreamBuffer<Any> requestChannel = null, StreamBuffer<Any> pushChannel = null)
        {
            Package = new BattlePackage(package);
            _hero = hero;
            _baseGold = gold;
            AccountId = account;
            PushChannel = pushChannel;
            RequestChannel = requestChannel;
            GateServer = info;
        }


        public bool SubGold(int gold)
        {
            if (gold <= 0) return false;
            if (Gold - gold < 0) return false;
            Gold -= gold;
            Dirty = true;
            return true;
        }

        public bool AddGold(int gold)
        {
            if (gold <= 0) return false;
            Gold += gold;
            Dirty = true;
            return true;
        }
        
        public Notify_PlayerJoinState GetNotifyPackage()
        {
            var notify = new Notify_PlayerJoinState()
            {
                AccountUuid = AccountId,
                Gold = Gold,
                Package = GetCompletedPackage(),
                Hero = _hero
            };
            return notify;

        }

        private PlayerPackage GetCompletedPackage()
        {
            var result = new PlayerPackage();
            foreach (var i in Package.Items)
            {
                result.Items.Add(i.Key, i.Value.Item);
                result.MaxSize = Package.MaxSize;
            }
            return result;
        }

        private int CurrentSize => Package.Items.Count;

        public bool AddDrop(PlayerItem item)
        {
            var config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
            if (config.MaxStackNum == 1)
            {
                item.GUID = CreateUuid();
                if (CurrentSize >= Package.MaxSize) return false;
                Package.Items.Add(item.GUID, new BattlePlayerItem(item, true));
            }
            else
            {
                foreach (var i in Package.Items)
                {
                    if (i.Value.Item.Locked) continue;
                    if (i.Value.Item.ItemID != item.ItemID) continue;
                    if (i.Value.Item.Num == config.MaxStackNum) continue;
                    var maxNum = config.MaxStackNum - i.Value.Item.Num; 
                    if (maxNum >= item.Num)
                    {
                        i.Value.Item.Num += item.Num;
                        item.Num = 0;
                        i.Value.SetDirty();
                        break;
                    }

                    i.Value.Item.Num += maxNum;
                    item.Num -= maxNum;
                    i.Value.SetDirty();
                }
                if (CurrentSize >= Package.MaxSize) return true;
                var needSize = item.Num / Mathf.Max(1, config.MaxStackNum);
                if (needSize + CurrentSize >= Package.MaxSize) return true;
                while (item.Num > 0)
                {
                    var num = Mathf.Min(config.MaxStackNum, item.Num);
                    item.Num -= num;
                    var playerItem = new PlayerItem { GUID = CreateUuid(), ItemID = item.ItemID, Level = item.Level, Num = num };
                    Package.Items.Add(playerItem.GUID, new BattlePlayerItem(playerItem, true));
                }
            }
            Dirty = true;
            return true;
        }

        internal int GetItemCount(int itemId, bool ignoreLocked = true)
        {
            int have = 0;
            foreach (var i in Package.Items)
            {
                if (i.Value.Item.Locked && ignoreLocked) continue;
                if (i.Value.Item.ItemID == itemId) have += i.Value.Item.Num;
            }
            return have;
        }

        public bool ConsumeItem(int item, int num = 1)
        {
            var have = GetItemCount(item);
            if (have < num) return false;
            var needRemoves = new HashSet<string>();
            foreach (var i in Package.Items)
            {
                if (i.Value.Item.ItemID != item) continue;
                if (i.Value.Item.Locked) continue;
                var left = num - i.Value.Item.Num;
                if (left < 0)
                {
                    i.Value.Item.Num -= num;
                    i.Value.SetDirty();
                    num = 0;
                    break;
                }
                i.Value.SetDirty();
                needRemoves.Add(i.Key);
                num = left;
            }
            foreach (var i in needRemoves)
            {
                if (!Package.RemoveItem(i))
                {
                    Debuger.LogWaring($"Not found {i}");
                }
            }
            Dirty = true;
            return true;
        }

        public bool Dirty { get; private set; } = false;

        public int DiffGold { get; private set; }
        public bool IsConnected
        {
            get
            {
                if (PushChannel == null) return false;
                return RequestChannel != null;
            }
        }

        internal PlayerItem GetEquipByGuid(string guid)
        {
            return Package.Items.TryGetValue(guid, out var item) ? item.Item : null;
        }

        private string CreateUuid()
        {
            var serverId = BattleServerApp.S.ServerID;
            return $"{serverId}-{Guid.NewGuid()}";
        }

        private bool AddExp(int totalExp, int level, out int exLevel, out int exExp)
        {
            exLevel = level;
            exExp = totalExp;
            var heroLevel = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == level + 1);
            if (heroLevel == null) return false;
            if (exExp < heroLevel.NeedExp) return true;
            exLevel += 1;
            exExp -= heroLevel.NeedExp;
            return exExp <= 0 || AddExp(exExp, exLevel, out exLevel, out exExp);
        }


        public int AddExp(int exp, out int oldLevel, out int newLevel)
        {
            oldLevel = newLevel = _hero.Level;
            if (exp <= 0) return _hero.Exprices;
            if (AddExp(exp + _hero.Exprices, _hero.Level, out var level, out var exLimit))
            {
                _hero.Level = level;
                _hero.Exprices = exLimit;
                newLevel = _hero.Level;
            }
            Dirty = true;
            return _hero.Exprices;
        }

        public void ModifyItem(IList<PlayerItem> modify =null,IList<PlayerItem> removes = null, IList<PlayerItem> adds = null)
        {
            if (modify!=null)
            {
                foreach (var item in modify)
                {
                    if (this.Package.Items.TryGetValue(item.GUID, out var v))
                    {
                        v.Item.Num = item.Num;
                    }
                }
            }

            if (removes !=null)
            {
                foreach (var item in removes)
                {
                    this.Package.RemoveItem(item.GUID);
                }
            }

            if (adds!=null)
            {
                foreach (var item in adds)
                {
                    this.Package.Items.Add(item.GUID, new BattlePlayerItem(item));
                }
            }
        }
    }
}