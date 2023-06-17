using System;
using System.Reflection;
using UnityEngine;

namespace App.Core.Core
{
    [AttributeUsage( AttributeTargets.Class)]
    public class DestroyOnLoadAttribute : Attribute
    {
    }
	
    public class XSingleton<T> : MonoBehaviour where T : MonoBehaviour, new()
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
            Destroy(_instance.gameObject);
            _instance = null;
        }

        /// <summary>
        /// See as Singleton
        /// </summary>
        /// <value>The s.</value>
        public static T S => Singleton;
    
        protected virtual void OnDestroy()
        {
            Debug.Log($"{this.GetType()} be destroy");
        }

        protected virtual void Awake()
        {
            if (_instance)
            {
                Debug.LogError($"Had create {_instance}");
            }
            else
            {
                Debug.Log($"Awake Singleton:{this.GetType().FullName}");
            }
            var att = typeof(T).GetCustomAttribute<DestroyOnLoadAttribute>();
            if (att == null) DontDestroyOnLoad(gameObject);
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