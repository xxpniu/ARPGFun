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

namespace GameLogic.Game.Elements
{
    public class BattleCharacterMagic
    {
        public MagicType Type { private set; get; }

        public int MpCost { private set; get; }

        public CharacterMagicData Config { private set; get; }

        public int ConfigId { get { return Config.ID; } }

        public BattleCharacterMagic(MagicType type, CharacterMagicData config, MagicLevelUpData lv = null, float? cdTime = null)
        {
            Type = type;
            Config = config;
            this.LevelData = lv;
            MpCost = config.MPCost;
            MpCost = lv?.MPCost ?? MpCost;
            if (cdTime.HasValue) CdTime = cdTime.Value;
            else CdTime = config.TickTime / 1000f;
        }


        private MagicLevelUpData LevelData { set; get; }

        public float CdTime { get; set; } = 1f;

        public float CdCompletedTime { set; get; }

        public string[] Params => new[]
        {
            LevelData?.Param1, LevelData?.Param2, LevelData?.Param3, LevelData?.Param4, LevelData?.Param5
        };

        public bool IsCoolDown(float time)
        {
            return time > CdCompletedTime ;
        }

    }

    public delegate bool EachWithBreak(BattleCharacterMagic item);

    public class DamageWatch
    {
        public int Index { set; get; }
        public int TotalDamage { set; get; }
        public float LastTime { set; get; }
        public float FristTime { get; internal set; }
    }

    public class BattleCharacter : BattleElement<IBattleCharacter>
    {
        private readonly Queue<IMessage> Actions = new Queue<IMessage>();
        
        private readonly List<ICharacterWatcher> EventWatchers = new List<ICharacterWatcher>();
        private readonly Dictionary<P, ComplexValue> Properties = new Dictionary<P, ComplexValue>();
        private Dictionary<int, BattleCharacterMagic> Magics { set; get; }
        private object tempObj;
        public TreeNode DefaultTree { get; set; }
        public string DefaultTreePath { set; get; }
        public string AcccountUuid { private set; get; }
        public HeroCategory Category { set; get; }

        public DefanceType TDefance { set; get; }
        public DamageType TDamage { set; get; }
        public UVector3 BronPosition { private set; get; }
        public Dictionary<int, DamageWatch> Watch { get; } = new Dictionary<int, DamageWatch>();

        public int GroupIndex {set;get;}
        public int MaxHP { get { return this[P.MaxHp]; } }
        public int MaxMP { get { return this[P.MaxMp]; } }
        public float NormalCdTime
        {
            get
            {
                return 1000f/ this[P.AttackSpeed] ;
            }
        }
        public string Name { set; get; }
        public int TeamIndex { private set; get; }
        public int Level { set; get; }
        public HanlderEvent<BattleCharacter> OnDead;
        public int ConfigID { private set; get; }
        private ActionLock Lock {  set; get; }
        public float Radius
        {
            get { return View.Radius; }
        }

        private float BaseSpeed;
        public float Speed
        {
            set
            {
                BaseSpeed = value;
                View.SetSpeed(Speed);
            }
            get
            {
                var speed =  BaseSpeed;
                return Math.Min(BattleAlgorithm.MAX_SPEED, speed);
            }
        }
  
        public int HP { private set; get; }
        public int MP { private set; get; }
        public bool IsDeath
        {
            get
            {
                return HP == 0;
            }
        }
        public AITreeRoot AiRoot { private set; get; }
        public UVector3 Position
        {
            get
            {
                var t = View?.Transform;
                if (!t) return UVector3.zero;
                return t.position;
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
                if (!t) return UVector3.forward;
                return t.forward;
            }
        }
        public bool IsMoving { get { return View.IsMoving; } }
        public Quaternion Rototion { get { return View.Rotation; } }
        public Transform Transform { get { return this.View.RootTransform; } }
        //property
        public ComplexValue this[P type] { get { return Properties[type]; } }
        //call unit owner
        public int OwnerIndex { private set; get; } 
        public CharacterData Config { private set; get; }

        public IBattleCharacter CharacterView => this.View; 

        public BattleCharacter (
            CharacterData data,
            IList<BattleCharacterMagic> magics,
            GControllor controllor, 
            IBattleCharacter view, 
            string account_uuid,int teamIndex,
            Dictionary<P,ComplexValue> properties, int ownerIndex = -1):base(controllor,view)
		{
            this.TeamIndex = teamIndex;
            this.OwnerIndex = ownerIndex;
            this.Config = data;
            AcccountUuid = account_uuid;
			HP = 0;
            
			ConfigID = data.ID;

            Magics = new Dictionary<int, BattleCharacterMagic>();
            
            foreach (var i in magics)
            {
                if (Magics.ContainsKey(i.ConfigId)) continue;
                Magics.Add(i.ConfigId, i);
            }
            var enums = Enum.GetValues(typeof(P));
            foreach (var i in enums)
            {
                var pr = (P)i;
                var value = new ComplexValue();
                Properties.Add(pr, value);

            }

            foreach (var i in properties)
            {
                Properties[i.Key].SetBaseValue(i.Value);
            }

            BaseSpeed = Properties[P.MoveSpeed]/100f;
            Lock = new ActionLock();
            Lock.OnStateOnchanged += (s, e) =>
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
            this.EventWatchers.Add(watcher);
        }

