using XNet.Libs.Utility;
using ServerUtility;
using GateServer;
using ExcelConfig;
using EConfig;
using System.Threading.Tasks;
using Proto.ServerConfig;
using Grpc.Core;
using org.apache.zookeeper;
using System.Text;
using static org.apache.zookeeper.ZooDefs;
using Utility;

namespace GServer
{
    public class Application:XSingleton<Application>
    {

        public static ConstantValue Constant { private set; get; }

        public GateServerConfig Config { get; private set; }
        //玩家访问端口
        public LogServer ListenServer {private set; get; }
        //游戏战斗服务器访问端口
        private LogServer ServiceServer;
        //zk
        private ZooKeeper Zk;
        public volatile bool IsRunning;
        private ResourcesLoader loader;
        public StreamServices StreamService { private set; get; }

        public WatcherServer<string,LoginServerConfig> LoginServers { get; private set; }
        public WatcherServer<string,NotifyServerConfig> NotifyServers { get; private set; }
        public WatcherServer<string,MatchServerConfig> MatchServers { get; private set; }

        public async Task Start(GateServerConfig config)
        {
            if (IsRunning) return;

            this.Config = config;
            IsRunning = true;
            loader = new ResourcesLoader();
            await loader.LoadAllConfig(this.Config.ZkServer, Config.ExcelRoot);
            Constant =  ExcelToJSONConfigManager.GetId<ConstantValue>(1);
            StreamService = new StreamServices();
            Debuger.Log($"Start Listen: {Config.ListenHost}");
            ListenServer = new LogServer
            {
                Ports = { new ServerPort("0.0.0.0", Config.ListenHost.Port, ServerCredentials.Insecure) }
             
            }.BindServices(Proto.GateServerService.BindService(new GateServerService()),
                    Proto.ServerStreamService.BindService(StreamService));

            ListenServer.Interceptor.SetAuthCheck((c) =>
            {
                if (!c.GetHeader("session-key", out string value)) return false;
                if (ListenServer.CheckSession(value, out string userid))
                {
                    c.RequestHeaders.Add("user-key", userid);
                    return true;
                }
                return false;
            });

            ListenServer.Start();

            Debuger.Log($"Start Services: {Config.ServicsHost}");

     
            ServiceServer = new LogServer
            {
                Ports = {  new ServerPort("0.0.0.0", Config.ServicsHost.Port, ServerCredentials.Insecure) },
               
            }.BindServices(Proto.GateServerInnerService.BindService(new GateBattleServerService()));

            ServiceServer.Start();

            Zk = new ZooKeeper(Config.ZkServer[0], 3000, new DefaultWatcher());

            if ((await Zk.existsAsync(Config.GateServersRoot)) == null)
            {
                var res = await Zk.createAsync(Config.GateServersRoot, new byte[] { 0 },
                    Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                Debuger.Log($"Create:{ res}");
            }
            var serverKey = $"{Config.GateServersRoot}/{Config.ServerID}";
            {
                var res = await Zk.createAsync(serverKey,
                   Encoding.UTF8.GetBytes(Config.ToJson()), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                Debuger.Log($"Add server:{res}");
            }
            DataBase.S.Init(this.Config.DBHost, this.Config.DBName);

            MatchServers = await new WatcherServer<string, MatchServerConfig>(Zk,
                config.MatchServerRoot, (c) => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}").RefreshData();
            LoginServers = await new WatcherServer<string, LoginServerConfig>(Zk, config.LoginServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
                .RefreshData();
            NotifyServers = await new WatcherServer<string, NotifyServerConfig>(Zk, config.NotifyServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
                .RefreshData();
        }

        public async Task Stop()
        {
            if (!IsRunning)  return;
            await loader?.Close();
            await Zk.closeAsync();
            IsRunning = false;
            
            await ListenServer.ShutdownAsync();
            await ServiceServer.ShutdownAsync();
            Debuger.Log("Server had stop");
        }

        public async Task Tick()
        {
            while (IsRunning)
            {
                await Task.Delay(100);
            }
        }

    }
}

