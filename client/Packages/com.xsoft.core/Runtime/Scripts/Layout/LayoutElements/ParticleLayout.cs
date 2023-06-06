using System;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
	public enum ParticleDestoryType
	{
		Normal,
		LayoutTimeOut,
		Time
	}

	[EditorLayout("粒子播放器", PType = PlayType.View)]
	public class ParticleLayout:LayoutBase
	{

		[Label("资源","客户端显示的资源目录")]
		[EditorResourcePath]
		public string path;

		[Label("开始对象")]
		public TargetType fromTarget;
		[Label("目标对象")]
		public TargetType toTarget;
		[Label("起始骨骼","绑定目标骨骼")]
		[EditorBone]
		public string fromBoneName;

		[Label("目标骨骼","绑定目标骨骼")]
		[EditorBone]
		public string toBoneName;

        [Label("绑定骨骼")]
        public bool Bind;

		[Label("销毁类型")]
		public ParticleDestoryType destoryType;
      
		[Label("销毁时间 s")]
		public float destoryTime;
		[Label("偏移")]
		public Vector3 offet= new Vector3();
		[Label("旋转")]
		public Vector3 rotation = new Vector3();

		[Label("大小")]
		public float localsize = 1;
		public override string ToString ()
		{
			return string.Format ("资源{0}",path);
		}

	}
}

