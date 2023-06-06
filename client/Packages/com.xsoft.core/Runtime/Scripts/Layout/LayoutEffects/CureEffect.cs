using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    [EditorEffect("恢复生命")]
    [EffectId(4)]
    public class CureEffect:EffectBase
    {
        [Label("取值来源")]
        public ValueOf valueType = ValueOf.NormalAttack;

        [Label("值")]
        public ValueSourceOf value =0;
    }
}

