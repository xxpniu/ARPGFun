using BehaviorTree;
using Layout;
using Layout.AITree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratonCd : Decorator
    {
        public DecoratonCd(Composite child) : base(child) { }
        public TreeNodeCd Node { get; internal set; }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            while (true)
            {
                var cd = Node.CdTime / 1000f;
                if (root.TryGet(this.Guid, out float cdTime))
                {
                    while (cdTime + cd > root.Time) yield return RunStatus.Running;
                    DecoratedChild.Start(context);
                    while (DecoratedChild.Tick(context) == RunStatus.Running)
                    {
                        yield return RunStatus.Running;
                    }
                }
                root[this.Guid] = root.Time;
                if (root.IsDebug) Attach("CdTime", root.Time);
                yield return RunStatus.Running;
            }
        }
    }
}
