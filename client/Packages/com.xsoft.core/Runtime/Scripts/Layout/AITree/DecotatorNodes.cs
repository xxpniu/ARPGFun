using System;
using Layout.EditorAttributes;

namespace Layout.AITree
{
	[EditorAITreeNode("结果取反", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeNegation : TreeNode{}
	[EditorAITreeNode("结果始终为Success", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeReturnSuccss : TreeNode { }
	[EditorAITreeNode("运行直到返回Failure", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeRunUnitlFailure : TreeNode { }
	[EditorAITreeNode("运行直到返回Success",  "Dec","修饰节点", AllowChildType.One)]
	public class TreeNodeRunUnitlSuccess : TreeNode { }
	[EditorAITreeNode("间隔时间执行", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeTick : TreeNode {
		[Label("间隔时间(ms)")]
		public FieldValue tickTime=1000;
	}
	[EditorAITreeNode("间隔时间执行直到返回Success", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeTickUntilSuccess : TreeNode 
	{
		[Label("间隔时间(ms)")]
		public FieldValue tickTime =1000;
	}

	[EditorAITreeNode("终止树并启动子树", "Break", "修饰节点", AllowChildType.One)]
	public class TreeNodeBreakTreeAndRunChild:TreeNode{ }

	[EditorAITreeNode("CD执行", "Dec", "修饰节点", AllowChildType.One)]
	public class TreeNodeCd:TreeNode
	{
		public FieldValue CdTime = 100;
	}
}

