using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace XNet.Libs.Utility
{
    public class ServerLoggerInterceptor : Interceptor
    {
        public ServerLoggerInterceptor(LogServer server)
        {
            this.Server = server;
        }

        public delegate bool AuthCheck(ServerCallContext context);

        public void SetAuthCheck(AuthCheck check)
        {
            authCheck = check;
        }

        private AuthCheck authCheck;

        public LogServer Server { get; }

        private bool HaveAuth(System.Reflection.MethodInfo info, ServerCallContext context)
        {
            if (info.GetCustomAttributes(typeof(AuthAttribute), false) is AuthAttribute[] auths && auths.Length > 0) return true;
            var auth = authCheck?.Invoke(context)?? CheckAuthDefault(context);
            return auth;
        }

        private bool CheckAuthDefault(ServerCallContext context)
        {
            if (!context. GetHeader("call-key", out string key1)) return false;
            if (!context. GetHeader("call-token", out string token)) return false;
            return Md5Tool.CheckToken(key1, token);
        }

        

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {

                if (!HaveAuth(continuation.Method, context))
                {
                    LogCall<TRequest,TResponse>(MethodType.Unary, context,request);
                    throw new AuthException();
                }
                var result = await continuation(request, context);
                LogCall(MethodType.Unary, context, request, result);
                return result;
            }
            catch (Exception ex)
            {
                Debuger.LogError($"Error thrown by {context.Method}.{ex}");
                throw;
            }
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.ClientStreaming, context);
            if (!HaveAuth(continuation.Method, context)) throw new AuthException();
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.ServerStreaming, context,request);
            if (!HaveAuth(continuation.Method, context)) throw new AuthException();
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.DuplexStreaming, context);
            if (!HaveAuth(continuation.Method, context)) { throw new AuthException(); }
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        private void LogCall<TRequest, TResponse>(MethodType methodType, ServerCallContext context,TRequest request =null, TResponse response =null)
            where TRequest : class
            where TResponse : class
        {

            string GetHeader(string key)
            {
                return context.RequestHeaders.Get(key)?.Value;
            }

            var headers = new[] { "caller-user" , "caller-machine" , "caller-os" , "call-key" , "call-token", "session-key" };
            var sb = new StringBuilder();
            foreach (var i in headers)
            {
                var str = GetHeader(i);
                if (string.IsNullOrEmpty(str)) continue;
                sb.Append($" {i}={str}");
            }

            Debuger.Log($"Call:{methodType} {sb}[{typeof(TRequest)}]->{request} Response: {typeof(TResponse)}{response}");

        }
    }
}