using System;

namespace Layout
{
    public class FieldValue
    {
        public enum ValueType
        {
            Fixed,
            Range
        }

        private static readonly Random _rander = new();

        public int max;


        public int min;


        public ValueType type = ValueType.Fixed;


        public int Value
        {
            get
            {
                switch (type)
                {
                    case ValueType.Range:
                        return _rander.Next(min, max);
                    case ValueType.Fixed:
                    default: return min;
                }
            }
        }

        public static implicit operator FieldValue(int value)
        {
            return new FieldValue { max = value, min = value, type = ValueType.Fixed };
        }

        public static implicit operator int(FieldValue value)
        {
            return value.Value;
        }

        public static implicit operator float(FieldValue value)
        {
            return value.Value;
        }
    }
}