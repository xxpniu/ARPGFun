using System;
using Layout.EditorAttributes;
//using System.Xml;

namespace Layout.LayoutElements
{
	public enum DamageType
	{
		Single=0,//单体
		//Rangle = 1 ,
		Area = 1
	}

	public enum FilterType
	{
		ALL=0,
		OwnerTeam, //自己队友
		EmenyTeam, //敌人队伍
		Alliance   //联盟队伍 
	}
	public class DamageRange
	{
		[Label("伤害筛选类型")]
		public DamageType damageType= DamageType.Area;
		[Label("过滤方式(释放者为过滤源)")]
		public FilterType fiterType = FilterType.EmenyTeam;
		[Label("半径 m")]
		public float radius = 1;
		[Label("范围角度方向")]
		public float angle = 360;
		[Label("方向偏移角")]
		public float offsetAngle =0;
		[Label("偏移向量 m")]
		public Vector3 offsetPosition = Vector3.zero;
        public override string ToString()
        {
			if (damageType == DamageType.Single)
				return $"{damageType}";
			return $"{damageType} R:{radius} offsetAngle:{ offsetAngle} of Angle:{angle}";
        }
    }

	[EditorLayout("目标判定")]
	public class DamageLayout:LayoutBase
	{
		[Label("目标")]
		public TargetType target = TargetType.Target;

        [Label("范围")]
		public DamageRange RangeType = new DamageRange();

		[Label("执行的效果组Key")]
		public string effectKey;

		public override string ToString ()
		{
			return string.Format ("目标{0} 范围{1} 效果 {2}",target , RangeType, effectKey);
		}
	}
}

