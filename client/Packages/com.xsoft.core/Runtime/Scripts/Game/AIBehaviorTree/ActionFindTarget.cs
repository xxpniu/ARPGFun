using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Layout.EditorAttributes;
using Proto;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeFindTarget))]
	public class ActionFindTarget:ActionComposite<TreeNodeFindTarget>
    {
		public ActionFindTarget(TreeNodeFindTarget n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;

			var per = root!.Perception;

			var distance = Node.Distance / 100f;

			int view = Node.View;
			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{

				yield return RunStatus.Failure;
				yield break;
			}


			distance *= (Node.ValueMul / 10000f);


			//是否保留之前目标
			if (!Node.findNew)
			{
				root.TryGetTarget(out var targetCharacter); 
				if (targetCharacter && !targetCharacter.IsDeath)
				{
					if (BattlePerception.InviewSide(root.Character, targetCharacter, distance, view))
					{
						yield return RunStatus.Success;
						yield break;
					}
				}
			}
			//清除
			root[AITreeRoot.TARGET_INDEX] = null;
			var type = Node.teamType;
			//处理使用魔法目标
			if (Node.useMagicConfig)
			{
				if (!root.TryGetMagic(out var data)) 
				{
					yield return RunStatus.Failure;
					yield break;
				}
				type = (TargetTeamType)data.AITargetType;
			}

			var target = per.FindTarget(root.Character, type, distance, view, true, Node.selectType, Node.filter);
			if (!target)
			{
				if (root.IsDebug) Attach("failure", "not found");
				yield return RunStatus.Failure;
				yield break;
			}
			if (context.IsDebug) Attach("Target", target);
			root[AITreeRoot.TARGET_INDEX] = target.Index;
			yield return RunStatus.Success;
		}

	}
}

