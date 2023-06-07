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

        private ZooKeeper zoo;
        private readonly ConcurrentDictionary<string, string> configs = new ConcurrentDictionary<string, string>();
        private string prefixPath;
        private ExcelToJSONConfigManager Manager;
        public OnDebug Printer => Debuger.LogWaring;

        public async Task LoadAllConfig(IList<string> configRoot, string path)
        {
            prefixPath = path;
            Debuger.DebugLog($"root->{configRoot[0]} Path:{path}");
            Manager = new ExcelToJSONConfigManager(this);
            zoo = new ZooKeeper(configRoot[0], 30000, this, true);
            await Reload();
        }

        private async Task Reload()
        {
            var res = await zoo.getChildrenAsync(prefixPath,this);
            foreach (var c in res.Children)
            {
                var p = $"{prefixPath}/{c}";
                var data = await zoo.getDataAsync(p);
                configs.TryRemove(c, out _);
                configs.TryAdd(c, Encoding.UTF8.GetString(data.Data));
                Debuger.Log($"Load:{p}");
            }
            Manager.Clear();
        }

        public List<T> Deserialize<T>() where T : JSONConfigBase
        {
            var name = ExcelToJSONConfigManager.GetFileName<T>();
            if (!configs.TryGetValue(name, out var json)) return null;
            if (string.IsNullOrEmpty(json)) return null;
           
            var res =  JsonConvert.DeserializeObject<List<T>>(json);

            Debuger.DebugLog($"Load:{name} Table:{res.Count}");
            return res;
        }

        public override async Task process(WatchedEvent @event)
        {
            var path = @event.getPath();
            Debuger.Log($"Process:{@event.getPath()}-> {@event}");
            if (string.IsNullOrEmpty(path)) return;
            if (@event.getPath().StartsWith(prefixPath))
            {
                Debuger.Log($"StartLoad Config changes:{@event.getPath()}");
                await Reload();
            }
            return ;
        }

        public async Task Close()
        {
            await zoo?.closeAsync();
        }
    }
}

