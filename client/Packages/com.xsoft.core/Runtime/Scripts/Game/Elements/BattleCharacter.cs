using EngineCore.Simulater;
using Layout.LayoutEffects;
using GameLogic.Game.AIBehaviorTree;
using System;
using System.Collections.Generic;
using Proto;
using GameLogic.Game.Perceptions;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using P = Proto.HeroPropertyType;
using Layout.AITree;
using UnityEngine;
using Google.Protobuf;
using XNet.Libs.Utility;

namespace GameLogic.Game.Elements
{
    public delegate bool EachWithBreak(BattleCharacterMagic item);

    public class BattleCharacter : BattleElement<IBattleCharacter>
    {
        private readonly Queue<IMessage> _actions = new();
        
        private readonly List<ICharacterWatcher> _eventWatchers = new ();
        private readonly Dictionary<P, ComplexValue> _properties = new ();
        private Dictionary<int, BattleCharacterMagic> Magics { set; get; }
        private object _tempObj;
        public TreeNode DefaultTree { get; set; }
        public string DefaultTreePath { set; get; }
        public string AccountUuid { private set; get; }
        public HeroCategory Category { set; get; }

        public DefanceType TDefance { set; get; }
        public DamageType TDamage { set; get; }
        public UVector3 BronPosition { private set; get; }
        public Dictionary<int, DamageWatch> Watch { get; } = new Dictionary<int, DamageWatch>();

        public int GroupIndex {set;get;}
        public int MaxHP => this[P.MaxHp];
        public int MaxMP => this[P.MaxMp];
        public float NormalCdTime =>  1000f/ this[P.AttackSpeed];
        public string Name { set; get; }
        public int TeamIndex { private set; get; }
        public int Level { set; get; }
        public HanlderEvent<BattleCharacter> OnDead;
        public int ConfigID { private set; get; }
        private ActionLock Lock {  set; get; }
        public float Radius => View.Radius;

        private float _baseSpeed;
        public float Speed
        {
            set
            {
                _baseSpeed = value;
                View.SetSpeed(Speed);
            }
            get
            {
                var speed =  _baseSpeed;
                return Math.Min(BattleAlgorithm.MaxSpeed, speed);
            }
        }
  
        public int HP { private set; get; }
        public int MP { private set; get; }
        public bool IsDeath => HP == 0;
        public AITreeRoot AiRoot { private set; get; }
        public UVector3 Position
        {
            get
            {
                var t = View?.Transform;
                return !t ? UVector3.zero : t.position;
            }
            set
            {
                var tart = View?.Transform;
                if (!tart) return;
                View.SetPosition(value.ToPV3());
            }
        }
        public UVector3 Forward {
            get
            {
                var t = View?.Transform;
                return !t ? UVector3.forward : t.forward;
            }
        }
        public bool IsMoving => View.IsMoving;
        public Quaternion Rotation => View.Rotation;
        public Transform Transform => this.View.RootTransform;
        //property
        public ComplexValue this[P type] => _properties[type];
        //call unit owner
        public int OwnerIndex { private set; get; } 
        public CharacterData Config { private set; get; }

        public IBattleCharacter CharacterView => this.View; 

        public BattleCharacter (
            CharacterData data,
            IList<BattleCharacterMagic> magics,
            GControllor controller, 
            IBattleCharacter view, 
            string accountUuid,int teamIndex,
            Dictionary<P,ComplexValue> properties, int ownerIndex = -1):base(controller,view)
		{
            this.TeamIndex = teamIndex;
            this.OwnerIndex = ownerIndex;
            this.Config = data;
            AccountUuid = accountUuid;
			HP = 0;
            
			ConfigID = data.ID;

            Magics = new Dictionary<int, BattleCharacterMagic>();
            
            foreach (var i in magics)
            {
                if (!Magics.TryAdd(i.ConfigId, i)) continue;
            }
            var enums = Enum.GetValues(typeof(P));
            foreach (var i in enums)
            {
                var pr = (P)i;
                var value = new ComplexValue();
                _properties.Add(pr, value);

            }

            foreach (var i in properties)
            {
                _properties[i.Key].SetBaseValue(i.Value);
            }

            _baseSpeed = _properties[P.MoveSpeed]/100f;
            Lock = new ActionLock();
            Lock.OnStateOnChanged += (s, e) =>
            {
                switch (e.Type)
                {
                    case ActionLockType.NoMove:
                        if (e.IsLocked)StopMove();
                        break;
                    case ActionLockType.NoAi:
                        this.AiRoot?.Stop();
                        break;

                }
            };
            BronPosition = Position;
		}

