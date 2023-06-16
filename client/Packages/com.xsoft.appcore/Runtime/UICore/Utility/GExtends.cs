using App.Core.Core;
using UnityEngine;
using UnityEngine.UI;

namespace App.Core.UICore.Utility
{
    public static class GExtends
	{

        public static void ActiveSelfObject(this Component com, bool active)
        {
            com.gameObject.SetActive(active);
        }


        public static void SetLayer(this GameObject u, string layer)
        {
            u.transform.SetLayer(LayerMask.NameToLayer(layer));
        }
        public static T TryAdd<T>(this GameObject obj) where T:Component
        {
            if (obj.TryGetComponent(out T c)) return c;
            return obj.AddComponent<T>();
        }

        public static T TryAdd<T>(this Component obj) where T : Component
        {
            return TryAdd<T>(obj.gameObject);
        }

        public static void RestRTS(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            trans.localRotation = Quaternion.identity;
        }

        public static void SetLayer(this Transform trans ,int layer)
        {
            trans.gameObject.layer = layer;
            foreach (var i in trans.GetComponentsInChildren<Transform>()) i.gameObject.layer = layer;
        }

        public static T FindChild<T> (this Transform trans, string name) where T :Component
		{
			var t = FindInAllChild (trans, name);
			if (t == null) return null;
			else return t.GetComponent<T>();
		}

        private static Transform FindInAllChild(Transform trans, string name)
        {
            if (trans.name == name) { return trans; }

            for (var i = 0; i < trans.childCount; i++)
            {
                var t = FindInAllChild(trans.GetChild(i), name);
                if (t != null) return t;
            }
            return null;
        }


		

        public static void DrawSphere(Vector3 center, float m_Radius, Vector3 forward)
        {

            // 绘制圆环
            Vector3 beginPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.2f)
            {
                float x = m_Radius * Mathf.Cos(theta);
                float z = m_Radius * Mathf.Sin(theta);
                Vector3 endPoint = new Vector3(x, 0, z);
                if (theta == 0)
                {
                    firstPoint = endPoint;
                }
                else
                {
                    Gizmos.DrawLine(beginPoint + center, endPoint + center);
                }
                beginPoint = endPoint;
            }

            // 绘制最后一条线段
            Gizmos.DrawLine(firstPoint + center, beginPoint + center);
            Gizmos.DrawLine(center, center + forward * m_Radius);
        }

        public static void SetText(this Button bt, string text)
        {
            var t = bt.transform.FindChild<Text>("Text");
            if (t == null) return;
            t.text = text;
        }

        public static void SetKey(this Button bt, string key, params object[] pars)
        {
            var t = bt.transform.FindChild<Text>("Text");
            if (t == null) return;
            t.SetKey(key, pars);
        }

        public static void SetKey(this Text t, string key, params object[] pars)
        {
            t.text = LanguageManager.S.Format(key, pars);
        }

    }
}
