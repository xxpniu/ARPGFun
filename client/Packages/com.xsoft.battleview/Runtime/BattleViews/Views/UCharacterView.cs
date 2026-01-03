using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Utility;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using UnityEngine;
using UnityEngine.AI;
using EventType = Layout.EventType;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace BattleViews.Views
{
    [BoneName("Top", "__Top")]
    [BoneName("Bottom", "__Bottom")]
    [BoneName("Body", "__Body")]
    public class UCharacterView : UElementView, IBattleCharacter
    {
        public const string TopBone = "Top";
        public const string BodyBone = "Body";
        public const string BottomBone = "Bottom";
        public const string RootBone = "ROOT";
        private const string DieMotion = "Die";
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IdleHash = Animator.StringToHash("Idle");

        public string accountUuid = string.Empty;

        public float vSpeed;
        public string lastMotion = string.Empty;
        public float damping = 5;
        public Quaternion targetLookQuaternion;

        public int lockDataValue;
        private readonly Dictionary<string, Transform> _bones = new();

        private readonly Dictionary<int, HeroMagicData> _magicCds = new();

        private readonly List<TimeLineViewPlayer> _timeLinePlayers = new();
        private NavMeshAgent _agent;
        private Animator _characterAnimator;
        private float _hideTime;
        private float _last;

        private float _lockRotationTime = -1f;

        private GameObject _range;

        private GameObject _viewRoot;

        public Action OnDead;

        public Action<UBattleItem> OnItemTrigger;


        public IList<HeroProperty> Properties = new List<HeroProperty>();

        private CharacterMoveState State;

        public bool InStartLayout
        {
            get
            {
                foreach (var i in _timeLinePlayers)
                {
                    if (i.RView.RMType != ReleaserModeType.RmtMagic) continue;
                    if (i.EventType == EventType.EVENT_START) return true;
                }

                return false;
            }
        }

        public int ConfigID { internal set; get; }
        public int TeamId { get; internal set; }
        public int Level { get; internal set; }

        public float Speed
        {
            get
            {
                if (!_agent) return 0;
                return _agent.speed;
            }
            set
            {
                if (!_agent) return;
                _agent.speed = value;
            }
        }

        public string Name { get; internal set; }

        public Quaternion LookQuaternion
        {
            set
            {
                if (_viewRoot) _viewRoot.transform.rotation = value;
            }
            get => _viewRoot ? _viewRoot.transform.rotation : Quaternion.identity;
        }

        public int OwnerIndex { get; internal set; }


        public bool ShowName { set; get; }
        public int MP { get; private set; }
        public int MpMax { get; private set; }

        public int HP { get; private set; }
        public int HpMax { get; private set; }

        public bool IsFullMp => MP == MpMax;
        public bool IsFullHp => HP == HpMax;

        public IList<HeroMagicData> Magics => _magicCds.Values.ToList();
        public bool IsDeath { get; private set; }

        private void Awake()
        {
            _agent = gameObject.AddComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updatePosition = true;
            _agent.acceleration = 20;
            _agent.radius = 0.1f;
            _agent.baseOffset = 0; //-0.15f;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _agent.speed = Speed;
        }

        private void Update()
        {
            LookQuaternion = Quaternion.Lerp(LookQuaternion, targetLookQuaternion, Time.deltaTime * damping);

            if (State == null || State?.Tick(PerView.GetTime()) == true) GoToEmpty();

            if (_lockRotationTime < Time.time && State?.Velocity.magnitude > 0.1f)
                targetLookQuaternion = Quaternion.LookRotation(State.Velocity, Vector3.up);

#if !UNITY_SERVER

            if (_hideTime < Time.time)
                if (_range && _range.activeSelf)
                    _range.SetActive(false);
            PlaySpeed(State?.Velocity.magnitude ?? 0);
#endif
        }


        private void OnTriggerEnter(Collider other)
        {
            var view = other.GetComponent<UCharacterView>();
            if (view == null) return;
            if (GElement is not BattleCharacter o) return;
            if (view.GElement is BattleCharacter ot) o.HitOther(ot);
        }

        void IBattleCharacter.SetLookRotation(float rotationY)
        {
            if (!this) return;
            if (_lockRotationTime > Time.time) return;
            LookQuaternion = targetLookQuaternion = Quaternion.Euler(0, rotationY, 0);
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterRotation
            {
                RotationY = rotationY,
                Index = Index
            });
#endif
        }

        Quaternion IBattleCharacter.Rotation => _viewRoot ? _viewRoot.transform.rotation : Quaternion.identity;
        float IBattleCharacter.Radius => _agent ? _agent.radius : 0;
        Transform IBattleCharacter.Transform => _viewRoot ? _viewRoot.transform : null;
        Transform IBattleCharacter.RootTransform => this ? transform : null;

        void IBattleCharacter.SetPosition(Proto.Vector3 pos)
        {
            if (!this) return;
            _agent.Warp(pos.ToUV3());
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterSetPosition { Index = Index, Position = pos });
#endif
        }

        void IBattleCharacter.LookAtTarget(int target, bool force)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_LookAtCharacter { Index = Index, Target = target, Force = force });
