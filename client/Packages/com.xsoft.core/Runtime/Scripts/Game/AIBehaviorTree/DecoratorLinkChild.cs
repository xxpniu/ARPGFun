using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorLinkChild : Decorator
    {
        public DecoratorLinkChild(Composite child) : base(child)
        {

        }

        public TreeNodeLinkNode Node { set; get; }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            DecoratedChild.Start(context);
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }
            yield return DecoratedChild.LastStatus.Value;
        }

    }
}
