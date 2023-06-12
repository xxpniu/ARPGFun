using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows;
using BattleViews.Components;
using BattleViews.Utility;
using BattleViews.Views;
using Core;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GameLogic;
using GameLogic.Game.Perceptions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
using XNet.Libs.Utility;
using static UApp.Utility.Stream;
using Vector3 = UnityEngine.Vector3;

namespace UApp.GameGates
{
    public class GMainGate : UGate
    {

        public N_Notify_MatchGroup Group;
        public UPerceptionView view;
        public MainData Data;
        public int Gold;
        public int Coin;
        public PlayerPackage package;
        public DHero hero;
        private UCharacterView characterView;
        private ServiceAddress ServerInfo;
        public LogChannel Client { private set; get; }


        public UCharacterView CreateOwner(int heroID, string heroName)
        {
            if (characterView)
            {
                if (characterView.ConfigID == heroID) return characterView;
                characterView.DestorySelf(0);
            }
            characterView = CreateHero(heroID, heroName);

            var thirdCamera = FindObjectOfType<ThirdPersonCameraContollor>();
            thirdCamera.SetLookAt(characterView.GetBoneByName(UCharacterView.BottomBone), true);
            thirdCamera.SetXY(2.8f, 0).SetDis(9f).SetForwardOffset(new Vector3(0,0.87f,0));
            characterView.ShowName = false;
            characterView.LookView(LookAtView);
            return characterView;
        }

        private UCharacterView CreateHero(int heroID, string heroName, int index = 0)
        {
            var character = ExcelToJSONConfigManager.GetId<CharacterData>(heroID);
            var properties = character.CreatePlayerProperties();
            var perView = view as IBattlePerception;

            var battleCharacterView = perView.CreateBattleCharacterView(string.Empty,
                    character.ID, 0,
                    Data.pos[index].position.ToPVer3(), new Proto.Vector3 { Y = 180 }, 1, heroName, null, -1,
                    properties.Select(t => new HeroProperty { Property = t.Key, Value = t.Value }).ToList()
                    , 100, 100)
                as UCharacterView;

      
            return battleCharacterView;
        }

        public RenderTexture LookAtView { private set; get; }

        internal void RotationHero(float x)
        {
            characterView.targetLookQuaternion *= Quaternion.Euler(0, x, 0);
            _timeTo = Time.time + 2;
        }

        private float _timeTo = -1f;
        private GameGMTools _gm;

        protected override async Task JoinGate(params object[] args)
        {
            LookAtView = new RenderTexture(128, 128, 32);
            ServerInfo = args[0] as ServiceAddress;
            if (ServerInfo == null) throw new NullReferenceException($"ServerInfo is null");
            UUIManager.Singleton.HideAll();
            UUIManager.Singleton.ShowMask(true);
            await SceneManager.LoadSceneAsync("Main");
            Data = FindObjectOfType<MainData>();
            view = UPerceptionView.Create(UApplication.S.Constant);
            var serverIP = $"{ServerInfo.IpAddress}:{ServerInfo.Port}";
            Debuger.Log($"Gat:{serverIP}");
            Client = new LogChannel(serverIP, ChannelCredentials.Insecure);
            GateFunction = Client.CreateClient<GateServerService.GateServerServiceClient>();
            await RequestPlayer();
            _gm = this.gameObject.AddComponent<GameGMTools>();
            _gm.ShowGM = true;
        }

