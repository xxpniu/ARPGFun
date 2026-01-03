using System;
using System.Collections.Generic;

namespace EngineCore.Simulater
{
    public abstract class GState
    {
        public delegate bool EachCondtion<T>(T el) where T : GObject;

        private readonly LinkedList<GObject> _elementList = new();

        private readonly Dictionary<int, GObject> _elements = new();
        private int lastIndex;

        public GPerception Perception { protected set; get; }

        public GObject this[int index]
        {
            get
            {
                if (_elements.TryGetValue(index, out var outObj))
                    if (outObj.Enable)
                        return outObj;
                return null;
            }
        }

        public bool IsEnable { get; private set; }

        public int NextElementID()
        {
            lastIndex++;
            return lastIndex;
        }

        public void Init()
        {
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        public void Pause(bool isPause)
        {
            IsEnable = !isPause;
        }

        public void Start(GTime time)
        {
            IsEnable = true;
            Tick(time);
        }

        public void Stop(GTime time)
        {
            foreach (var i in _elements) GObject.Destroy(i.Value);
            Tick(time);
            IsEnable = false;
        }

        protected virtual void Tick(GTime time)
        {
            if (!IsEnable) return;
            var current = _elementList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.Enable)
                    current.Value.Controller?.GetAction(time, current.Value)?.Execute(time, current.Value);

                if (!current.Value.Enable && current.Value.CanDestroy)
                {
                    GObject.ExitState(current.Value);
                    _elements.Remove(current.Value.Index);
                    _elementList.Remove(current);
                }

                current = next;
            }
        }


        public static void Tick(GState state, GTime now)
        {
            if (state.IsEnable)
                state.Tick(now);
            else throw new Exception("You can't tick a state before you start it.");
        }

        internal bool AddElement(GObject el)
        {
            var temp = el;
            if (_elements.ContainsKey(temp.Index)) return false;
            _elements.Add(temp.Index, temp);
            _elementList.AddLast(temp);
            GObject.JoinState(temp);
            return true;
        }

        public void Each<T>(EachCondtion<T> cond) where T : GObject
        {
            foreach (var i in _elementList)
            {
                if (!i.Enable) continue;
                if (i is T t)
                    if (cond(t))
                        return;
            }
        }
    }
}