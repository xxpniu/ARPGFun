using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ExcelConfig;
using org.apache.zookeeper;
using XNet.Libs.Utility;
using System.Text;
using Newtonsoft.Json;

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
           
            var res =  JsonConvert.DeserializeObject<List<T>>(json);

            Debuger.DebugLog($"Load:{name} Table:{res!.Count}");
            return res;
        }

        public override async Task process(WatchedEvent @event)
        {
            var path = @event.getPath();
            Debuger.Log($"Process:{@event.getPath()}-> {@event}");
            if (string.IsNullOrEmpty(path)) return;
            if (@event.getPath().StartsWith(_prefixPath))
            {
                Debuger.Log($"StartLoad Config changes:{@event.getPath()}");
                await Reload();
            }
            return ;
        }

        public async Task Close()
        {
            await _zoo?.closeAsync()!;
        }
    }
}

