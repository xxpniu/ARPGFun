using System;
using System.Collections.Generic;
using EngineCore.Simulater;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Layout;
using Layout.AITree;
using Layout.LayoutEffects;
using Layout.LayoutElements;
using Proto;
using UnityEngine;
using EventType = Layout.EventType;
using Vector3 = UnityEngine.Vector3;

//using UnityEngine;


namespace GameLogic.Game.Elements
{
    public enum ReleaserStates
    {
        NOStart,
        Starting,
        Releasing,
        ToComplete,
        Completing,
        Ended
    }

    /// <summary>
    /// 释放者所系统 技能 还是 buff
    /// </summary>
    public enum ReleaserType
    {
        Magic,
        Buff
    }

    public class RevertActionLock
    {
        public BattleCharacter target;
        public ActionLockType type;
    }

    public class RevertData
    {
        public AddType addtype;
        public float addValue;
        public HeroPropertyType property;
        public BattleCharacter target;
    }

    public class MagicReleaser : BattleElement<IMagicReleaser>, ICharacterWatcher
    {
        private readonly List<RevertActionLock> _actionReverts = new();

        private readonly HashSet<int> _hitList = new();

        private readonly Dictionary<int, AttachedElement> _objs = new();

        private readonly LinkedList<TimeLinePlayer> _players = new();
        private readonly Queue<int> _removeTemp = new();
        private readonly List<RevertData> _reverts = new();

        private int _playerIndex;

        private TimeLinePlayer _startLayout;
        public float TickTime = -1;
        
        public MagicReleaseType MRT { private set;  get; }

        public MagicReleaser(
            string key,
            MagicData magic,
            BattleCharacter owner,
            IReleaserTarget target,
            GControllor controllor,
            IMagicReleaser view,
            ReleaserType type,MagicReleaseType mrt,  float durTime, bool moveCancel,
            string[] magicParams = default
        )
            : base(controllor, view)
        {
            MoveCancel = moveCancel;
            MagicKey = key;
            Owner = owner;
            ReleaserTarget = target;
            Magic = magic;
            RType = type;
            OnExitedState = ReleaseAll;
            Durtime = type == ReleaserType.Buff ? durTime : -1;
            MRT = mrt;
            Params = magicParams;
        }

        public bool MoveCancel { get; }

        public string MagicKey { private set; get; }

        public BattleCharacter Owner { private set; get; }

        //绑定生命周期 buff用
        public BattleCharacter BindLifeCharacter { get; private set; }

        public float Durtime { set; get; }

        public string[] Params { private set; get; }


        public ReleaserType RType { get; }

        public MagicData Magic { get; }

        public IReleaserTarget ReleaserTarget { get; }

        public ReleaserStates State { private set; get; }

        public int UnitCount => _objs.Count;

        public bool IsCompleted
        {
            get
            {
                if (State == ReleaserStates.NOStart)
                    return false;

                var current = _players.First;
                while (current != null)
                {
                    if (!current.Value.IsFinshed) return false;
                    current = current.Next;
                }

                if (_objs.Count > 0)
                    foreach (var i in _objs)
                        if (i.Value.Element.Enable)
                            return false;
                return true;
            }
        }

        public EventType? LastEvent { get; private set; }

        public bool IsLayoutStartFinish
        {
            get
            {
                if (State == ReleaserStates.NOStart) return false;
                if (State == ReleaserStates.Starting && _startLayout != null) return _startLayout.IsFinshed;
                return true;
            }
        }

        public BattleCharacter Releaser => ReleaserTarget.Releaser;

        public BattleCharacter Target => ReleaserTarget.ReleaserTarget;

        public int DisposeValue { get; internal set; } = 0;
        public Vector3 Position => View.Position;
        public Quaternion Rotation => View.Rotation;

        void ICharacterWatcher.OnFireEvent(BattleEventType eventType, object args)
        {
            if (RType == ReleaserType.Buff)
                switch (eventType)
                {
                    case BattleEventType.Skill:
                        if ((DisposeValue & (int)DisposeType.SKILL) > 0) ToCompleted();
                        break;
                    case BattleEventType.Move:
                        if ((DisposeValue & (int)DisposeType.MOVE) > 0) ToCompleted();
                        break;
                    case BattleEventType.Hurt:
                        if ((DisposeValue & (int)DisposeType.HURT) > 0) ToCompleted();
                        break;
                    case BattleEventType.NormalAttack:
                        if ((DisposeValue & (int)DisposeType.NormarlAttack) > 0) ToCompleted();
                        break;
                }
        }

        public void SetParam(params string[] parms)
        {
            Params = parms;
        }

        public void SetState(ReleaserStates state)
        {
            State = state;
        }

        public void OnEvent(EventType eventType, BattleCharacter target = null)
        {
            target = target ?? ReleaserTarget.ReleaserTarget;
            var per = Controller.Perception as BattlePerception;
            LastEvent = eventType;

            for (var index = 0; index < Magic.Containers.Count; index++)
            {
                var i = Magic.Containers[index];
                if (i.type == eventType)
                {
                    var timeLine = i.line ?? per.View.GetTimeLineByPath(i.layoutPath);
                    if (timeLine == null) continue;
                    _playerIndex++;
                    var player = new TimeLinePlayer(_playerIndex, timeLine, this, i, target);
                    _players.AddLast(player);
                    if (i.line == null)
                        View.PlayTimeLine(_playerIndex, index, target.Index, (int)eventType); //for runtime
                    else View.PlayTest(_playerIndex, i.line);
                    if (i.type == EventType.EVENT_START)
                    {
                        if (_startLayout != null) throw new Exception("Start layout must only one!");
                        _startLayout = player;
                    }
                }
            }
        }

