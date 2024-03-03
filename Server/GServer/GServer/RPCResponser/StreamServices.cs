using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using ServerUtility;
using Utility;
using XNet.Libs.Utility;

// ReSharper disable once CheckNamespace
// ReSharper disable once IdentifierTypo
namespace GServer.RPCResponsor
{
    public class StreamServices:Proto.ServerStreamService.ServerStreamServiceBase
    {
        public bool Push(string account, IMessage message)
        {
            if (!_workers.TryGetValue(account, out var channel)) return false;
            Debuger.Log($"{account}-{message}");
            return channel.Push(Any.Pack(message));
        }

        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> _workers = new ConcurrentDictionary<string, StreamBuffer<Any>>();

        public override async Task ServerAnyStream(Proto.Void request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var channel = new AsyncStreamBuffer<Any>(20);
            var id = context.GetAccountId();
            if (_workers.TryGetValue(id, out var c))
            {
                c.Close();
                _workers.TryRemove(id,out _);
            }

            try
            {
                if (!_workers.TryAdd(id, channel))
                {
                    Debuger.LogError($"Error add channel error!");
                    throw new Exception("add error");
                }

                try
                {
                    var matchSever = Application.S.MatchServers.NextServer();
                    if (matchSever != null)
                    {
                        await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                            matchSever.ServicsHost,
                            async (client) => await client.TryToReJoinMatchAsync(
                                new S2M_TryToReJoinMatch { Account = id },
                                headers: context.GetTraceMeta())
                        );
                    }
                }
                catch
                {
                    //ignore
                }

                try
                {
                    
                    await foreach (var res in channel.TryPullAsync(context.CancellationToken))
                    {
                        Debuger.Log($"SendTo:{id} by {res}");
                        await responseStream.WriteAsync(res).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // ignore
                }

            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }

            Debuger.Log($"Stop account ID:{id}");
            _workers.TryRemove(id,out _);
        }

        internal bool Exist(string accountId)
        {
            return _workers.ContainsKey(accountId);
        }

        internal void TryClose(string uuid)
        {
            if (_workers.TryGetValue(uuid, out var channel))
            {
                channel.Close();
            }
        }
    }
}
