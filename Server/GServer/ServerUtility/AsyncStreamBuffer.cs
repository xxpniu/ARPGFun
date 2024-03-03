using System;
using Google.Protobuf;
using Utility;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using XNet.Libs.Utility;

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

        private readonly SemaphoreSlim _semaphoreSlim = new(0, 1);

        private void Resume()
        {
            _semaphoreSlim.Release();
        }

        
        public async IAsyncEnumerable<TData> TryPullAsync([EnumeratorCancellation] CancellationToken token)
        {
            while (IsWorking)
            {
                while (TryPull(out var data))
                {
                    token.ThrowIfCancellationRequested();
                    yield return data;
                }

                token.ThrowIfCancellationRequested();
                await _semaphoreSlim.WaitAsync(cancellationToken: token);
            }
        }
    }
}
