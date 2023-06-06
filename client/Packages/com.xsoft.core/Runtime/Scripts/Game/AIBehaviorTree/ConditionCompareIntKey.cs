using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeCompareIntKey))]
    public class ConditionCompareIntKey : ActionComposite<TreeNodeCompareIntKey>
    {
        public ConditionCompareIntKey(TreeNodeCompareIntKey node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {

            if (context is AITreeRoot root)
            {
                if (!root.TryGet<int>(Node.Key, out int value))
                {
                    if (root.IsDebug) Attach("failure", $"No found key {Node.Key}");
                    yield return RunStatus.Failure;
                    yield break;
                }


                int compareValue = Node.CompareValue;
                if (root.IsDebug)
                {
                    Attach("Value", value);
                    Attach("CompareValue", compareValue);
                }

                switch (Node.compareType)
                {
                    case CompareType.Equal:
                        {
                            yield return compareValue == value ? RunStatus.Success : RunStatus.Failure;
                            yield break;
                        }
                    case CompareType.Greater:
                        {
                            yield return compareValue > value ? RunStatus.Success : RunStatus.Failure;
                            yield break;
                        }
                    case CompareType.Less:
                        {
                            yield return compareValue < value ? RunStatus.Success : RunStatus.Failure;
                        }
                        yield break;
                }
            }
            yield return RunStatus.Failure;
            yield break;
        }
    }
}
