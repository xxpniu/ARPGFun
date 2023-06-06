using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    public enum AddType
    {
        Base,
        Append,
        Rate
    }

    public enum RevertType
    {
        None,
        ReleaserDeath
    }

    [EditorEffect("修改属性")]
    [EffectId(6)]
    public class AddPropertyEffect : EffectBase
    {
        [Label("修改类型")]
        public AddType addType =AddType.Append;

        [Label("修改值")]
        public ValueSourceOf addValue =0 ;

        [Label("恢复方式")]
        public RevertType revertType= RevertType.ReleaserDeath;

        [Label("属性类型")]
        public Proto.HeroPropertyType property;
    }
}