        public void AddEventWatcher(ICharacterWatcher watcher)
        {
            this._eventWatchers.Add(watcher);
        }

        public void RemoveEventWatcher(ICharacterWatcher watcher)
        {
            _eventWatchers.Remove(watcher);
        }

        public bool AddMagic(CharacterMagicData data)
        {
            if (Magics.ContainsKey(data.ID)) return false;
            Magics.Add(data.ID, new BattleCharacterMagic(MagicType.MtMagic, data));
            return true;
        }

        public bool RemoveMaic(int id)
        {
           return  Magics.Remove(id);
        }

        public bool MoveTo(UVector3 target, out UVector3 warpTarget, float stopDis = 0f)
        {
            warpTarget = target;
            if (IsLock(ActionLockType.NoMove)) return false;
            var r = View.MoveTo(View.Transform.position.ToPV3(), target.ToPV3(), stopDis);
            if (!r.HasValue) return false;
            warpTarget = r.Value; 
            FireEvent(BattleEventType.Move, this);
            return true;
        }

        private Action<BattleCharacter,object> _launchHitCallback;

        internal void BeginLaunchSelf(Quaternion rotation, float distance, float speed, Action<BattleCharacter,object> hitCallback, MagicReleaser releaser)
        {
            if (!TryStartPush(rotation, distance, speed)) return;
            PushEnd = () =>
            {
                _launchHitCallback = null;
                releaser.DeAttachElement(this);
            };
            releaser.AttachElement(this, true);
            _tempObj = releaser;
            _launchHitCallback = hitCallback;
        }

        public void HitOther(BattleCharacter character)
        {
            _launchHitCallback?.Invoke(character, _tempObj);
        }

        public void StopMove(UVector3? pos =null)
        {
            var p = pos ?? Position;
            View.StopMove(p.ToPV3());
        }

        internal void TryToSetPosition(UVector3 pos, float rotation)
        {
            View.SetPosition(pos.ToPV3());
            View.SetLookRotation(rotation);
        }

        public bool SubHP(int hp, out bool dead)
        {
            dead = HP == 0;
            if (hp <= 0) return false;
            if (HP == 0) return false;
            HP -= hp;
            if (HP <= 0) HP = 0;
            dead = HP == 0;//is dead
            View.ShowHPChange(-hp, HP, this.MaxHP);
            if (dead) OnDeath();
            return dead;
        }

        public void SetTeamIndex(int tIndex,int ownerIndex)
        {
            this.TeamIndex = tIndex;
            this.OwnerIndex = ownerIndex;
            this.View.SetTeamIndex(tIndex, ownerIndex);
        }

        public Action PushEnd;

        private bool TryStartPush(Quaternion rotation, float distance, float speed)
        {
            if (Lock.IsLock(ActionLockType.NoMove)) return false;
            var dir = rotation * UVector3.forward;
            var dis = dir * distance;
            var ps = dir * speed;
            View.Push(Position.ToPV3(), dis.ToPV3(), ps.ToPV3());
            return true;
        }

        public void EndPush()
        {
            PushEnd?.Invoke();
            PushEnd = null;
        }

        public bool AddHP(int hp)
        {
            var maxHp = MaxHP;
            if (hp <= 0 || HP >= maxHp) return false;
            if (HP == 0)
            {
                Debug.LogError($"{HP}==0");
                return false;
            }
            var t = HP;
            HP += hp;
            if (HP >= maxHp) HP = maxHp;
            if (t == HP) return false;
            View.ShowHPChange(hp, HP, maxHp); 
            return true;
        }

        public bool Relive(int hp)
        {
            if (HP > 0) return true;
            if (hp < 0) return false;
            HP = hp;
            View.Relive();
            View.ShowHPChange(hp, HP, MaxHP);
            return true;
        }

        public void LookRotation(float rY)
        {
            View.SetLookRotation(rY);
        }

        public void LookAt(BattleCharacter character, bool force = false)
        {
            View.LookAtTarget(character.Index, force);
        }

        public bool SubMp(int mp)
        {
            if (mp == 0) return true;
            if (mp < 0) return false;
            if (MP - mp < 0) return false;
            MP -= mp;
            View.ShowMPChange(-mp, MP, this.MaxMP);
            return true;
        }

        public bool AddMp(int mp)
        {
            var temp = MP;
            MP += mp;
            if (MP >= MaxMP) MP = MaxMP;
            if (temp == MP) return false;
            View.ShowMPChange(mp, MP, MaxMP);
            return true;
        }

        private readonly Queue<AITreeRoot> _next = new();

        private const int MaxActionBuffer = 20;

