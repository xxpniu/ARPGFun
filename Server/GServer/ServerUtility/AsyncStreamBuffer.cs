using System;
using Google.Protobuf;
using Utility;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ServerUtility
{
    public class AsyncStreamBuffer<TData>:StreamBuffer<TData>
        where TData: IMessage,new()
    {
        public AsyncStreamBuffer(int max = 300):base(max)
        {

        }

        public override bool Push(TData request)
        {
            var t= base.Push(request);
            Resume();
            return t;
        }

        public override void Close()
        {
            base.Close();
            Resume();
        }

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);

        private void Resume()
        {
            semaphoreSlim.Release();
        }

        
        public async IAsyncEnumerable<TData> TryPullAsync([EnumeratorCancellation] CancellationToken token)
        {

            while (IsWorking)
            {
                while (TryPull(out TData data)) yield return data;
                await semaphoreSlim.WaitAsync(cancellationToken: token);
            }
        }
    }
}
