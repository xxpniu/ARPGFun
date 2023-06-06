using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
    public enum TargetReachType
    {
        MaxDistance,
        DistanceOfTaget
    }

    [EditorLayout("发射自己")]
    public class LaunchSelfLayout: LayoutBase
    {    
        [Label("速度 m/s")]
        public float speed;
        [Label("目标方式")]
        public TargetReachType reachType = TargetReachType.MaxDistance;
        [Label("长度 m")]
        public float distance=5;

        public override string ToString()
        {
            return $"发射自己 {reachType} {speed} m/s";
        }
    }
}
