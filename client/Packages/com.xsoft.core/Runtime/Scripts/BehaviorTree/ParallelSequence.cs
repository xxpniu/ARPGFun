using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    /// <summary>
    /// 同时启动所有的子节点
    /// 并且一直运行直到任何一个子节点返回 failure 跳出
    /// 如果所有的节点返回success返回success
    /// </summary>
    public class ParallelSequence : GroupComposite
    {
        public ParallelSequence(params Composite[] children)
            : base(children)
        {
        }

   
        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {

            foreach (var i in Children)
            {
                i.Start(context);
            }

            var status = RunStatus.Running;
            //如果没有为failure的返回
            while (status == RunStatus.Running)
            {
                foreach (var i in Children)
                {
                    if (i.LastStatus.HasValue && i.LastStatus != RunStatus.Running) continue;
                    i.Tick(context);
                }
                status = RunStatus.Success;
                foreach (var i in Children)
                {
                    if (i.LastStatus == RunStatus.Failure)
                    {
                        status = RunStatus.Failure;
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