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

		public GObject(GControllor controller) : this(controller.Perception.State.NextElementID())
		{
			Controller = controller;
		}

		public GControllor Controller { private set; get; }

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
			OnChangedControllor(this.Controller, controllor);
			this.Controller = controllor;
		}

		private bool _hadBeenDestroy = false;

		public bool Enable { private set; get; }

		public bool IsAliveAble => !_hadBeenDestroy;

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

		private DateTime? _destroyTime;

		public bool CanDestroy
		{
			get
			{
				if (this.Enable) return false;
				if (_destroyTime.HasValue)
				{
					return _destroyTime.Value < DateTime.Now;
				}
				return true;
			}
		}

		internal static void JoinState(GObject el)
		{
			if (el._hadBeenDestroy) return;
			el.Enable = true;
			el.OnJoinState();
		}

		internal static void ExitState(GObject el)
		{
			el.OnExitState();
		}

		public static void Destroy(GObject el, float time = -1f)
		{
			if (time > 0) el._destroyTime = DateTime.Now.AddSeconds(time);
			el._hadBeenDestroy = true;
			if (el.Enable) el.Enable = false;
		}

		public static implicit operator bool(GObject el)
		{
			return el is { Enable: true };
		}
		
		
		 
	}
	
}

