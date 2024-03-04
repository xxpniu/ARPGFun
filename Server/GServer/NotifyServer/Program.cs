using System;
using System.Threading.Tasks;
using ServerUtility;
using XNet.Libs.Utility;
using Proto.ServerConfig;
using System.IO;
using System.Text;
using CommandLine;
using Utility;
using Grpc.Core;
using org.apache.zookeeper;
using static org.apache.zookeeper.ZooDefs;
using Proto;

namespace NotifyServer
{
    internal static class Program
    {
        public class NotifyOption : CustomOption
        {
            [Option('r',"zkroot", Required = true)]
            public string ZkRoot { set; get; }
            [Option('h',"zkchat", Required = true)]
            public string ZkChatRoot { set; get; }
        }

        public static async Task Main(string[] args)
        {
            var config = new NotifyServerConfig();
            Parser.Default.ParseArguments<NotifyOption>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, o.Config);
                        var json = File.ReadAllText(file, new UTF8Encoding(false));
                        config = json.TryParseMessage<NotifyServerConfig>();
                    }

                    o.Kafka?.SplitInsert(config.KafkaServer);
                    o.ZK.SplitInsert(config.ZkServer);
                    o.ServiceHost?.SetAddress(a => config.ServicsHost = a);
                    o.DBHost?.Set(s => config.DBHost = s);
                    o.DBName?.Set(s => config.DBName = s);
                    o.ZkRoot?.Set(s => config.NotifyServerRoot = s);
                    o.ZkChatRoot?.Set(s => config.ChatServerRoot = s);
                });

            using var log = new DefaultLogger(config.KafkaServer, "Log", $"notify_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);
            Debuger.Log($"{config}");


            var zk = new ZooKeeper(GRandomer.RandomList(config.ZkServer), 3000, new DefaultWatcher());

            var chat = new WatcherServer<int, ChatServerConfig>(zk, config.ChatServerRoot, (s) => s.ChatServerID);
            var service = new NotifyServerService(chat);
            await chat.RefreshData();
            var server = new LogServer()
            {
                Ports = {new ServerPort("0.0.0.0", config.ServicsHost.Port, ServerCredentials.Insecure)}
            }.BindServices(Proto.NotifyServices.BindService(service));
            server.Start();



            if (await zk.existsAsync(config.NotifyServerRoot) == null)
            {
                await zk.createAsync(config.NotifyServerRoot, new byte[] {0}, Ids.OPEN_ACL_UNSAFE,
                    CreateMode.PERSISTENT);
            }

            await zk.createAsync($"{config.NotifyServerRoot}/{config.ServicsHost.IpAddress}:{config.ServicsHost.Port}",
                Encoding.UTF8.GetBytes(config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);


            await App.S.Create(setup: async (a, token) =>
            {
                ChatTool.DataBase.S.Init(config.DBHost, config.DBName);
                await Task.CompletedTask;
            }).Run();

            await server.ShutdownAsync();
            await GrpcEnvironment.ShutdownChannelsAsync();
        }
    }
}
