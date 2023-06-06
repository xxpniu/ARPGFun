using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class EventBattle:Decorator,ICharacterWatcher
    {
        public EventBattle(Composite child):base(child)
        {

        }

        private bool IsReceived = false;

        public int eventType;

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            while (true)
            {
                while (!IsReceived) yield return RunStatus.Running;
                IsReceived = false;
                DecoratedChild.Start(context);
                while (DecoratedChild.Tick(context) == RunStatus.Running)
                {
                    yield return RunStatus.Running;
                }
            }
        }


        public override void Start(ITreeRoot context)
        {
            base.Start(context);

            if (context is AITreeRoot r)
            {
                r.Character.AddEventWatcher(this);
            }
        }


        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            if (context is AITreeRoot r)
            {
                r.Character.RemoveEventWathcer(this);
            }
        }

        void ICharacterWatcher.OnFireEvent(BattleEventType eventType, object args)
        {
            int et = (int)eventType;
            if((et & this.eventType )>0) IsReceived = true;
        }
    }
}
