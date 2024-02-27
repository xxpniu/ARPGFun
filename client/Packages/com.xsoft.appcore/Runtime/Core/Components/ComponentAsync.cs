using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace App.Core.Core.Components
{
    public class ComponentAsync : MonoBehaviour
    {
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

        private readonly ConcurrentQueue<AsyncCall> _updateCall = new ConcurrentQueue<AsyncCall>();

        protected virtual void Update()
        {
            if (_updateCall.Count == 0) return;
            while (_updateCall.TryDequeue(out AsyncCall c))
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
    }
}
