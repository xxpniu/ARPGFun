using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using DataBase;
using Grpc.Core;
using Newtonsoft.Json;
using org.apache.zookeeper;
using Proto;
using Proto.ServerConfig;
using ServerUtility;
using Utility;
using XNet.Libs.Utility;
using static org.apache.zookeeper.ZooDefs;

namespace MatchServer
{
    public class Program
    {
        public class MatchOption:CustomOption
        {
            [Option('r',"zkroot", Required = true)]
            public string ZKRoot { set; get; }
            [Option('t',"zknotify", Required = true)]
            public string ZKNotify { set; get; }
            [Option('b',"zkbattle", Required = true)]
            public string ZKBattle { set; get; }
            
            [Option('j',"jenkins", Required=true)]
            public string Jenkins { set; get; }
            
            
        }
        public static async Task Main(string[] args)
        {
            var config = new MatchServerConfig();

            Parser.Default.ParseArguments<MatchOption>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, o.Config);
                        var json = File.ReadAllText(file, new UTF8Encoding(false));
                        config = json.TryParseMessage<MatchServerConfig>();
                    }

                    o.Kafka?.SplitInsert(config.KafkaServer);
                    o.ZK.SplitInsert(config.ZkServer);
                    o.ServiceHost?.SetAddress(a => config.ServicsHost = a);
                    o.DBHost?.Set(s => config.DBHost = s);
                    o.DBName?.Set(s => config.DBName = s);
                    o.ZKBattle?.Set(s=>config.BattleServerRoot=s);
                    o.ZKNotify?.Set(s=>config.NotifyServerRoot=s);
                    o.ZKRoot?.Set(s=>config.MatchServerRoot=s);
                    o.Jenkins?.Set(s=> config.JenkinsUrl = s);
                });

            using var log = new DefaultLogger(config.KafkaServer, "Log", $"match_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            Debuger.Log($"{config}");


            var zk = new ZooKeeper(GRandomer.RandomList(config.ZkServer), 3000, new DefaultWatcher());

            var battleWatcher =
                new WatcherServer<string, BattleServerConfig>(zk, config.BattleServerRoot, (s) => s.ServerID)
                {
                    OnChanged = OnBattleChanged
                };
            await battleWatcher.RefreshData();
            
            var notifyWatcher = await new WatcherServer<string, NotifyServerConfig>(zk, config.NotifyServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}").RefreshData();
            var service = new MatchServerService(battleWatcher, notifyWatcher);

            var server = new LogServer()
            {
                Ports = { new ServerPort("0.0.0.0", config.ServicsHost.Port, ServerCredentials.Insecure) }
            }.BindServices(Proto.MatchServices.BindService(service));

            server.Start();
      
            if (await zk.existsAsync(config.MatchServerRoot) == null)
            {
                await zk.createAsync(config.MatchServerRoot, new byte[] { 0 }, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }

            await zk.createAsync($"{config.MatchServerRoot}/{config.ServicsHost.IpAddress}:{config.ServicsHost.Port}",
                Encoding.UTF8.GetBytes(config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);

            DataBase.DataBaseTool.S.Init(config.DBHost, config.DBName);

            await App.S.Create().Run();
            await zk.closeAsync();
            await server.ShutdownAsync();
            Debuger.Log("Application had exited!");
        }

        private static async void OnBattleChanged(BattleServerConfig[] old, BattleServerConfig[] newList)
        {
            var newServerIds = newList.Select(t => t.ServerID);
            var diff = old.Where(t => !newServerIds.Contains(t.ServerID)).Select(t=>t.ServerID).ToArray();
            foreach (var d in diff)
            {
                Debuger.Log($"Remove Battle match:{d}");
                await DataBaseTool.S.RemoveMatchByServerId(d);
            }
        }
    }
}
