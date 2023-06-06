using System;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
	public enum MovementType
	{
		Line =0,//直线
		Paracurve =1,//抛物线
		Follow =2,//跟踪
        AutoTarget =3//自动跟踪目标
	}


	[EditorLayout("子物体发射器")]
	public class MissileLayout:LayoutBase
	{
		public MissileLayout ()
		{
			maxLifeTime = -1;
			maxDistance = -1;
			offset = new Vector3(0, 0, 0);
		}

		[Label("子物体资源目录")]
		[EditorResourcePath]
		public string resourcesPath;
		[Label("轨迹类型")]
		public MovementType movementType;
		[Label("最大存活时间 s")]
		public float maxLifeTime;
		[Label("最大运动距离 s")]
		public float maxDistance;
		[Label("速度 m/s")]
		public float speed;
		[Label("起始骨骼")]
		[EditorBone]
		public string fromBone;
		[Label("目标骨骼")]
		[EditorBone]
		public string toBone;
		[Label("偏移量 m")]
		public Vector3 offset;

		public override string ToString ()
		{
			return string.Format ("轨迹{0} 速度{1} 资源 {2}",movementType , speed, resourcesPath);
		}
	}
}

