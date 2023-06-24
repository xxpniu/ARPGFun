using System;

namespace EngineCore.Simulater
{
	public interface ITimeSimulator
	{
		GTime Now { get; }
	}

	public struct GTime
	{
		public GTime(float time, float delta)
		{
			_timeNow = time;
			DeltaTime = delta;
		}

		private double _timeNow;
		public float Time => (float)_timeNow;
		public float DeltaTime { get; set; }

		public void TickTime(float delta)
		{
			DeltaTime = delta;
			_timeNow += delta;
		}

		public static double operator -(GTime t, GTime t1)
		{
			return t._timeNow - t1._timeNow;
		}

		public static double operator +(GTime t, float t1)
		{
			return t._timeNow + t1;
		}

		public static implicit operator double(GTime t)
		{
			return t._timeNow;
		}

		public static implicit operator float(GTime t)
		{
			return (float)t._timeNow;
		}

		public static double operator -(GTime t, float t1)
		{
			return t._timeNow - t1;
		}

		public override string ToString()
        {
            return $"({Time:0.0},{DeltaTime:0.00})";
        }
	}
}

