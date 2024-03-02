using System.Collections.Concurrent;
using Google.Protobuf;

namespace XNet.Libs.Utility
{

    public class StreamBuffer<TData> where TData : IMessage, new()
    {
        private readonly ConcurrentQueue<TData> _requests = new ConcurrentQueue<TData>();

        public int Max { get; }

        public StreamBuffer(int max = 100)
        {
            this.Max = max;
        }

        protected bool TryPull(out TData data)
        {
            return _requests.TryDequeue(out data);
        }

        public virtual bool Push(TData request)
        {
            if (_requests.Count > Max) _requests.TryDequeue(out _);
            _requests.Enqueue(request);
            return true;
        }

        public bool IsWorking { private set; get; } = true;

        public virtual void Close()
        {
            IsWorking = false;
        }
    }

}
