using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Utility;

namespace XNet.Libs.Utility
{
    public static class NetProtoTool
    {
        public static bool EnableLog { get; set; } = false;

        public static bool GetHeader(this ServerCallContext context, string key, out string value)
        {
            value = context.RequestHeaders.Get(key)?.Value;
            return !string.IsNullOrEmpty(value);
        }

        public static string GetAccountId(this ServerCallContext context,string key = null)
        {
            context.GetHeader(key ?? "user-key", out var account);
            return account;
        }

        public static async Task< bool> WriteSession(this ServerCallContext context, string account, LogServer server)
        {
            if (!server.TryCreateSession(account, out var session))  return false;
            await context.WriteResponseHeadersAsync(new Metadata { { "session-key", session } });
            return true;
        }
    }
}
