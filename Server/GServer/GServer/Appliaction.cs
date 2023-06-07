using System;
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
using System.Threading;
using static org.apache.zookeeper.ZooDefs;
using Utility;

namespace GServer
{
    public class Application:ServerApp<Application>
    {
        public Application Create(GateServerConfig config)
        {
            this.Config = config;
            return this;
        }
        public static ConstantValue Constant { private set; get; }

        public GateServerConfig Config { get; private set; }
        //玩家访问端口
        public LogServer ListenServer {private set; get; }
        //游戏战斗服务器访问端口
        private LogServer ServiceServer;
        //zk
        private ZooKeeper Zk;
        private volatile bool _isRunning;
        private ResourcesLoader _loader;
        public StreamServices StreamService { private set; get; }

        public WatcherServer<string,LoginServerConfig> LoginServers { get; private set; }
        public WatcherServer<string,NotifyServerConfig> NotifyServers { get; private set; }
        public WatcherServer<string,MatchServerConfig> MatchServers { get; private set; }
        

        protected override async Task Start(CancellationToken token = default)
        {
           if (_isRunning) return;
           _isRunning = true;
            _loader = new ResourcesLoader();
            await _loader.LoadAllConfig(this.Config.ZkServer, Config.ExcelRoot);
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
                if (!ListenServer.CheckSession(value, out string userid)) return false;
                c.RequestHeaders.Add("user-key", userid);
                return true;
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

        protected override async Task Stop(CancellationToken token = default)
        {
            if (!_isRunning)  return;
            await _loader?.Close()!;
            await Zk.closeAsync();
            _isRunning = false;
            
            await ListenServer.ShutdownAsync();
            await ServiceServer.ShutdownAsync();
            Debuger.Log("Server had stop");
        }
    }
    
    public class App:ServerApp<App>
    {

        private Func<App,Task> _s, _t, _e;
        public App Create(Func<App,Task> setup = default, Func<App,Task> tick =default, Func<App,Task> stop=default)
        {
            _s = setup;
            _t = tick;
            _e = stop;
            return this;
        }

        protected override async Task Start(CancellationToken token = default)
        {
            if(_s==null) return;
            await _s.Invoke(this);
        }
        
        protected override async Task Stop(CancellationToken token = default)
        {
            if(_e ==null) return;
            await _e?.Invoke(this)!;
        }
    }
}

