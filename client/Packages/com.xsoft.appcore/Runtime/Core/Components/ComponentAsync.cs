using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace App.Core.Core.Components
{
    public class ComponentAsync : MonoBehaviour
    {
        private readonly ConcurrentQueue<AsyncCall> _updateCall = new();

        protected virtual void Update()
        {
            if (_updateCall.Count == 0) return;
            while (_updateCall.TryDequeue(out var c))
            {
                c.Call?.Invoke();
                c.Complete();
            }
        }

        public AsyncCall Invoke(Action call)
        {
            if (call == null) throw new NullReferenceException();
            var asyncCall = new AsyncCall(call);
            _updateCall.Enqueue(asyncCall);
            return asyncCall;
        }

        public struct AsyncCall
        {
            public readonly Action Call;
            public bool IsCompleted { private set; get; }

            public AsyncCall(Action call)
            {
                Call = call;
                IsCompleted = false;
            }

            internal void Complete()
            {
                IsCompleted = true;
            }
        }
    }
}