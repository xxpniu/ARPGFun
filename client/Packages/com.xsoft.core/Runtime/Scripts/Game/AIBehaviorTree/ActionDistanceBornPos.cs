using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Perceptions;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreedNodeDistanceBornPos))]
    public class ActionDistanceBornPos:ActionComposite<TreedNodeDistanceBornPos>
    {
        public ActionDistanceBornPos(TreedNodeDistanceBornPos node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            float distance = Node.distance / 100f;

            if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
            {
                yield return RunStatus.Failure;
                yield break;
            }
            var dis = BattlePerception.Distance(root.Character, root.Character.BronPosition);
            switch (Node.compareType)
            {
                case CompareType.Greater:
                    yield return dis > distance ? RunStatus.Success : RunStatus.Failure;
                    break;
                default:
                    yield return dis < distance ? RunStatus.Success : RunStatus.Failure;
                    break;
            }
            yield break;

        }
    }
}
