using System;
using System.Threading.Tasks;
using ServerUtility;
using XNet.Libs.Utility;
using Proto.ServerConfig;
using System.IO;
using System.Text;
using Utility;
using Grpc.Core;
using org.apache.zookeeper;
using static org.apache.zookeeper.ZooDefs;
using Proto;

namespace NotifyServer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new NotifyServerConfig
            {
                DBHost = "mongodb://127.0.0.1:27017/",
                ServicsHost = new ServiceAddress { IpAddress = "localhost", Port = 1300 },
                ZkServer = { "129.211.9.75:2181" },
                DBName = "Chat",
                ChatServerRoot = "/Chat",
                NotifyServerRoot = "/notify",
                KafkaServer = { "129.211.9.75:9092" }
            };

            if (args.Length > 0)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, args[0]);
                var json = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                config = json.TryParseMessage<NotifyServerConfig>();
            }
            using var log = new DefaultLoger(config.KafkaServer, "Log", $"notify_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);
            Debuger.Log($"{config}");


            var zk = new ZooKeeper(GRandomer.RandomList(config.ZkServer), 3000, new DefaultWatcher());

            var chat = new WatcherServer<int, ChatServerConfig>(zk, config.ChatServerRoot, (s) => s.ChatServerID);
            var service = new NotifyServerService(chat);
            await chat.RefreshData();
            var server = new LogServer()
            {
                Ports = { new ServerPort("0.0.0.0", config.ServicsHost.Port, ServerCredentials.Insecure) }
            }.BindServices(Proto.NotifyServices.BindService(service));
            server.Start();

            var app = App.S;

            if (await zk.existsAsync(config.NotifyServerRoot) == null)
            {
                await zk.createAsync(config.NotifyServerRoot, new byte[] { 0 }, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }
            await zk.createAsync($"{config.NotifyServerRoot}/{config.ServicsHost.IpAddress}:{config.ServicsHost.Port}",
                Encoding.UTF8.GetBytes(config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
            await app.Startup(async (a)=>
            {
                ChatTool.DataBase.S.Init(config.DBHost, config.DBName);
                await Task.CompletedTask;
            });
            await app.Tick();
            await app.Stop();

            await server.ShutdownAsync();
            await GrpcEnvironment.ShutdownChannelsAsync();
        }
    }
}
