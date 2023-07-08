using EConfig;
using Proto;
using UnityEngine;

namespace GameLogic.Game.Elements
{
    public class BattleCharacterMagic
    {
        public MagicType Type { private set; get; }

        public int MpCost { private set; get; }

        public CharacterMagicData Config { private set; get; }

        public int ConfigId => Config.ID;

        public BattleCharacterMagic(MagicType type, CharacterMagicData config, MagicLevelUpData lv = null, float? cdTime = null)
        {
            Type = type;
            Config = config;
            this.LevelData = lv;
            MpCost = config.MPCost;
            MpCost = lv?.MPCost ?? MpCost;
            if (cdTime.HasValue) CdTime = cdTime.Value;
            else CdTime = config.TickTime / 1000f;
        }
        
        private MagicLevelUpData LevelData { set; get; }

        public float CdTime { get; set; }

        public float CdCompletedTime { set; get; }

        public string[] Params => new[]
        {
            LevelData?.Param1, LevelData?.Param2, LevelData?.Param3, LevelData?.Param4, LevelData?.Param5
        };

        public bool IsCoolDown(float time) => time > CdCompletedTime;

    }
}