using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    /// <summary>
    ///   Will execute each branch of logic in order, until one succeeds. This composite
    ///   will fail only if all branches fail as well.
    /// </summary>
    public class PrioritySelector : GroupComposite
    {
        public PrioritySelector(params Composite[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            foreach (Composite node in Children)
            {
                node.Start(context);
                while (node.Tick(context) == RunStatus.Running)
                {
                    yield return RunStatus.Running;
                }
                if (node.LastStatus == RunStatus.Success)
                {
                    yield return RunStatus.Success;
                    yield break;
                }
            }
            yield return RunStatus.Failure;
            yield break;
        }
    }
}