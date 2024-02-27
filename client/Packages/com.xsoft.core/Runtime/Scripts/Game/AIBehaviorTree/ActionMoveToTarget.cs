using System;
using System.Collections.Generic;
using BehaviorTree;
using EngineCore;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Proto;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeMoveCloseTarget))]
	public class ActionMoveToTarget : ActionComposite<TreeNodeMoveCloseTarget>
	{

		public ActionMoveToTarget(TreeNodeMoveCloseTarget n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			if (!root!.TryGetTarget(out var target))
			{
				if (context.IsDebug) Attach("failure", $"not found target by target");
				yield return RunStatus.Failure;
				yield break;
			}

			if (!root.GetDistanceByValueType(Node.valueOf, Node.distance/100f, out var stopDistance)) 
			{
				if (context.IsDebug)
					Attach("failure", $"not found stop distance");
				yield return RunStatus.Failure;
				yield break;
			}


			while (true)
			{
				if (!target || target.IsDeath || target.IsLock(ActionLockType.NoInhiden) )
				{
					if (root.IsDebug) Attach("failure", $"target is isDeath:{target.IsDeath} InHidden:{target.IsLock(ActionLockType.NoInhiden)} ");
					root.Character?.StopMove();
					yield return RunStatus.Failure;
					yield break;
				}
				if (!root.Character.MoveTo(target.Position, out _, stopDistance))
				{
					yield return RunStatus.Failure;
					yield break;
				}
				
				var time = root.Time;
				while (time + .5f > root.Time && root.Character.IsMoving)
				{
					yield return RunStatus.Running;
				}

				if (!root.Character.IsMoving)
				{
					break;
				}
			}

			if (BattlePerception.Distance(root.Character, target) > stopDistance)
			{
				if (root.IsDebug) { Attach("failure", "move failure"); }
				yield return RunStatus.Failure;
				yield break;
			}
			

			yield return RunStatus.Success;

		}

		public override void Stop(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			if (LastStatus == RunStatus.Running) if (root.Character) root.Character?.StopMove();
			base.Stop(context);
		}
	}
}

