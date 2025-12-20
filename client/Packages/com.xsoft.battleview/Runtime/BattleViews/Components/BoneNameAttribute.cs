using System;

namespace BattleViews.Components
{
	[AttributeUsage( AttributeTargets.Class,AllowMultiple = true)]
	public class BoneNameAttribute:Attribute
	{
		public BoneNameAttribute (string name):this(name,name)
		{
		
		}

		public BoneNameAttribute(string name,string boneName,bool temp = false)
		{
			this.BoneName = boneName;
			this.Name = name;
			Temp = temp;
		}

		public string BoneName{ set; get; }

		public string Name{ set; get;}

		public bool Temp{set;get;}
	}
}


