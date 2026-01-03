using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeMoveToBronPosition))]
    public class ActionMoveToBronPosition : ActionComposite<TreeNodeMoveToBronPosition>
    {
        public ActionMoveToBronPosition(TreeNodeMoveToBronPosition n) : base(n)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            var stop = Node.distance / 100f;
            if (!root.Character.MoveTo(root.Character.BronPosition, out _, stop))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            while (root.Character.IsMoving) yield return RunStatus.Running;

            yield return RunStatus.Success;
        }

        public override void Stop(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            if (LastStatus == RunStatus.Running)
                if (root.Character.IsMoving)
                    root.Character.StopMove();
            base.Stop(context);
        }
    }
}