        public void RemoveEventWathcer(ICharacterWatcher watcher)
        {
            EventWatchers.Remove(watcher);
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
            if (r.HasValue)
            {
                warpTarget = r.Value; 
                FireEvent(BattleEventType.Move, this);
            }
            return r.HasValue;
        }

        private Action<BattleCharacter,object> launchHitCallback;

        internal void BeginLauchSelf(Quaternion rototion, float distance, float speed, Action<BattleCharacter,object> hitCallback, MagicReleaser releaser)
        {
            if (TryStartPush(rototion, distance, speed))
            {
                PushEnd = () =>
                {
                    launchHitCallback = null;
                    releaser.DeAttachElement(this);
                };
                releaser.AttachElement(this, true);
                tempObj = releaser;
                launchHitCallback = hitCallback;
            }
        }

        public void HitOther(BattleCharacter character)
        {
            launchHitCallback?.Invoke(character, tempObj);
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

        internal bool TryStartPush(Quaternion rotation, float distance, float speed)
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
            var maxHP = MaxHP;
            if (hp <= 0 || HP >= maxHP) return false;
            if (HP == 0)
            {
                Debug.LogError($"{HP}==0");
                return false;
            }
            var t = HP;
            HP += hp;
            if (HP >= maxHP) HP = maxHP;
            if (t == HP) return false;
            View.ShowHPChange(hp, HP, maxHP);
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

        public bool SubMP(int mp)
        {
            if (mp == 0) return true;
            if (mp < 0) return false;
            if (MP - mp < 0) return false;
            MP -= mp;
            View.ShowMPChange(-mp, MP, this.MaxMP);
            return true;
        }

        public bool AddMP(int mp)
        {
            var temp = MP;
            MP += mp;
            if (MP >= MaxMP) MP = MaxMP;
            if (temp == MP) return false;
            View.ShowMPChange(mp, MP, MaxMP);
            return true;
        }

        private readonly Queue<AITreeRoot> _next = new Queue<AITreeRoot>();


        public T AddNetAction<T>(T action) where T : IMessage
        {
            Actions.Enqueue(action);
            if (Actions.Count > 20) Actions.Dequeue();
            return action;
        }

        public bool TryDequeueNetAction(out IMessage message)
        {
            if (Actions.Count > 0)
            {
                message = Actions.Dequeue();
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

        public void ResetHPMP(int hp = -1, int mp = -1)
        {
            this.HP = hp == -1 ? MaxHP : (int)Mathf.Max(MaxHP * 0.1f, hp);
            this.MP = mp == -1 ? MaxMP : mp;
            View.SetHpMp(HP, MaxHP, MP, MaxMP);
        }


		protected void OnDeath()
		{
            FireEvent(BattleEventType.Death, this);
			View.Death();
            OnDead?.Invoke(this);
            var per = this.Controllor.Perception as BattlePerception;
            per.StopAllReleaserByCharacter(this);
            AiRoot?.BreakTree();
		}

        public void AttachMagicHistory(int magicID, float now, float? cdTime =null)
        {
            if (Magics.TryGetValue(magicID, out BattleCharacterMagic magic))
            {
                var cd = (cdTime ?? magic.CdTime);
                magic.CdCompletedTime = now+ (cdTime ?? magic.CdTime);
                View.AttachMagic(magic.Type, magic.ConfigId, magic.CdCompletedTime ,cd);
            }
        }

        internal bool IsLock(ActionLockType type)
        {
            return Lock.IsLock(type);
        }

        public bool IsCoolDown(int magicID, float now, bool autoAttach = false,float? cdTime =null)
        {
            bool isOK = true;
            if (Magics.TryGetValue(magicID, out BattleCharacterMagic h)) isOK = h.IsCoolDown(now);
            if (autoAttach) AttachMagicHistory(magicID, now,cdTime);
            return isOK;
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
            if (Magics.TryGetValue(magicId, out data))
            {
                return data.IsCoolDown(time);
            }
            return false;
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
            foreach (var i in EventWatchers)
            {
                i.OnFireEvent(ev, args);
            }
        }

        public void AttachDamage(int sources, int damage, float time)
        {
            if (damage > 0)
            {
                if (Watch.TryGetValue(sources, out DamageWatch w))
                {
                    w.TotalDamage += damage;
                }
                else
                {
                    w = new DamageWatch { Index = sources, TotalDamage = damage, FristTime = time };
                    Watch.Add(sources, w);
                }
                w.LastTime = time;
            }
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

