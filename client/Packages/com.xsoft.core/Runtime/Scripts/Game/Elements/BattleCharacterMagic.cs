using EConfig;
using Proto;

namespace GameLogic.Game.Elements
{
    public class BattleCharacterMagic
    {
        public BattleCharacterMagic(MagicType type, CharacterMagicData config, MagicLevelUpData lv = null,
            float? cdTime = null)
        {
            Type = type;
            Config = config;
            LevelData = lv;
            MpCost = config.MPCost;
            MpCost = lv?.MPCost ?? MpCost;
            if (cdTime.HasValue) CdTime = cdTime.Value;
            else CdTime = config.TickTime / 1000f;
        }

        public MagicType Type { private set; get; }

        public int MpCost { get; }

        public CharacterMagicData Config { get; }

        public int ConfigId => Config.ID;

        private MagicLevelUpData LevelData { get; }

        public float CdTime { get; set; }

        public float CdCompletedTime { set; get; }

        public string[] Params => new[]
        {
            LevelData?.Param1, LevelData?.Param2, LevelData?.Param3, LevelData?.Param4, LevelData?.Param5
        };

        public bool IsCoolDown(float time)
        {
            return time > CdCompletedTime;
        }
    }
}