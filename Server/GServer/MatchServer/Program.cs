using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
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
        public static async Task Main(string[] args)
        {
            var config = new MatchServerConfig
            {
                DBHost = "mongodb://127.0.0.1:27017/",
                ServicsHost = new ServiceAddress { IpAddress = "localhost", Port = 1500 },
                ZkServer = { "129.211.9.75:2181" },
                DBName = "Match",
                BattleServerRoot = "/battle",
                NotifyServerRoot = "/notify",
                MatchServerRoot = "/match",
                KafkaServer = { "129.211.9.75:9092" }
            };

            if (args.Length > 0)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[0]);
                var json = File.ReadAllText(file, new UTF8Encoding(false));
                config = json.TryParseMessage<MatchServerConfig>();
            }

            using var log = new DefaultLoger(config.KafkaServer, "Log", $"match_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            Debuger.Log($"{config}");


            var zk = new ZooKeeper(GRandomer.RandomList(config.ZkServer), 3000, new DefaultWatcher());

            var battleWatcher = new WatcherServer<string, BattleServerConfig>(zk, config.BattleServerRoot, (s) => s.ServerID);
            await battleWatcher.RefreshData();
            var notifyWatcher = await new WatcherServer<string, NotifyServerConfig>(zk, config.NotifyServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}").RefreshData();
            var service = new MatchServerService(battleWatcher, notifyWatcher);

            var server = new LogServer()
            {
                Ports = { new ServerPort("0.0.0.0", config.ServicsHost.Port, ServerCredentials.Insecure) }
            }.BindServices(Proto.MatchServices.BindService(service));

            server.Start();
            var app = App.S;
            if (await zk.existsAsync(config.MatchServerRoot) == null)
            {
                await zk.createAsync(config.MatchServerRoot, new byte[] { 0 }, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }

            await zk.createAsync($"{config.MatchServerRoot}/{config.ServicsHost.IpAddress}:{config.ServicsHost.Port}",
                Encoding.UTF8.GetBytes(config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);

            DataBase.DataBaseTool.S.Init(config.DBHost, config.DBName);

            await app.Startup();
            await app.Tick();
            await app.Stop();
            await zk.closeAsync();
            await server.ShutdownAsync();
            Debuger.Log("Application had exited!");
        }
    }
}
