using Google.Protobuf;
using Grpc.Core;
using Proto;

namespace ServerUtility
{
    public static class ServerExtends
    {
        public static void Request<Client, Request, Response>( string serverHost, Request request)
            where Client : ClientBase
            where Request : IMessage
            where Response : IMessage
        {
            
        }
    }
}