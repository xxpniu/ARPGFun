using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using App.Core.Core.Components;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;
using XNet.Libs.Utility;

namespace UApp.Utility
{
    public class Stream : ComponentAsync
    {
        private readonly ConcurrentQueue<Action> _calls = new();
        public Action DestroyCall { get; internal set; }
        public Action UpdateCall { get; internal set; }

        protected override void Update()
        {
            base.Update();
            UpdateCall?.Invoke();
        }

        public void OnDestroy()
        {
            Debuger.Log($"{name} OnDestroyed");
            DestroyCall?.Invoke();
        }

        public abstract class StreamChannel<TData>
            where TData : IMessage, new()
        {
            protected readonly StreamBuffer<TData> Buffer = new();

            public Action OnDisconnect;

            public StreamChannel(bool dontUpload = false, string tag = null)
            {
                CancellationToken = new CancellationTokenSource();
                var go = new GameObject(tag ?? $"Channel_{typeof(TData).Name}");
                Com = go.AddComponent<Stream>();
                if (dontUpload) DontDestroyOnLoad(go);
                Com.UpdateCall = OnUpdate;
                Com.DestroyCall = OnDestroy;
                IsWorking = true;
                Task.Factory.StartNew(async () =>
                {
                    await Process().ConfigureAwait(false);
                    await ShutDownAsync();
                }, CancellationToken.Token);
            }

            private Stream Com { get; }

            protected CancellationTokenSource CancellationToken { get; }

            public bool IsWorking { get; private set; }

            protected abstract Task ShutDownProcessAsync();

            public async Task ShutDownAsync(bool haveCallBack = true)
            {
                IsWorking = false;
                if (!TryCancel()) return;
                await ShutDownProcessAsync();
                await UniTask.SwitchToMainThread();
                if (haveCallBack) OnDisconnect?.Invoke();
                OnDisconnect = null;
                Destroy(Com.gameObject);
            }

            private bool TryCancel()
            {
                if (CancellationToken == null) return false;
                if (CancellationToken.IsCancellationRequested) return false;
                CancellationToken.Cancel();
                return true;
            }

            protected virtual async void OnDestroy()
            {
                await ShutDownAsync(false);
            }

            protected virtual void OnUpdate()
            {
            }

            protected abstract Task Process();
        }

        public class ResponseChannel<TData> : StreamChannel<TData>
            where TData : IMessage, new()
        {
            public Action<TData> OnReceived;

            public ResponseChannel(IAsyncStreamReader<TData> call, bool dontUpload = false, string tag = null) : base(
                dontUpload, tag)
            {
                Call = call;
            }

            private IAsyncStreamReader<TData> Call { get; }

            protected override async Task Process()
            {
                try
                {
                    while (await Call.MoveNext(CancellationToken.Token)
                               .ConfigureAwait(false)
                           && Buffer.IsWorking)
                        Buffer.Push(Call.Current);
                }
                catch (TaskCanceledException)
                {
                }
                catch (RpcException)
                {
                }
                catch (Exception ex)
                {
                    Debuger.LogError(ex);
                    Debug.LogException(ex);
                }

                Buffer.Close();
            }

            protected override void OnUpdate()
            {
                while (Buffer.TryPull(out var data)) OnReceived?.Invoke(data);
            }

            protected override async Task ShutDownProcessAsync()
            {
                await Task.CompletedTask;
            }
        }

        public class RequestChannel<TData> : StreamChannel<TData>
            where TData : IMessage, new()
        {
            private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

            public RequestChannel(IAsyncStreamWriter<TData> call, bool dontUpload = false, string tag = null) : base(
                dontUpload, tag)
            {
                Call = call;
            }

            private IAsyncStreamWriter<TData> Call { get; }

            protected override async Task Process()
            {
                try
                {
                    while (Buffer.IsWorking)
                    {
                        while (Buffer.TryPull(out var i)) await Call.WriteAsync(i).ConfigureAwait(false);
                        await _semaphoreSlim.WaitAsync(CancellationToken.Token);
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
                var t = Buffer.Push(data);
                Resume();
                return t;
            }

            private void Resume()
            {
                _semaphoreSlim.Release();
            }

            protected override void OnDestroy()
            {
                Buffer.Close();
                Resume();
                base.OnDestroy();
            }

            protected override async Task ShutDownProcessAsync()
            {
                await Task.CompletedTask;
            }
        }
    }
}