using System;
using Layout.EditorAttributes;
using Proto;

namespace Layout.LayoutEffects
{

    [EditorEffect("行为锁")]
    [EffectId(2)]
    public class ModifyLockEffect:EffectBase
    {
        [Label("类型")]
        public ActionLockType lockType = ActionLockType.NoAttack;

        [Label("回滚方式")]
        public RevertType revertType = RevertType.ReleaserDeath;
    }
}

