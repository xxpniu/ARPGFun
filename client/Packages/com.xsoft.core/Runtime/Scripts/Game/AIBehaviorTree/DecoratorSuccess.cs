using BehaviorTree;
using System.Collections.Generic;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorSuccess : Decorator
    {
        public DecoratorSuccess(Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            DecoratedChild.Start(context);
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }
            yield return RunStatus.Success;
        }
    }
}
