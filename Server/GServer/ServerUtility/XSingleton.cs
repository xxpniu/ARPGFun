 

namespace ServerUtility
{
    public class XSingleton<T> where T : class, new()
    {
        protected XSingleton() { }

        private class InnerClass
        {
            public static T Inst = new T();
            internal static void Reset()
            {
                Inst = new T();
            }
        }

        public static T Singleton => InnerClass.Inst;

        public static T S => Singleton;

        public static void ResetSingle()
        {
            InnerClass.Reset();
        }
        
    }
}

