using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExcelConfig;
using org.apache.zookeeper;
using XNet.Libs.Utility;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerUtility
{
    public class ResourcesLoader : Watcher, IConfigLoader
    {

        private ZooKeeper _zoo = null!;
        private readonly ConcurrentDictionary<string, string> _configs = new ConcurrentDictionary<string, string>();
        private string _prefixPath = null!;
        private ExcelToJSONConfigManager _manager = null!;
        public OnDebug Printer => Debuger.LogWaring;

        public async Task LoadAllConfig(IList<string> configRoot, string path)
        {
            _prefixPath = path;
            Debuger.DebugLog($"root->{configRoot[0]} Path:{path}");
            _manager = new ExcelToJSONConfigManager(this);
            _zoo = new ZooKeeper(configRoot[0], 30000, this, true);
            while (!(await Reload()))
            {
                await Task.Delay(2000);
            }
        }

        private async Task<bool> Reload()
        {
            Debuger.Log($"Load:{_prefixPath}");
            try
            {
                var res = await _zoo.getChildrenAsync(_prefixPath, this);
                foreach (var c in res.Children)
                {
                    var p = $"{_prefixPath}/{c}";
                    var data = await _zoo.getDataAsync(p);
                    _configs.TryRemove(c, out _);
                    _configs.TryAdd(c, Encoding.UTF8.GetString(data.Data));
                    Debuger.Log($"Load:{p}");
                }

                _manager.Clear();
                return true;
            }
            catch (KeeperException.NoNodeException)
            {
                Debuger.LogError($"Node not found:{_prefixPath}");
                return false;
            }
            catch
            {
                return false;
            }
        }

        public List<T> Deserialize<T>() where T : JSONConfigBase
        {
            var name = ExcelToJSONConfigManager.GetFileName<T>();
            if (!_configs.TryGetValue(name, out var json)) return null!;
            if (string.IsNullOrEmpty(json)) return null!;
    
			var type = typeof(T);
			//ExcelConfigColIndexAttribute
			var properties = type.GetProperties(BindingFlags.Public| BindingFlags.Instance)
				.Where(t => t.GetCustomAttribute<ExcelConfigColIndexAttribute>() is not null)
				.Select(t=> new
				{
					Index = t.GetCustomAttribute<ExcelConfigColIndexAttribute>(),
					Info = t
				})
				.OrderBy(t=>t.Index.Index).ToArray()
				;
			var jArr =  JsonConvert.DeserializeObject<JArray>(json);
			var raw = jArr!.Count;
			var list = new List<T>();
			for (var i = 0; i < raw; i++)
			{
				var item = Activator.CreateInstance<T>();
				var rawData = (JArray)jArr[i];
				if (rawData.Count != properties.Length)
				{
					throw new ArgumentException($"raw != properties {rawData.Count} != {properties.Count()}");
				}

				for (var index = 0; index < rawData.Count; index++)
				{
					var property = properties[index];
					try
					{
						if (property.Info.PropertyType == typeof(string))
						{
							property.Info.SetValue(item, rawData[index].Value<string>());
						}
						else if (property.Info.PropertyType == typeof(int))
						{
							var rawVal = rawData[index];
							switch (rawVal.Type)
							{
								case JTokenType.Float:
									property.Info.SetValue(item, (int)rawData[index]);
									break;
								case JTokenType.Integer:
									property.Info.SetValue(item, rawVal.Value<int>());
									break;
								case JTokenType.String:
								{
									var str = rawVal.Value<string>();
									if (!int.TryParse(str, out var v)) v = -1;
									property.Info.SetValue(item, v);
								}
									
									break;
								default:
									throw new InvalidCastException($"{rawVal.Type} can't case into int");
							}
						}
						else if (property.Info.PropertyType == typeof(float))
						{
							property.Info.SetValue(item, rawData[index].Value<float>());
						}
						else
						{
							throw new InvalidCastException(
								$"{property.Info.PropertyType} unsupported in type {typeof(T)}");
						}
					}
					catch (Exception ex)
					{
						Debuger.LogError(ex);
						Debuger.LogError($"{typeof(T)} {property.Info.PropertyType} {property.Info.Name} [{index}] of {rawData[index]} - {rawData}");
					}
				}

				list.Add(item);
			}

			return list;
        }
        
        

        public override async Task process(WatchedEvent @event)
        {
            var path = @event.getPath();
            Debuger.Log($"Process:{@event.getPath()}-> {@event}");
            if (string.IsNullOrEmpty(path)) return;
            if (!@event.getPath().StartsWith(_prefixPath)) return;
            Debuger.Log($"StartLoad Config changes:{@event.getPath()}");
            await Reload();
        }

        public async Task Close()
        {
            await _zoo?.closeAsync()!;
        }
    }
}

