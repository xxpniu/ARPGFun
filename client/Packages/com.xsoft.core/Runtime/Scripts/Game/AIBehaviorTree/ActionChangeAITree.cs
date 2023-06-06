using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeChangeAITree))]
    public class ActionChangeAITree : ActionComposite<TreeNodeChangeAITree>
    {
        public ActionChangeAITree(TreeNodeChangeAITree node) : base(node) { }
        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            if (context is AITreeRoot root)
            {
                root.Perception.ChangeCharacterAI(Node.Path, root.Character);
                yield return RunStatus.Success;
                yield break;
            }
            else
            {
                yield return RunStatus.Failure;
                yield break;
            }
        }
    }
}
