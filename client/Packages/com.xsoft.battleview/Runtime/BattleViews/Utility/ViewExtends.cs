using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Views;
using UnityEngine;

namespace BattleViews.Utility
{

    public static class ViewExtends
    {

        public static Layout.Vector3 ToLVer3(this Proto.Vector3 v3)
        {
            return new Layout.Vector3(v3.X, v3.Y, v3.Z);
        }

        public  static Layout.Vector3 ToLVer3(this Vector3 v3)
        {
            return new Layout.Vector3(v3.x, v3.y, v3.z);
        }

        public static Vector3 ToGVer3(this Proto.Vector3 v3)
        {
            return new Vector3(v3.X, v3.Y, v3.Z);
        }
        public static UnityEngine.Vector3 ToVer3(this Proto.Vector3 v3)
        {
            return new Vector3(v3.X, v3.Y, v3.Z);
        }

        public static Proto.Vector3 ToPVer3(this Vector3 uv3)
        {
            return new Proto.Vector3() { X = uv3.x, Y = uv3.y, Z = uv3.z };
        }


        public static void LookView(this UCharacterView character, RenderTexture LookAtView)
        {
            var go = new GameObject("Look", typeof(Camera));
            character.transform.SetLayer(LayerMask.NameToLayer("Player"));
            go.transform.SetParent(character.GetBoneByName(UCharacterView.RootBone), false);
            go.transform.RestRTS();
            var c = go.GetComponent<Camera>();
            c.targetTexture = LookAtView;
            c.cullingMask = LayerMask.GetMask("Player");
            go.transform.localPosition = new Vector3(0, 1.1f, 1.5f);
            c.farClipPlane = 5;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(52 / 255f, 44 / 255f, 33 / 255f, 1);
            go.TryAdd<LookAtTargetTransfrom>().target = character.GetBoneByName(UCharacterView.BodyBone);
        }
    }
}
