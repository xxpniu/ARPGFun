using System.Xml.Serialization;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    [EditorEffect("传送目标")]
    [EffectId(1)]
    public class TransportEffect : EffectBase
    {
        public enum TranportValueOf
        {
            ReleaseTargetPos,
            Value
        }

        [Label("取值方式")] public TranportValueOf ValueOf = TranportValueOf.ReleaseTargetPos;

        [HideInEditor] public float x, y, z;

        [XmlIgnore]
        [Label("目标位置")]
        public Vector3 TargetPos
        {
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
            get => new(x, y, z);
        }
    }
}