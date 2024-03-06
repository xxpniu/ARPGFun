using System;
using System.Xml.Serialization;
using Layout.EditorAttributes;
using Proto;

namespace Layout.AITree
{

	public enum TargetSelectType
	{
		Nearest,
        ForwardNearest,
		Random,
		HPMax,
		HPMin,
		HPRateMax,
		HPRateMin
	}

	public enum TargetFilterType
	{
		None,
		Hurt
	}

	public enum BattleEventType
	{
        NONE =0 ,
        TeamBeAttack =1,//队友被攻击
		Hurt =2,  //被伤害
        Killed =4, //杀死对手
        Death =8, // 死亡
		Skill =16, //释放技能
		Move =32, //移动
		NormalAttack = 64,//普通攻击
		TeamChanged = 128 //队伍变化
		
    }

	[EditorAITreeNode("战斗事件", "Event", "战斗节点", AllowChildType.One)]
	public class TreeNodeBattleEvent: TreeNode
	{
        [HideInEditor]
        [Obsolete]
		public BattleEventType eventType;

		[Label("监听事件")]
		[EnumMasker]
		[XmlIgnore]
		public BattleEventType DType 
		{ 
			set => DiType = (int)value;
			get => (BattleEventType)DiType;
		}

		[HideInEditor]
		public int DiType = 0;
	}

	[EditorAITreeNode("看向目标", "Act", "战斗节点/目标", AllowChildType.None)]
	public class TreeNodeLookAtTarget : TreeNode
	{
	}

	[EditorAITreeNode("查找目标", "Act", "战斗节点/目标",AllowChildType.None)]
	public class TreeNodeFindTarget:TreeNode
	{
		[Label("取值来源")]
		public DistanceValueOf valueOf;
		[Label("距离 cm")]
		public FieldValue Distance = 0;

		[Label("距离修正倍数 (万分比)")]
		public FieldValue ValueMul = 10000;

		[Label("视野(0-360)")]
		public FieldValue View = 360;

		[Label("挑选方式")]
		public TargetSelectType selectType;

		[Label("过滤方式")]
		public TargetFilterType filter;

        [Label("过滤方式使用魔法配置表")]
        public bool useMagicConfig;

		[Label("阵营类型")]
		public TargetTeamType teamType;

        [Label("重新查找")]
        public bool findNew = true;

	}

	[EditorAITreeNode("等待时间", "Act", "战斗节点", AllowChildType.None)]
	public class TreeNodeWaitForSeconds : TreeNode
	{
		[Label("等待毫秒")]
		public FieldValue seconds = 1000;
	}

	public enum MagicValueOf
	{ 
	    BlackBoard,
		MagicKey
	}

	[EditorAITreeNode("释放技能", "Act", "战斗节点/技能", AllowChildType.None)]
	public class TreeNodeReleaseMagic : TreeNode
	{
		[Label("取值来源")]
		public MagicValueOf valueOf = MagicValueOf.MagicKey;

		[Label("魔法Key")]
		public string magicKey = string.Empty;

		[Label("目标（获取为目标点或者目标位置）")]
		public bool ReleaseATPos = false;
		
	}

	public enum DistanceValueOf
	{
		BlackboardMagicRangeMin,
		BlackboardMagicRangeMax,
		Value,
        ViewDistance
	}
	
	[EditorAITreeNode("判断目标距离", "Cond", "战斗节点/目标", AllowChildType.None)]
	public class TreeNodeDistancTarget : TreeNode
	{
		[Label("取值来源")]
		public DistanceValueOf valueOf = DistanceValueOf.Value;

		[Label("距离(cm)")]
		public FieldValue distance = 100;
		[Label("距离修正倍数 (万分比)")]
		public FieldValue ValueMul = 10000;

		[Label("比较类型")]
		public CompareType compareType = CompareType.Less;
	}

	[EditorAITreeNode("判断距离出生点", "Cond", "战斗节点/目标", AllowChildType.None)]
	public class TreedNodeDistanceBornPos : TreeNode
    {
		[Label("取值来源")]
		public DistanceValueOf valueOf = DistanceValueOf.Value;
		[Label("距离(cm)")]
		public FieldValue distance = 100;
		[Label("比较类型")]
		public CompareType compareType = CompareType.Less;

	
	}

	public enum MagicResultType
	{
		Random,
		Frist,
		Sequence //顺序以此
	}

	[EditorAITreeNode("选择可释放魔法", "Act", "战斗节点/技能", AllowChildType.None)]
	public class TreeNodeSelectCanReleaseMagic : TreeNode
	{
        [Label("魔法类型")]
		public MagicType MTpye = MagicType.MtMagic;
		[Label("魔法选择类型")]
		public MagicResultType resultType;
	}

	[EditorAITreeNode("移动远离目标", "Act", "战斗节点/移动", AllowChildType.None)]
	public class TreeNodeFarFromTarget : TreeNode
	{
		[Label("取值方式")]
		public DistanceValueOf valueOf = DistanceValueOf.Value;
		[Label("远离距离(cm)")]
		public FieldValue distance = 100;
	}

	[EditorAITreeNode("出生点范围内移动", "Act", "战斗节点/移动", AllowChildType.None)]
	public class TreeNodeAroundBornPosMove : TreeNode
	{
		[Label("半径取值方式")]
		public DistanceValueOf Value = DistanceValueOf.Value;
		[Label("半径(cm)")]
		public FieldValue distance = 100;
    }

	[EditorAITreeNode("比较目标数", "Cond", "战斗节点/目标", AllowChildType.None)]
	public class TreeNodeCompareTargets : TreeNode
	{

		[Label("阵营类型")]
		public TargetTeamType teamType;

		[Label("距离取值来源")]
		public DistanceValueOf valueOf = DistanceValueOf.Value;

	    [Label("距离(cm)")]
		public FieldValue Distance =100;

		[Label("距离修正倍数 (万分比)")]
		public FieldValue ValueMul = 10000;

		[Label("比较值(cm)")]
		public FieldValue compareValue=0;

		[Label("比较类型")]
		public CompareType compareType;

		

	}

	[EditorAITreeNode("靠近目标", "Act", "战斗节点/移动", AllowChildType.None)]
	public class TreeNodeMoveCloseTarget : TreeNode
	{
		[Label("停止取值方式")]
		public DistanceValueOf valueOf = DistanceValueOf.Value;
		[Label("停止距离(cm)")]
		public FieldValue distance = 100;
	}

	[EditorAITreeNode("随机移动", "Act", "战斗节点/移动", AllowChildType.None)]
	public class TreeNodeMoveRandom : TreeNode
	{
		[Label("偏移方向")]
		public FieldValue Forward = 0;
		[Label("移动距离(cm)")]
		public FieldValue distance = 100;
	}

	[EditorAITreeNode("靠近出生点", "Act", "战斗节点/移动", AllowChildType.None)]
	public class TreeNodeMoveToBronPosition : TreeNode
	{
		[Label("停止距离(cm)")]
		public FieldValue distance = 100;
	}

	[EditorAITreeNode("设置传送目标", "Act", "传送点/设置传送目标", AllowChildType.None)]
	public class TreeNodeTransport : TreeNode
	{
		//link pos
		[Label("目标坐标")]
		public Vector3 linkPos = Vector3.zero;
	}
}

