using XNet.Libs.Utility;
using ServerUtility;
using MongoTool;
using RPCResponsers;
using Grpc.Core;
using System.Threading.Tasks;
using Proto.ServerConfig;
using org.apache.zookeeper;
using static org.apache.zookeeper.ZooDefs;
using System.Text;
using System.Collections.Concurrent;
using System;
using Utility;
using System.Linq;

namespace LoginServer
{
    public class Appliaction:XSingleton<Appliaction>
    {
  
        public  LoginServerConfig Config { get; private set; }
        private ZooKeeper zk;
        private LogServer Server;
        private LogServer ServiceServer;
        private WatcherServer<int, GateServerConfig> GateWatcher;
        private WatcherServer<int, ChatServerConfig> ChatWatcher;

        public volatile bool IsRunning = false;

        public async Task Start(LoginServerConfig con)
        {
            if (IsRunning) return;

            this.Config = con;
            NetProtoTool.EnableLog = Config.Log;

            IsRunning = true;

            
            Debuger.Log($"Start Login server");

            //var new = new ChannelOption();
            //对外端口不能全部注册
            Server = new LogServer()
            {
                Ports = { new ServerPort("0.0.0.0", Config.ListenHost.Port, ServerCredentials.Insecure) },
            }
            .BindServices(Proto.LoginServerService.BindService(new LoginServerService()));


            Server.Start();

            Debuger.Log($"Start Logininner server");
            ServiceServer = new LogServer
            {
                Ports = { new ServerPort("0.0.0.0", Config.ServicsHost.Port, ServerCredentials.Insecure) }
            }.BindServices(Proto.LoginBattleGameServerService.BindService(new LoginBattleGameServerService()));

            ServiceServer.Start();
            await DataBase.S.Init(Config.DBHost, Config.DBName);
            Debuger.Log($"Init db server");
            zk = new ZooKeeper(Config.ZkServer[0], 3000, new DefaultWatcher());
            if ((await zk.existsAsync(Config.LoginServerRoot)) == null )
            {
                Debuger.Log(zk.ToString());
                Debuger.Log($"create root:{Config.LoginServerRoot}");
                var res = await zk.createAsync(Config.LoginServerRoot,
                    new byte[] { 0},Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                Debuger.Log($"create:{res}");
            }

            var serverName = $"{Config.LoginServerRoot}/{Config.ListenHost.IpAddress}:{Config.ListenHost.Port}";
            Debuger.Log($"begin add listener: {serverName}");
            if ((await zk.existsAsync(serverName)) == null)
            {
                var res= await zk.createAsync(serverName,
                   Encoding.UTF8.GetBytes(Config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                Debuger.Log($"add zookeeper {res}");
            }
            else
            {
                this.IsRunning = false;
                return;
            }

            this.GateWatcher =  await new WatcherServer<int, GateServerConfig>(zk,
                Config.GateServersRoot,
                (s)=>s.ServerID).RefreshData();
            this.ChatWatcher = await new WatcherServer<int, ChatServerConfig>(zk,
                Config.ChatServerRoot, (s) => s.ChatServerID).RefreshData();
        }

        public GateServerConfig FindFreeGateServer()
        {
            var free = GateWatcher.Where(t => t.MaxPlayer > t.Player).ToList();
            return GRandomer.RandomList(free);
        }

        public GateServerConfig FindGateServer(int serverid)
        {
            return GateWatcher.Find(serverid);
        }

        public async Task Stop()
        {
            if (!IsRunning) return;
           
            IsRunning = false;
            await ServiceServer.ShutdownAsync();
            await Server.ShutdownAsync();
            await zk.closeAsync();
        }

        public async Task Tick()
        {
            while (IsRunning)
            {
                await Task.Delay(100);
            }
        }

        public ChatServerConfig FindFreeChatServer()
        {
            var free = ChatWatcher.Where(t => t.MaxPlayer > t.Player).ToArray();
            return GRandomer.RandomArray(free);
        }
    }
}

