using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
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
        public class LoginOption:CustomOption
        {
            [Option('r',"zkroot", Required = true)]
            public string ZKRoot { set; get; }
            [Option('g',"zkgate", Required = true)]
            public string ZKGate { set; get; }
            [Option('h',"zkchat", Required = true)]
            public string ZKChat { set; get; }
        }

        public static async Task Main(string[] args)
        {

            LoginServerConfig config = new LoginServerConfig();

            Parser.Default.ParseArguments<LoginOption>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, o.Config);
                        var json = File.ReadAllText(file, new UTF8Encoding(false));
                        config = json.TryParseMessage<LoginServerConfig>();
                    }

                    o.Kafka?.SplitInsert(config.KafkaServer);
                    o.ZK?.SplitInsert(config.ZkServer);
                    o.ListenHost?.SetAddress((add) => config.ListenHost = add);
                    o.ServiceHost?.SetAddress(a => config.ServicsHost = a);
                    o.DBHost?.Set(s => config.DBHost = s);
                    o.DBName?.Set(s => config.DBName = s);
                    o.ZKRoot?.Set(s => config.LoginServerRoot = s);
                    o.ZKChat?.Set(s => config.ChatServerRoot = s);
                    o.ZKGate?.Set(s => config.GateServersRoot = s);
                });
            if (config == null) return;

            using var log = new DefaultLogger(config.KafkaServer, "Log", $"login_server");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            NetProtoTool.EnableLog = config.Log;
            Debuger.Log(config);
            await Application.S.Create(config).Run();
            await GrpcEnvironment.ShutdownChannelsAsync();
            Debuger.Log("Application had exited!");
        }
    }
}
