using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Core;
using BattleViews.Utility;
using Cysharp.Threading.Tasks;
using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Layout.AITree;
using Proto;
using UnityEngine;
using XNet.Libs.Utility;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Vector3 = UnityEngine.Vector3;

namespace Server.Map
{
    public struct BattleStandData
    {
        public Vector3 Pos;
        public Vector3 Forward;
    }

    public class DropItem
    {
        public Vector3 Pos { get; internal set; }
        public MonsterData MDate { get; internal set; }
        public DropGroupData DataConfig { get; internal set; }
        public int OwnerIndex { get; internal set; }
        public int TeamIndex { get; internal set; }
        public BattleCharacter Owner { get; internal set; }
    }


    public class MapElementSpawn
    {
        private class TeamChangedWatcher: ICharacterWatcher
        {
            private BattleCharacter Monster { get; }
            private MapElementSpawn Spawn { get; }

            public TeamChangedWatcher(BattleCharacter monster, MapElementSpawn spawn)
            {
                Monster = monster;
                Spawn = spawn;
            }

            public void OnFireEvent(BattleEventType eventType, object args)
            {
                if (eventType != BattleEventType.TeamChanged) return;
                
                Monster.OnDead = null;
                Spawn.AliveCount--;
                //只触发一次
                Monster.RemoveEventWatcher(this);
            }
        }

        private bool _finished = false;
        public BattlePerception Per { get; }
        public MapConfig Config { get; }

        public MapElementSpawn(BattlePerception per, MapConfig config )
        {
            this.Per = per;
            this.Config = config;
        }

        public async Task Spawn()
        {
            foreach (var i in Config.Elements)
            {
                Debuger.Log($"{i}");
                switch (i.Type)
                {
                    case MapElementType.MetMonsterGroup:
                    {
                        var data = CM.GetId<MonsterGroupData>(i.ConfigID);
                        if (data != null)
                            await SpawnMonster(i.Position.ToVer3(), i.Forward.ToVer3(), data);
                        else
                            Debuger.LogError($"No found monster group {i.ConfigID}");
                    }
                        break;
                    case MapElementType.MetElementGroup:
                    {
                        var data = CM.GetId<MapElementGroup>(i.ConfigID);
                        if (data != null)
                            SpawnElement(i.Position.ToUV3(), i.Forward.ToUV3(), i.LinkPos.ToUV3(), data);
                        else
                            Debuger.LogError($"No found monster group {i.ConfigID}");
                    }
                        break;
                    case MapElementType.MetNpc:
                        break;
                    case MapElementType.MetMonster:
                        await CreateMonster(i.ConfigID, i.Position.ToUV3(), i.Forward.ToUV3());
                        break;
                    //case MapElementType.MetElementGroup:
                    //    CreateTransport(i.ConfigID, i.Position.ToUV3(), i.Forward.ToUV3(), i.LinkPos.ToUV3());
                    //    break;
                    case MapElementType.MetNone:
                    case MapElementType.MetPlayerInit:
                    case MapElementType.MetTransport:
                    default:
                        break;
                }
            }

            _finished = true;
        }

        //传送点的config param0 是作用技能 释放方式为目标 作用就是自己身边3米的然后到目标点
        private void CreateTransport(MapElementData config, Vector3 pos, Vector3 forward, Vector3 linkPos)
        {
            //var st = Per.State as BattleState;
            var ai = SpawnLogicUtil.CreateTransportAI(linkPos,config.Params01);
            var character = CM.GetId<CharacterData>(config.CharacterID);
            var properties = character.CreatePlayerProperties();
            var ch= Per.CreateCharacter(Per.AIControllor, 0, character, null, properties, -10, pos, forward, null, config.NameKey);
            Per.ChangeCharacterAI(ai, ch,"Transport AI");
        }

        private void SpawnElement(Vector3 pos, Vector3 forward, Vector3 linkPos, MapElementGroup data)
        {
            var ids = data.ElementID.SplitToInt();
            var nums = data.Nums.SplitToInt();
            for (var index = 0; index < ids.Count(); index++)
            {
                int num = nums[index];
                if (num <= 0) break;
                var map = CM.GetId<MapElementData>(ids[index]);
                switch ((Proto.MapLevelElementType)map.METype)
                {
                    case MapLevelElementType.MletChestBox:
                        break;
                    case MapLevelElementType.MletDropGroupElement:
                        break;
                    case MapLevelElementType.MletTransport:
                        CreateTransport(map, pos, forward, linkPos);
                        break;
                }
            }
        }

        public Action<DropItem> OnDrop;

