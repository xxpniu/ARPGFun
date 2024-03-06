using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeAroundBornPosMove))]
    public class ActionAroundBornPosMove:ActionComposite<TreeNodeAroundBornPosMove>
    {
        public ActionAroundBornPosMove(TreeNodeAroundBornPosMove node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            var pos = root!.Character.BronPosition;

            var angle = Randomer.RandomMinAndMax(0, 360);
            root.GetDistanceByValueType(Node.Value, Node.distance / 100f, out float dis);

            var forward = UnityEngine.Quaternion.Euler(0, angle, 0) * UnityEngine.Vector3.forward ;

            var target = pos + forward * dis;

            if (root.IsDebug)
            {
                Attach("angle", angle);
                Attach("target", target);
            }

            if (!root.Character.MoveTo(target, out _))
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
                if (root!.Character.IsMoving) root.Character.StopMove();
            base.Stop(context);
        }
    }
}
