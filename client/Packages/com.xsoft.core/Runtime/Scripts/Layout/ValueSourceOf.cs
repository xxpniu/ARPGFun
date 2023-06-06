using System;
using GameLogic.Game.Elements;
using Layout.EditorAttributes;
using Proto;

namespace Layout
{
    public class ValueSourceOf
    {
        [Label("取值来源")]
        public GetValueFrom ValueForm = GetValueFrom.CurrentConfig;

        [Label("数值")]
        public int Value = 0;

        public override string ToString()
        {
            if (ValueForm == GetValueFrom.CurrentConfig)
                return $"{ValueForm} {Value}";
            else return $"{ValueForm}";
        }

        public static implicit operator ValueSourceOf(int value)
        {
            return new ValueSourceOf() {  ValueForm = GetValueFrom.CurrentConfig, Value = value};
        }
        public static implicit operator ValueSourceOf(float value)
        {
            return new ValueSourceOf() { ValueForm = GetValueFrom.CurrentConfig, Value = (int)value };
        }


        public int ProcessValue(MagicReleaser releaser)
        {
            switch (ValueForm)
            {
                case GetValueFrom.CurrentConfig: return Value;
                default:
                    return releaser.TryGetParams(ValueForm);
            }
        
        }
    }
}
