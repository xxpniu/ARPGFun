using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using XNet.Libs.Utility;

namespace Utility
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false)]
    public class AuthAttribute:Attribute
    {
        public AuthAttribute()
        { }
    }

    public class AuthException : Exception
    {
    }

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

    public class LogServer : Server
    {
        public ServerLoggerInterceptor Interceptor { get; } 

        public LogServer() : base(null)
        {
            Interceptor = new ServerLoggerInterceptor(this);
        }

        public LogServer BindServices(params ServerServiceDefinition[] definitions)
        {
            foreach (var i in definitions)
            {
                this.Services.Add(i.Intercept(Interceptor));
            }

            return this;
        }

        private readonly ConcurrentDictionary<string, string> SessionKey = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> UserSession = new ConcurrentDictionary<string, string>();

        public bool TryCreateSession(string accountID,out string sessionKey)
        {
            var gui = Guid.NewGuid().ToString();
            if (UserSession.TryGetValue(accountID, out string oldSession))
            {
                SessionKey.TryRemove(oldSession, out _);
                UserSession.TryRemove(accountID, out _);
            }
            sessionKey = gui;
            return SessionKey.TryAdd(gui, accountID) && UserSession.TryAdd(accountID,gui);
        }
        public bool CheckSession(string key,out string valu)
        {
            return SessionKey.TryGetValue(key,out valu) ;
        }
    }
}
