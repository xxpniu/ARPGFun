using System;
using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using EngineCore.Simulater;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Google.Protobuf;
using Layout.AITree;
using UnityEngine;

namespace GameLogic.Game.AIBehaviorTree
{
    public class AITreeRoot :  ITreeRoot
    {

        public const string SELECT_MAGIC_ID = "__Magic_ID__";
        public const string TARGET_INDEX = "__Target_Index__";
        public const string TARGET_POS = "__Target_Pos__";
       
        public bool IsDebug { set; get; }
        public object UserState { get { return Character; } }
        private Composite Current;

        private readonly Dictionary<string, object> _blackbroad = new Dictionary<string, object>();

        public string TreePath { private set; get; }

        public AITreeRoot(ITimeSimulater timeSimulater, BattleCharacter userstate,
            Composite root,TreeNode nodeRoot, string path)
        {
            this.TreePath = path;
            TimeSimulater = timeSimulater;
            Character = userstate;
            Character = userstate;
            Root = root;
            NodeRoot = nodeRoot;
        }
        public bool GetDistanceByValueType(DistanceValueOf type, float value, out float outValue)
        {
            outValue = value;
            switch (type)
            {
                case DistanceValueOf.BlackboardMagicRangeMax:
                    {
                        var data = this[SELECT_MAGIC_ID];
                        if (data == null)
                        {
                            return false;
                        }
                        var magic = ExcelToJSONConfigManager.GetId<CharacterMagicData>((int)data);
                        if (magic == null)
                        {
                            return false;
                        }
                        outValue = magic.RangeMax/100f;
                    }
                    break;
                case DistanceValueOf.BlackboardMagicRangeMin:
                    {
                        var data = this[SELECT_MAGIC_ID];
                        if (data == null)
                        {
                            return false;
                        }
                        var magic =ExcelToJSONConfigManager.GetId<CharacterMagicData>((int)data);
                        if (magic == null)
                        {
                            return false;
                        }
                        outValue = magic.RangeMin/100f;
                    }
                    break;
                case DistanceValueOf.ViewDistance:
                    outValue = Character[Proto.HeroPropertyType.ViewDistance].FinalValue / 100f;
                    break;
                default:
                    break;
            }
            return true;
        }

        public TreeNode NodeRoot { private set; get; }

        public ITimeSimulater TimeSimulater { private set; get; }

        public BattlePerception Perception { get { return Character.Controllor.Perception as BattlePerception; } }

        public BattleCharacter Character { get; private set; }

        public Composite Root { private set; get; }

        private bool NeedBreak = false;

        public void Tick()
        {
            if (Current == null)
            {
                Current = Root;
            }

            if (next != null)
            {
                if (Current?.LastStatus == RunStatus.Running)
                    Current.Stop(this);
                Current = next;
                next = null;
            }

            if (NeedBreak)
            {
                NeedBreak = false;
                if (Current?.LastStatus == RunStatus.Running) Current.Stop(this);
            }

            if (Current.LastStatus != RunStatus.Running)
            {
                Current.Start(this);
            }
            if (Current.Tick(this)!= RunStatus.Running)
            {
                Current = Root;
            }
        }

        private Composite next;

        public void Chanage(Composite cur)
        {
            next = cur;
        }

        public void BreakTree()
        {
            NeedBreak = true;
        }

        public GTime Time => TimeSimulater.Now;

		public object this[string key] 
        { 
			set {
                if (value == null)
                {
                    _blackbroad.Remove(key);
                    return;
                }
				if (_blackbroad.ContainsKey(key)) _blackbroad[key] = value;
				else
					_blackbroad.Add(key, value);
			}
			get {
                if (_blackbroad.TryGetValue(key, out object v)) return v;
                return null;
			}
		}

        public bool TryGet<T>(string key, out T v)
        {
            var t = this[key];
            v = default;
            if (!(t is T val)) return false;
            v = val;
            if (v == null) return false;
            return true;
        }

        public bool TryGetTarget( out BattleCharacter target, bool igHidden = true)
        {
            target = null;
            if (!TryGet(TARGET_INDEX, out int index)) return false;
            target = Perception.FindTarget(index);
            if (target == null) return false;
            if (igHidden && target.IsLock(Proto.ActionLockType.NoInhiden)) return false;
            return !target.IsDeath;
        }

        public bool TryGetTargetPos(out Vector3 target)
        {
            if (!TryGet(TARGET_POS, out target)) return false;
            return true;
        }

        internal bool TryGetMagic(out CharacterMagicData magicData)
        {
            magicData = null;
            if (!TryGet(SELECT_MAGIC_ID, out int id))
            {
                return false;
            }
            magicData = ExcelToJSONConfigManager.GetId<CharacterMagicData>(id);
            return magicData != null;

        }

        internal void Stop()
        {
            if (Current?.LastStatus == RunStatus.Running) Current?.Stop(this);
        }

        public override string ToString()
        {
            return $"{TreePath}";
        }

    }
}

