using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeDistancTarget))]
	public class ActionDistancTarget : ActionComposite<TreeNodeDistancTarget>
	{
		public ActionDistancTarget(TreeNodeDistancTarget node):base(node)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;

			if (!root!.TryGetTarget(out var target)) 
			{
				yield return RunStatus.Failure;
				yield break;
			}

			if (target.IsDeath)
			{
				yield return RunStatus.Failure;
				yield break;

			}

			if (target.IsLock(Proto.ActionLockType.NoInhiden))
			{
				yield return RunStatus.Failure;
				yield break;
			}



            if (!root.GetDistanceByValueType(Node.valueOf, Node.distance/100f, out var  distance))
            {
                yield return RunStatus.Failure;
                yield break;
            }

			distance *= (Node.ValueMul / 10000f);

			switch (Node.compareType)
			{
				case CompareType.Less:
					if (BattlePerception.Distance(target, root.Character) > distance)
						yield return RunStatus.Failure;
					else
						yield return RunStatus.Success;
					break;
				case CompareType.Greater:
					if (BattlePerception.Distance(target, root.Character) > distance)
						yield return RunStatus.Success;
					else
						yield return RunStatus.Failure;
					break;
			}
		}

		//private float distance;
	}
}

