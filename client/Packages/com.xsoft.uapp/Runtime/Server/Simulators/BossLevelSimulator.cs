using System;
using System.Linq;
using GameLogic;
using Google.Protobuf.WellKnownTypes;
using Proto;
using Server.Map;
using XNet.Libs.Utility;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Server
{
    [LevelSimulator(MType = MapType.Boss)]
    [Serializable]
    public class BossLevelSimulator : BattleLevelSimulator
    {
        private MapElementSpawn _spawn;

        protected override void OnLoadCompleted()
        {
            base.OnLoadCompleted();

            Debuger.Log($"Total Time:{totalTime}");

            _spawn = new MapElementSpawn(Per, Config)
            {
                OnDrop = DoDrop
            };
            BeginSpawn();
        }

        private async void BeginSpawn()
        {
            await _spawn.Spawn();
        }

        public override bool CheckEnd()
        {
            if (_spawn == null) return false;
            return _spawn.IsAllMonsterDeath() || base.CheckEnd();
        }

        private void DoDrop(DropItem it)
        {
            BattlePlayer player = null;
            if (it.Owner && Simulator.TryGetPlayer(it.Owner.AccountUuid, out player))
            {
                var exp = player.GetHero().Exprices;
                var expNew = player.AddExp(it.MDate.Exp, out var old, out var newLevel);
                if (newLevel != old)
                {
                    player.HeroCharacter.SetLevel(newLevel);
                    player.HeroCharacter.ResetHpMp(); //full mp and hp
                }

                var expNotify = new Notify_CharacterExp { Exp = expNew, Level = newLevel, OldExp = exp, OldLeve = old };
                player.PushChannel?.Push(Any.Pack(expNotify));
            }

            if (it.DataConfig == null) return;
            if (!GRandomer.Probability10000(it.DataConfig.DropPro)) return;
            var items = it.DataConfig.DropItem.SplitToInt();
            var pors = it.DataConfig.Pro.SplitToInt();
            var nums = it.DataConfig.DropNum.SplitToInt();
            if (it.Owner)
            {
                var gold = GRandomer.RandomMinAndMax(it.DataConfig.GoldMin, it.DataConfig.GoldMax);
                if (gold > 0)
                    if (player != null)
                    {
                        player.AddGold(gold);
                        var notify = new Notify_DropGold { Gold = gold, TotalGold = player.Gold };
                        player.PushChannel?.Push(Any.Pack(notify));
                    }
            }

            var count = GRandomer.RandomMinAndMax(it.DataConfig.DropMinNum, it.DataConfig.DropMaxNum);
            while (count > 0)
            {
                count--;
                var offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                var index = GRandomer.RandPro(pors.ToArray());
                var item = new PlayerItem { ItemID = items[index], Num = nums[index] };
                Per.CreateItem(it.Pos + offset, item, it.OwnerIndex, it.TeamIndex);
            }
        }
    }
}