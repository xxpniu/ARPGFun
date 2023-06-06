using System;
using Layout.EditorAttributes;
using System.Xml.Serialization;

namespace Layout.LayoutElements
{
	[
		XmlInclude(typeof(MissileLayout)),
		XmlInclude(typeof(MotionLayout)),
		XmlInclude(typeof(DamageLayout)),
		XmlInclude(typeof(ParticleLayout)),
		XmlInclude(typeof(LookAtTarget)),
        XmlInclude(typeof(CallUnitLayout)),
		XmlInclude(typeof(PlaySoundLayout)),
        XmlInclude(typeof(LaunchSelfLayout)),
        XmlInclude(typeof(RepeatTimeLine))
	]
	public class LayoutBase
	{
		[HideInEditor]
		public string GUID;

		public static T CreateInstance<T> ()where T: LayoutBase, new()
		{
			var t = new T
			{
				GUID = Guid.NewGuid().ToString()
			};
			return t;
		}

		public static LayoutBase CreateInstance(Type t)
		{
			var instance = Activator.CreateInstance (t) as LayoutBase;
			instance.GUID = Guid.NewGuid ().ToString ();
			return instance;
		}

		public static bool IsViewLayout(LayoutBase layout)
		{
			if (layout.GetType().GetCustomAttributes(typeof(EditorLayoutAttribute), false) is EditorLayoutAttribute[] att)
			{
				if (att == null || att.Length == 0)
                    throw new Exception($"no found EditorLayoutAttribute in type {layout.GetType()}");
				return (att[0].PType & PlayType.View)>0;
            }
			return false;
		}

		public static bool IsLogicLayout(LayoutBase layout)
		{
			if (layout.GetType().GetCustomAttributes(typeof(EditorLayoutAttribute), false) is EditorLayoutAttribute[] att)
			{
				if (att == null || att.Length == 0)
					throw new Exception($"no found EditorLayoutAttribute in type {layout.GetType()}");
				return (att[0].PType & PlayType.Logic) > 0;
			}
			return false;
		}
	}
}

