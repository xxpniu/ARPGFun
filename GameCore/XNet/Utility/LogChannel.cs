using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Proto;

namespace XNet.Libs.Utility
{

    public class ClientLoggerInterceptor : Interceptor
    {
        public LogChannel Channel { get; }
        public ClientLoggerInterceptor(LogChannel channel)
        {
            this.Channel = channel;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method,request);
            AddCallerMetadata(ref context);
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(HandleResponse(call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
        {
            try
            {
                var response = await t;
                Debuger.Log($"{typeof(TResponse)}:{response}");
                return response;
            }
            catch (Exception ex)
            {
                Debuger.LogError($"Call error: {ex.Message}");
                throw;
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(request, context);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method,TRequest request =null ,TResponse response =null)
            where TRequest : class
            where TResponse : class
        {
            Debuger.Log($"Starting Type: [{method.Type}] Req: {typeof(TRequest)}{request}. Res: {typeof(TResponse)}{response}");
        }

        private void AddCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var headers = context.Options.Headers;
            // Call doesn't have a headers collection to add to.
            // Need to create a new context with headers for the call.
            if (headers == null)
            {
                headers = new Metadata();
                var options = context.Options.WithHeaders(headers);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            var ticks = DateTime.Now.Ticks;
            var time = $"{ticks}{Channel.Token}";
            headers.Add(HeadKeys.CallOs, Environment.OSVersion.ToString());
            headers.Add(HeadKeys.CallKey, Md5Tool.GetTokenKey(time));
            headers.Add(HeadKeys.CallToken, time);
            headers.Add(HeadKeys.SessionKey, Channel.SessionKey ?? string.Empty);
            if (headers.Get(HeadKeys.TraceId) == null)
                headers.Add(HeadKeys.TraceId, NewTraceId());
            headers.Add(HeadKeys.Ticks, ticks.ToString());
        }

        private static string NewTraceId()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class LogChannel : Channel
    {
        public string Token { get; set; }
        public string SessionKey { set; get; }

        public LogChannel(ServiceAddress address) : this($"{address.IpAddress}:{address.Port}",
            ChannelCredentials.Insecure)
        {
        }
        
        public LogChannel(string target, ChannelCredentials credentials) : base(target, credentials)
        {
            Debuger.Log($"LogChannel Request Server Ip:{target}");
        }
        
        private CallInvoker CreateLogCallInvoker()
        {
            return this.Intercept(new ClientLoggerInterceptor(this));
        }

        public async Task<T> CreateClientAsync<T>(DateTime? deadline = default) where T : ClientBase
        {
            if (deadline == null) deadline = DateTime.UtcNow.AddSeconds(10);
            await ConnectAsync(deadline);

            return CreateClient<T>();
        }

        public T CreateClient<T>() where T : ClientBase
        {
            return Activator.CreateInstance(typeof(T), this.CreateLogCallInvoker()) as T;
        }
        
    }


    public class C<TClient> : LogChannel
        where TClient : ClientBase
    {
        private C(ServiceAddress address) : base(address)
        {
        }

        public static async Task<TRes> RequestOnceAsync<TRes>(
            ServiceAddress ip,
            Func<TClient, Task<TRes>> expression,
            DateTime? deadTime = default, 
            ServerCallContext refContext = default)
        {
            var server = new C<TClient>(ip);
            var client = await server.CreateClientAsync<TClient>(deadTime);
            try
            {
                if (refContext != null)
                {
                    Debuger.Log($"Ref headers: ${refContext.ToLog()}");
                    //ref
                } 
                var res = await expression.Invoke(client);
                return res;
            }
            finally
            {
                try
                {
                    await server.ShutdownAsync();
                }
                catch
                {
                    // ignore
                }
            }

        }
    }
}