#endif
            LookAtIndex(target);
        }

        void IBattleCharacter.PropertyChange(HeroPropertyType type, int finalValue)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_PropertyValue { Index = Index, Type = type, FinallyValue = finalValue });
#endif
            foreach (var i in Properties)
            {
                if (i.Property != type) continue;
                i.Value = finalValue;
                return;
            }

            Properties.Add(new HeroProperty { Property = type, Value = finalValue });
        }

        void IBattleCharacter.Death()
        {
            if (!this) return;
            var view = this as IBattleCharacter;
            PlayMotion(DieMotion);
            GoToEmpty();
            IsDeath = true;
            OnDead?.Invoke();
            SendMessage("OnDead", SendMessageOptions.DontRequireReceiver);
            //MoveDown.BeginMove (ViewRoot, 1, 1, 5);
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterDeath { Index = Index });
#endif
        }

        void IBattleCharacter.SetSpeed(float speed)
        {
            if (!this) return;
            Speed = speed;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterSpeed { Index = Index, Speed = speed });
#endif
        }

        void IBattleCharacter.SetPriorityMove(float priorityMove)
        {
            if (!this) return;
            _agent.avoidancePriority = (int)priorityMove;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterPriorityMove { Index = Index, PriorityMove = priorityMove });
#endif
        }

        void IBattleCharacter.SetScale(float scale)
        {
            if (!this) return;
            SetScale(scale);
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterSetScale { Index = Index, Scale = scale });
#endif
        }

        void IBattleCharacter.ShowHPChange(int hp, int cur, int max)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_HPChange { Index = Index, Cur = cur, Hp = hp, Max = max });
#endif
            if (IsDeath) return;
            HP = cur;
            HpMax = max;
#if !UNITY_SERVER
            if (hp > 0) PerView.ShowHpCure(GetBoneByName(BodyBone).position, hp);
            else SendMessage("OnHpChanged", SendMessageOptions.DontRequireReceiver);
#endif
        }

        void IBattleCharacter.ShowMPChange(int mp, int cur, int maxMp)
        {
            if (!this) return;
            MpMax = maxMp;
            MP = cur;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_MPChange { Cur = cur, Index = Index, Max = maxMp, Mp = mp });
#endif
#if !UNITY_SERVER
            if (mp > 0) PerView.ShowMpCure(GetBoneByName(BodyBone).position, mp);
            //cost mp
            //Debug.Log($"{mp} - {cur} - {maxMp}");
#endif
        }

        void IBattleCharacter.AttachMagic(MagicType type, int magicID, float cdCompletedTime, float cdTime)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterAttachMagic
            {
                Index = Index,
                MagicId = magicID,
                CompletedTime = cdCompletedTime,
                MType = type,
                CdTime = cdTime
            });
#endif
            AddMagicCd(magicID, cdCompletedTime, type, cdTime, null);
        }

        void IBattleCharacter.SetLock(int lockValue)
        {
            if (!this) return;
            lockDataValue = lockValue;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterLock { Index = Index, Lock = lockValue });
#endif
            if (Index == PerView.OwnerIndex)
            {
                if (!IsLock(ActionLockType.NoInhiden))
                {
                    var g = _viewRoot.GetComponent<AlphaOperator>();
                    if (g) Destroy(g);
                }
                else
                {
                    AlphaOperator.Operator(_viewRoot);
                }
            }
            else
            {
                _viewRoot.SetActive(!IsLock(ActionLockType.NoInhiden));
            }
        }

        void IBattleCharacter.Push(Proto.Vector3 startPos, Proto.Vector3 length, Proto.Vector3 speed)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(
                new Notify_CharacterPush { Index = Index, Speed = speed, Length = length, StartPos = startPos });
