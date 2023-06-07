using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using Newtonsoft.Json;
using Proto.ServerConfig;
using ServerUtility;
using XNet.Libs.Utility;
using Utility;

namespace ChatServer
{
    internal static class Program
    {
        public class ChatOption: CustomOption
        {
            [Option('p',"max", Required = false)]
            public string MaxPlayer { set; get; }
            [Option('r',"zkroot", Required =true)]
            public string ChatRoot { set; get; }
            //zklogin
            [Option('a',"zklogin", Required =true)]
            public string LoginRoot { set; get; }
            
            [Option('i',"serverid", Required =true)]
            public string ServerId { set; get; }
        }

        public static async Task Main(string[] args)
        {
            var config = new ChatServerConfig();
            Parser.Default.ParseArguments<ChatOption>(args)
                .WithParsed(o =>
                {
                    Console.WriteLine("input:"+JsonConvert.SerializeObject(o));
                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, o.Config);
                        var json = File.ReadAllText(file, new UTF8Encoding(false));
                        config = json.TryParseMessage<ChatServerConfig>();
                    }

                    o.Kafka?.SplitInsert(config.KafkaServer);
                    o.ZK?.SplitInsert(config.ZkServer);
                    o.ListenHost?.SetAddress((add) => config.ListenHost = add);
                    o.ServiceHost?.SetAddress(a => config.ServicsHost = a);
                    o.DBHost?.Set(s => config.DBHost = s);
                    o.DBName?.Set(s => config.DBName = s);
                    o.MaxPlayer?.Set(s => config.MaxPlayer = int.Parse(s));
                    o.ChatRoot?.Set(s => config.ChatServerRoot = s);
                    o.LoginRoot?.Set(s=>config.LoginServerRoot = s);
                    o.ServerId?.Set(s=>config.ChatServerID = int.Parse(s));
                    
                });

            using var log = new DefaultLogger(config.KafkaServer, "Log", $"chat_{config.ChatServerID}");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);

            Debuger.Log($"Config:{config}");

            await Application.S.Create(config).Run();
            Debuger.Log("Application had exited!");

        }
    }
}
