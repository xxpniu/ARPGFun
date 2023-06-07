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

namespace GServer.RPCResponsor
{
    public class StreamServices:Proto.ServerStreamService.ServerStreamServiceBase
    {
        public bool Push(string account, IMessage message)
        {
            if (!workers.TryGetValue(account, out var channel)) return false;
            Debuger.Log($"{account}-{message}");
            return channel.Push(Any.Pack(message));
        }

        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> workers = new ConcurrentDictionary<string, StreamBuffer<Any>>();

        public override async Task ServerAnyStream(Proto.Void request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var channel = new AsyncStreamBuffer<Any>(20);
            var id = context.GetAccountId();
            if (workers.TryGetValue(id, out var c))
            {
                c.Close();
                workers.TryRemove(id,out _);
            }

            try
            {
                if (!workers.TryAdd(id, channel))
                {
                    Debuger.LogError($"Error add channel error!");
                    throw new Exception("add error");
                }

                try
                {
                    var matchSever = Application.S.MatchServers.FirstOrDefault();
                    if (matchSever != null)
                    {
                        var mc = new LogChannel(matchSever.ServicsHost);
                        var mQuery = await mc.CreateClientAsync<MatchServices.MatchServicesClient>();
                        await mQuery.TryToReJoinMatchAsync(new S2M_TryToReJoinMatch { Account = id },
                            cancellationToken:mc.ShutdownToken);
                        await mc.ShutdownAsync();
                    }
                }
                catch { }

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
            workers.TryRemove(id,out _);
        }

        internal bool Exist(string accountId)
        {
            return workers.ContainsKey(accountId);
        }

        internal void TryClose(string uuid)
        {
            if (workers.TryGetValue(uuid, out var channel))
            {
                channel.Close();
            }
        }
    }
}
