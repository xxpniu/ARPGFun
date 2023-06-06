using System;
using System.Xml.Serialization;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    [EditorEffect("传送目标")]
    [EffectId(1)]
    public class TransportEffect:EffectBase
    {
        public enum TranportValueOf
        {
            ReleaseTargetPos,
            Value
        }

        public TransportEffect()
        {
        }

        [Label("取值方式")]
        public TranportValueOf ValueOf = TranportValueOf.ReleaseTargetPos;

        [HideInEditor]
        public float x, y, z = 0;

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
            get
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
