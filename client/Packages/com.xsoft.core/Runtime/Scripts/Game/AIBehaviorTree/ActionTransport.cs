using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeTransport))]
    public class ActionTransport:ActionComposite<TreeNodeTransport>
    {
        public ActionTransport(TreeNodeTransport nod):base(nod)
        {

        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            //born po as link the sp logic
            var tar =  Node.linkPos.ToUV3();

            root[AITreeRoot.TARGET_POS] = tar;

            yield return RunStatus.Success;
        }
    }
}
