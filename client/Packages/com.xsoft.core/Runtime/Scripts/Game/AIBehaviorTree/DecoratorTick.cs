using BehaviorTree;
using Layout;
using Layout.AITree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorTick : Decorator
    {
        public DecoratorTick(Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
			var lastTime = context.Time;

            while (true)
            {
				if (lastTime + (Node.tickTime / 1000f) <= context.Time)
                {
					lastTime = context.Time;
                    DecoratedChild.Start(context);
                    while (DecoratedChild.Tick(context) == RunStatus.Running)
                    {
                        yield return RunStatus.Running;
                    }
                }
                yield return RunStatus.Running;
            }
        }
        public TreeNodeTick Node { get; internal set; }

        public override void Stop(ITreeRoot context)
		{
			base.Stop(context);
			DecoratedChild.Stop(context);
		}
    }
}
