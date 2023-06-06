using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Core.Components;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;
using Utility;
using XNet.Libs.Utility;

public class Stream : ComponentAsync
{
    protected override void Update()
    {
        base.Update();
        UpdateCall?.Invoke();
    }
    public void OnDestroy()
    {
        Debuger.Log($"{this.name} OnDestoryed");
        DestroyCall?.Invoke();
    }
    public Action DestroyCall { get; internal set; }
    public Action UpdateCall { get; internal set; }
    private readonly ConcurrentQueue<Action> calls = new ConcurrentQueue<Action>();

    public abstract class StreamChannel<TData>
    where TData : IMessage, new()
    {
        protected readonly StreamBuffer<TData> Buffer = new StreamBuffer<TData>();

        private Stream Com { get; }

        public StreamChannel(bool dontupload = false, string Tag = null)
        {
            CancellationToken = new CancellationTokenSource();
            var go = new GameObject(Tag ?? $"Channel_{typeof(TData).Name}");
            Com = go.AddComponent<Stream>();
            if (dontupload) DontDestroyOnLoad(go);
            Com.UpdateCall = this.OnUpdate;
            Com.DestroyCall = this.OnDestroy;
            IsWorking = true;
            Task.Factory.StartNew(async () =>
            {
                await Process().ConfigureAwait(false);
                await ShutDownAsync(true);
            }, this.CancellationToken.Token);
        }

        protected CancellationTokenSource CancellationToken { get; }

        protected abstract Task ShutDownProcessAsync();

        public async Task ShutDownAsync(bool haveCallBack = true)
        {
            IsWorking = false;
            if (!TryCancel()) return;
            await ShutDownProcessAsync();
            if (haveCallBack) OnDisconnect?.Invoke();
            else
            {
                OnDisconnect = null;
            }
            if (Com) Com.Invoke(() => { Destroy(Com.gameObject); });
        }

        private bool TryCancel()
        {
            if (CancellationToken == null) return false;
            if (CancellationToken.IsCancellationRequested) return false;
            CancellationToken.Cancel();
            return true;
        }

        protected virtual void OnDestroy()
        {
            _ = ShutDownAsync(false);
        }

        protected virtual void OnUpdate()
        {
        }

        protected abstract Task Process();

        public Action OnDisconnect;

        protected void InvokeCall(Action action)
        {
            if (Com) Com.Invoke(action);
        }

        public bool IsWorking { get; private set; }
    }

    public class ResponseChannel<TData> : StreamChannel<TData>
    where TData : IMessage, new()
    {
        public ResponseChannel(IAsyncStreamReader<TData> call, bool dontupload = false, string tag = null) : base(dontupload, tag)
        {
            this.Call = call;
        }

        public IAsyncStreamReader<TData> Call { get; }

        protected async override Task Process()
        {
            try
            {
                while (await Call.MoveNext(CancellationToken.Token).ConfigureAwait(false)
                    && Buffer.IsWorking)
                {
                    Buffer.Push(Call.Current);
                }
            }
            catch (TaskCanceledException) { }
            catch (RpcException) { }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }

            Buffer.Close();
        }

        protected override void OnUpdate()
        {
            while (this.Buffer.TryPull(out TData data))
            {
                OnReceived?.Invoke(data);
            }
        }

        public Action<TData> OnReceived;

        protected async override Task ShutDownProcessAsync()
        {
            await Task.CompletedTask;
        }
    }

    public class RequestChannel<TData> : StreamChannel<TData>
        where TData : IMessage, new()
    {
        public RequestChannel(IAsyncStreamWriter<TData> call, bool dontupload = false, string tag = null) : base(dontupload, tag)
        {
            this.Call = call;
        }

        public IAsyncStreamWriter<TData> Call { get; }

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        protected async override Task Process()
        {
            try
            {
                while (Buffer.IsWorking)
                {
                    while (Buffer.TryPull(out TData i))
                    {
                        await Call.WriteAsync(i).ConfigureAwait(false);
                    }
                    await semaphoreSlim.WaitAsync(this.CancellationToken.Token);
                }
            }
            catch (RpcException)
            {
                Debuger.LogWaring("Exit!");
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }
            Buffer.Close();
        }

        public bool Push(TData data)
        {
            var t = this.Buffer.Push(data);
            Resume();
            return t;
        }

        private void Resume()
        {
            semaphoreSlim.Release();
        }

        protected override void OnDestroy()
        {
            Buffer.Close();
            Resume();
            base.OnDestroy();
        }

        protected async override Task ShutDownProcessAsync()
        {
            await Task.CompletedTask;
        }
    }


}


