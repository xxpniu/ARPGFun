using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace BehaviorTree
{
    public abstract class Composite
    {
        private readonly Dictionary<string, object> _attachVariables = new();
        public string Guid { set; get; }

        private IEnumerator<RunStatus> Current { set; get; }

        public RunStatus? LastStatus { private set; get; }

        public virtual void Start(ITreeRoot context)
        {
            _attachVariables.Clear();
            LastStatus = null;
            Current = Execute(context).GetEnumerator();
        }

        public virtual void Stop(ITreeRoot context)
        {
            if (Current != null)
            {
                Current.Dispose();
                Current = null;
            }

            if (LastStatus == RunStatus.Running)
            {
                Attach("failure", "block by other");
                LastStatus = RunStatus.Failure;
            }
        }

        public RunStatus Tick(ITreeRoot context)
        {
            if (LastStatus.HasValue && LastStatus.Value != RunStatus.Running) return LastStatus.Value;

            if (Current == null) throw new Exception($" {GetType()} of {Guid} You Must start it!");

            if (Current.MoveNext()) LastStatus = Current.Current;
            else
                throw new Exception($"{GetType()} of {Guid} Nothing to run? Somethings gone terribly, terribly wrong!");
            if (LastStatus != RunStatus.Running)
                Stop(context);
            return LastStatus.Value;
        }

        public abstract IEnumerable<RunStatus> Execute(ITreeRoot context);

        public virtual Composite FindGuid(string id)
        {
            return Guid == id ? this : null;
        }

        protected void Attach(string key, object val)
        {
            if (_attachVariables.ContainsKey(key)) _attachVariables.Remove(key);
            _attachVariables.Add(key, val);
        }

        public void DebugVals(Action<string, object> callback)
        {
            foreach (var i in _attachVariables) callback(i.Key, i.Value);
        }
    }
}