using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Utility;
using BattleViews.Views;
using Cysharp.Threading.Tasks;
using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Google.Protobuf;
using Proto;
using UGameTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Vector3 = UnityEngine.Vector3;

namespace UApp.GameGates
{
    public class LevelSimulatorGate : UGate, IStateLoader,IBattleGate
    {
        private int LevelId;
        private DHero Hero; 
        public RenderTexture LookAtView { get; private set; }
        public BattleLevelData LevelData { get; private set; }
        public UPerceptionView PerView { get; private set; }

        private ITimeSimulator _timeSimulator;

        public BattleState State { private set; get; }

        public BattlePerception Per => State.Perception as BattlePerception;

        private bool _isFinished = false;

        protected override async Task JoinGate(params object[] args)
        {
            await base.JoinGate();
        
            UUIManager.S.ShowMask(true);
            UUIManager.S.HideAll();
            Hero = args[0] as DHero;
            LevelId = (int)args[2];
            await InitLevel(LevelId);
        }

        protected override async Task ExitGate()
        {
            State?.Stop(_timeSimulator.Now);
            await Task.CompletedTask;
        }

        private async Task InitLevel(int levelId)
        {
            LookAtView = new RenderTexture(128, 128, 32);
            LevelData = CM.GetId<BattleLevelData>(levelId);
            await Addressables.LoadSceneAsync($"Assets/Levels/{LevelData.LevelName}.unity").Task;
         

            PerView = UPerceptionView.Create(UApplication.S.Constant);
            PerView.OnCreateCharacter = (c) => c.TryAdd<HpTipShower>();
            _timeSimulator = PerView;
            await UniTask.Yield();
            State = new BattleState(PerView, this, PerView);
            State.Start(this.GetTime());

        
            await ResourcesManager.S.LoadResourcesWithExName<TextAsset>(LevelData.ElementConfigPath, (rs) =>
            {
                Config = rs.text.Parser<MapConfig>();
            });
            _mCreator = new Server.Map.MapElementSpawn(this.Per, Config);

            var data = CM.GetId<CharacterData>(Hero.HeroID);
            var magic = Hero.CreateHeroMagic();
            var properties = BattleUtility.CreateHeroProperties(Hero, ((IBattleGate)this).Package);
    
            var playerBornPositions = Config.Elements.Where(t => t.Type == MapElementType.MetPlayerInit)
                .Select(t => t).ToArray();


            var pos = GRandomer.RandomArray(playerBornPositions);//.position;        
            _characterOwner = Per.CreateCharacter(Per.StateControllor,
                Hero.Level,
                data,
                magic, properties,
                0,
                pos.Position.ToUV3(),
                Quaternion.LookRotation(pos.Forward.ToUV3()).eulerAngles,
                string.Empty,
                Hero.Name);
            await UniTask.Delay(1000);
            //var root = Per.ChangeCharacterAI(data.AIResourcePath, _characterOwner);
            //Debug.Log($"AI:{root.NodeRoot.name}"); 
            Owner = _characterOwner.CharacterView as UCharacterView;
            PerView.OwnerTeamIndex = Owner!.TeamId;
            PerView.OwnerIndex = Owner.Index;
            FindFirstObjectByType<ThirdPersonCameraContollor>()
                .SetLookAt(Owner.GetBoneByName(UCharacterView.RootBone))
                //.SetForwardOffset(Vector3.forward * 2f)
                .SetXY(40, Owner.GetBoneByName(UCharacterView.RootBone).rotation.eulerAngles.y)
                .SetDis(18.2f);
            Owner.LookView(LookAtView);
            await UUIManager.S.CreateWindowAsync<Windows.UUIBattle>(
                ui => ui.ShowWindow(this), wRender: WRenderType.WithCanvas
                    );
            UUIManager.S.ShowMask(false);

            _characterOwner.OnDead = (obj) =>
            {
                Windows.UUIPopup.ShowConfirm(LanguageManager.S["Level_Relive_Title"], 
                    LanguageManager.S["Level_Relive_Content"],   () => { UApplication.S.GoBackToMainGate(); });
                //UUIManager.S.CreateWindowAsync<Windows.>
            };   
            
            await _mCreator.Spawn();
            _startTime = ((IBattleGate)this).TimeServerNow;
            _isFinished = false;
        }

        private float _startTime = 0;

        private Server.Map.MapElementSpawn _mCreator;

    
        private BattleCharacter _characterOwner;
        private float _lastSyncTime;

        public UCharacterView Owner { private set; get; }

        private GTime GetTime()
        {
            return _timeSimulator.Now;
        }

        void IStateLoader.Load(GState state)
        {
            
        }
        