#endif
            _agent.Warp(startPos.ToUV3());
            var pushSpeed = speed.ToUV3();
            var pushLeftTime = length.ToUV3().magnitude / pushSpeed.magnitude;
            ChangeState(new PushMove(this, pushSpeed, pushLeftTime))
                .OnExit = () =>
            {
                switch (GElement)
                {
                    case null:
                        return;
                    case BattleCharacter c:
                        c.EndPush();
                        break;
                }
            };
        }

        void IBattleCharacter.StopMove(Proto.Vector3 pos)
        {
            if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterStopMove { Position = pos, Index = Index });
#endif
            if (!TryToSetPosition(pos.ToUV3())) GoToEmpty();
        }

        void IBattleCharacter.SetHpMp(int hp, int hpMax, int mp, int mpMax)
        {
            HP = hp;
            HpMax = hpMax;
            MP = mp;
            MpMax = mpMax;
        }

        bool IBattleCharacter.IsMoving => !(State is Empty);

        Vector3? IBattleCharacter.MoveTo(Proto.Vector3 position, Proto.Vector3 target, float stopDis)
        {
            if (!this) return null;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterMoveTo
            {
                Index = Index,
                Position = position,
                Target = target,
                StopDis = stopDis
            });
#endif

            return MoveToPos(target.ToUV3(), stopDis);
        }

        void IBattleCharacter.Relive()
        {
            if (!this) return;
            IsDeath = false;

#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterRelive
            {
                Index = Index
            });
#endif

#if !UNITY_SERVER
            if (_characterAnimator) _characterAnimator.SetTrigger(IdleHash);
#endif
        }

        void IBattleCharacter.SetLevel(int level)
        {
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterLevel
            {
                Index = Index,
                Level = level
            });
#endif
            Level = level;
        }

        void IBattleCharacter.SetTeamIndex(int tIndex, int ownerIndex)
        {
            TeamId = tIndex;
            OwnerIndex = ownerIndex;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CharacterTeamIndex
            {
                Index = Index,
                TeamIndex = tIndex,
                OwnerIndex = ownerIndex
            });
