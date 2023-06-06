using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.vxwo.csharp.json;
using ServerUtility;
using XNet.Libs.Utility;
using Proto.ServerConfig;
using org.apache.zookeeper;
using Grpc.Core;
using Grpc.Core.Logging;
using Proto;
using Utility;

namespace LoginServer
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            var config = new LoginServerConfig
            {
                DBHost = "mongodb://127.0.0.1:27017/",
                ServicsHost = new ServiceAddress { IpAddress = "localhost", Port = 1800 },
                GateServersRoot = "/gate",
                ZkServer = { "129.211.9.75:2181" },
                DBName = "CenterAccount",
                ListenHost = new ServiceAddress { IpAddress = "localhost", Port = 1900 },
                Log = true,
                LoginServerRoot = "/login",
                KafkaServer = { "129.211.9.75:9092" }
            };
            if (args.Length > 0)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[0]);
                var json = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                config = json.TryParseMessage<LoginServerConfig>();
            }

            using var log = new DefaultLoger(config.KafkaServer, "Log", $"login_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            NetProtoTool.EnableLog = config.Log;
            Debuger.Log(config);

            var app = Appliaction.S;
            await app.Start(config);
            await app.Tick();
            await app.Stop();
            await GrpcEnvironment.ShutdownChannelsAsync();
            Debuger.Log("Application had exited!");
        }
    }
}
