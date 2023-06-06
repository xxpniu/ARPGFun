using System;

namespace Layout.EditorAttributes
{
	
	public class EditorEffectAttribute :Attribute
	{
		public string Name { get; private set; }
		public EditorEffectAttribute (string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
	public class EffectIdAttribute : Attribute
	{
		public EffectIdAttribute(int id) {
			this.ID = id;
		}

		public int ID { get; set; }
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ParamIndexAttribute : Attribute
	{
		public ParamIndexAttribute(int index) {
			this.Index = index;
		}

		public int Index { get; set; }
	}
	public class EditorEffectsAttribute : Attribute
	{ }
}

