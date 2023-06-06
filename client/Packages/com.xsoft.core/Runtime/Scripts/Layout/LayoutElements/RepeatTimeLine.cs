using System;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
    [EditorLayout("重复时间轴", PType = PlayType.BOTH)]
    public class RepeatTimeLine:LayoutBase
    {
      
        [Label("重复次数(-1无限重复)","只有一个有效果")]
        public int RepeatCount = 1;
        [Label("重置时间 s")]
        public float ToTime = -1;
    }
}
