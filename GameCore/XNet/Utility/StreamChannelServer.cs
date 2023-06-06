using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Utility
{

    public class StreamBuffer<TData> where TData : IMessage, new()
    {
        readonly ConcurrentQueue<TData> requests = new ConcurrentQueue<TData>();

        public int Max { get; }

        public StreamBuffer(int max = 100)
        {
            this.Max = max;
        }

        public bool TryPull(out TData data)
        {
            return requests.TryDequeue(out data);
        }

        public virtual bool Push(TData request)
        {
            if (requests.Count > Max) requests.TryDequeue(out _);
            requests.Enqueue(request);
            return true;
        }

        public bool IsWorking { private set; get; } = true;

        public virtual void Close()
        {
            IsWorking = false;
        }
    }

}
