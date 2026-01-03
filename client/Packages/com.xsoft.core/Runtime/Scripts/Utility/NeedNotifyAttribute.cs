using System;

namespace GameLogic.Utility
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NeedNotifyAttribute : Attribute
    {
        public NeedNotifyAttribute(Type notifyType, params string[] pars)
        {
            NotifyType = notifyType;
            FieldNames = pars;

            Check();
        }

        public Type NotifyType { get; }
        public string[] FieldNames { get; }

        private void Check()
        {
            foreach (var i in FieldNames)
                if (NotifyType.GetProperty(i) == null)
                    throw new Exception($"{i} not found in type{NotifyType}");
        }
    }
}