using BehaviorTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorRunUntilSuccess :Decorator
    {
        public DecoratorRunUntilSuccess(Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            while (true)
            {
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
                yield return RunStatus.Running;
            }
        }
    }
}
