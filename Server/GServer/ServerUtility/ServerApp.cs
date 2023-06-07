using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XNet.Libs.Utility;

namespace ServerUtility
{
    public abstract class ServerApp<T>:XSingleton<T> where T :class ,new()
    {
        protected abstract Task Start(CancellationToken token = default);
        protected abstract Task Stop(CancellationToken token= default);

        private  CancellationTokenSource _source;
        public async Task Run(CancellationToken token = default)
        {
            _source = new CancellationTokenSource();
            var link = CancellationTokenSource.CreateLinkedTokenSource(_source.Token, token);
           
            using IHost host = new HostBuilder()
                .ConfigureLogging(l => l.AddConsole()).Build();
            host.Start();
            
            await Start(link.Token);
            try
            {
                await host.WaitForShutdownAsync(token: token);
                Debuger.Log("Shutting down gracefully.");
            }
            catch (Exception e)
            {
                Debuger.LogError(e.ToString());
            }

            // ReSharper disable once MethodSupportsCancellation
            await Stop();
            Debuger.Log("Exited");

        }
    }
}