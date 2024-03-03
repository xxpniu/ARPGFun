using System;
using System.Reflection;
using UnityEngine;

namespace App.Core.Core
{
    [AttributeUsage( AttributeTargets.Class)]
    public class DestroyOnLoadAttribute : Attribute
    {
    }
    [AttributeUsage( AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        public string Name { set; get; }
        public NameAttribute(string name) => Name = name;
    }
    
    
	
    public class XSingleton<T> : MonoBehaviour where T : MonoBehaviour, new()
    {
        private static T _instance;

        public static T Singleton
        {
            get
            {
                if (_instance != null) return _instance;
                var type = typeof(T);
                var name = type.GetCustomAttribute<NameAttribute>()?.Name ?? typeof(T).FullName;
                
                {
                    _instance = FindFirstObjectByType(typeof(T)) as T;
                    if (_instance)
                    {
                        _instance.name = name!;
                        Debug.Log($"Instance from FindFirstObjectByType<{typeof(T).FullName}>()");
                    }
                    
                }
                if (_instance) return _instance;
                Debug.Log($"Instance create from AddComponent<{typeof(T).FullName}>()");
                _instance = new GameObject(name).AddComponent<T>();
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
        
        public static T Try()
        {
            var ins = _instance ? _instance : null;
            return ins;
        }


    }
}