        public void AttachElement(GObject el, bool onlyWatch = false, float time = -1f)
        {
            if (_objs.ContainsKey(el.Index)) return;
            var att = new AttachedElement
            {
                time = time,
                Element = el,
                HaveLeftTime = time >= 0f,
                Managed = !onlyWatch
            };
            _objs.Add(el.Index, att);
            ;
        }

        internal void Cancel()
        {
            if (!IsLayoutStartFinish) StopAllPlayer();
            SetState(ReleaserStates.ToComplete);
        }

        public void Tick(GTime time)
        {
            var current = _players.First;
            while (current != null)
            {
                if (current.Value.Tick(time))
                {
                    current.Value.Destory();
                    _players.Remove(current);
                }

                current = current.Next;
            }

            if (_objs.Count == 0) return;

            foreach (var i in _objs)
            {
                if (!i.Value.Managed) continue;
                if (i.Value.Element.IsAliveAble)
                {
                    if (i.Value.HaveLeftTime)
                    {
                        i.Value.time -= time.DeltaTime;
                        if (i.Value.Element is BattleCharacter character)
                            if (i.Value.time <= 0)
                                character.SubHP(character.MaxHP, out _);
                    }

                    continue;
                }

                _removeTemp.Enqueue(i.Key);
            }

            while (_removeTemp.Count > 0) _objs.Remove(_removeTemp.Dequeue());
        }

        internal void ShowDamageRange(DamageLayout layout, Vector3 tar, Quaternion rototion)
        {
            View.ShowDamageRanger(layout, tar, rototion);
        }

        public float GetLayoutTimeByPath(string path)
        {
            foreach (var i in _players)
                if (i.TypeEvent.layoutPath == path)
                    return i.PlayTime;
            return -1f;
        }

        public void StopAllPlayer()
        {
            foreach (var i in _players)
            {
                i.Destory();
                View.CancelTimeLine(i.Index);
            }

            _players.Clear();
        }

        internal bool TryHit(BattleCharacter hit)
        {
            if (_hitList.Contains(hit.Index)) return false;
            _hitList.Add(hit.Index);
            return true;
        }

        private void ReleaseAll(GObject el)
        {
            foreach (var i in _reverts)
                if (i.target.Enable)
                    i.target.ModifyValueMinutes(i.property, i.addtype, i.addValue);

            foreach (var i in _actionReverts)
                if (i.target.Enable)
                    i.target.UnLockAction(i.type);

            _actionReverts.Clear();
            _reverts.Clear();

            foreach (var i in _objs)
            {
                if (!i.Value.Managed) continue;
                Destroy(i.Value.Element);
            }

            _objs.Clear();
            foreach (var i in _players) i.Destory();

            _players.Clear();
        }


        internal void DeAttachElement(BattleCharacter battleCharacter)
        {
            _objs.Remove(battleCharacter.Index);
        }

        public bool IsRunning(EventType type)
        {
            foreach (var i in _players)
            {
                if (i.IsFinshed) continue;
                if (i.TypeEvent.type == type) return true;
            }

            return false;
        }

        internal RevertData RevertProperty(BattleCharacter effectTarget, HeroPropertyType property, AddType addType,
            float addValue)
        {
            var rP = new RevertData
                { addtype = addType, addValue = addValue, property = property, target = effectTarget };
            _reverts.Add(rP);
            return rP;
        }

        public RevertActionLock RevertLock(BattleCharacter effectTarget, ActionLockType lockType)
        {
            var rLock = new RevertActionLock { target = effectTarget, type = lockType };
            _actionReverts.Add(rLock);
            return rLock;
        }

        protected override void OnJoinState()
        {
            base.OnJoinState();
            Releaser.AddEventWatcher(this);
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            Releaser.RemoveEventWatcher(this);
        }

        private void ToCompleted()
        {
            if (State == ReleaserStates.Completing || State == ReleaserStates.Ended ||
                State == ReleaserStates.ToComplete) return;
            State = ReleaserStates.ToComplete;
        }

        internal int TryGetParams(GetValueFrom vF)
        {
            var index = (int)vF - 1;
            if (Params == null) return 0;
            if (Params.Length <= index) return 0;
            if (index < 0) return 0;
            if (int.TryParse(Params[index], out var v)) return v;
            return 0;
        }

        /// <summary>
        ///     绑定一个对象的生命周期 如果消失就结束技能
        /// </summary>
        /// <param name="lifeCharacter"></param>
        public void BindCharacter(BattleCharacter lifeCharacter)
        {
            BindLifeCharacter = lifeCharacter;
        }

        public class AttachedElement
        {
            public GObject Element;
            public bool HaveLeftTime;
            public bool Managed;
            public float time;
        }
    }
}