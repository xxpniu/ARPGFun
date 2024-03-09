using System.Collections.Generic;
using Layout.LayoutElements;
using UnityEngine;

namespace BattleViews.Components
{
    public class DamageRangeDebuger : MonoBehaviour
    {
        public static DamageRangeDebuger TryGet(GameObject go)
        {
            var c = go.GetComponent<DamageRangeDebuger>();
            if (c) return c;
            return go.AddComponent<DamageRangeDebuger>();
        }

        public void AddDebug(DamageLayout layout, Vector3 pos, Quaternion rotation)
        {

            _ranges.Add(new DebugOfRange
            {
                Angle = layout.RangeType.angle,
                forward = rotation,
                Pos = pos,
                Radius = layout.RangeType.radius,
                targetsNums = 0,
                time = Time.time + .3f
            });
        }

        private class DebugOfRange
        {
            public Vector3 Pos;
            public Quaternion forward;
            public float Radius;
            public float Angle;
            public float targetsNums;
            public float time;
        }

        private readonly List<DebugOfRange> _ranges = new List<DebugOfRange>();

        public void OnDrawGizmos()
        {
            foreach (var i in _ranges)
            {
                if (i.time < Time.time) continue;
                DrawClire(i.Pos, i.forward, i.Radius, i.Angle);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(i.Pos, string.Format("A{1:0.0}_R{2:0.0}_{0}", i.targetsNums, i.Angle, i.Radius));
#endif
            }
        }

        private void DrawClire(Vector3 pos, Quaternion forward, float r, float a)
        {
            if (a > 360) a = 360;

            var c = Gizmos.color;
            Gizmos.color = Color.red;

            var qu2 = forward * Quaternion.Euler(0, a / 2, 0);
            var qu1 = forward * Quaternion.Euler(0, -a / 2, 0);
            var pos1 = qu1 * Vector3.forward * r + pos;
            var pos2 = qu2 * Vector3.forward * r + pos;
            Gizmos.DrawLine(pos, pos1);
            Gizmos.DrawLine(pos, pos2);
            Vector3 start = pos1;
            for (float i = -a / 2; i < a / 2 - 5;)
            {
                i += 5;
                var diffQu = forward * Quaternion.Euler(0, i, 0);
                var temp = diffQu * Vector3.forward * r + pos;
                Gizmos.DrawLine(start, temp);
                start = temp;
            }
            Gizmos.DrawLine(start, pos2);
            Gizmos.color = c;
        }

    }
}