        private async void ShowBattleResult(bool win = false)
        {
            if (!win)
            {
                //go back
                //todo:failure result no reward
                UApplication.S.GoBackToMainGate();
                return;
            }

            var request = new C2G_LocalBattleFinished();
            var reward = await GateManager.S.GateFunction.LocalBattleFinishedAsync(request,
                cancellationToken: this.destroyCancellationToken);
            await UniTask.SwitchToMainThread();
            if(!reward.Code.IsOk()) UApplication.S.ShowError(reward.Code);
            //todo:: show reward result
            UApplication.S.GoBackToMainGate();
        }

        protected override void Tick()
        {
            if (State == null) return;
            if(_isFinished) return;
            GState.Tick(State, _timeSimulator.Now);
            PerView.GetAndClearNotify();
            
            
            
            if (((IBattleGate)this).LeftTime <= 0)
            {
                //time Over
                _isFinished = true;
                ShowBattleResult(false);
            }

            if (_mCreator?.IsAllMonsterDeath() == true)
            {
                _isFinished = true;
                ShowBattleResult(true);
            }
        }

        float IBattleGate.TimeServerNow => _timeSimulator.Now.Time;

        UPerceptionView IBattleGate.PreView => PerView;

        Texture IBattleGate.LookAtView => LookAtView;

        UCharacterView IBattleGate.Owner => Owner;

        PlayerPackage IBattleGate.Package =>  GateManager.S.Package;

        DHero IBattleGate.Hero => Hero;

        public MapConfig Config { get; private set; }

        StateType IBattleGate.State => StateType.Running;

        float IBattleGate.LeftTime => LevelData.LimitTime - (((IBattleGate)this).TimeServerNow - _startTime);

        bool IBattleGate.ReleaseSkill(HeroMagicData data, Vector3? forward)
        {
            if (data.MPCost > Owner.MP) return false;
            var character = Owner as IBattleCharacter;
            var rotation = character.Rotation.eulerAngles.ToPV3();
            if (forward.HasValue)
            {
                rotation = Quaternion.LookRotation(forward.Value).eulerAngles.ToPV3();
                Debug.Log($"Euler:{Quaternion.LookRotation(forward.Value).eulerAngles}");
            }
            SendAction(new Action_ClickSkillIndex
            {
                MagicId = data.MagicID,
                Position = character.Transform.position.ToPV3(),
                Rotation = rotation 
            });

            return true;
        }

        void IBattleGate.Exit()
        {
            UApplication.S.GoBackToMainGate();
        }

        bool IBattleGate.MoveDir(UnityEngine.Vector3 dir)
        {
            if (Owner.IsLock(ActionLockType.NoMove)) return false;
            if (Owner.IsDeath) return false;

            var pos = Owner.transform.position;
            if (dir.magnitude > 0.01f)
            {
                var dn = new Vector3(dir.x, 0, dir.z);
                dn = dn.normalized;
                var willPos = Owner.MoveJoystick(dn);
                if (!(_lastSyncTime + 0.2f < Time.time)) return true;
                var joystickMove = new Action_MoveJoystick
                {
                    Position = pos.ToPV3(),
                    WillPos = willPos.ToPV3()
                };
                SendAction(joystickMove);
                _lastSyncTime = Time.time;
            }
            else
            {
                var stopMove = new Action_StopMove { StopPos = pos.ToPV3() };
                if (Owner.DoStopMove())
                {
                    SendAction(stopMove);
                    //if (this is IBattleGate b)
                    //    b.TrySendLookForward(true);
                }
            }

            return true;
        }

        private void SendAction(IMessage action)
        {
            if (Owner.IsDeath) return;
            _characterOwner?.AddNetAction(action);
        }

        bool IBattleGate.TrySendLookForward(bool force)
        {
            return false;
        }

        bool IBattleGate.DoNormalAttack()
        {
            SendAction(new Action_NormalAttack());
            return true;
        }

        bool IBattleGate.SendUseItem(ItemType type)
        {

            var package = GateManager.S.Package;
            foreach (var i in package.Items)
            {
                var config = CM.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType != type) continue;
                if (i.Value.Num < 1) return false;
                UseItem(i.Value, config);
                return true;
            }

            return false;


            async void UseItem(PlayerItem item,ItemData config)
            {
                var res = await GateManager.S.GateFunction.UseItemAsync(new C2G_UseItem
                {
                    ItemId = config.ID,
                    Num = 1
                });
                if (!res.Code.IsOk())
                {
                    UApplication.S.ShowError(res.Code);
                }

                await UniTask.SwitchToMainThread();
                var rTarget = new ReleaseAtTarget(_characterOwner, _characterOwner);
                Per.CreateReleaser(config.Params1, _characterOwner, rTarget, ReleaserType.Magic, ReleaserModeType.RmtNone,
                    -1);
            }

        }

        bool IBattleGate.IsHpFull()
        {
            return false;
        }

        bool IBattleGate.IsMpFull()
        {
            return false;
        }


    }
}
