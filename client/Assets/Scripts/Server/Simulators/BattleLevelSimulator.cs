using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Layout;
using Layout.AITree;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Core;
using BattleViews;
using BattleViews.Views;
using UnityEngine;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Google.Protobuf;
using GameLogic.Game.LayoutLogics;
using UnityEngine.Serialization;
using XNet.Libs.Utility;


namespace Server
{

    public class LevelSimulaterAttribute : Attribute
    {
        public MapType MType { set; get; }
    }

    [Serializable]
    public class BattleLevelSimulator :  IStateLoader, IAIRunner
    {

        public BattleSimulator Simulator { private set; get; }

        #region AI RUN
        private BattleCharacter aiAttach;
        AITreeRoot IAIRunner.RunAI(TreeNode ai)
        {
            if (aiAttach == null)
            {
                Debug.LogError($"Need attach a battlecharacter");
                return null;
            }

            if (this.State.Perception is BattlePerception p)
            {
                var root = p.ChangeCharacterAI(ai, this.aiAttach);
                root.IsDebug = true;
                return root;
            }

            return null;
        }

        bool IAIRunner.IsRunning(Layout.EventType eventType)
        {
            return false;
        }

        bool IAIRunner.ReleaseMagic(MagicData data)
        {
            return false;
        }

        void IAIRunner.Attach(BattleCharacter character)
        {
            aiAttach = character;
            if (character.AiRoot == null) return;
            character.AiRoot.IsDebug = true;
        }

        #endregion

        void IStateLoader.Load(GState state)
        {

        }

        private ITimeSimulator _timeSimulator;
        public UPerceptionView PerView;
        public BattleLevelData LevelData;

        public BattleState State { private set; get; }
        public GTime GetTime() { return _timeSimulator.Now; }
        public MapConfig Config { private set; get; }
        public GTime TimeNow => GetTime();
        
        public float totalTime = 0f;


        public async  Task Init(BattleSimulator simulator, BattleLevelData data, UPerceptionView view)
        {
            this.Simulator = simulator;
            LevelData = data;
            this.PerView = view;
            _timeSimulator = PerView;
            AIRunner.Current = this;
            await ResourcesManager.S.LoadResourcesWithExName<TextAsset>(LevelData.ElementConfigPath,(res)=> {
                Config = res.text?.Parser<MapConfig>();
            });

            Debuger.Log($"Map:{Config}");
            State = new BattleState(PerView, this, PerView);
            State.Start(this.GetTime());
            OnLoadCompleted();
        }

        protected virtual void OnLoadCompleted()
        {
            totalTime = LevelData.LimitTime;
        }

        public BattlePerception Per { get { return State.Perception as BattlePerception; } }

        public bool TryGetElementByIndex<T>(int index, out T el) where T : GObject
        {
            if (this.State[index] is T e)
            {
                el = e;
                return true;
            }
            el = null;
            return false;
        }

        protected virtual int PlayerTeamIndex  { get; } = 1;

        public BattleCharacter CreateUser(BattlePlayer user)
        {
            BattleCharacter character = null;
            State.Each<BattleCharacter>(t =>
            {
                if (!t.Enable) return false;
                if (t.AccountUuid != user.AccountId) return false;
                character = t;
                return true;
            });
            
            if (character != null) return character;
            var per = State.Perception as BattlePerception;
            var data = CM.GetId<CharacterData>(user.GetHero().HeroID);
            var level = CM.First<CharacterLevelUpData>(t => t.Level == user.GetHero().Level);
            var properties = data.CreatePlayerProperties(level);
            
            Debuger.Log($"Hero: {user.GetHero()}");
            var magic = user.GetHero().CreateHeroMagic();
            Debuger.Log($"Magic Count:{magic.Count}");
            
            

            foreach (var i in user.GetHero().Equips)
            {
                var equip = user.GetEquipByGuid(i.GUID);
                if (equip == null)
                {
                    Debug.LogError($"No found equip {i.GUID}");
                    continue;
                }
                var ps = equip.GetProperties();
                foreach (var p in ps)
                {
                    properties.TryAdd(p.Key, p.Value);
                }
            }
            //hp
            //mp
            var hero = user.GetHero();
            var playerBornPositions = Config.Elements.Where(t => t.Type == MapElementType.MetPlayerInit)
            .Select(t => t).ToArray();
            var pos = GRandomer.RandomArray(playerBornPositions);//.transform;//.position;        
            character = per!.CreateCharacter(per.StateControllor,
                hero.Level,
                data,
                magic, properties,
                PlayerTeamIndex,
                pos.Position.ToUV3(),
                Quaternion.LookRotation(pos.Forward.ToUV3()).eulerAngles,
                user.AccountId,
                user.GetHero().Name);

            return character;
        }

        public void Stop()
        {
            State?.Stop(TimeNow);
        }

        public IMessage[] GetInitNotify()
        {
            return PerView.GetInitNotify();
        }

        public (bool end, IMessage[] msgs) Tick()
        {
            if (State == null) return (false,null);
            OnTick();
            GState.Tick(State, TimeNow);
            return (CheckEnd(), PerView.GetAndClearNotify());
        }

        protected virtual void OnTick()
        {
            if (totalTime > 0) totalTime -= TimeNow.DeltaTime;
        }

        public virtual bool CheckEnd()
        {
            if (totalTime <= 0)
            {
                return true;
            }
            return false;
        }

        public MagicReleaser CreateReleaser(string key, BattleCharacter heroCharacter, ReleaseAtTarget rTarget, ReleaserType Rt, ReleaserModeType rmType, int dur)
        {
            if (State.Perception is BattlePerception per)
            {
               return  per.CreateReleaser(key, heroCharacter, rTarget, Rt, rmType, dur);
            }
            return null;
        }

        private static readonly Dictionary<MapType, Type> Types = new Dictionary<MapType, Type>();

        static BattleLevelSimulator()
        {
            var t = typeof(BattleLevelSimulator);
            var types =t .Assembly.GetTypes();
            foreach (var i in types)
            {
                if (!i.IsSubclassOf(t)) continue;
                if (!(i.GetCustomAttributes(typeof(LevelSimulaterAttribute), false) is LevelSimulaterAttribute[] atts) || atts.Length == 0) continue;
                Types.Add(atts[0].MType, i);
            }
        }

        public static BattleLevelSimulator Create( BattleLevelData level)
        {
            var MType = (MapType)level.MapType;
            Debuger.Log($"LoadType:{MType}");
            if (Types.TryGetValue(MType, out Type t))
            {
                var si = Activator.CreateInstance(t) as BattleLevelSimulator;
                Debuger.Log($"Simualter:{si.GetType()}");
                return si;
            }
            Debug.LogError($"nofound {MType}");
            return null;
        }
    }
}
