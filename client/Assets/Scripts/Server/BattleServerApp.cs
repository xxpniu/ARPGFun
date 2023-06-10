using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto.ServerConfig;
using UnityEngine;
using EConfig;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Utility;
using Grpc.Core;
using org.apache.zookeeper;
using static org.apache.zookeeper.ZooDefs;
using System.Text;
using XNet.Libs.Utility;
using System.Collections;
using System.Threading;
using System.Linq;
using Core;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine.SceneManagement;
using Extends = Utility.Extends;


public class BattleServerApp : XSingleton<BattleServerApp>
{
    private class ZkWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    [Header("Server ID")]
    public string ServerID = "local";

    public BattleServerConfig Config { private set; get; }

    public ConstantValue Constant { private set; get; }

    public ZooKeeper Zk { get; private set; }
    public Server.BattleInnerServices BattleInner { get; private set; }
    public LogServer ServerHost { get; private set; }
    public WatcherServer<string,LoginServerConfig> LoginServer { private set; get; }
    public WatcherServer<string, MatchServerConfig> MatchServer { get; private set; }

    /// <summary>
    /// Is running
    /// </summary>
    /// <param name="accountUuid"></param>
    /// <returns></returns>
    public bool KillUser(string accountUuid)
    {
        if (!BattleSimulater) return false;
        BattleSimulater.KickUser(accountUuid);
        return BattleSimulater.StateOfRun == RunState.Running;
    }

    internal async Task<bool> BeginSimulator(IList<string> players, int levelID)
    {
        if (BattleSimulater) return false;
        await UnRegBattleServer();
        await UniTask.SwitchToMainThread();
        await BeginSimulatorWorker(players, levelID);
        Debuger.Log($"start simulator of Level:{levelID}");
        return BattleSimulater;
    }

    private async Task BeginSimulatorWorker(IList<string> players, int levelID)
    {
        var level = CM.GetId<BattleLevelData>(levelID);
        await  ResourcesManager.S.LoadLevelAsync(level);

        var go = new GameObject($"Simulator_{levelID}", typeof(BattleSimulater));
        var si = go.GetComponent<BattleSimulater>();
        si.OnExited = () =>
        {
            BattleSimulater = null;
            Services?.CloseAllChannel();
        };

        await si.Begin(level, players);
        BattleSimulater = si;
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
        
        if (BattleSimulater)
        {
            Destroy(BattleSimulater);
            BattleSimulater = null;
        }
        SceneManager.LoadScene("null");
        await UniTask.Delay(1000);
        await RegBattleServer();
        //on end by time default
    }

    public BattleSimulater BattleSimulater { private set; get; }

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 30;
        var json = ResourcesManager.S.ReadStreamingFile("server.json");

#if UNITY_SERVER && !UNITY_EDITOR
        var CommandLine = Environment.CommandLine;
        var commandLineArgs = Environment.GetCommandLineArgs();
        if (commandLineArgs.Length >1)
        {
            var file = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, commandLineArgs[1]);
            json = System.IO.File.ReadAllText(file);
            Debuger.Log($"File:{file}");
        }
#endif
        Config = BattleServerConfig.Parser.ParseJson(json);
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

    public LogServer ListenServer { get; private set; }
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

        LoginServer = await new WatcherServer<string, LoginServerConfig>(Zk, Config.LoginServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
            .RefreshData();

        MatchServer = await new WatcherServer<string, MatchServerConfig>(Zk, Config.MatchServerRoot, c => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
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
        
        await Zk.createAsync(_serverRoot, Encoding.UTF8.GetBytes(Extends.ToJson(Config)), Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
        Debuger.Log($"reg server to zk:{_serverRoot}");
        return true;
    }

    private async void OnDestroy()
    {
        await Exit();
    }

    private volatile bool Exited = false;

    private async Task Exit()
    {
        if (Exited) return;
        Exited = true;
        Debuger.Log($"exit server");
        await Zk.closeAsync();
        await ServerHost.ShutdownAsync();
        await ListenServer.ShutdownAsync();
        await GrpcEnvironment.ShutdownChannelsAsync();
    }
}
