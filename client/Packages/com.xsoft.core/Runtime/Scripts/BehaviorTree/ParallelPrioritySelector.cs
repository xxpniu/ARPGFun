using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    /// <summary>
    /// 并行执行所有子节点 直到有一个子节点返回success
    /// 所有子节点都返回failure则返回failure
    /// </summary>
    public class ParallelPrioritySelector : GroupComposite
    {
        public ParallelPrioritySelector(params Composite[] children)
            : base(children)
        {
        }

        public override void Start(ITreeRoot context)
        {
            base.Start(context);
           
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            foreach (var i in Children)
            {
                i.Start(context);
            }
            RunStatus status = RunStatus.Running;
            while (status == RunStatus.Running)
            {
                status = RunStatus.Failure;
                foreach (var i in Children)
                {
                    //如果已经执行完跳过
                    if (i.LastStatus.HasValue && i.LastStatus.Value != RunStatus.Running) continue;
                    i.Tick(context);
                }
                foreach(var i in Children)
                { 
                    if (i.LastStatus == RunStatus.Success)
                    {
                        status = RunStatus.Success;
                        break;
                    }
                    if (i.LastStatus == RunStatus.Running) status = RunStatus.Running;
                }
                if (status == RunStatus.Running) yield return RunStatus.Running;
            }
            yield return status;
        }
	}
}