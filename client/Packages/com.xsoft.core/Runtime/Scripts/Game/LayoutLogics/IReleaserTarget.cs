using GameLogic.Game.Elements;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.LayoutLogics
{
    public interface IReleaserTarget
	{
		BattleCharacter Releaser{ get; }
		BattleCharacter ReleaserTarget { get; }
		UVector3 TargetPosition{ get; }
	}
}

