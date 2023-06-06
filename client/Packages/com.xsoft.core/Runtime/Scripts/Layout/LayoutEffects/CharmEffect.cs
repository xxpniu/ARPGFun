using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    [EditorEffect("魅惑")]
    public class CharmEffect:EffectBase
    {
        [Label("成功概率万分比")]
        public ValueSourceOf ProValue = 10000;

        [Label("控制最大等级")]
        public ValueSourceOf Level = 1;

        [Label("控制时间")]
        public ValueSourceOf Time =1000;
    }
}
