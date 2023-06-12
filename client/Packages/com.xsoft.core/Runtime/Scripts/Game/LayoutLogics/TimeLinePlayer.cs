using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using EngineCore.Simulater;
using Layout;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Game.LayoutLogics
{
	public abstract class TimeLinePlayerBase
	{
		public int Index {  set; get; }
		public TimeLinePlayerBase(TimeLine timeLine, int index)
		{
			this.Index = index;
			Line = timeLine;
		}

		private readonly Queue<TimePoint> NoActivedPoints = new Queue<TimePoint>();
		public TimeLine Line { private set; get; }
		public bool Tick(GTime time)
		{
			if (PlayTime < 0)
			{
				PlayTime = ToTime>0?ToTime:0;
				var orpoint = Line.Points.OrderBy(t => t.Time).ToList();
				NoActivedPoints.Clear();
				foreach (var i in orpoint)
				{
					if (i.Time < ToTime) continue;
					NoActivedPoints.Enqueue(i);
				}
				return false;
			}

			while (NoActivedPoints.Count > 0
				&& NoActivedPoints.Peek().Time < PlayTime)
			{
				var point = NoActivedPoints.Dequeue();
				var layout = Line.FindLayoutByGuid(point.GUID);
				EnableLayout(layout);
			}
			IsFinshed = PlayTime >= Line.Time;
			PlayTime +=time.DeltaTime ;
			return IsFinshed;
		}

		protected abstract void EnableLayout(LayoutBase layout);

		public bool IsFinshed { get; private set; } = false;

		protected virtual void OnDestroy() { }

		public void Destory() { this.OnDestroy(); }

		public float PlayTime { get; private set; } = -1f;
		private float ToTime = -1;

		private int currentRepeatTime = 0;


		public void Repeat(int maxTimes,float toTime)
		{
			if (maxTimes != -1 && currentRepeatTime >= maxTimes) return;
			PlayTime = -1;
			currentRepeatTime++;
			ToTime = toTime;
		}
	}

	public class TimeLinePlayer : TimeLinePlayerBase
	{
        

		public TimeLinePlayer(int pIndex, TimeLine timeLine,
            MagicReleaser releaser,
            EventContainer eventType,
            BattleCharacter eventTarget) : base( timeLine, pIndex)
		{
			this.Index = pIndex;
			this.Releaser = releaser;
			this.TypeEvent = eventType;
			this.EventTarget = eventTarget;
		}
		public EventContainer TypeEvent { private set; get; }
		public MagicReleaser Releaser { private set; get; }
        public BattleCharacter EventTarget { set; get; }

		protected override void EnableLayout(LayoutBase layout)
		{
			if (LayoutBase.IsLogicLayout(layout))
				LayoutBaseLogic.EnableLayout(layout, this);
		}
	}

}

