using System.Collections.Generic;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
    public class TimeLine
    {
        public List<LayoutBase> Layouts;


        public List<TimePoint> Points;

        [Label("持续时间", "单位秒(s)")] public float Time;

        public TimeLine()
        {
            Points = new List<TimePoint>();
            Layouts = new List<LayoutBase>();
            Time = 1f;
        }

        public LayoutBase FindLayoutByGuid(string guid)
        {
            foreach (var i in Layouts)
                if (i.GUID == guid)
                    return i;
            return null;
        }

        public T FindLayoutByGuid<T>(string guid) where T : LayoutBase
        {
            return FindLayoutByGuid(guid) as T;
        }

        public void RemoveByGuid(string guid)
        {
            Points.RemoveAll(obj => { return obj.GUID == guid; });

            foreach (var i in Layouts)
                if (i.GUID == guid)
                {
                    Layouts.Remove(i);
                    break;
                }
        }

        public List<TimePoint> FindPointByGuid(string guid)
        {
            var result = new List<TimePoint>();
            foreach (var i in Points)
                if (i.GUID == guid)
                    result.Add(i);
            return result;
        }
    }

    public class TimePoint
    {
        [Label("对应的Layout")] public string GUID;

        [Label("时间点")] public float Time;
    }
}