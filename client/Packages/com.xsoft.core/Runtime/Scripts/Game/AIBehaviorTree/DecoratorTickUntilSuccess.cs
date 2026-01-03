using System.Collections.Generic;
using BehaviorTree;
using Layout;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorTickUntilSuccess : Decorator
    {
        public DecoratorTickUntilSuccess(Composite child) : base(child)
        {
        }

        public FieldValue TickTime { set; get; }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var lastTime = context.Time;

            while (true)
            {
                if (lastTime + TickTime / 1000f <= context.Time)
                {
                    lastTime = context.Time;
                    DecoratedChild.Start(context);
                    while (DecoratedChild.Tick(context) == RunStatus.Running) yield return RunStatus.Running;
                    if (DecoratedChild.LastStatus == RunStatus.Success)
                    {
                        yield return RunStatus.Success;
                        yield break;
                    }
                }

                yield return RunStatus.Running;
            }
        }

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            DecoratedChild.Stop(context);
        }
    }
}