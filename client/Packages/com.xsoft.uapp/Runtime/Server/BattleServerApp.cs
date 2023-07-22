using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Core.Core;
using CommandLine;
using Cysharp.Threading.Tasks;
using EConfig;
using Grpc.Core;
using org.apache.zookeeper;
using Proto;
using Proto.ServerConfig;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
using XNet.Libs.Utility;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using static org.apache.zookeeper.ZooDefs;
using Extends = Utility.Extends;
using Server;


public class BattleServerApp : XSingleton<BattleServerApp>
{
    private class ZkWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    [Header("Server ID")] public string ServerID = "local";

    public BattleServerConfig Config { private set; get; }

    public ConstantValue Constant { private set; get; }

    public ZooKeeper Zk { get; private set; }
    public Server.BattleInnerServices BattleInner { get; private set; }
    public LogServer ServerHost { get; private set; }
    public WatcherServer<string, LoginServerConfig> LoginServer { private set; get; }
    public WatcherServer<string, MatchServerConfig> MatchServer { get; private set; }

    /// <summary>
    /// Is running
    /// </summary>
    /// <param name="accountUuid"></param>
    /// <returns></returns>
    public bool KillUser(string accountUuid)
    {
        if (!BattleSimulator) return false;
        BattleSimulator.KickUser(accountUuid);
        return BattleSimulator.stateOfRun == RunState.Running;
    }

    internal async Task<bool> BeginSimulator(IList<string> players, int levelID)
    {
        if (BattleSimulator)
        {
            Debuger.LogError($"Not found BattleSimulator");

            return false;
        }

        await UnRegBattleServer();
        await BeginSimulatorWorker(players, levelID);
        Debuger.Log($"start simulator of Level:{levelID}");
        return BattleSimulator;
    }

    private async Task BeginSimulatorWorker(IList<string> players, int levelID)
    {
        var level = CM.GetId<BattleLevelData>(levelID);
        await ResourcesManager.S.LoadLevelAsync(level).Task;

        var go = new GameObject($"Simulator_{levelID}", typeof(BattleSimulator));
        var si = go.GetComponent<BattleSimulator>();
        si.OnExited = () =>
        {
            BattleSimulator = null;
            Services?.CloseAllChannel();
        };

        await si.Begin(level, players);
        BattleSimulator = si;
        si.OnEnd = SiOnEnd;
    }

    private async void SiOnEnd(bool force)
    {
        Debuger.Log($"Process end!");
        Debuger.Log($"Begin Process end task!");
        try
        {
            if (!force)
            {
                const int delay = 30;
                var end = new Notify_BattleEnd() { EndTime = delay };
                //send to client
                Services?.PushToAll(end);
                await UniTask.Delay(delay * 1000);
            }

            var matchServer = MatchServer.FirstOrDefault();
            if (matchServer == null) return;

            var chn = new LogChannel(matchServer.ServicsHost);
            var query = chn.CreateClient<MatchServices.MatchServicesClient>();
            await query.FinishBattleAsync(new S2M_FinishBattle { BattleServerID = Config.ServerID });
            await chn.ShutdownAsync();
            Services?.CloseAllChannel();
        }
        catch (Exception ex)
        {
            Debuger.LogError(ex);
        }

        if (BattleSimulator)
        {
            Destroy(BattleSimulator);
            BattleSimulator = null;
        }

        SceneManager.LoadScene("null");
        await UniTask.Delay(1000);
        await RegBattleServer();
        //on end by time default
    }

    public BattleSimulator BattleSimulator { private set; get; }

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 30;
        var commandLineArgs = Environment.GetCommandLineArgs();

        foreach (var arg in commandLineArgs)
        {
            print(arg);
        }

        Config = new BattleServerConfig();

