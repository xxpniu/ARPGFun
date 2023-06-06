using System.Collections;
using System.Collections.Generic;
using BattleViews.Views;
using Proto;
using UnityEngine;

public enum StateType
{
    None,
    Running,
    Ending
}

public interface IBattleGate
{
    float TimeServerNow { get; }
    UPerceptionView PreView { get; }
    Texture LookAtView { get; }
    UCharacterView Owner { get; }
    PlayerPackage Package { get; }
    DHero Hero { get; }
    bool ReleaseSkill(HeroMagicData data);
    void Exit();
    bool MoveDir(UnityEngine.Vector3 dir);
    bool TrySendLookForward(bool force);
    bool DoNormalAttack();
    bool SendUseItem(ItemType type);
    bool IsHpFull();
    bool IsMpFull();
    StateType State { get; }

    float LeftTime { get; }
    
}