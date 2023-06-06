using System;
using Google.Protobuf;

namespace GameLogic.Utility
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple =  false) ]
    public class NeedNotifyAttribute : Attribute
    {
        public Type NotifyType{ private set; get; }
        public string[] FieldNames { private set; get; }

        public NeedNotifyAttribute(Type notifyType, params string[] pars)
        {
            NotifyType = notifyType;
            FieldNames = pars;

            Check();
           
        }

        private void Check()
        {
            foreach (var i in FieldNames)
            {
                if (NotifyType.GetProperty(i) == null)
                {
                    throw new Exception($"{i} nofound in type{NotifyType}");
                }
            }
        }

    }
}
