using System;
using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace XNet.Libs.Utility
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

        private readonly ConcurrentDictionary<string, string> _sessionKey = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _userSession = new ConcurrentDictionary<string, string>();

        public bool TryCreateSession(string accountId,out string sessionKey)
        {
            var gui = Guid.NewGuid().ToString();
            if (_userSession.TryGetValue(accountId, out string oldSession))
            {
                _sessionKey.TryRemove(oldSession, out _);
                _userSession.TryRemove(accountId, out _);
            }
            sessionKey = gui;
            return _sessionKey.TryAdd(gui, accountId) && _userSession.TryAdd(accountId,gui);
        }
        public bool CheckSession(string key,out string valu)
        {
            return _sessionKey.TryGetValue(key,out valu) ;
        }
    }
}
