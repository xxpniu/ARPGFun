using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeSetIntKey))]
    public class ActionSetIntKey:ActionComposite<TreeNodeSetIntKey>
    {
        public ActionSetIntKey(TreeNodeSetIntKey node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            if (context is AITreeRoot root)
            {
                if (!root.TryGet(Node.Key, out int value))
                {
                    value = 0;
                }

                if (Node.operatorType == OperatorType.Clear)
                {
                    root[Node.Key] = null;
                    yield return RunStatus.Success;
                    yield break;
                }


                if (root.IsDebug)
                {
                    Attach("Org Value", value);
                }
                var opValue = Node.OperatorValue;
                switch (Node.operatorType)
                {
                    case OperatorType.Add:
                        value += opValue;
                        break;
                    case OperatorType.Minus:
                        value -= opValue;
                        break;
                    case OperatorType.Reset:
                        value = opValue;
                        break;
                    default:
                        yield return RunStatus.Failure;
                        yield break;
                }

                root[Node.Key] = value;

                if (root.IsDebug)
                {
                    Attach("Value", value);
                }


                yield return RunStatus.Success;
                yield break;
            }

            yield return RunStatus.Failure;
            yield break;
        }
    }
}
