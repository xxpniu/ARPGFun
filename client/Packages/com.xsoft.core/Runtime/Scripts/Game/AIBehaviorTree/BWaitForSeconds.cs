using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    //TreeNodeWaitForSeconds
    [TreeNodeParse(typeof(TreeNodeWaitForSeconds))]
    public class BWaitForSeconds : ActionComposite<TreeNodeWaitForSeconds>
    {
        public BWaitForSeconds(TreeNodeWaitForSeconds n) : base(n)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var Seconds = Node.seconds / 1000f;
            //var root = context as AITreeRoot;
            var time = context.Time;
            //var lastTime = time;
            while (time + Seconds >= context.Time)
                yield return RunStatus.Running;
            yield return RunStatus.Success;
        }
    }
}