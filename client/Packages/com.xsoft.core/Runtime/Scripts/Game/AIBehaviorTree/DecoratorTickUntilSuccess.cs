using BehaviorTree;
using Layout;
using System.Collections.Generic;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorTickUntilSuccess : Decorator
    {
        public DecoratorTickUntilSuccess(BehaviorTree.Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
			var lastTime = context.Time;

            while (true)
            {
                if (lastTime + (TickTime / 1000f) <= context.Time)
                {
                    lastTime = context.Time;
                    DecoratedChild.Start(context);
                    while (DecoratedChild.Tick(context) == RunStatus.Running)
                    {
                        yield return RunStatus.Running;
                    }
                    if (DecoratedChild.LastStatus == RunStatus.Success)
                    {
                        yield return RunStatus.Success;
                        yield break;
                    }
                }
                yield return RunStatus.Running;
            }
        }

        public FieldValue TickTime { set; get; }

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            DecoratedChild.Stop(context);
        }
    }
}

