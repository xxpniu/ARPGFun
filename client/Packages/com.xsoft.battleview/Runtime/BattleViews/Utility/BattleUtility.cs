using System.Collections.Generic;
using Confluent.Kafka;
using EConfig;
using GameLogic;
using GameLogic.Game;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Proto;
using UnityEngine;
using XNet.Libs.Utility;
using  CM = ExcelConfig.ExcelToJSONConfigManager;
using P = Proto.HeroPropertyType;
namespace BattleViews.Utility
{
    public static class BattleUtility
    {
        
        public static Dictionary<P, ComplexValue>  CreateHeroProperties(DHero hero,PlayerPackage package)
        {
            var data = CM.GetId<CharacterData>(hero.HeroID);
            var level = CM.First<CharacterLevelUpData>(t => t.Level == hero.Level && t.CharacterID == hero.HeroID);
            var properties = data.CreatePlayerProperties(level);
            foreach (var i in hero.Equips)
            {
                package.Items.TryGetValue(i.GUID, out var equip);
                //var equip =  GetEquipByGuid(i.GUID);
                if (equip == null)
                {
                    Debug.LogError($"No found equip {i.GUID}");
                    continue;
                }
                var ps = equip.GetProperties();
                foreach (var p in ps)
                {
                    properties.TryToAddBase(p.Key, p.Value);
                }
            }

            return properties;
        }
    }
}