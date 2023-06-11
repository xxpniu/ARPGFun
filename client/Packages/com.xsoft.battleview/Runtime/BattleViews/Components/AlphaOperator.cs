using System.Collections.Generic;
using UnityEngine;

namespace BattleViews.Components
{
    public class AlphaOperator : MonoBehaviour
    {
        private struct RevertShader
        {
            public Renderer Renderer;
            public Shader Shader;
        }

        private readonly Queue<RevertShader> _renders = new Queue<RevertShader>();

        private void OnEnable()
        {
            var shader = Shader.Find("Shader Graphs/AlphaCharacter");
            foreach (var i in this.transform.GetComponentsInChildren<Renderer>())
            {
                var r = new RevertShader { Renderer = i, Shader = i.material.shader };
                _renders.Enqueue(r);
                r.Renderer.material.shader = shader;
            }
        }

        private void OnDisable()
        {
            while (_renders.Count > 0)
            {
                var t = _renders.Dequeue();
                t.Renderer.material.shader = t.Shader;
            }
        }

        public static AlphaOperator Operator(GameObject root)
        {
            var a = root.GetComponent<AlphaOperator>();
            return a ? a : root.AddComponent<AlphaOperator>();
        }
    }
}
