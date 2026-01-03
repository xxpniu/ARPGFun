using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    [EditorEffect("恢复魔法")]
    [EffectId(3)]
    public class CureMPEffect : EffectBase
    {
        [Label("值")] public ValueSourceOf value = 0;

        [Label("取值来源")] public ValueOf valueType = ValueOf.NormalAttack;
    }
}