using System;
using System.Collections.Generic;
using EConfig;
using ExcelConfig;
using GameLogic.Game;
using GameLogic.Game.Elements;
using Layout.LayoutEffects;
using Proto;
using UnityEngine;
using P = Proto.HeroPropertyType;

namespace GameLogic
{
    public static class ExtandsConfig
    {
        public static void TryToAddBase(this Dictionary<HeroPropertyType, ComplexValue> values, HeroPropertyType type,
            ComplexValue value)
        {
            if(values.TryGetValue(type,out var v))
            {v.SetBaseValue(v.BaseValue+value.BaseValue);}
            else
            {
                values.TryAdd(type, value);
            }
        }
        public static IList<int> SplitToInt(this string str, char sKey = '|')
        {
            var arrs = str.Split(sKey);
            var list = new List<int>();
            foreach (var i in arrs) list.Add(int.Parse(i));
            return list;
        }

        public static Dictionary<P, ComplexValue> GetProperties(this PlayerItem pItem)
        {
            var config = ExcelToJSONConfigManager.GetId<ItemData>(pItem.ItemID);
            var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);
            var properties = new Dictionary<P, ComplexValue>();
            if (equip == null)
            {
                Debug.LogError($"No found Equip By Id:{config.Params[0]}");
                return properties;
            }
            var level = ExcelToJSONConfigManager.First<EquipmentLevelUpData>(t => t.Level == pItem.Level && t.Quality == config.Quality);

            properties.TryAddBase(equip.Properties, equip.PropertyValues);

    
            if (pItem.Data != null)
            {
                foreach (var v in pItem.Data.Values)
                {
                    var k = (P)v.Key;
                    properties.TryAdd(k, v.Value, AddType.Append);
                }
            }

            foreach (var p in properties)
            {
                p.Value.SetRate(level?.AppendRate ?? 0);
            }
            return properties;
        }

        public static bool CanReleaseAtPos(this CharacterMagicData att)
        {
            return att.NeedTarget != 1;
        }

        public static TargetTeamType GetTeamType(this CharacterMagicData att)
        {
            //var att = mc;
            var aiType = (MagicReleaseAITarget)att.AITargetType;
            TargetTeamType type = TargetTeamType.All;
            switch (aiType)
            {
                case MagicReleaseAITarget.MatEnemy:
                    type = TargetTeamType.Enemy;
                    break;
                case MagicReleaseAITarget.MatOwn:
                    type = TargetTeamType.Own;
                    break;
                case MagicReleaseAITarget.MatOwnTeam:
                    type = TargetTeamType.OwnTeam;
                    break;
                case MagicReleaseAITarget.MatOwnTeamWithOutSelf:
                    type = TargetTeamType.OwnTeamWithOutSelf;
                    break;
                case MagicReleaseAITarget.MatAll:
                    break;
                default:
                    type = TargetTeamType.All;
                    break;
            }
            return type;
        }

        public static HeroMagicData ToHeroMagic(this BattleCharacterMagic magic, double now)
        {
            return new HeroMagicData
            {
                CDCompletedTime = (float)now,
                MagicID = magic.ConfigId,
                MType = magic.Type,
                MPCost = magic.MpCost,
                CdTotalTime = magic.CdTime
            };
        }

        public static IList<BattleCharacterMagic> CreateHeroMagic(this CharacterData data, DHero hero = null)
        {
            
            var magics = ExcelToJSONConfigManager.Find<CharacterMagicData>(t =>
            {
                return t.CharacterID == data.ID
                && (MagicReleaseType)t.ReleaseType == MagicReleaseType.MrtMagic;
            });
            var list = new List<BattleCharacterMagic>();
            foreach (var i in magics)
            {
                var level = GetMagicLevel(hero, i.ID);
                if (level == null) continue;
                list.Add(new BattleCharacterMagic(MagicType.MtMagic, i, GetMagicLevel(hero, i.ID)));
            }
            if (data.NormalAttack > 0)
            {
                var config = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.NormalAttack);
                list.Add(new BattleCharacterMagic(MagicType.MtNormal, config, GetMagicLevel(hero, data.NormalAttack)));
            }

            return list;
        }

        public static IList<BattleCharacterMagic> CreateHeroMagic(this DHero hero)
        {
            var data = ExcelToJSONConfigManager.GetId<CharacterData>(hero.HeroID);
            return CreateHeroMagic(data, hero);
        }

        private static MagicLevelUpData GetMagicLevel(DHero hero, int magicID)
        {
            if (hero == null) return null;
            foreach (var i in hero.Magics)
            {
                if (i.MagicKey == magicID)
                {
                    return ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.MagicID == magicID && t.Level == i.Level);
                }
            }
            return null;
        }

        private static Dictionary<P, ComplexValue> GetInitStat()
        {
            var initProperties = new Dictionary<P, ComplexValue>();

            foreach (var i in Enum.GetValues(typeof(P)))
            {
                if ((int)i <= 0) continue;
                var stat = ExcelToJSONConfigManager.GetId<StatData>((int)i);
                if (stat == null)
                {
                    Debug.LogError($"No found {(P)i} in StatData");
                    continue;
                }
                ComplexValue value = Mathf.Max(0, stat.InitValue);
                if (stat?.MaxValue > 0) value.Max = stat.MaxValue;
                initProperties.Add((P)i, value);
            }

            return initProperties;
        }

        public static bool TryAdd(this Dictionary<P, ComplexValue> dic, P p, int v, AddType addType = AddType.Base)
        {
            if (!dic.ContainsKey(p))
            {
                dic.Add(p, v);
                return false;
            }
            dic[p].ModifyValueAdd(addType, v);
            return true;
        }

        public static bool TryAddBase(this Dictionary<P, ComplexValue> dic, string properties, string propertyValues)
        {
            var types = properties.SplitToInt();
            var propertyeValues = propertyValues.SplitToInt();

            if (types.Count != propertyeValues.Count) return false;

            for (var i = 0; i < types.Count; i++)
            {
                var p = (P)types[i];
                var v = propertyeValues[i];
                dic.TryToAddBase(p, v);
            }

            return true;
        }

        public static IList<HeroProperty> ToHeroProperty(this Dictionary<P, ComplexValue> dic)
        {
            var list = new List<HeroProperty>();
            foreach (var i in dic)
            {
                list.Add(new HeroProperty { Value = i.Value, Property = i.Key });
            }
            return list;
        }

        public static Dictionary<P, ComplexValue> CreateMonsterProperties(this CharacterData data, MonsterData monster )
        {
            var p = GetInitStat();
            p.TryAddBase(data.Properties, data.PropertyValues);
            p.TryAddBase(monster.Properties, monster.PropertyValues);
            return p;
        }

        public static Dictionary<P, ComplexValue> CreatePlayerProperties(this CharacterData data, CharacterLevelUpData level = null)
        {
            var p = GetInitStat();
            p.TryAddBase(data.Properties, data.PropertyValues);
            if (level != null)  p.TryAddBase(level.Properties, level.PropertyValues);
            return p;
        }
    }
}
