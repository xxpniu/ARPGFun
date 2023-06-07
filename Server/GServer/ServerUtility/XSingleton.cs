﻿using System;
namespace ServerUtility
{
    public class XSingleton<T> where T : class, new()
    {
        protected XSingleton() { }

        private class InnerClass
        {
            public static T inst = new T();
            internal static void Reset()
            {
                inst = new T();
            }
        }

        public static T Singleton
        {
            get
            {
                return InnerClass.inst;
            }
        }

        public static T S { get { return Singleton; } }

        public static void ResetSingle()
        {
            InnerClass.Reset();
        }

    }
}