        public T AddNetAction<T>(T action) where T : IMessage
        {
            _actions.Enqueue(action);
            if (_actions.Count <= MaxActionBuffer) return action;
            Debuger.LogWaring($"{this.AccountUuid} have more than {MaxActionBuffer}");
            _actions.Dequeue();
            return action;
        }

        public bool TryDequeueNetAction(out IMessage message)
        {
            if (_actions.Count > 0)
            {
                message = _actions.Dequeue();
                return true;
            }
            message = null;
            return false;
        }
        internal void SetAITreeRoot(AITreeRoot root, bool force = false)
        {
            if (force) _next.Clear();
            _next.Enqueue(root);
        }


        internal void TickAi()
        {
            if (_next.Count > 0)
            {
                AiRoot?.Stop();
                AiRoot = _next.Dequeue();
            }
            if (Lock.IsLock(ActionLockType.NoAi)) return;
            AiRoot?.Tick();
        }

        public void ResetHpMp(int hp = -1, int mp = -1)
        {
            this.HP = hp == -1 ? MaxHP : (int)Mathf.Max(MaxHP * 0.1f, hp);
            this.MP = mp == -1 ? MaxMP : mp;
            View.SetHpMp(HP, MaxHP, MP, MaxMP);
        }


        private void OnDeath()
		{
            FireEvent(BattleEventType.Death, this);
			View.Death();
            OnDead?.Invoke(this);
            var per = this.Controller.Perception as BattlePerception;
            per!.StopAllReleaserByCharacter(this);
            AiRoot?.BreakTree();
		}

        public void AttachMagicHistory(int magicID, float now, float? cdTime =null)
        {
            if (!Magics.TryGetValue(magicID, out var magic)) return; 
            var cd = cdTime ?? magic.CdTime;
            magic.CdCompletedTime = now + (cdTime ?? magic.CdTime);
            View.AttachMagic(magic.Type, magic.ConfigId, magic.CdCompletedTime ,cd);
        }

        internal bool IsLock(ActionLockType type)
        {
            return Lock.IsLock(type);
        }

        public bool IsCoolDown(int magicID, float now, bool autoAttach = false, float? cdTime = null)
        {
            var isOk = true;
            if (Magics.TryGetValue(magicID, out var h)) isOk = h.IsCoolDown(now);
            if (autoAttach) AttachMagicHistory(magicID, now, cdTime);
            return isOk;
        }

        public void ModifyValueAdd(P property, AddType addType, float resultValue)
        {
            var value = this[property];
            value.ModifyValueAdd(addType, resultValue);
            View.PropertyChange(property, value.FinalValue);
        }

        public void ModifyValueMinutes(P property, AddType miType, float resultValue)
        {
            var value = this[property];
            value.ModifyValueMinutes(miType, resultValue);
            View.PropertyChange(property, value.FinalValue);
        }

        public void EachActiveMagicByType(MagicType ty, float time, EachWithBreak call)
        {
            foreach (var i in Magics)
            {
                if (i.Value.Type != ty) continue;
                if (!i.Value.IsCoolDown(time)) continue;
                if (call?.Invoke(i.Value) == true) break;
            }
        }

        public void EachMagicByType(MagicType ty, EachWithBreak call)
        {
            foreach (var i in Magics)
            {
                if (i.Value.Type != ty) continue;
                if (call?.Invoke(i.Value) == true) break;
            }
        }


        public bool TryGetActiveMagicById(int magicId, float time, out BattleCharacterMagic data)
        {
            return Magics.TryGetValue(magicId, out data) && data.IsCoolDown(time);
        }

        internal bool HaveMagicByType(MagicType ty)
        {
            foreach (var i in Magics)
            {
                if (i.Value.Type != ty) continue;

                return true;
            }
            return false;
        }

        public void LockAction(ActionLockType type)
        {
            Lock.Lock(type);
            View.SetLock(Lock.Value);
        }

        public void UnLockAction(ActionLockType type)
        {
            Lock.Unlock(type);
            View.SetLock(Lock.Value);
        }

        public void FireEvent(BattleEventType ev, object args)
        {
            foreach (var i in _eventWatchers)
            {
                i.OnFireEvent(ev, args);
            }
        }

        public void AttachDamage(int sources, int damage, float time)
        {
            if (damage <= 0) return;
            if (Watch.TryGetValue(sources, out var w)) 
            {
                w.TotalDamage += damage;
            }
            else
            {
                w = new DamageWatch { Index = sources, TotalDamage = damage, FirstTime = time };
                Watch.Add(sources, w);
            }
            w.LastTime = time;
        }

        public void SetLevel(int level)
        {
            View.SetLevel(level);
        }

        public override string ToString()
        {
            return $"[{Index}]{Name}";
        }

    }
}