        private void TryDropReward(DropGroupData drGroupData ,MonsterData monsterData , BattleCharacter el)
        {
            var os = el.Watch.Values.OrderBy(t => t.FirstTime).ToList();
            foreach (var e in os)
            {
                var owner = Per.FindTarget(e.Index);
                if (!owner) continue;
                if (owner.OwnerIndex > 0) owner = Per.FindTarget(owner.OwnerIndex);
                OnDrop?.Invoke(new DropItem
                {
                    Pos = el.Position,
                    MDate = monsterData,
                    DataConfig = drGroupData,
                    OwnerIndex = owner?.Index ?? -1,
                    TeamIndex = owner?.TeamIndex ?? -1,
                    Owner = owner
                });
                break;
            }
        }

        private async Task SpawnMonster(Vector3 pos, Vector3 forward, MonsterGroupData monsterGroup)
        {
            var standPos = new List<BattleStandData>();
            var monsterID = new List<int>();
            var m = monsterGroup.MonsterID.SplitToInt();
            var nums = monsterGroup.Nums.SplitToInt();
            var p = monsterGroup.Weight.SplitToInt();

            var count = monsterGroup.Count; 
            if (monsterGroup.Repeat == 1)
            {
                while (count-- > 0)
                {
                    var index = GRandomer.RandPro(p.ToArray());
                    var id = m[index];
                    var n = nums[index];
                    while (n-- > 0)
                    {
                        monsterID.Add(id);
                    }
                }
            }
            else
            {
                while (count-- > 0)
                {
                    var index = GRandomer.RandPro(p.ToArray());
                    var id = m[index];
                    var n = nums[index];
                    while (n-- > 0)
                    {
                        monsterID.Add(id);
                    }

                    m.RemoveAt(index);
                    p.RemoveAt(index);
                    nums.RemoveAt(index);
                }
            }

            var maxCount = monsterID.Count;

            switch ((StandType)monsterGroup.StandType)
            {
                case StandType.StAround:
                    {
                        var r = monsterGroup.StandParams01;
                        var ang = 360 / maxCount;
                        for (var i = 0; i < maxCount; i++)
                        {
                            var offset = Quaternion.Euler(0, ang * i, 0) *  forward * r;
                            var f = Quaternion.LookRotation(Quaternion.Euler(0, ang * i, 0) * Vector3.forward);
                            standPos.Add(new BattleStandData { Pos = pos + offset, Forward = new Vector3(0, f.eulerAngles.y, 0) });
                        }
                    }
                    break;
                case StandType.StRandom:
                default:
                    {
                        var r = (int)monsterGroup.StandParams01;
                        for (var i = 0; i < maxCount; i++)
                        {
                            var offset = new Vector3(GRandomer.RandomMinAndMax(-r, r), 0, GRandomer.RandomMinAndMax(-r, r));
                            standPos.Add(new BattleStandData
                            {
                                Pos = pos + offset,
                                Forward = new Vector3(0, GRandomer.RandomMinAndMax(0, 360), 0)
                            }); ;
                        }
                    }

                    break;
            }

            for (var i = 0; i < maxCount; i++)
            {
               await  CreateMonster( monsterID[i], standPos[i].Pos, standPos[i].Forward);
               await UniTask.Yield();
            }

            await Task.CompletedTask;
        }

        public bool IsAllMonsterDeath()
        {
            if (!_finished) return false;
            return AliveCount == 0;
        }

        private int AliveCount { get; set; } = 0;

        private async Task CreateMonster(int id, Vector3 pos,Vector3 forward)
        {
            var monsterData = CM.GetId<MonsterData>(id);
            if (monsterData == null)
            {
                Debug.LogError($" no found id {id} in monsterData");
                return;
            }
            var data = CM.GetId<CharacterData>(monsterData.CharacterID);
            if (data == null)
            {
                Debuger.LogError($"{monsterData.CharacterID} not found in character data" );
                return;
            }
            var magic = data.CreateHeroMagic(); ;
            var mName = $"{data.Name}";

            var append = data.CreateMonsterProperties(monsterData);

            var monster = Per.CreateCharacter(Per.AIControllor, monsterData.Level, data, magic, append, 2,
                pos, forward, string.Empty, mName);
            Per.ChangeCharacterAI(data.AIResourcePath, monster); 
            var drop = CM.GetId<DropGroupData>(monsterData.DropId);
            if (drop != null)
            {
                monster["__Drop"] = drop;
                monster["__Monster"] = monsterData;
            }
            AliveCount++;
            monster.OnDead = (el) =>
            {
                AliveCount--;
                Debuger.Log($"{LanguageManager.S[el.Name]} death!!");
                GObject.Destroy(el, 10f);
                if (el["__Drop"] is DropGroupData d
                && el["__Monster"] is MonsterData mData)
                {
                   TryDropReward(d, mData, el);
                };
            };
            // 召唤物 不会触发掉落
            monster.AddEventWatcher(  new TeamChangedWatcher(monster,this) );

            await Task.CompletedTask;
        }
    }
}
