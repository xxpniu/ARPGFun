using System;
using System.Collections.Generic;
using System.Linq;
using BattleViews.Utility;
using Core;
using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
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
        public BattlePerception Per { get; }
        public MapConfig Config { get; }

        public MapElementSpawn(BattlePerception per, MapConfig config )
        {
            this.Per = per;
            this.Config = config;
        }

        public void Spawn()
        {
            var config = Config;

            foreach (var i in config.Elements)
            {
                Debuger.Log($"{i}");
                switch (i.Type)
                {
                    case MapElementType.MetMonsterGroup:
                        {
                            var data = CM.GetId<MonsterGroupData>(i.ConfigID);
                            if (data != null)
                                SpawnMonster(i.Position.ToVer3(), i.Forward.ToVer3(), data);
                            else
                                Debuger.LogError($"No found monster group {i.ConfigID}");
                        }
                        break;
                    case MapElementType.MetElementGroup:
                        {
                            var data = CM.GetId<MapElementGroup>(i.ConfigID);
                            if (data != null)
                                SpawnElement(i.Position.ToUV3(), i.Forward.ToUV3() ,i.LinkPos.ToUV3(), data);
                            else
                                Debuger.LogError($"No found monster group {i.ConfigID}");
                        }
                        break;
                    case MapElementType.MetNpc:
                        break;
                    case MapElementType.MetMonster:
                        CreateMonster(i.ConfigID, i.Position.ToUV3(), i.Forward.ToUV3());
                        break;
                    //case MapElementType.MetElementGroup:
                    //    CreateTransport(i.ConfigID, i.Position.ToUV3(), i.Forward.ToUV3(), i.LinkPos.ToUV3());
                    //    break;
                    default:
                        break;
                }
            }
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

        private void SpawnMonster(Vector3 pos, Vector3 forward, MonsterGroupData monsterGroup)
        {
            var standPos = new List<BattleStandData>();
            var monsterID = new List<int>();
            var m = monsterGroup.MonsterID.SplitToInt();
            var nums = monsterGroup.Nums.SplitToInt();
            var p = monsterGroup.Weight.SplitToInt();

            int count = monsterGroup.Count;
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
                            var forword = Quaternion.LookRotation(Quaternion.Euler(0, ang * i, 0) * Vector3.forward);
                            standPos.Add(new BattleStandData { Pos = pos + offset, Forward = new Vector3(0, forword.eulerAngles.y, 0) });
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
                CreateMonster( monsterID[i], standPos[i].Pos, standPos[i].Forward);
            }
        }

        private int alive = 0;

        public bool IsAllMonsterDeath()
        {
            return alive == 0;
        }

        private void CreateMonster(int id, Vector3 pos,Vector3 forward)
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
                Debuger.LogError($"{monsterData.CharacterID} nofound in characterdata" );
                return;
            }
            var magic = data.CreateHeroMagic(); ;
            var mName = $"{data.Name}";

            var append = data.CreateMonsterProperties(monsterData);

            var Monster = Per.CreateCharacter(Per.AIControllor, monsterData.Level, data, magic, append, 2,
                pos, forward, string.Empty, mName);
            Per.ChangeCharacterAI(data.AIResourcePath, Monster);
            var drop = CM.GetId<DropGroupData>(monsterData.DropId);
            if (drop != null)
            {
                Monster["__Drop"] = drop;
                Monster["__Monster"] = monsterData;
            }
            alive++;
            Monster.OnDead = (el) =>
            {
                alive--;
                
                Debuger.Log($"{LanguageManager.S[el.Name]} death!!");
                GObject.Destroy(el, 3f);
                if (el["__Drop"] is DropGroupData d
                && el["__Monster"] is MonsterData mdata)
                {
                    var os = el.Watch.Values.OrderBy(t => t.FristTime).ToList();
                    foreach (var e in os)
                    {
                        var owner = Per.FindTarget(e.Index);
                        if (!owner) continue;
                        //召唤物掉落归属问题
                        if (owner.OwnerIndex > 0) owner = Per.FindTarget(owner.OwnerIndex);
                        OnDrop?.Invoke(new DropItem
                        {
                            Pos = el.Position,
                            MDate = mdata,
                            DataConfig = d,
                            OwnerIndex = owner?.Index ?? -1,
                            TeamIndex = owner?.TeamIndex ?? -1,
                            Owner = owner
                        });
                        break;
                    }

                };
            };
        }
    }
}
