using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows;
using App.Core.Core;
using BattleViews.Components;
using BattleViews.Utility;
using BattleViews.Views;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GameLogic;
using GameLogic.Game.Perceptions;
using Proto;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNet.Libs.Utility;
using Vector3 = UnityEngine.Vector3;

namespace UApp.GameGates
{
    public class GMainGate : UGate
    {

        private N_Notify_MatchGroup _matchGroup;
        public N_Notify_MatchGroup Group {
            set
            {
                print($"value Set:{value}");
                _matchGroup = value;
            }
            get => _matchGroup;
        }
        public UPerceptionView view;
        public MainData data;
        
        private UCharacterView _characterView;
        private ServiceAddress _serverInfo;
    
        public UCharacterView CreateOwner(int heroID, string heroName)
        {
            if (_characterView)
            {
                if (_characterView.ConfigID == heroID) return _characterView;
                _characterView.DestroySelf(0);
            }
            _characterView = CreateHero(heroID, heroName);

            var thirdCamera = FindFirstObjectByType<ThirdPersonCameraContollor>();
            thirdCamera.SetLookAt(_characterView.GetBoneByName(UCharacterView.BottomBone), true);
            thirdCamera.SetXY(2.8f, 0).SetDis(9f).SetForwardOffset(new Vector3(0,0.87f,0));
            _characterView.ShowName = false;
            _characterView.LookView(LookAtView);
            return _characterView;
        }

        private UCharacterView CreateHero(int heroID, string heroName, int index = 0)
        {
            var character = ExcelToJSONConfigManager.GetId<CharacterData>(heroID);
            var properties = character.CreatePlayerProperties();
            var perView = view as IBattlePerception;

            var battleCharacterView = perView.CreateBattleCharacterView(string.Empty,
                    character.ID, 0,
                    data.pos[index].position.ToPVer3(), new Proto.Vector3 { Y = 180 }, 1, heroName, null, -1,
                    properties.Select(t => new HeroProperty { Property = t.Key, Value = t.Value }).ToList()
                    , 100, 100)
                as UCharacterView;

      
            return battleCharacterView;
        }

        public RenderTexture LookAtView { private set; get; }

        internal void RotationHero(float x)
        {
            _characterView.targetLookQuaternion *= Quaternion.Euler(0, x, 0);
            _timeTo = Time.time + 2;
        }

        private float _timeTo = -1f;
        private GameGMTools _gm;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override async Task JoinGate(params object[] args)
        {
            LookAtView = new RenderTexture(128, 128, 32);
            _serverInfo = args[0] as ServiceAddress;
            if (_serverInfo == null) throw new NullReferenceException($"ServerInfo is null");
            UUIManager.Singleton.HideAll();
            UUIManager.Singleton.ShowMask(true);
            await SceneManager.LoadSceneAsync("Main");
            data = FindFirstObjectByType<MainData>();
            view = UPerceptionView.Create(UApplication.S.Constant);
            var serverIP = $"{_serverInfo.IpAddress}:{_serverInfo.Port}";
            Debuger.Log($"Gat:{serverIP}");


            GateManager.S.OnSyncHero = OnSyncHero; 
            ChatManager.S.OnMatchGroup = RefreshMatchGroup;
            ChatManager.S.OnInviteJoinMatchGroup = InviteJoinGroup;
            var r = await GateManager.S.TryToConnectedGateServer(_serverInfo);
            if (!r.Code.IsOk())
            {
                UUITipDrawer.S.ShowNotify("GateServer Response:" + r.Code);
                UApplication.S.GotoLoginGate();
                return;
            }
            if (await ChatManager.S.TryConnectChatServer(UApplication.S.ChatServer, r.Hero?.Name) == false)
            {
                UApplication.S.ShowError(ErrorCode.Error);
            }
            ShowPlayer(r);
            await GateManager.S.GateFunction.ReloadMatchStateAsync(new C2G_ReloadMatchState());
            _gm = gameObject.AddComponent<GameGMTools>();
            _gm.ShowGM = true;
        }

       

        private void OnSyncHero(Task_G2C_SyncHero syncHero)
        {
            var hero = syncHero.Hero;
            CreateOwner(hero.HeroID, hero.Name); 
        }
        

        private void InviteJoinGroup(N_Notify_InviteJoinMatchGroup obj)
        {
            var level = ExcelToJSONConfigManager.GetId<BattleLevelData>(obj.LevelID);
            var levelName = level.Name.GetLanguageWord();
            var userName = obj.Inviter.Name;

            UUIPopup.ShowConfirm("Invite_Title".GetLanguageWord(),
                "Invite_Content".GetAsKeyFormat(userName, levelName), OkCallBack);
            return;

            async void OkCallBack()
            {
                var (s, g) = GateManager.TryGet();
                if (!s) return;
                var rs = await g.GateFunction.JoinMatchAsync(new C2G_JoinMatch { GroupID = obj.GroupId });
                await UniTask.SwitchToMainThread();
                if (!rs.Code.IsOk()) UApplication.S.ShowError(rs.Code);
            }
        }

        private async void ShowPlayer(G2C_Login result)
        {
            if (result.HavePlayer)
            {
            
                var hero= GateManager.S.Hero = result.Hero; 
                CreateOwner(hero.HeroID, hero.Name);

                var m = GateManager.Try();
                m.Package = result.Package;
                m.gold = result.Gold;
                m.coin = result.Coin;
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
                i.DestroySelf(0);
            }
            _views = new List<UCharacterView>();

            if (group != null)
            {
                Debuger.Log($"Match group:{group}");
                if (group.Players.Any(t => t.AccountID == UApplication.S.accountUuid))
                {
                    int index = 1;
                    foreach (var i in group.Players)
                    {
                        if (i.AccountID == UApplication.S.accountUuid) continue;
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
            //await UniTask.Delay(10);
            await UUIManager.S.CreateWindowAsync<UUIMain>((ui) =>
            {
                ui.ShowWindow();
                UUIManager.S.ShowMask(false);
            });
        }

        protected override async Task ExitGate()
        {
            GateManager.S.OnSyncHero -= OnSyncHero;  
            ChatManager.S.OnMatchGroup -= RefreshMatchGroup;
            ChatManager.S.OnInviteJoinMatchGroup -= InviteJoinGroup;
            Destroy(_gm);
            await Task.CompletedTask;
        }

        protected override void Tick()
        {
            if (!(_timeTo > 0) || !(_timeTo < Time.time)) return;
            _timeTo = -1;
            if (!_characterView) return;
            var character = ExcelToJSONConfigManager
                .First<CharacterPlayerData>(t => t.CharacterID == GateManager.S.Hero.HeroID);
            if (!string.IsNullOrEmpty(character?.Motion))
            {
                _characterView.PlayMotion(character?.Motion);
            }
            _characterView.targetLookQuaternion = Quaternion.Euler(0, 180, 0);
        }
    }
}