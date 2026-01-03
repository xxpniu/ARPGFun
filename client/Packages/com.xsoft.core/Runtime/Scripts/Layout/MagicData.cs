using System.Collections.Generic;
using Layout.EditorAttributes;

namespace Layout
{
    public class MagicData
    {
        //[Label("事件")]
        public List<EventContainer> Containers;

        [Label("名称")] public string name;

        [Label("触发间隔时间")] public float triggerTicksTime;

        [Label("唯一(唯一不允许多个释放实例)")] public bool unique = false;

        public MagicData()
        {
            Containers = new List<EventContainer>();
            triggerTicksTime = -1;
        }
    }
}