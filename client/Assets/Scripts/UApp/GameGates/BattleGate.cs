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
using GameLogic.Game.Elements;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utility;
using XNet.Libs.Utility;
using Vector3 = UnityEngine.Vector3;
using static UApp.Utility.Stream;



namespace UApp.GameGates
{
    public class BattleGate : UGate,IBattleGate
    {
        public StateType state = StateType.None;
        public StateType State { 
            get => state;
            private set => state = value;
        }
        float IBattleGate.TimeServerNow => TimeServerNow;
        UPerceptionView IBattleGate.PreView => PreView;
        Texture IBattleGate.LookAtView => LookAtView;
        UCharacterView IBattleGate.Owner => Owner;
        void IBattleGate.Exit()
        {
            Task.Factory.StartNew(async () =>
            {
                var r = await _battleService.ExitBattleAsync(new C2B_ExitBattle
                {
                    AccountUuid = UApplication.S.AccountUuid
                });
                await Client.ShutdownAsync();
            
                UApplication.S.GoBackToMainGate();
                if (!r.Code.IsOk()) UApplication.S.ShowError(r.Code);
            });

        }
        PlayerPackage IBattleGate.Package => Package;
        DHero IBattleGate.Hero => Hero;
        private void SetServer(ServiceAddress serverInfo, int levelID)
        {
            _battleServer = serverInfo;
            Level = ExcelToJSONConfigManager.GetId<BattleLevelData>(levelID);
        }
        public BattleLevelData Level { private set; get; }
        public float TimeServerNow
        {
            get
            {
                if (_startTime < 0)  return 0f;
                return Time.time - _startTime + _serverStartTime;
            }
        }
        private float _startTime = -1f;
        private float _serverStartTime = 0;
        public PlayerPackage Package { get; private set; }
        public DHero Hero { private set; get; }
        private  NotifyPlayer _player;
        private ServiceAddress _battleServer ;
        public LogChannel Client { set; get; }
        private BattleServerService.BattleServerServiceClient _battleService;
        public UPerceptionView PreView { get; internal set; }

