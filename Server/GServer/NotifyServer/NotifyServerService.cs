using System;
using System.Linq;
using System.Threading.Tasks;
using ChatTool;
using Grpc.Core;
using Proto;
using Proto.ServerConfig;
using ServerUtility;
using System.Collections.Concurrent;
using Utility;
using System.Collections.Generic;
using XNet.Libs.Utility;
using Google.Protobuf.WellKnownTypes;

namespace NotifyServer
{
    public class ChatChannel
    {
        public LogChannel Channel;
        public ChatServerService.ChatServerServiceClient Client;
        public ChatServerConfig Config;
    }

    public class NotifyServerService:Proto.NotifyServices.NotifyServicesBase
    {
        private readonly WatcherServer<int, ChatServerConfig> _chat;
        private readonly ConcurrentDictionary<int, ChatChannel> _chatServers = new ConcurrentDictionary<int, ChatChannel>();


        public NotifyServerService(WatcherServer<int, ChatServerConfig> chat)
        {
            this._chat = chat;
            chat.OnRefreshed = () =>
            {
               _= chat.ForEach(async( c) => await TryConnect(c));
            };
        }

        private async Task< bool> TryConnect(ChatServerConfig c)
        {
            if (_chatServers.TryGetValue(c.ChatServerID, out ChatChannel channel))
            {
                if (channel.Config.Equals(c)) return false;
                await channel.Channel.ShutdownAsync();
                _chatServers.TryRemove(c.ChatServerID, out _);
            }
            
            var ch = new LogChannel(c.ServicsHost);
            var client = await ch.CreateClientAsync<Proto.ChatServerService.ChatServerServiceClient>();
            channel = new ChatChannel { Channel = ch, Client = client, Config = c };
            return _chatServers.TryAdd(c.ChatServerID, channel);
        }

        public override async Task<N2S_RouteSendNotify> RouteSendNotify(S2N_RouteSendNotify request,
            ServerCallContext context)
        {
            var user = request.Msg.Select(t => t.AccountID).Distinct().ToList();

            var players = await DataBase.S.FindPlayersByUuid(user);
            var dic = new Dictionary<string, PlayerState>();
            foreach (var i in players)
            {
                dic.Add(i.User.Uuid, i);
            }

            foreach (var i in request.Msg)
            {
                if (!dic.TryGetValue(i.AccountID, out var ps)) continue;
                if (_chatServers.TryGetValue(ps.ServerID, out var channel))
                {
                    await channel.Client.CreateNotifyAsync(i);
                }
                else
                {
                    Debuger.LogWaring($"{ps} not connected chat server");
                }
            }

            //ok
            return new N2S_RouteSendNotify { Code = ErrorCode.Ok };
        }

    }
}