        public Proto.GateServerService.GateServerServiceClient GateFunction { private set; get; }
        private AsyncServerStreamingCall<Any> Call { get; set; }
        private ResponseChannel<Any> HandleChannel { get; set; }

  
        private async Task RequestPlayer()
        {
            var login = GateFunction.LoginAsync(new C2G_Login
            {
                Session = UApplication.S.SesssionKey,
                UserID = UApplication.S.AccountUuid,
                Version = 1
            });
            var r = await login;
            var header = await login.ResponseHeadersAsync;
            Client.SessionKey = header.Get("session-key")?.Value??string.Empty;
            Debuger.Log(Client.SessionKey);

            if (r.Code.IsOk())
            {
                ChatManager.S.OnMatchGroup = RefreshMatchGroup;
                ChatManager.S.OnInviteJoinMatchGroup = InviteJoinGroup;
                if (await ChatManager.S.TryConnectChatServer(UApplication.S.ChatServer, r.Hero?.Name) == false)
                {
                    UApplication.S.ShowError(ErrorCode.Error);
                }

                Call = Client.CreateClient<ServerStreamService.ServerStreamServiceClient>()
                    .ServerAnyStream(new Proto.Void(), cancellationToken: Client.ShutdownToken);

                HandleChannel = new ResponseChannel<Any>(Call.ResponseStream, tag: "MainGateHandle")
                {
                    OnReceived = (res) =>
                    {
                        Debuger.Log(res);
                        if (res.TryUnpack(out Task_G2C_SyncHero syncHero))
                        {
                            hero = syncHero.Hero;
                            CreateOwner(hero.HeroID, hero.Name);
                        }
                        else if (res.TryUnpack(out Task_G2C_SyncPackage p))
                        {
                            this.package = p.Package;
                            this.Gold = p.Gold;
                            this.Coin = p.Coin;
                        }
                        else if (res.TryUnpack(out Task_ModifyItem item))
                        {
                            foreach (var i in item.ModifyItems)
                            {
                                package.Items.Remove(i.GUID);
                                package.Items.Add(i.GUID, i);
                            }

                            foreach (var i in item.RemoveItems) package.Items.Remove(i.GUID);
                        }
                        else if (res.TryUnpack(out Task_PackageSize size))
                        {
                            package.MaxSize = size.Size;
                        }
                        else if (res.TryUnpack(out Task_CoinAndGold coin))
                        {
                            this.Coin = coin.Coin;
                            this.Gold = coin.Gold;
                        }
                        else
                        {
                            Debuger.LogError($"No handler:{res}");
                        }

                        UUIManager.S.UpdateUIData();
                    },

                    OnDisconnect = () =>
                    {
                        UApplication.S.GotoLoginGate();
                        Debuger.LogError($"Disconnect form gate server");

                    }
                };
                ShowPlayer(r);
            }
            else
            {
                UUITipDrawer.S.ShowNotify("GateServer Response:" + r.Code);
                UApplication.S.GotoLoginGate();
            }
        }

        private void InviteJoinGroup(N_Notify_InviteJoinMatchGroup obj)
        {
            var level = ExcelToJSONConfigManager.GetId<BattleLevelData>(obj.LevelID);
            var levelName = level.Name.GetLanguageWord();
            var userName = obj.Inviter.Name;

            UUIPopup.ShowConfirm("Invite_Title".GetLanguageWord(),
                "Invite_Content".GetAsKeyFormat(userName, levelName), async () =>
                {
                    var gate = UApplication.G<GMainGate>();
                    if (!gate) return;

                    var rs = await gate.GateFunction.JoinMatchAsync(new C2G_JoinMatch { GroupID = obj.GroupId });
                    if (!rs.Code.IsOk())
                    {
                        UApplication.S.ShowError(rs.Code);
                    }

                });
        }

        private async void ShowPlayer(G2C_Login result)
        {
            if (result.HavePlayer)
            {
            
                hero = result.Hero;
                CreateOwner(hero.HeroID, hero.Name);

                this.package = result.Package;
                this.Gold = result.Gold;
                this.Coin = result.Coin;

                ShowMain();
            }
            else
            {
                await  UUIManager.S.CreateWindowAsync<UUIHeroCreate>((ui) =>
                {
                    ui.ShowWindow();
                    UUIManager.Singleton.ShowMask(false);
                });
            }


        }

        private List<UCharacterView> _views = new();

        private void RefreshMatchGroup(N_Notify_MatchGroup group)
        {
            this.Group = group;
            foreach (var i in _views)
            {
                i.DestorySelf(0);
            }
            _views = new List<UCharacterView>();

            if (group != null)
            {
                Debuger.Log($"Match group:{group}");
                if (group.Players.Any(t => t.AccountID == UApplication.S.AccountUuid))
                {
                    int index = 1;
                    foreach (var i in group.Players)
                    {
                        if (i.AccountID == UApplication.S.AccountUuid) continue;
                        _views.Add(CreateHero(i.Hero.HeroID, i.Name, index));
                    }
                }
                else {
                    Group = null;
                }
            }

            UUIManager.S.UpdateUIData();
        }

        public async void ShowMain()
        {
            await UUIManager.S.CreateWindowAsync<UUIMain>((ui) =>
            {
                ui.ShowWindow();
                UUIManager.S.ShowMask(false);
            });
       
        }

        protected override async Task ExitGate()
        {
            var (h, s) = ChatManager.TryGet();
            if (h) s.OnMatchGroup = null;
            if (HandleChannel != null)
                await HandleChannel?.ShutDownAsync(false)!;
            Call?.Dispose();
            Call = null;
            if (Client != null)
                await Client?.ShutdownAsync()!;
            Client = null;
            Destroy(_gm);
        }

        protected override void Tick()
        {
            if (!(_timeTo > 0) || !(_timeTo < Time.time)) return;
            _timeTo = -1;
            if (!characterView) return;
            var character = ExcelToJSONConfigManager.First<CharacterPlayerData>(t => t.CharacterID == hero.HeroID);
            if (!string.IsNullOrEmpty(character?.Motion))
            {
                characterView.PlayMotion(character?.Motion);
            }
            characterView.targetLookQuaternion = Quaternion.Euler(0, 180, 0);
        }
    }
}