using System;
using Layout.AITree;
using Layout.EditorAttributes;

namespace Layout.AITree
{
	public enum CompareType
	{
		Less,
		Greater,
		Equal
	}

	public enum OperatorType
	{
        Clear = 0,
        Reset = 1,
        Add = 2,
        Minus = 3
    }

	[EditorAITreeNode("选择节点","Sel","基础节点")]
	public class TreeNodeSelector :TreeNode{}
	[EditorAITreeNode("顺序节点", "Seq", "基础节点")]
	public class TreeNodeSequence : TreeNode { }
	[EditorAITreeNode("并行选择节点", "PSel", "基础节点")]
	public class TreeNodeParallelSelector : TreeNode { }
	[EditorAITreeNode("并行顺序节点", "PSeq", "基础节点")]
	public class TreeNodeParallelSequence : TreeNode { }
	[EditorAITreeNode("分段概率选择节点", "PRSel", "基础节点", AllowChildType.Probability)]
	public class TreeNodeProbabilitySelector : TreeNode { }
	[EditorAITreeNode("分段概率子节点", "PRNode", "基础节点", AllowChildType.One)]
	public class TreeNodeProbabilityNode : TreeNode
    {
		[Label("概率")]
		public int probability = 1;
	}
	[EditorAITreeNode("链接子树", "Link", "基础节点", AllowChildType.None)]
	public class TreeNodeLinkNode : TreeNode
	{
		[Label("子树")]
        [EditorStreamingPath]
		public string Path ;
    }
	[EditorAITreeNode("切换行为树", "Act", "基础节点", AllowChildType.None)]
	public class TreeNodeChangeAITree : TreeNode
	{
		[Label("行为树")]
		[EditorStreamingPath]
		public string Path;
	}
	[EditorAITreeNode("比较黑板值(Int)", "Cond", "基础节点", AllowChildType.None)]
	public class TreeNodeCompareIntKey : TreeNode
	{
		[Label("黑板 Key")]
		public string Key = string.Empty;
		[Label("比较方式")]
		public CompareType compareType = CompareType.Equal;
		[Label("比较值")]
		public FieldValue CompareValue = 0;
    }

	[EditorAITreeNode("操作黑板值(Int)", "Act", "基础节点", AllowChildType.None)]
	public class TreeNodeSetIntKey : TreeNode
	{
		[Label("黑板 Key")]
		public string Key = string.Empty;
		[Label("比较方式")]
		public OperatorType operatorType = OperatorType.Reset;
		[Label("操作值")]
		public FieldValue OperatorValue = 0;
	}
}

