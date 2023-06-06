using System;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
	[EditorLayout("看向目标",PType = PlayType.BOTH)]
	public class LookAtTarget:LayoutBase
	{
		public override string ToString()
		{
			return $"看向目标";
		}
	}
}

