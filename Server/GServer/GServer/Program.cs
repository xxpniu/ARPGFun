using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using org.vxwo.csharp.json;
using Proto;
using Proto.ServerConfig;
using ServerUtility;
using XNet.Libs.Utility;
using Utility;

namespace GServer
{
    
    internal static class Program
    {
        
        
        public class GateOption :CustomOption
        {
            [Option("zkroot", Required = true)]
            public string ZKRoot { set; get; }
            
            [Option("serverid", Required = true)]
            public string ServerId { set; get; }
            
            [Option("zklogin", Required = true)]
            public string ZKLogin { set; get; }
            
            [Option("zknotify", Required = true)]
            public string ZKNotify{ set; get; }
            
            [Option("zkmatch", Required = true)]
            public string ZKMatch { set; get; }
            
            [Option("zkeconfig", Required = true)]
            public string ZKEConfig { set; get; }
            
            [Option("maxplayer", Required = true)]
            public string MaxPlayer { set; get; }
            
            [Option("gm", Required = false)]
            public string GMEnable { set; get; }
        }
        

        public static async Task Main(string[] args)
        {
            var config = new GateServerConfig();
            Parser.Default.ParseArguments<GateOption>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, o.Config);
                        var json = File.ReadAllText(file, new UTF8Encoding(false));
                        config = json.TryParseMessage<GateServerConfig>();
                    }

                    o.ServiceHost?.SetAddress(a => config.ServicsHost = a);
                    o.DBHost?.Set(s => config.DBHost = s);
                    o.ListenHost?.SetAddress(a => config.ListenHost = a);
                    o.DBName?.Set(s => config.DBName = s);
                    o.ZK?.SplitInsert(config.ZkServer);
                    o.Kafka?.SplitInsert(config.KafkaServer);
                    o.ServerId?.Set(s => config.ServerID = int.Parse(s));
                    o.ZKLogin?.Set(s => config.LoginServerRoot = s);
                    o.ZKMatch?.Set(s => config.MatchServerRoot = s);
                    o.ZKEConfig?.Set(s => config.ExcelRoot = s);
                    o.ZKRoot?.Set(s => config.GateServersRoot = s);
                    o.ZKNotify?.Set(s => config.NotifyServerRoot = s);
                    o.MaxPlayer?.Set(s => config.MaxPlayer = int.Parse(s));
                    o.GMEnable?.Set(s => config.EnableGM = s=="YES");
                });

            using var log = new DefaultLogger(config.KafkaServer, "Log", $"gate_{config.ServerID}");
            Debuger.Loger = log;
            GrpcEnvironment.SetLogger(log);
            Debuger.Log(config);

            await Application.S.Create(config).Run();


            Debuger.Log("Application had exited!");
        }


    }
}
