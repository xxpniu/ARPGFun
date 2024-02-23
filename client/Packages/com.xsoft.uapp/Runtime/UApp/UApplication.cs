using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Core;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Grpc.Core;
using Proto;
using UApp.GameGates;
using UnityEngine;
using UnityEngine.Serialization;
using XNet.Libs.Utility;

namespace UApp
{
    /// <summary>
    /// 处理 App
    /// </summary>
    public class UApplication : XSingleton<UApplication>
    {
        public bool localGame = false;
        public int localDataIndex = 2;
        public ServiceAddress LoginServer;
        public ServiceAddress GateServer;
        public ServiceAddress ChatServer;
        public ServiceAddress BattleServer;

        public int ReceiveTotal;
        public int SendTotal;
        public float ConnectTime;

        public int index = 0;

        public string accountUuid;
        public string sessionKey; //login token
        public string heroName;

        public float pingDelay = 0f;

        #region Gate

        public void GoBackToMainGate() => GoToMainGate(GateServer);

        public async void GoToMainGate(ServiceAddress info)
        {
            await ChangeGate<GMainGate>(info);
            GateServer = info;
        }

        public void GoServerMainGate(GameServerInfo chatServer, GameServerInfo server, string userID, string session)
        {
            ChatServer = new ServiceAddress { IpAddress = chatServer.Host, Port = chatServer.Port };
            GateServer = new ServiceAddress { IpAddress = server.Host, Port = server.Port };
            accountUuid = userID;
            sessionKey = session;
            GoToMainGate(GateServer);

        }

        public async void StartLocalLevel(DHero hero, PlayerPackage package, int levelID)
        {
            await ChangeGate<LevelSimulatorGate>(hero, package, levelID);
        }

        public async void GotoLoginGate()
        {
            ChatManager.Reset();
            UUIManager.S.ShowMask(false);
            await ChangeGate<LoginGate>();
        }

        public async void GotoBattleGate(GameServerInfo serverInfo, int mapID)
        {
            BattleServer = new ServiceAddress { IpAddress = serverInfo.Host, Port = serverInfo.Port };
            await ChangeGate<BattleGate>(BattleServer, mapID);
        }

        private async Task<T> ChangeGate<T>(params object[] args) where T : UGate, new()
        {
            var old = _gate;
            var oType = _gate == null ? null : _gate.GetType();
            Debug.Log($"from {oType} to {typeof(T)}");
            _gate = gameObject.AddComponent<T>();
            await UGate.DoJoinGate(_gate, args);
            if (old == null) return (T)_gate;
            await UGate.DoExitGate(old);
            Destroy(old);
            return (T)_gate;
        }

        private UGate _gate;

        #endregion

        #region mono behavior

        protected override void Awake()
        {
            base.Awake();
            _ = new ExcelToJSONConfigManager(ResourcesManager.S);
            var config = ResourcesManager.S.ReadStreamingFile("client.json");
            var clientConfig = ClientConfig.Parser.ParseJson(config);
            LoginServer = new ServiceAddress
                { IpAddress = clientConfig.LoginServerHost, Port = clientConfig.LoginServerPort };
            Debug.Log($"Login:{LoginServer}");
            RunReader();
            Constant = ExcelToJSONConfigManager.GetId<ConstantValue>(1);
            var la = ExcelToJSONConfigManager.Find<LanguageData>();
            LanguageManager.S.AddLanguage(la);
            GrpcEnvironment.SetLogger(new GrpcLoger());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopChannel();
        }

        protected void Update()
        {
            if (_gate == null) return;
            UGate.DoTick(_gate);
        }

        public ConstantValue Constant { get; private set; }

        private async void Start()
        {
            if (!localGame) GotoLoginGate();
            else
            {
                // Init(args[0] as DHero, args[1] as PlayerPackage, (int)args[2]);
                var config = ExcelToJSONConfigManager.GetId<CharacterData>(localDataIndex);
                var itemCfg = ExcelToJSONConfigManager.GetId<ItemData>(config.InitEquip);
                var item = new PlayerItem()
                {
                    ItemID = itemCfg.ID,
                    Level = 10,
                    Locked = true,
                    GUID = Guid.NewGuid().ToString(),
                    Data = new EquipData
                    {
                        RefreshTime = 10
                    },
                    Num = 1
                };

                var hero = new DHero()
                {
                    Level = 40,
                    HeroID = config.ID,
                    Equips =
                    {
                        new WearEquip
                        {
                            ItemID = item.ItemID,
                            GUID = item.GUID,
                            Part = EquipmentType.Arm
                        }
                    },
                    HP = 100000,
                };

                var playerPackage = new PlayerPackage()
                {
                    Items = { { item.GUID, item } },
                    MaxSize = 200
                };




                await ChangeGate<LevelSimulatorGate>(hero, playerPackage, 1);
            }
        }


        #endregion

        #region Reader

        private async void RunReader()
        {
            try
            {
                var token = this.GetCancellationTokenOnDestroy();
                while (!token.IsCancellationRequested)
                {
                    await UniTask.Yield(token);
                    if (NotifyMessages.Count <= 0) continue;
                    var message = NotifyMessages.Dequeue();
                    UUITipDrawer.Singleton.ShowNotify(message);
                }
            }
            catch
            {
                //ignore
            }
        }

        public void ShowError(ErrorCode code)
        {
            ShowNotify("ErrorCode:" + code);
        }

        public void ShowNotify(string msg)
        {
            NotifyMessages.Enqueue(new AppNotify { Message = msg, endTime = Time.time + 3.2f });
        }

        private Queue<AppNotify> NotifyMessages { get; } = new();

        #endregion

        private async void StopChannel()
        {
            if (_gate != null)
            {
                var temp = _gate;
                _gate = null;
                //await UGate.DoExitGate(temp);
            }

            await GrpcEnvironment.ShutdownChannelsAsync();
            Debuger.Log("Application Quit: stop all channel!!");

        }

        public static T G<T>() where T : UGate
        {
            return S._gate as T;
        }

    }
}


