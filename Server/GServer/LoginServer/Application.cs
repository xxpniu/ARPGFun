using XNet.Libs.Utility;
using ServerUtility;
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
using System.Threading;
using LoginServer.MongoTool;
using LoginServer.RPCResponser;

namespace LoginServer
{
    public class Application:ServerApp<Application>
    {
  
        private LoginServerConfig Config { get;  set; }
        private ZooKeeper zk;
        private LogServer Server;
        private LogServer ServiceServer;
        private WatcherServer<int, GateServerConfig> GateWatcher;
        private WatcherServer<int, ChatServerConfig> ChatWatcher;

        public Application Create(LoginServerConfig con)
        {
            Config = con;
            return this;
        }

        protected override async Task Start(CancellationToken token = default)
        {
            NetProtoTool.EnableLog = Config.Log;
            
            Debuger.Log($"Start Login server");

            //var new = new ChannelOption();
            //对外端口不能全部注册
            Server = new LogServer()
            {
                Ports = { new ServerPort("0.0.0.0", Config.ListenHost.Port, ServerCredentials.Insecure) },
            }
            .BindServices(Proto.LoginServerService.BindService(new LoginServerService()));


            Server.Start();

            Debuger.Log($"Start Login server");
            ServiceServer = new LogServer
            {
                Ports = { new ServerPort("0.0.0.0", Config.ServicsHost.Port, ServerCredentials.Insecure) }
            }.BindServices(
                Proto.LoginBattleGameServerService.BindService(new LoginBattleGameServerService()));

            ServiceServer.Start();
            await DataBase.S.Init(Config.DBHost, Config.DBName,token:token);
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

        public GateServerConfig FindGateServer(int serverId)
        {
            return GateWatcher.Find(serverId);
        }

        protected override async Task Stop(CancellationToken token = default)
        {
            await ServiceServer.ShutdownAsync();
            await Server.ShutdownAsync();
            await zk.closeAsync();
        }
        

        public ChatServerConfig FindFreeChatServer()
        {
            var free = ChatWatcher.Where(t => t.MaxPlayer > t.Player).ToArray();
            return GRandomer.RandomArray(free);
        }
    }
}

