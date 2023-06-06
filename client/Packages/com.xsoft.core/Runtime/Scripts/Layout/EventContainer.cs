using System;
using Layout.EditorAttributes;
using System.Collections.Generic;
using Layout.LayoutEffects;
using Layout.LayoutElements;
using System.Xml.Serialization;

namespace Layout
{

	/// <summary>
	/// 技能和buff使用一个处理，
	/// 相对来buff的trigger是多次的，如果这个buff设置了间隔时间的话
	/// 
	/// </summary>
	public enum EventType
	{
		EVENT_START, //技能开始
		EVENT_TRIGGER, //技能trigger在start之后启动
		EVENT_ANIMATOR_TRIGGER , //动作的关键帧 （暂时没有启用）
		EVENT_MISSILE_CREATE, //子物体创建的时候
		EVENT_MISSILE_HIT, //子物体碰到目标 
		EVENT_MISSILE_DEAD,//子物体死亡（删除）
		EVENT_UNIT_CREATE,//召唤物创建
		EVENT_UNIT_HIT,//召唤物碰到目标
		EVENT_UNIT_DEAD, //召唤物死亡
		EVENT_END //技能结束
	}

	public class EffectGroup
	{
		public EffectGroup()
		{
			effects = new List<EffectBase> ();
		}

		[EditorEffectsAttribute]
		public List<EffectBase> effects;
		[Label("描述")]
		public string Des;
		[Label("标记")]
		public string key;
	}

	public class EventContainer
	{
		public EventContainer ()
		{
			type = EventType.EVENT_START;
			effectGroup = new List<EffectGroup> ();
		}
		[Label("类型")]	
		public EventType type;
		[Label("事件相应Layout")]
		[LayoutPath]
		public string layoutPath;

		public List<EffectGroup> effectGroup;

		[XmlIgnore]
		public TimeLine line;

		public EffectGroup FindGroupByKey(string key)
		{
			foreach (var i in effectGroup) {
				if (i.key == key)
					return i;
			}
			return null;
		}

	}
}

