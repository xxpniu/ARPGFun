using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Proto;
using System.Collections.Concurrent;
using XNet.Libs.Utility;
using ServerUtility;
using Google.Protobuf.WellKnownTypes;
using GServer.Managers;
using Grpc.Core.Utils;
using System.Linq;
using System.Collections.Generic;
using Utility;
using GServer;

namespace GateServer
{
    public class StreamServices:Proto.ServerStreamService.ServerStreamServiceBase
    {

        public bool Push(string account, IMessage message)
        {
            if (workers.TryGetValue(account, out StreamBuffer<Any> channel))
            {
                Debuger.Log($"{account}-{message}");
                return channel.Push(Any.Pack(message));
            }
            return false;
        }

        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> workers = new ConcurrentDictionary<string, StreamBuffer<Any>>();

        public async override Task ServerAnyStream(Proto.Void request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var channel = new AsyncStreamBuffer<Any>(20);
            string id = context.GetAccountId();
            if (workers.TryGetValue(id, out StreamBuffer<Any> c))
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
                        var mQuery = mc.CreateClient<MatchServices.MatchServicesClient>();
                        await mQuery.TryToReJoinMatchAsync(new S2M_TryToReJoinMatch { Account = id });
                        await mc.ShutdownAsync();
                    }
                }
                catch { }

                await foreach (var res in channel.TryPullAsync(context.CancellationToken))
                {
                    await responseStream.WriteAsync(res).ConfigureAwait(false);
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
            if (workers.TryGetValue(uuid, out StreamBuffer<Any> channel))
            {
                channel.Close();
            }
        }
    }
}
