using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using org.vxwo.csharp.json;
using Proto;
using Proto.ServerConfig;
using ServerUtility;
using XNet.Libs.Utility;
using Utility;

namespace GServer
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            GateServerConfig config;

            if (args.Length > 0)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[0]);
                var json = File.ReadAllText(file, new UTF8Encoding(false));
                config = json.TryParseMessage<GateServerConfig>();
            }
            else
            {
                var testHost = "127.0.0.1";

                config = new GateServerConfig
                {
                    DBHost = "mongodb://127.0.0.1:27017/",
                    DBName ="gate01",
                    ServicsHost = new ServiceAddress { IpAddress = testHost, Port = 2001 },
                    GateServersRoot = "/gate",
                    Log = true,
                    ServerID = 9999999,
                    ZkServer = { "129.211.9.75:2181" },
                    EnableGM = true,
                    ListenHost = new ServiceAddress { IpAddress= testHost, Port = 1700 },
                    MaxPlayer = 10,
                    Player = 1,
                    ExcelRoot ="/configs",
                    LoginServerRoot ="/login",
                    MatchServerRoot="/match",
                    NotifyServerRoot="/notify",
                    KafkaServer = { "129.211.9.75:9092" }
                };
            }

            using var log = new DefaultLoger(config.KafkaServer, "Log", $"gate_{config.ServerID}");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);
            Debuger.Log(config);

            var app = Application.S;
            await app.Start(config);
            await app.Tick();
            await app.Stop();

            Debuger.Log("Appliaction had exited!");
        }

      
    }
}
