using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{


    public enum ValueFromType
    {
        Releaser,
        Value
    }

    //[EditorEffect("切换团队")]
    //[EffectId(2)]
    public class ModifyTeamIndexEffect:EffectBase
    {
        [Label("取值来源")]
        public ValueFromType valueFromType = ValueFromType.Releaser;

        [Label("队伍ID")]
        public int TeamIndex = 0;

        [Label("最大控制角色等级")]
        public ValueSourceOf Level = 1;
    }
}