        #region implemented abstract members of UGate
        protected override async  Task JoinGate(params object[] args)
        {
            SetServer(args[0] as ServiceAddress, (int)args[1]);
            UUIManager.S.HideAll();
            UUIManager.S.ShowMask(true);
            await Init();
        }
        private async Task ConnectChannel()
        {
            Debuger.Log(_battleServer);
            Client = new LogChannel(_battleServer);
            _battleService = Client.CreateClient<BattleServerService.BattleServerServiceClient>();
            var query = _battleService.JoinBattleAsync(new C2B_JoinBattle
            {
                Session = UApplication.S.SesssionKey,
                AccountUuid = UApplication.S.AccountUuid,
                MapID = Level.ID,
                Version = 0
            }, cancellationToken: Client.ShutdownToken);

            var header = await query.ResponseHeadersAsync;

            Client.SessionKey = header.Get("session-key")?.Value ?? string.Empty;
            Debuger.Log($"Get:{Client.SessionKey}");
            var res = await query;
            if (!res.Code.IsOk())
            {

                UApplication.S.ShowError(res.Code);
                UApplication.S.GoBackToMainGate();
                return;
            }

            ChannelCall = _battleService.BattleChannel(cancellationToken: Client.ShutdownToken);

            HandleChannel = new ResponseChannel<Any>(ChannelCall.ResponseStream, tag: "BattleHandle")
            {
                OnReceived = (any) => { _player.Process(any); },
                OnDisconnect = () =>
                {
                    Debuger.Log($"Exit handle from battle server");
                    UApplication.S.GoBackToMainGate();
                }
            };
            PushToChannel = new RequestChannel<Any>(ChannelCall.RequestStream, tag: "BattlePushChannel");
        }
        private async Task Init()
        {
            LookAtView = new RenderTexture(128, 128, 32);
            await Addressables.LoadSceneAsync($"Assets/Levels/{Level.LevelName}.unity").Task;
            await UniTask.SwitchToMainThread();
            PreView = UPerceptionView.Create(ExcelToJSONConfigManager.GetId<ConstantValue>(1));
            _player = new NotifyPlayer(PreView)
            {
                #region OnCreateUser
                OnCreateUser = async (view) =>
                {
                    var character = view as UCharacterView;

                    if (character==null ||character.OwnerIndex > 0) return;
                    if (UApplication.S.AccountUuid != character.accoundUuid) return;
                    Owner = character;
                    //Owner.transform.SetLayer( LayerMask.NameToLayer("Player"));
                    Owner.ShowName = true;
                    PreView.OwnerTeamIndex = character.TeamId;
                    PreView.OwnerIndex = character.Index;

                    FindObjectOfType<ThirdPersonCameraContollor>()
                        .SetLookAt(Owner.GetBoneByName(UCharacterView.RootBone))
                        .SetXY(40, Owner.GetBoneByName(UCharacterView.RootBone).rotation.eulerAngles.y)
                        .SetDis(18.2f);

                    character.OnItemTrigger = TriggerItem;
                    character.LookView(LookAtView);

                    await UUIManager.Singleton.CreateWindowAsync<UUIBattle>((ui) =>
                    {
                        ui.ShowWindow(this);
                        UUIManager.S.ShowMask(false);
                    });
                    character.OnDead = () =>
                    {
                        UUIPopup.ShowConfirm("Level_Relive_Title".GetLanguageWord(),
                            "Level_Relive_Content".GetLanguageWord(),
                            () => { SendAction(new Action_Relive { }); },
                            () => { UApplication.S.GoBackToMainGate(); });
                    };
                },
                #endregion

                #region OnJoined
                OnJoined = (initPack) =>
                {
                    _startTime = Time.time;
                    _serverStartTime = initPack.TimeNow;
                    Package = initPack.Package;
                    Hero = initPack.Hero;
                    UUIManager.S.UpdateUIData();
                },
                #endregion

                #region OnAddExp
                OnAddExp = async (exp) =>
                {
                    Hero.Exprices = exp.Exp;
                    Hero.Level = exp.Level;

                    if (exp.Level != exp.OldLeve)
                    {
                        await  UUIManager.S.CreateWindowAsync<UUILevelUp>((ui) => { ui.ShowWindow(exp.Level); });
                    }

                    UUIManager.S.UpdateUIData();
                    //UUIManager.S.GetUIWindow<UUIBattle>()?.InitHero(Hero);
                },
                #endregion

                #region OnDropGold

                OnDropGold = (gold) =>
                {
                    UApplication.S.ShowNotify($"获得金币{gold.Gold}");
                },
                
                #endregion

                #region OnSyncServerTime
                OnSyncServerTime = (sTime) =>
                {
                    _serverStartTime = sTime.ServerNow;
                },
                #endregion
             
                #region OnBattleEnd
                OnBattleEnd = (end) =>
                {
                    EndTime = end.EndTime;
                    State = StateType.Ending;
                }
                #endregion
            };
            await ConnectChannel();
            State = StateType.Running;
        }
        public float EndTime { private set; get; } = -1f;
        public RenderTexture LookAtView {private set; get; }
        private void TriggerItem(UBattleItem item)
        {
            if (item.IsOwner(Owner.Index))
            {
                SendAction(new Action_CollectItem { Index = item.Index });
            }
            else
            {
                UApplication.S.ShowNotify($"{item.config.Name.GetLanguageWord()} Can't collect!");
            }
        }
        public UCharacterView Owner { private set; get; }
        public RequestChannel<Any> PushToChannel { get; private set; }
        public AsyncDuplexStreamingCall<Any, Any> ChannelCall { get; private set; }
        public ResponseChannel<Any> HandleChannel { get; private set; }
        float IBattleGate.LeftTime {
            get
            {
                switch (State)
                {
                    case StateType.Running: return Level.LimitTime - TimeServerNow; //Level.LimitTime
                    case StateType.Ending: return EndTime;
                    case StateType.None:
                    default: return 0;
                }
            }
        }
        private float _lastSyncTime = 0;
        private float _releaseLockTime = -1;
        bool IBattleGate.MoveDir(Vector3 dir)
        {
            if (!CanNetAction()) return false;
            if (_releaseLockTime > Time.time) return false;
            if (Owner.IsLock(ActionLockType.NoMove)) return false;
            var pos = Owner.transform.position;
            if (dir.magnitude > 0.01f)
            {
                var dn = new Vector3(dir.x, 0, dir.z);
                dn = dn.normalized;
                Vector3 willPos = Owner.MoveJoystick(dn);
                if (_lastSyncTime + 0.2f < Time.time)
                {
                    var joystickMove = new Action_MoveJoystick
                    {
                        Position = pos.ToPV3(),
                        WillPos = willPos.ToPV3()
                    };
                    SendAction(joystickMove);
                    _lastSyncTime = Time.time;
                }
                return true;
            }
            else
            {
                var stopMove = new Action_StopMove { StopPos = pos.ToPV3() };
                if (Owner.DoStopMove())
                {
                    SendAction(stopMove);
                }
                return true;
            }
        }
        bool IBattleGate.TrySendLookForward(bool force)
        {
            if (!force)
            {
                if (!Owner) return false;

                if (Owner is IBattleCharacter view)
                {
                    if (view.IsMoving) return false;
                }
                if (Owner.InStartLayout) return false;
            }
            var act = new Action_LookRotation { LookRotationY = ThirdPersonCameraContollor.Current.rotationY };
            SendAction(act);
            return true;
        }
        bool IBattleGate.IsMpFull() =>Owner && Owner.IsFullMp;
        bool IBattleGate.IsHpFull() => Owner && Owner.IsFullHp;
        private void ReleaseLock()
        {
            _releaseLockTime = Time.time + .3f;
            if (Owner) Owner.DoStopMove();
        }
        protected override async Task ExitGate()
        {
            await PushToChannel?.ShutDownAsync(false)!;
            await HandleChannel?.ShutDownAsync(false)!;
            ChannelCall?.Dispose();
            ChannelCall = null;
            await Client?.ShutdownAsync()!;
            Client = null;
        }
        protected override void Tick()
        {
            if (Client != null)
            {
                PreView.GetAndClearNotify();
            }
            if (EndTime>0)
            {
                EndTime -= Time.deltaTime;
            }
        }
        private void SendAction(IMessage action)
        {
            Debug.Log($"{action.GetType()}{action}");
            PushToChannel?.Push(Any.Pack(action));
        }
        private bool CanNetAction()
        {
            if (!Owner) return false;
            if (Owner.IsDeath) return false;
            return !Owner.IsLock(ActionLockType.NoAi);
        }
        bool IBattleGate.ReleaseSkill(HeroMagicData magicData)
        {
            if (!CanNetAction()) return false;
            if (!Owner.TryGetMagicData(magicData.MagicID, out var data)) return false;
            var character = Owner as IBattleCharacter;
            var config = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
            if (config != null) Owner.ShowRange(config.RangeMax);
            Debug.Log($"magic:{data}");
            if (magicData.MPCost <= Owner.MP)
            {
                ReleaseLock();
                SendAction(new Action_ClickSkillIndex
                {
                    MagicId = data.MagicID,
                    Position = character.Transform.position.ToPV3(),
                    Rotation = character.Rotation.eulerAngles.ToPV3()
                });
                return true;
            }
            UApplication.S.ShowNotify("BATTLE_NO_MP_TO_CAST".GetAsFormatKeys(config!.Name));
            return true;
        }
        bool IBattleGate.DoNormalAttack()
        {
            SendAction(new Action_NormalAttack());
            return true;
        }
        bool IBattleGate.SendUseItem(ItemType type)
        {
            if (!Owner) return false;
            if (Owner.IsDeath) return false;
            foreach (var i in Package.Items)
            {
                var config = ExcelToJSONConfigManager.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType != type) continue;
                SendAction(new Action_UseItem { ItemId = i.Value.ItemID });
                return true;
            }
            return false;
        }
        #endregion
    }
}