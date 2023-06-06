﻿using System;
using System.Collections.Generic;

namespace EngineCore.Simulater
{
    public abstract class GState
    {
        private int lastIndex = 0;

        public int NextElementID()
        {
            lastIndex++;
            return lastIndex;
        }

        private readonly Dictionary<int, GObject> _elements = new Dictionary<int, GObject>();
        private readonly LinkedList<GObject> _elementList = new LinkedList<GObject>();

        public GPerception Perception { protected set; get; }

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

        public GObject this[int index]
        {
            get
            {
                if (_elements.TryGetValue(index, out GObject outObj))
                {
                    if (outObj.Enable) return outObj;
                }
                return null;
            }
        }

        public void Start(GTime time)
        {
            IsEnable = true;
            this.Tick(time);
        }

        public void Stop(GTime time)
        {
            foreach (var i in _elements)
            {
                GObject.Destroy(i.Value);
            }
            this.Tick(time);
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
                {
                    current.Value.Controllor?.GetAction(time, current.Value)?.Execute(time, current.Value);
                }

                if (!current.Value.Enable && current.Value.CanDestory)
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
            {
                state.Tick(now);
            }
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

        public delegate bool EachCondtion<T>(T el) where T : GObject;

        public void Each<T>(EachCondtion<T> cond) where T : GObject
        {
            foreach (var i in _elementList)
            {
                if (!i.Enable) continue;
                if (i is T t)
                {
                    if (cond(t)) return;
                }
            }
        }

        public bool IsEnable { get; private set; } = false;

    }
}

