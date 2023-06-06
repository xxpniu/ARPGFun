using System.Collections.Generic;
using UnityEngine;

namespace BattleViews.Components
{
    public class AlphaOperator : MonoBehaviour
    {

        public struct RevertShader
        {
            public Renderer renderer;
            public Shader shader;
        }

        private readonly Queue<RevertShader> renders = new Queue<RevertShader>();

        private void OnEnable()
        {
            var shader = Shader.Find("ARPG/alpha");
            foreach (var i in this.transform.GetComponentsInChildren<Renderer>())
            {
                var r = new RevertShader { renderer = i, shader = i.material.shader };
                renders.Enqueue(r);
                r.renderer.material.shader = shader;
            }
        }

        private void OnDisable()
        {
            while (renders.Count > 0)
            {
                var t = renders.Dequeue();
                t.renderer.material.shader = t.shader;
            }
        }

        public static AlphaOperator Operator(GameObject root)
        {
            var a = root.GetComponent<AlphaOperator>();
            if (a) return a;
            return root.AddComponent<AlphaOperator>();
        }
    }
}