        var json = ResourcesManager.S.ReadStreamingFile("server.json");
        Config = BattleServerConfig.Parser.ParseJson(json);

#if UNITY_SERVER || UNITY_EDITOR
        Parser.Default.ParseArguments<CommandOption>(commandLineArgs)
            .WithParsed(o =>
            {
                o.Id?.Set(str => Config.ServerID = str);
                o.kafka?.SplitInsert(Config.KafkaServer);
                o.Zk?.SplitInsert(Config.ZkServer);
                o.Zk?.Set(s => Config.BattleServerRoot = s);
                o.ListenAddress?.SetAddress(s => Config.ListenHost = s);
                o.ServiceAddress?.SetAddress(s => Config.ServicsHost = s);
                o.MaxPlayer?.Set(s => Config.MaxPlayer = int.Parse(s));
                o.ZkLogin?.Set(s => Config.LoginServerRoot = s);
                o.ZkMatch?.Set(s => Config.MatchServerRoot = s);
                o.ZkExConfig?.Set(s => Config.ConfigRoot = s);
                o.MapId?.Set(s => Config.Level = int.Parse(s));
                //Config.Level = 0;
                o.ZkRoot?.Set(s => Config.BattleServerRoot = s);
            });
#endif
        ServerID = Config.ServerID;
        Debuger.Log($"{ServerID}->{Config}");
        _ = new CM(ResourcesManager.S);
        LanguageManager.S.AddLanguage(CM.Find<LanguageData>());
        Constant = CM.GetId<ConstantValue>(1);
    }

    private async void Start()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        Debuger.Log("Starting");
        await StartServerAsync(cts.Token);
        Debuger.Log($"Listen:{Config.ServicsHost}");
        Debuger.Log($"Start task finish:{cts.IsCancellationRequested}");
    }

    public async void StartTest()
    {
        Debuger.Log($"Start:1");
#if UNITY_EDITOR
        await BeginSimulator(new List<string>(), 1);
        Debuger.Log($"Start:1");
#endif
    }

    private LogServer ListenServer { get; set; }
    public Server.BattleServerService Services { private set; get; }

    private async Task StartServerAsync(CancellationToken token = default)
    {
        Debuger.Log($"StartServerAsync:{Config.ServicsHost}");
        ListenServer = new LogServer
        {
            Ports = { new ServerPort("0.0.0.0", Config.ListenHost.Port, ServerCredentials.Insecure) }
        };
        Services = new Server.BattleServerService(ListenServer);
        ListenServer.BindServices(Proto.BattleServerService.BindService(Services));
        ListenServer.Interceptor.SetAuthCheck((c) =>
        {
            if (!c.GetHeader("session-key", out string value)) return false;
            if (!ListenServer.CheckSession(value, out string userid)) return false;
            c.RequestHeaders.Add("user-key", userid);
            return true;
        });
        ListenServer.Start();

        BattleInner = new Server.BattleInnerServices();
        ServerHost = new LogServer
            {
                Ports = { new ServerPort("0.0.0.0", Config.ServicsHost.Port, ServerCredentials.Insecure) }
            }
            .BindServices(Proto.BattleInnerServices.BindService(BattleInner));
        ServerHost.Start();


        var zkHost = GRandomer.RandomList(Config.ZkServer.ToList());
        Zk = new ZooKeeper(zkHost, 3000, new ZkWatcher());

        Debuger.Log($"Begin get login server:{zkHost} by path :{Config.LoginServerRoot}");

        LoginServer = await new WatcherServer<string, LoginServerConfig>(Zk, Config.LoginServerRoot,
                c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
            .RefreshData();

        MatchServer = await new WatcherServer<string, MatchServerConfig>(Zk, Config.MatchServerRoot,
                c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
            .RefreshData();

        Debuger.Log($"Begin try create :{Config.BattleServerRoot}");
        if (await Zk.existsAsync(Config.BattleServerRoot) == null)
        {
            await Zk.createAsync(Config.BattleServerRoot, new byte[] { 0 }, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
        }

        _serverRoot = $"{Config.BattleServerRoot}/{ServerID}";
        await RegBattleServer();

    }

    private string _serverRoot;

    private async Task<bool> UnRegBattleServer()
    {
        await Zk.deleteAsync(_serverRoot);
        Debuger.Log($"un reg server to zk:{_serverRoot}");
        return true;
    }

    private async Task<bool> RegBattleServer()
    {

        await Zk.createAsync(_serverRoot, Encoding.UTF8.GetBytes(Extends.ToJson(Config)), Ids.OPEN_ACL_UNSAFE,
            CreateMode.EPHEMERAL);
        Debuger.Log($"reg server to zk:{_serverRoot}");
        return true;
    }

    protected override async void OnDestroy()
    {
        base.OnDestroy();
        await Exit();
    }

    private volatile bool _exited;

    private async Task Exit()
    {
        if (_exited) return;
        _exited = true;
        Debuger.Log($"exit server");
        await Zk.closeAsync();
        await ServerHost.ShutdownAsync();
        await ListenServer.ShutdownAsync();
        await GrpcEnvironment.ShutdownChannelsAsync();
    }
}
