using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using EngineCore.Simulater;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Proto;
using Vector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
    public class AITreeRoot : ITreeRoot
    {
        public const string SELECT_MAGIC_ID = "__Magic_ID__";
        public const string TARGET_INDEX = "__Target_Index__";
        public const string TARGET_POS = "__Target_Pos__";

        private readonly Dictionary<string, object> _blackbroad = new();
        private Composite _current;

        private bool NeedBreak;

        private Composite next;

        public AITreeRoot(ITimeSimulator timeSimulator, BattleCharacter userstate,
            Composite root, TreeNode nodeRoot, string path)
        {
            TreePath = path;
            TimeSimulator = timeSimulator;
            Character = userstate;
            Character = userstate;
            Root = root;
            NodeRoot = nodeRoot;
        }

        public string TreePath { get; }

        public TreeNode NodeRoot { private set; get; }

        public ITimeSimulator TimeSimulator { get; }

        public BattlePerception Perception => Character.Controller.Perception as BattlePerception;

        public BattleCharacter Character { get; }

        public Composite Root { get; }

        public object this[string key]
        {
            set
            {
                if (value == null)
                {
                    _blackbroad.Remove(key);
                    return;
                }

                _blackbroad[key] = value;
            }
            get => _blackbroad.GetValueOrDefault(key);
        }

        public bool IsDebug { set; get; }
        public object UserState => Character;

        public void Change(Composite cur)
        {
            next = cur;
        }

        public GTime Time => TimeSimulator.Now;

        public bool GetDistanceByValueType(DistanceValueOf type, float value, out float outValue)
        {
            outValue = value;
            switch (type)
            {
                case DistanceValueOf.BlackboardMagicRangeMax:
                {
                    var data = this[SELECT_MAGIC_ID];
                    if (data == null) return false;
                    var magic = ExcelToJSONConfigManager.GetId<CharacterMagicData>((int)data);
                    if (magic == null) return false;
                    outValue = magic.RangeMax / 100f;
                }
                    break;
                case DistanceValueOf.BlackboardMagicRangeMin:
                {
                    var data = this[SELECT_MAGIC_ID];
                    if (data == null) return false;
                    var magic = ExcelToJSONConfigManager.GetId<CharacterMagicData>((int)data);
                    if (magic == null) return false;
                    outValue = magic.RangeMin / 100f;
                }
                    break;
                case DistanceValueOf.ViewDistance:
                    outValue = Character[HeroPropertyType.ViewDistance].FinalValue / 100f;
                    break;
            }

            return true;
        }

        public void Tick()
        {
            if (_current == null) _current = Root;

            if (next != null)
            {
                if (_current?.LastStatus == RunStatus.Running)
                    _current.Stop(this);
                _current = next;
                next = null;
            }

            if (NeedBreak)
            {
                NeedBreak = false;
                if (_current?.LastStatus == RunStatus.Running) _current.Stop(this);
            }

            if (_current!.LastStatus != RunStatus.Running) _current.Start(this);
            if (_current.Tick(this) != RunStatus.Running) _current = Root;
        }

        public void BreakTree()
        {
            NeedBreak = true;
        }

        public void ClearBlackBroad()
        {
            _blackbroad.Clear();
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

        public bool TryGetTarget(out BattleCharacter target, bool igHidden = true)
        {
            target = null;
            if (!TryGet(TARGET_INDEX, out int index)) return false;
            target = Perception.FindTarget(index);
            if (target == null) return false;
            if (igHidden && target.IsLock(ActionLockType.NoInhiden)) return false;
            return !target.IsDeath;
        }

        public bool TryGetTargetPos(out Vector3 target)
        {
            return TryGet(TARGET_POS, out target);
        }

        internal bool TryGetMagic(out CharacterMagicData magicData)
        {
            magicData = null;
            if (!TryGet(SELECT_MAGIC_ID, out int id)) return false;
            magicData = ExcelToJSONConfigManager.GetId<CharacterMagicData>(id);
            return magicData != null;
        }

        internal void Stop()
        {
            if (_current?.LastStatus == RunStatus.Running) _current?.Stop(this);
        }

        public override string ToString()
        {
            return $"{TreePath}";
        }
    }
}