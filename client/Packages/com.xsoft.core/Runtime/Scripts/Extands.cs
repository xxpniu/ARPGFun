using Proto;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic
{
    public static class Extands
    {
        public static Vector3 ToV3(this Vector3 v3)
        {
            return new Vector3 { X = v3.X, Y = v3.Y, Z = v3.Z };
        }

        public static UVector3 ToUV3(this Vector3 v3)
        {
            return new UVector3(v3.X, v3.Y, v3.Z);
        }

        public static Vector3 ToV3(this Layout.Vector3 v3)
        {
            return new Vector3 { X = v3.x, Y = v3.y, Z = v3.z };
        }

        public static UVector3 ToUV3(this Layout.Vector3 v3)
        {
            return new UVector3(v3.x, v3.y, v3.z);
        }

        public static Vector3 ToPV3(this UVector3 v3)
        {
            return new Vector3 { X = v3.x, Y = v3.y, Z = v3.z };
        }
    }
}