using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    public enum BreakReleaserType
    {
        ALL = 0,
        InStartLayoutMagic,
        Buff
    }

    [EditorEffect("打断施法")]
    [EffectId(5)]
    public class BreakReleaserEffect :EffectBase
    {
        [Label("打断类型")]
        public BreakReleaserType breakType = BreakReleaserType.InStartLayoutMagic;
    }
}

