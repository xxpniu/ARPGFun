using System;

namespace Layout.EditorAttributes
{
	public enum PlayType
	{
        Logic = 1,
        View = 2,
        BOTH = 3
    }

	[AttributeUsage(AttributeTargets.Class)]
	public class EditorLayoutAttribute:Attribute
	{
		public EditorLayoutAttribute (string name)
		{
			Name = name;
		}

		public string Name{set;get;}

		public PlayType PType { set; get; } = PlayType.Logic;
	}
}

