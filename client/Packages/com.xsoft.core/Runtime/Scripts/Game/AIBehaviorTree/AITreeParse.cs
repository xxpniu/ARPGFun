using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
	public class TreeNodeParseAttribute : Attribute
	{
		public TreeNodeParseAttribute(Type paserType)
		{
			ParserType = paserType;
		}

		public Type ParserType { set; get; }
	}

	public interface ITreeLoader
	{
		TreeNode Load(string path);
    }

	public class AITreeParse
	{
		private static readonly Dictionary<Type, Type> _handler = new Dictionary<Type, Type>();
		static AITreeParse()
		{
			_handler.Clear();
			var types = typeof(AITreeRoot).Assembly.GetTypes();
			foreach (var i in types)
			{
				if (i.IsSubclassOf(typeof(Composite)))
				{
					var attrs = i.GetCustomAttributes(typeof(TreeNodeParseAttribute), false) as TreeNodeParseAttribute[];
					if (attrs.Length == 0) continue;
					_handler.Add(attrs[0].ParserType, i);
				}
			}
		}

		public static Composite CreateFrom(TreeNode node, ITreeLoader loader)
		{
			if (node is TreeNodeProbabilitySelector)
			{
				var sels = new List<ProbabilitySelection>();
				foreach (var i in node.childs)
				{
					var n = i as TreeNodeProbabilityNode;
					var comp = CreateFrom(i.childs[0], loader);
					var ps = new ProbabilitySelection(comp, n.probability);
					sels.Add(ps);
				}
				return new ProbabilitySelector(sels.ToArray()) { Guid = node.guid };
			}

			var list = new List<Composite>();
			foreach (var i in node.childs)
			{
				list.Add(CreateFrom(i, loader));
			}

			if (node is TreeNodeSequence)
			{
				return new Sequence(list.ToArray()) { Guid = node.guid };
			}
			else if (node is TreeNodeSelector)
			{
				return new PrioritySelector(list.ToArray()) { Guid = node.guid };
			}
			else if (node is TreeNodeParallelSelector)
			{
				return new ParallelPrioritySelector(list.ToArray()) { Guid = node.guid };
			}
			else if (node is TreeNodeParallelSequence)
			{
				return new ParallelSequence(list.ToArray()) { Guid = node.guid };
			}
			else if (node is TreeNodeTick n)
			{
				//var n = node as TreeNodeTick;
				return new DecoratorTick(list[0])
				{
					Node = n,
					Guid = node.guid
				};
			}
			else if (node is TreeNodeNegation)
			{
				return new DecoratorNegation(list[0]) { Guid = node.guid };
			}
			else if (node is TreeNodeReturnSuccss)
			{
				return new DecoratorSuccess(list[0]) { Guid = node.guid };
			}
			else if (node is TreeNodeRunUnitlSuccess)
			{
				return new DecoratorRunUntilSuccess(list[0]) { Guid = node.guid };
			}
			else if (node is TreeNodeRunUnitlFailure)
			{
				return new DecoratorRunUnitlFailure(list[0])
				{
					Guid = node.guid
				};
			}
			else if (node is TreeNodeTickUntilSuccess ns)
			{
				return new DecoratorTickUntilSuccess(list[0])
				{
					Guid = node.guid,
					TickTime = ns.tickTime
				};
			}
			else if (node is TreeNodeBreakTreeAndRunChild)
			{
				return new DecoratonBreakTreeAndRunChild(list[0]) { Guid = node.guid };
			}
			else if (node is TreeNodeCd cd)
  			{
				return new DecoratonCd(list[0]) { Guid = node.guid, Node = cd };
			}
			else if (node is TreeNodeLinkNode linkNode)
			{
				var childNode = loader.Load(linkNode.Path);
				var child = CreateFrom(childNode, loader);
				linkNode.childs.Add(childNode);
				return new DecoratorLinkChild(child) { Guid = node.guid };
			}

			else if (node is TreeNodeBattleEvent ev)
			{
				var t = CreateFrom(node.childs[0], loader);
				return new EventBattle(t) { eventType = ev.DiType, Guid = node.guid };
			}
			else
			{
				return Parse(node);
			}
		}

		private static Composite Parse(TreeNode node)
		{
            if (_handler.TryGetValue(node.GetType(), out Type handle))
            {
                var comp = Activator.CreateInstance(handle, node);
                var t = comp as Composite;
                t.Guid = node.guid;
                return t;
            }
            throw new Exception("Not parsed TreeNode:" + node.GetType());
			//return null;
		}

	}
}

