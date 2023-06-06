using GameLogic.Game.Elements;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.LayoutLogics
{
	public class ReleaseAtTarget : IReleaserTarget
	{
		public ReleaseAtTarget(BattleCharacter releaser, BattleCharacter target)
		{
			Releaser = releaser;
			ReleaserTarget = target;
			TargetPosition = target.Position;
		}

		public BattleCharacter Releaser { get; private set; }

		public BattleCharacter ReleaserTarget { get; private set; }

		public UVector3 TargetPosition
		{
			get;
			private set;
		}
	}

    public class ReleaseAtPos : IReleaserTarget
	{
		public ReleaseAtPos(BattleCharacter releaser, UVector3 target)
		{
			ReleaserTarget = Releaser = releaser;
			TargetPosition = target;
		}

		public BattleCharacter Releaser { get; private set; }

		public BattleCharacter ReleaserTarget { get; private set; }

		public UVector3 TargetPosition
		{
			get;
			private set;
		}
	}
}

