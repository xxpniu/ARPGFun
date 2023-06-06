using System;
using System.Collections.Generic;

namespace Proto
{

    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    public class IndexAttribute:Attribute
    {
         public IndexAttribute(int index,Type tOm) 
         {
            this.Index = index;
            this.TypeOfMessage = tOm;
         }

        public int Index { set; get; }

        public Type TypeOfMessage { set; get; }
    }

    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class ApiVersionAttribute : Attribute
    {
        public ApiVersionAttribute(int m, int dev, int bate)
        {
            if (m > 99 || dev > 99 || bate > 99) throw new Exception("must less then 100");
            v = m * 10000 + dev * 100 + bate;
        }
        private readonly int v = 0;
        public int Version { get { return v; } }
    }


    //[ATTRIBUTES]
    [ApiVersion(1,1,1)]
    public static class MessageTypeIndexs
    {
        private static readonly Dictionary<int, Type> types = new Dictionary<int, Type>();

        private static readonly Dictionary<Type, int> indexs = new Dictionary<Type, int>();
        
        static MessageTypeIndexs()
        {
            var tys = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(IndexAttribute), false) as IndexAttribute[];

            foreach(var t in tys)
            {
                types.Add(t.Index, t.TypeOfMessage);
                indexs.Add(t.TypeOfMessage, t.Index);
            }

            var ver = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(ApiVersionAttribute), false) as ApiVersionAttribute[];
            if (ver != null && ver.Length > 0)
                Version = ver[0].Version;
        }


        public static int Version { get; private set; }

        /// <summary>
        /// Tries the index of the get.
        /// </summary>
        /// <returns><c>true</c>, if get index was tryed, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="index">Index.</param>
        public static bool TryGetIndex(Type type,out int index)
        {
            return indexs.TryGetValue(type, out index);
        }
        /// <summary>
        /// Tries the type of the get.
        /// </summary>
        /// <returns><c>true</c>, if get type was tryed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="type">Type.</param>
        public static bool TryGetType(int index,out Type type)
        {
            return types.TryGetValue(index, out type);
        }
    }
}
