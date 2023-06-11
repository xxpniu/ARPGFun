using System;
using Layout.EditorAttributes;
using Proto;

namespace Layout.LayoutElements
{

    [EditorLayout("召唤单位")]
    public class CallUnitLayout:LayoutBase
    {

        [Label("召唤角色ID")]
        public ValueSourceOf characterID;

        [Label("等级")]
        public ValueSourceOf level=1;

        [Label("AIPath(默认角色表AI)")]
        [EditorStreamingPath]
        public string AIPath;

        [Label("持续时间(ms)")]
        public ValueSourceOf time=1000;

        [Label("召唤物最大数量")]
        public int maxNum;

        [Label("相对于释放者偏移")]
        public Vector3 offset = new(0,0,1);

        public override string ToString()
        {
            return $"time:{time} ID:{characterID}";
        }
    }
}
