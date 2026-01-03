using System;

namespace Layout.EditorAttributes
{
    public class EditorEffectAttribute : Attribute
    {
        public EditorEffectAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EffectIdAttribute : Attribute
    {
        public EffectIdAttribute(int id)
        {
            ID = id;
        }

        public int ID { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ParamIndexAttribute : Attribute
    {
        public ParamIndexAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; set; }
    }

    public class EditorEffectsAttribute : Attribute
    {
    }
}