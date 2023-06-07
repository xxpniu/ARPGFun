using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Proto;
using Proto.ServerConfig;
using ServerUtility;
using XNet.Libs.Utility;
using Utility;

namespace ChatServer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            ChatServerConfig config;
            if (args.Length > 0)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, args[0]);
                var json = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                config = json.TryParseMessage<ChatServerConfig>();
            }
            else
            {
                var testHost = "127.0.0.1";

                config = new ChatServerConfig
                {
                    DBHost = "mongodb://127.0.0.1:27017/",
                    DBName = "Chat",
                    ServicsHost = new ServiceAddress { IpAddress = testHost, Port = 1500 },
                    ChatServerRoot = "/Chat",
                    ChatServerID = 9999999,
                    ListenHost = new ServiceAddress { IpAddress = testHost, Port = 2200 },
                    LoginServerRoot ="/login",
                    MaxPlayer = 5000,
                    Player = 0,
                    ZkServer = { "129.211.9.75:2181" },
                    KafkaServer = { "129.211.9.75:9092" }
                };
            }
            using var log = new DefaultLoger(config.KafkaServer, "Log", $"chat_{config.ChatServerID}");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            Debuger.Log(config);

            var app = Application.S;
            await app.Start(config);
            await app.Tick();
            await app.Stop();
            Debuger.Log("Application had exited!");
            
        }
    }
}
