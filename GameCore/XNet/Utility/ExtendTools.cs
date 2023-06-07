using System;
using System.Collections;
using Proto;

namespace XNet.Libs.Utility
{
    public static class ExtendTools
    {
        private static ServiceAddress AsAddress(this string str)
        {
            var args = str.Split(':');
            return new ServiceAddress
            {
                IpAddress = args[0],
                Port = int.Parse(args[1])
            };
        }

        public static void SetAddress(this string str, Action<ServiceAddress> setter)
        {
            var add = str.AsAddress();
            setter.Invoke(add);
        }
        public static void SplitInsert<T>(this string str, T list, char key =',')
            where T : IList
        {
            list.Clear();
            var arrs = str.Split(key); 
            foreach (var a in arrs)
            {
                list.Add(a);
            }
        }

        public static void Set(this string str, Action<string> setter)
        {
            setter?.Invoke(str);
        }
        
        
    }
}