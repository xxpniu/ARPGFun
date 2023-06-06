using System;
using System.Collections.Generic;
using BehaviorTree;
using EngineCore;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeMoveRandom))]
	public class ActionMoveRandom : ActionComposite<TreeNodeMoveRandom>
	{

		public ActionMoveRandom(TreeNodeMoveRandom n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;

			int forwad = Node.Forward;
			float dis = Node.distance / 100f;
			if (context.IsDebug)
			{
				Attach("Forward", forwad);
				Attach("dis", dis);
			}
			var diff =  Quaternion.Euler(0, forwad, 0)* root.Character.Forward;
			var target = root.Character.Position + (diff * dis);
			if (!root.Character.MoveTo(target,out _))
			{
				yield return RunStatus.Failure;
				yield break;
            }
			while (root.Character.IsMoving)
			{
				yield return RunStatus.Running;
            }

			yield return RunStatus.Success;

		}

		public override void Stop(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			if (LastStatus == RunStatus.Running)
                if (root.Character.IsMoving) root.Character.StopMove();
			base.Stop(context);
		}
	}
}

