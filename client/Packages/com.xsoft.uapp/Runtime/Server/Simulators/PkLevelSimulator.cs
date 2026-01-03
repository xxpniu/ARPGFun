using Proto;

namespace Server
{
    [LevelSimulator(MType = MapType.Pk)]
    internal class PkLevelSimulator : BattleLevelSimulator
    {
        private int TeamIndex;

        protected override int PlayerTeamIndex => TeamIndex++;
    }
}