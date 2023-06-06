using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ExcelConfig
{
    public class JSONConfigBase
    {
        public int ID { set; get; }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigFileAttribute : Attribute
    {
        public ConfigFileAttribute(string fileName, string tableName)
        {
            this.FileName = fileName;
            this.TableName = tableName;
        }

        public string TableName { set; get; }

        public string FileName { set; get; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelConfigColIndexAttribute : Attribute
    {
        public ExcelConfigColIndexAttribute(int index)
        {
            Index = index;
        }
        public int Index { set; get; }
    }
    public interface IConfigLoader
    {
        List<T> Deserialize<T>() where T : JSONConfigBase;
        OnDebug Printer { get; }
    }

    public delegate bool FindCondition<T>(T item) where T : JSONConfigBase;

    public delegate void OnDebug(string log);

    public class ExcelToJSONConfigManager
    {
        public ExcelToJSONConfigManager(IConfigLoader loader)
        {
            Loader = loader;
            Current = this;
        }

        private IConfigLoader Loader { set; get; }

        public static ExcelToJSONConfigManager Current { private set; get; }

        private  ConcurrentDictionary<Type, Dictionary<int, JSONConfigBase>> Configs { get; } = new ConcurrentDictionary<Type, Dictionary<int, JSONConfigBase>>();

        private (bool existed, Dictionary<int, JSONConfigBase>) TryToLoad<T>() where T : JSONConfigBase
        {
            var ty = typeof(T);
            if (Configs.TryGetValue(ty, out Dictionary<int, JSONConfigBase> dicTable))
            {
                return (true, dicTable);
            }
            dicTable = new Dictionary<int, JSONConfigBase>();
            var table = Loader.Deserialize<T>();
            foreach (var i in table)
            {
                dicTable.Add(i.ID, i);
            }
            if (!Configs.TryAdd(ty, dicTable))
            {
                return (false, null);
            }
            return (dicTable.Count > 0, dicTable);
        }

        private T FirstConfig<T>(FindCondition<T> conditon) where T : JSONConfigBase
        {
            (var suc, var values) =  TryToLoad<T>();
            if (suc)
            {
                foreach (var i in values)
                {
                    if (i.Value is T temp && conditon(temp))
                    {
                        return temp;
                    }
                }
            }
            return default;
        }

        public void Clear()
        {
            Configs.Clear();
        }

        private T GetConfigByID<T>(int id) where T : JSONConfigBase
        {
            var ( suc,  values) =  TryToLoad<T>();
            if (suc)
            {
                if (values.TryGetValue(id, out JSONConfigBase t))
                {
                    return t as T;
                }
            }
            return default;
        }

        private  T[] GetConfigs<T>() where T : JSONConfigBase
        {
             var( suc,  values) =  TryToLoad<T>();
            if (suc) return values.Select(t => t.Value as T).ToArray();
            else return new T[0];
        }

        private T[] GetConfigs<T>(FindCondition<T> condition) where T : JSONConfigBase
        {
            (var suc, var values) =  TryToLoad<T>();
            if (suc)
            {
                var list = new List<T>(16);
                foreach (var i in values)
                {
                    if (i.Value is T temp && condition(temp))
                    {
                        list.Add(temp);
                    }
                }
                return list.ToArray();
            }
            return new T[0];
        }

        public static T GetId<T>(int id) where T : JSONConfigBase
        {
            return Current.GetConfigByID<T>(id);
        }

        public static T[] Find<T>(FindCondition<T> condition = null) where T : JSONConfigBase
        {
            if (condition == null) return Current.GetConfigs<T>();
            else return Current.GetConfigs(condition);
        }

        public static string GetFileName<T>() where T : JSONConfigBase
        {
            var type = typeof(T);
            return GetFileName(type);
        }

        public static string GetFileName(Type type)
        {
            var atts = type.GetCustomAttributes(typeof(ConfigFileAttribute), false) as ConfigFileAttribute[];
            if (atts.Length > 0)
            {
                return atts[0].FileName;
            }
            return string.Empty;
        }

        public static T First<T>(FindCondition<T> conditon) where T : JSONConfigBase
        {
            return Current.FirstConfig(conditon);
        }
    }
}
