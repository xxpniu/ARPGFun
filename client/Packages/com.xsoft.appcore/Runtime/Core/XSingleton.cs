using Core.Components;
using UnityEngine;

namespace Core
{
	public class XSingleton<T> : ComponentAsync where T : ComponentAsync, new()
	{
		private static T _instance;

		public static T Singleton
		{
			get
			{
				if (!_instance) _instance = FindObjectOfType(typeof(T)) as T;
				if (!_instance) _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
				return _instance;
			}
		}


		public void Reset()
		{
			if (!_instance) return;
			GameObject.Destroy(_instance.gameObject);
			_instance = null;
		}

		/// <summary>
		/// See as Singleton
		/// </summary>
		/// <value>The s.</value>
		public static T S => Singleton;

		protected virtual void Awake()
		{
			if (_instance)
			{
				Debug.LogError($"Had create {_instance}");
			}
        
			_instance = this as T;
			DontDestroyOnLoad(this.gameObject);
		}

		public static (bool, T) TryGet()
		{
			return _instance ? (true, _instance) : (false, null);
		}

		/// <summary>
		/// get don't auto create
		/// </summary>
		/// <returns></returns>
		public static T G()
		{
			return _instance ? _instance : null;
		}
	}
}

