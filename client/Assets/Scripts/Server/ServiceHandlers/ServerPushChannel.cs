using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Utility;
using System.Threading;

namespace Server.ServiceHandlers
{
    public class ServerPushChannel<TData>:StreamBuffer<TData>
        where TData :IMessage,new()
    {
        public ServerPushChannel(int max = 300):base(max)
        {

        }

        private CancellationTokenSource _token;

        private void Resume()
        {
            if (_token == null) return;
            if (_token.IsCancellationRequested) return;
            _token.Cancel();
        }

        public override bool Push(TData request)
        {
            var r = base.Push(request);
            Resume();
            return r;
        }

        public override void Close()
        {
            base.Close();
            Resume();
        }

        public async Task ProcessAsync(IServerStreamWriter<TData> responseStream)
        {
            while (IsWorking)
            {
                try
                {
                    while (TryPull(out var data))
                        await responseStream.WriteAsync(data).ConfigureAwait(false);
                    try
                    {
                        using (_token = new CancellationTokenSource())
                        {
                            await Task.Delay(10000, _token.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                }
                catch (Exception)
                {
                    Close();
                    break;
                }
            }
        }
    }
}
