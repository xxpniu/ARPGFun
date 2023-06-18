using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    public abstract class Composite
    {
        public string Guid { set; get; }

        private IEnumerator<RunStatus> Current { set; get; }

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
            if (LastStatus.HasValue && LastStatus.Value != RunStatus.Running)
            {
                return LastStatus.Value;
            }

            if (Current == null)
            {
                throw new Exception($" {this.GetType()} of {Guid} You Must start it!");
            }

            if (Current.MoveNext()) LastStatus = Current.Current;
            else throw new Exception($"{this.GetType()} of {Guid} Nothing to run? Somethings gone terribly, terribly wrong!");
            if (LastStatus != RunStatus.Running)
                Stop(context);
            return this.LastStatus.Value;
        }

        public abstract IEnumerable<RunStatus> Execute(ITreeRoot context);

        public RunStatus? LastStatus { private set; get; }

        private readonly Dictionary<string, object> _attachVariables = new();

        public virtual Composite FindGuid(string id)
        {
            if (Guid == id) return this;
            return null;
        }

        protected void Attach(string key, object val)
        {
            if (_attachVariables.ContainsKey(key)) _attachVariables.Remove(key);
            _attachVariables.Add(key, val);
        }

        public void DebugVals(Action<string, object> callback)
        {
            foreach (var i in _attachVariables)
            {
                callback(i.Key, i.Value);
            }
        }
    }
}