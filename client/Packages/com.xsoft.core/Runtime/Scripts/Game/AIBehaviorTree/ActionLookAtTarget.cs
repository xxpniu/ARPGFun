using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeLookAtTarget))]
    public class ActionLookAtTarget : ActionComposite<TreeNodeLookAtTarget>
    {
        public ActionLookAtTarget(TreeNodeLookAtTarget n) : base(n) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
    
            while (true)
            {
                if (!root.TryGetTarget(out BattleCharacter character))
                {
                    yield return RunStatus.Failure;
                    yield break;
                }

                root.Character.LookAt(character);
                var time = root.Time;
                while (root.Time - time < .3f)
                {
                    yield return RunStatus.Running;
                }
                yield return RunStatus.Running;
            }

        }
    }
}
