using BattleViews.Views;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace UApp.GameGates
{
    public enum StateType
    {
        None,
        Running,
        Ending
    }

    public enum ReleaseResult
    {
        Success,
        NoMp,
        NoTarget,
        Stopping,
        NotFoundSkill
    }

    public interface IBattleGate
    {
        float TimeServerNow { get; }
        UPerceptionView PreView { get; }
        Texture LookAtView { get; }
        UCharacterView Owner { get; }
        PlayerPackage Package { get; }
        DHero Hero { get; }
        StateType State { get; }
        float LeftTime { get; }
        bool ReleaseSkill(HeroMagicData data, Vector3? dir, out ReleaseResult result); 
        void Exit();
        bool MoveDir(Vector3 dir);
        bool TrySendLookForward(bool force);
        bool DoNormalAttack();
        bool SendUseItem(ItemType type);
        bool IsHpFull();
        bool IsMpFull();
        void TryBreakRelease();
        bool IsInStartLayout();
    }
}