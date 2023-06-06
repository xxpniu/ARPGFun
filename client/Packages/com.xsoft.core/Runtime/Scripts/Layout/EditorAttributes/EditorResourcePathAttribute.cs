﻿using System;

namespace Layout.EditorAttributes
{
	[AttributeUsage(AttributeTargets.Field)]
	//用来显示选择资源编辑
	public class EditorResourcePathAttribute:Attribute
	{

		public EditorResourcePathAttribute()
		{}
	}

	[AttributeUsage(AttributeTargets.Field)]
	//用来显示选择资源编辑
	public class EditorStreamingPathAttribute : Attribute
	{

	}
}

