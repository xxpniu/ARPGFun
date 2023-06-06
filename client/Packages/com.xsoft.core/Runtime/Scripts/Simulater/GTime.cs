using System;

namespace EngineCore.Simulater
{
	public interface ITimeSimulater
	{
		GTime Now { get; }
	}

	public struct GTime
	{
		public GTime(float time, float delta)
		{
			timeNow = time;
			DeltaTime = delta;
		}

		private double timeNow;
		public float Time { get { return (float)timeNow; } }
		public float DeltaTime { get; set; }

		public void TickTime(float delta)
		{
			DeltaTime = delta;
			timeNow += delta;
		}

		public static double operator -(GTime t, GTime t1)
		{
			return t.timeNow - t1.timeNow;
		}

		public static double operator +(GTime t, float t1)
		{
			return t.timeNow + t1;
		}

		public static implicit operator double(GTime t)
		{
			return t.timeNow;
		}

		public static implicit operator float(GTime t)
		{
			return (float)t.timeNow;
		}

		public static double operator -(GTime t, float t1)
		{
			return t.timeNow - t1;
		}

		public override string ToString()
        {
            return string.Format("({0:0.0},{1:0.00})",Time,DeltaTime);
        }
	}
}

