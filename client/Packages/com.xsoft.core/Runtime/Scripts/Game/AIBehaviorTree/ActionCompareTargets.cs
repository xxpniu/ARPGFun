using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Proto;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeCompareTargets))]
	public class ActionCompareTargets:ActionComposite<TreeNodeCompareTargets>
	{
		public ActionCompareTargets(TreeNodeCompareTargets node):base(node)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{

			var root = context as AITreeRoot;
			var per = root.Perception;
			var targets = new List<BattleCharacter>();
			float distance = Node.Distance;
			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{
				yield return RunStatus.Failure;
				yield break;
			}

			distance *= (Node.ValueMul / 10000f);

			per.State.Each<BattleCharacter>(t =>
			{
				if (t.IsDeath) return false;
				switch (Node.teamType)
				{
					case TargetTeamType.Enemy:
						if (t.TeamIndex == root.Character.TeamIndex) return false;
						break;
					case TargetTeamType.OwnTeam:
						if (t.TeamIndex != root.Character.TeamIndex) return false;
						break;
				}
				if (BattlePerception.Distance(t, root.Character) <= distance)
					targets.Add(t);
				return false;
			});

			int count = Node.compareValue;
			switch (Node.compareType)
			{
				case CompareType.Greater:
					if (count < targets.Count)
						yield return RunStatus.Success;
					else
						yield return RunStatus.Failure;
					break;
				case CompareType.Less:
					if (count > targets.Count)
						yield return RunStatus.Success;
					else
						yield return RunStatus.Failure;
					break;
			}
			yield break;

		}
	}
}

