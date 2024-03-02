using System; 
using System.Threading.Tasks;
using App.Core.Core;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using UApp.Utility;
using UnityEngine;
using XNet.Libs.Utility;

namespace UApp
{
    [Name("GateManager")]
    public class GateManager : XSingleton<GateManager>
    {

        public Action<Task_G2C_SyncHero> OnSyncHero;
        public Action<Task_G2C_SyncPackage> OnSyncPackage;
        public Action<Task_ModifyItem> OnModifyItem;
        public Action<Task_PackageSize> OnPackageSize;
        public Action<Task_CoinAndGold> OnCoinAndGold;


        public string host;
        public int port;
        private LogChannel _client;
        public GateServerService.GateServerServiceClient GateFunction { get; private set; }

        private G2C_Login _login  = null;
        public async Task<G2C_Login> TryToConnectedGateServer(ServiceAddress serverInfo)
        {
            if (_login is { HavePlayer: false })
            {
                await Release();
            }
            if (_login != null) return _login;
            
            host = serverInfo.IpAddress;
            port = serverInfo.Port;

            var serverIP = $"{serverInfo.IpAddress}:{serverInfo.Port}";
            Debuger.Log($"Gat:{serverIP}");
            _client = new LogChannel(serverIP, ChannelCredentials.Insecure);
            GateFunction = _client.CreateClient<GateServerService.GateServerServiceClient>();
            var login = GateFunction.LoginAsync(new C2G_Login
            {
                Session = UApplication.S.sessionKey,
                UserID = UApplication.S.accountUuid,
                Version = 1
            });
            var r = await login;
            var header = await login.ResponseHeadersAsync;
            _client.SessionKey = header.Get("session-key")?.Value ?? string.Empty;
            Debuger.Log(_client.SessionKey);

            if (!r.Code.IsOk())
            {
                UUITipDrawer.S.ShowNotify("GateServer Response:" + r.Code);
                return r;
            }

            _login = r;
            
            Call = _client.CreateClient<ServerStreamService.ServerStreamServiceClient>()
                .ServerAnyStream(new Proto.Void(),
                    cancellationToken: _client.ShutdownToken);
            
            HandleChannel = new Stream.ResponseChannel<Any>(Call.ResponseStream, dontUpload:true, tag: "MainGateHandle")
            {
                OnReceived = HandleOnReceived,
                OnDisconnect = HandleOnDisconnect
            };
            

            return r;
        }

        private  async void HandleOnDisconnect()
        {
            await UniTask.SwitchToMainThread();
            this.host = null;
            UApplication.Try()?.GotoLoginGate(); 
            Debuger.LogWaring($"Disconnect form gate server");
        }

        private async void HandleOnReceived(Any res)
        {
            await UniTask.SwitchToMainThread();
            Debuger.Log(res);
            if (res.TryUnpack(out Task_G2C_SyncHero syncHero))
            {
                OnSyncHero?.Invoke(syncHero);
            }
            else if (res.TryUnpack(out Task_G2C_SyncPackage p))
            {
                OnSyncPackage?.Invoke(p);
            }
            else if (res.TryUnpack(out Task_ModifyItem item))
            {
                OnModifyItem?.Invoke(item);
            }
            else if (res.TryUnpack(out Task_PackageSize size))
            {
                OnPackageSize?.Invoke(size);
            }
            else if (res.TryUnpack(out Task_CoinAndGold coin))
            {
                OnCoinAndGold?.Invoke(coin);
            }
            else
            {
                Debuger.LogError($"No handler:{res}");
            }

            UUIManager.S.UpdateUIData();
        }

        public Stream.ResponseChannel<Any> HandleChannel { get; set; }

        public AsyncServerStreamingCall<Any> Call { get; set; }

        private async Task Release()
        {
            if (HandleChannel != null)
            {
                await HandleChannel.ShutDownAsync(false);
                HandleChannel = null;
            }
            if (Call != null)
            {
                Call.Dispose();
                Call = null;
            }
            _login = null;
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();

            await Release();
        }

        public async void SendCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            var r = await GateManager.S.GateFunction.GMToolAsync(new C2G_GMTool
            {
                GMCommand = command
            });
            Debug.Log("GMResult:" + r.Code);

            await UniTask.SwitchToMainThread();
            UApplication.S.ShowNotify(r.Code.IsOk() ? $"GM send success!!" : $"GM Executed error with {r.Code}");
        }
    }
}