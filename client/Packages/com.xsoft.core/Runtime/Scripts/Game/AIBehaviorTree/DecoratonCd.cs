using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratonCd : Decorator
    {
        public DecoratonCd(Composite child) : base(child)
        {
        }

        public TreeNodeCd Node { get; internal set; }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            while (true)
            {
                var cd = Node.CdTime / 1000f;
                if (root.TryGet(Guid, out float cdTime))
                {
                    while (cdTime + cd > root.Time) yield return RunStatus.Running;
                    DecoratedChild.Start(context);
                    while (DecoratedChild.Tick(context) == RunStatus.Running) yield return RunStatus.Running;
                }

                root[Guid] = root.Time;
                if (root.IsDebug) Attach("CdTime", root.Time);
                yield return RunStatus.Running;
            }
        }
    }
}