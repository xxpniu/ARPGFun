using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UnityEngine;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeFarFromTarget))]
	public class ActionFarFromTarget:ActionComposite<TreeNodeFarFromTarget>
	{
		public ActionFarFromTarget(TreeNodeFarFromTarget node):base(node)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			var distance = Node.distance.Value / 100f;
			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{
				yield return RunStatus.Failure;
				yield break;
			}

			if (!root.TryGetTarget(out BattleCharacter targetCharacter))
			{
				if (root.IsDebug) Attach("failure", "notarget");
				yield return RunStatus.Failure;
				yield break;
			}
			var noraml = (root.Character.Position - targetCharacter.Position).normalized;
			var target = noraml * distance + root.Character.Position;
			if (!root.Character.MoveTo(target, out _))
			{
				if (root.IsDebug) Attach("failure", "move failure");
				yield return RunStatus.Failure;
				yield break;
			}
			while (root.Character.IsMoving) yield return RunStatus.Running;
			yield return RunStatus.Success;


		}

		public override void Stop(ITreeRoot context)
        {
            if (LastStatus == RunStatus.Running)
            {
                var root = context as AITreeRoot;
				if (root.Character.IsMoving)
					root.Character.StopMove();
            }
			base.Stop(context);
		}
	}
}

