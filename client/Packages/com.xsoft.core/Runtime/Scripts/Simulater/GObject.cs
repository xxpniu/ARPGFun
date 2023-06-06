using System;
using System.Collections.Generic;

namespace EngineCore.Simulater
{
	public abstract class GObject
	{

		public int Index { private set; get; }

		private readonly Dictionary<string, object> values = new Dictionary<string, object>();

		private GObject(int index)
		{
			this.Index = index;
		}

		public GObject(GControllor controllor) : this(controllor.Perception.State.NextElementID())
		{
			Controllor = controllor;
		}

		public GControllor Controllor { private set; get; }

        public object this[string key]
        {
			set
            {
				values.Remove(key);
				values[key] = value;
            }
			get
            {
				if (values.TryGetValue(key, out object v)) return v;
				return null;
            }
        }

		public void Clear()
        {
			values.Clear();
        }

		public void SetControllor(GControllor controllor)
		{
			OnChangedControllor(this.Controllor, controllor);
			this.Controllor = controllor;
		}

		private bool HadBeenDestory = false;

		public bool Enable { private set; get; }

		public bool IsAliveAble { get { return !HadBeenDestory; } }

		#region Events

		protected virtual void OnJoinState()
		{

		}

		protected virtual void OnExitState()
		{

		}

		protected virtual void OnChangedControllor(GControllor old, GControllor current)
		{

		}

		#endregion

		private DateTime? destroyTime;

		public bool CanDestory
		{
			get
			{
				if (this.Enable) return false;
				if (destroyTime.HasValue)
				{
					return destroyTime.Value < DateTime.Now;
				}
				return true;
			}
		}

		internal static void JoinState(GObject el)
		{
			if (el.HadBeenDestory) return;
			el.Enable = true;
			el.OnJoinState();
		}

		internal static void ExitState(GObject el)
		{
			el.OnExitState();
		}

		public static void Destroy(GObject el, float time = -1f)
		{
			if (time > 0) el.destroyTime = DateTime.Now.AddSeconds(time);
			el.HadBeenDestory = true;
			if (el.Enable) el.Enable = false;
		}

		public static implicit operator bool(GObject el)
		{
			if (el == null) return false;
			if (!el.Enable) return false;
			return true;
		}
	}
}

