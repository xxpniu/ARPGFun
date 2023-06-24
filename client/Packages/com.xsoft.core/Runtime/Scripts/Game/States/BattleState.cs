using System;
using GameLogic.Game.Perceptions;
using EngineCore.Simulater;
using GameLogic.Game.Elements;
using P = Proto.HeroPropertyType;
namespace GameLogic.Game.States
{

    public class BattleState : GState
    {
        public BattleState(IViewBase viewBase, IStateLoader loader, ITimeSimulator simulator)
        {
            ViewBase = viewBase;
            Perception = new BattlePerception(this, viewBase.Create(simulator));
            loader.Load(this);
        }
        public IViewBase ViewBase { private set; get; }

        protected override void Tick(GTime time)
        {
            base.Tick(time);
        }

    }
}