#endif
        }

        internal void AttachLayoutView(TimeLineViewPlayer timeLineViewPlayer)
        {
            _timeLinePlayers.Add(timeLineViewPlayer);
        }

        internal void DeAttachLayoutView(TimeLineViewPlayer timeLineViewPlayer)
        {
            _timeLinePlayers.Remove(timeLineViewPlayer);
        }

        public Vector3 MoveJoystick(Vector3 forward)
        {
            MoveByDir(forward);
            return transform.position + forward * Speed * .4f;
        }

        private T ChangeState<T>(T s) where T : CharacterMoveState
        {
            State?.Exit();
            State = s;
            State?.Enter();
            return s;
        }

        private void GoToEmpty()
        {
            if (State is Empty) return;
            ChangeState(new Empty(this));
        }

        public bool DoStopMove()
        {
            if (State is not ForwardMove) return false;
            GoToEmpty();
            return true;
        }

        private void PlaySpeed(float speed)
        {
            vSpeed = speed;
            if (_characterAnimator == null) return;
            _characterAnimator.SetFloat(SpeedHash, speed);
        }

        public Transform GetBoneByName(string boneName)
        {
            if (!this || !transform) return null;
            return _bones.TryGetValue(boneName, out var bone) ? bone : transform;
        }

        public async void SetCharacter(GameObject root, string path)
        {
            _viewRoot = root;
            //bones.Add(ViewRootBone, ViewRoot);
            _bones.Add(RootBone, _viewRoot.transform);
            var gameTop = new GameObject("__Top");
            gameTop.transform.SetParent(transform);
            _bones.Add(TopBone, gameTop.transform);

            var bottom = new GameObject("__Bottom");
            bottom.transform.SetParent(transform, false);
            _bones.Add(BottomBone, bottom.transform);
            var body = new GameObject("__Body");
            body.transform.SetParent(transform, false);
            _bones.Add(BodyBone, body.transform);

            if (HP == 0)
            {
                PlayMotion(DieMotion);
                IsDeath = true;
            }

            ;
            await Init(path);
        }

        internal void SetScale(float viewSize)
        {
            gameObject.transform.localScale = Vector3.one * viewSize;
        }

        private async Task Init(string path)
        {
            var obj = await ResourcesManager.S.LoadResourcesWithExName<GameObject>(path);

            var character = Instantiate(obj, _viewRoot.transform, true);

            character.transform.RestRTS();
            character.name = "VIEW";
            var capsuleCollider = character.GetComponent<CapsuleCollider>();

            var height = 1f;
            var radius = .5f;
            var direction = 1;
            var center = new Vector3(0, 0.5f, 0);
            if (capsuleCollider)
            {
                height = capsuleCollider.height;
                radius = capsuleCollider.radius;
                center = capsuleCollider.center;
                direction = capsuleCollider.direction;
                capsuleCollider.enabled = false;
            }

            character.transform.SetLayer(_viewRoot.layer);

            GetBoneByName(TopBone).localPosition = new Vector3(0, height, 0);
            GetBoneByName(BottomBone).localPosition = new Vector3(0, 0, 0);
            GetBoneByName(BodyBone).localPosition = new Vector3(0, height / 2, 0);
            _agent.radius = radius;
            _agent.height = height;
            var c = gameObject.AddComponent<CapsuleCollider>();
            c.radius = radius;
            c.height = height;
            c.center = center;
            c.direction = direction;
            c.isTrigger = true;

            var r = gameObject.AddComponent<Rigidbody>();
            r.isKinematic = true;
            r.useGravity = false;

#if UNITY_SERVER
           Destroy(character);
#else
            _characterAnimator = character.GetComponent<Animator>();
#endif
        }

        private void LookAt(Transform target, bool force = false)
        {
            if (target == null) return;
            var look = target.position - transform.position;
            if (look.magnitude <= 0.01f) return;
            look.y = 0;
            if (!force && _lockRotationTime > Time.time) return;
            _lockRotationTime = Time.time + 0.3f;
            LookQuaternion = targetLookQuaternion = Quaternion.LookRotation(look, Vector3.up);
            ;
        }

        public bool TryGetMagicData(int magicID, out HeroMagicData data)
        {
            return _magicCds.TryGetValue(magicID, out data);
        }

        public bool TryGetMagicByType(MagicType type, out HeroMagicData data)
        {
            data = null;
            foreach (var i in _magicCds)
            {
                if (i.Value.MType != type) continue;
                data = i.Value;
                return true;
            }

            return false;
        }

        public bool TryGetMagicsType(MagicType type, out IList<HeroMagicData> data)
        {
            data = new List<HeroMagicData>();
            foreach (var i in _magicCds)
                if (i.Value.MType == type)
                    data.Add(i.Value);
            return data.Count > 0;
        }

        private bool TryToSetPosition(Vector3 pos)
        {
            if (!(Vector3.Distance(pos, transform.position) > .05f)) return false;
            MoveToPos(pos);
            return true;
        }

        public void LookAtIndex(int target, bool force = false)
        {
            var v = PerView.GetViewByIndex<UElementView>(target);
            if (!v) return;
            LookAt(v.transform, force);
        }

        public void PlayMotion(string motion)
        {
            if (!this) return;
            var an = _characterAnimator;
            if (an == null) return;
            if (motion == "Hit")
                if (_last + 0.3f > Time.time)
                    return;
            if (IsDeath) return;
            if (!string.IsNullOrEmpty(lastMotion) && lastMotion != motion) an.ResetTrigger(lastMotion);
            lastMotion = motion;
            _last = Time.time;
            an.SetTrigger(motion);
        }

        public void AddMagicCd(int id, float cdTimeCompleted, MagicType type, float cdTime, int? mpCost)
        {
            if (_magicCds.TryGetValue(id, out var data))
            {
                data.CDCompletedTime = cdTimeCompleted;
                data.CdTotalTime = cdTime;
            }
            else
            {
                _magicCds.Add(id, new HeroMagicData
                {
                    MType = type,
                    MagicID = id,
                    CDCompletedTime = cdTime,
                    CdTotalTime = cdTime,
                    MPCost = mpCost ?? 0
                });
            }
        }

        public override IMessage ToInitNotify()
        {
            var createNotify = new Notify_CreateBattleCharacter
            {
                Index = Index,
                AccountUuid = accountUuid,
                ConfigID = ConfigID,
                Position = transform.position.ToPVer3(),
                Forward = LookQuaternion.eulerAngles.ToPVer3(),
                Level = Level,
                Name = Name,
                TeamIndex = TeamId,
                OwnerIndex = OwnerIndex,
                Hp = HP,
                Mp = MP
            };

            foreach (var i in Properties) createNotify.Properties.Add(i);

            foreach (var i in _magicCds) createNotify.Cds.Add(i.Value);
            return createNotify;
        }

        public bool IsLock(ActionLockType type)
        {
            return (lockDataValue & (1 << (int)type)) > 0;
        }

        public async void ShowRange(float r)
        {
            if (_range == null)
            {
                _range = new GameObject();
                await ResourcesManager.S.LoadResourcesWithExName<GameObject>("Range.prefab", prefab =>
                {
                    if (_range) Destroy(_range);
                    _range = Instantiate(prefab, GetBoneByName(BottomBone));
                    _range.transform.RestRTS();
                });
            }

            _range.SetActive(true);
            _hideTime = Time.time + 0.2f;
            _range.transform.localScale = Vector3.one * r / 100f;
        }

        private void MoveByDir(Vector3 forward)
        {
            if (State is ForwardMove m)
            {
                m.ChangeDir(forward);
            }
            else
            {
                if (forward.magnitude > 0.01f)
                    ChangeState(new ForwardMove(this, forward));
            }
        }

        private Vector3? MoveToPos(Vector3 target, float stopDis = 0)
        {
            return State switch
            {
                DestinationMove m => m.ChangeTarget(target, stopDis),
                Empty => ChangeState(new DestinationMove(this)).ChangeTarget(target, stopDis),
                _ => transform.position
            };
        }

        #region Move

        private class Empty : CharacterMoveState
        {
            public Empty(UCharacterView v) : base(v)
            {
            }

            public override bool Tick(GTime gTime)
            {
                return false;
            }
        }

        private class PushMove : CharacterMoveState
        {
            public Action OnExit;

            //private readonly UCharacterView view;
            private readonly Vector3 speed;
            private float time;

            public PushMove(UCharacterView view, Vector3 speed, float pushLeftTime) : base(view)
            {
                this.speed = speed;
                time = pushLeftTime;
            }

            public override Vector3 Velocity => speed;

            public override bool Tick(GTime gTime)
            {
                time -= gTime.DeltaTime;
                if (time < 0) return true;
                View._agent.Move(speed * gTime.DeltaTime);
                return false;
            }

            public override void Exit()
            {
                OnExit?.Invoke();
            }
        }

        private class ForwardMove : CharacterMoveState
        {
            public ForwardMove(UCharacterView view, Vector3 forward) : base(view)
            {
                Forward = forward;
            }

            private Vector3? Forward { get; set; }

            public override Vector3 Velocity => (Forward ?? Vector3.zero) * View.Speed;

            public void ChangeDir(Vector3 dir)
            {
                if (dir.magnitude < 0.001f)
                {
                    Forward = null;
                    return;
                }

                Forward = dir;
            }

            public override bool Tick(GTime gTime)
            {
                if (Forward == null) return true;
                View._agent.Move(Forward.Value * (gTime.DeltaTime * View.Speed));
                return false;
            }
        }

        private class DestinationMove : CharacterMoveState
        {
            private float stopDis;

            public DestinationMove(UCharacterView view) : base(view)
            {
            }

            private Vector3? Target { get; set; }

            public override Vector3 Velocity => View._agent.velocity;


            public override bool Tick(GTime gTime)
            {
                return !Target.HasValue || Vector3.Distance(View.transform.position, Target.Value) < stopDis + 0.02f;
            }

            private bool MoveTo(Vector3 target)
            {
                if (!View._agent) return false;
                Target = null;
                View._agent.isStopped = false;
                var path = new NavMeshPath();
                if (!View._agent.CalculatePath(target, path)) return false;
                Vector3? wrapTar = path.corners.LastOrDefault();
                Target = wrapTar;
                if (Vector3.Distance(wrapTar.Value, View.transform.position) < stopDis) return true;
                View._agent.stoppingDistance = stopDis;
                View._agent.SetDestination(wrapTar.Value);
                return true;
            }

            public Vector3? ChangeTarget(Vector3 target, float dis)
            {
                stopDis = dis;
                if (MoveTo(target)) return Target;
                return null;
            }

            public override void Exit()
            {
                View._agent.velocity = Vector3.zero;
                View._agent.ResetPath();
                View._agent.isStopped = true;
            }
        }

        #endregion
    }
}