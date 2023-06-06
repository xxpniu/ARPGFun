using System;
using System.Threading.Tasks;
using Grpc.Core;
using Proto;
using System.Collections.Concurrent;
using Utility;
using XNet.Libs.Utility;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using ServerUtility;
using ChatTool;
using System.Linq;

namespace ChatServer
{
    public class ChatService:Proto.ChatService.ChatServiceBase
    {
        
        private ConcurrentDictionary<string, StreamBuffer<Any>> NotifyChannels { get; } = new ConcurrentDictionary<string, StreamBuffer<Any>>();
     
        public LogServer Server { get; }

        public ChatService(LogServer server)
        {
            this.Server = server;
        }

        [Auth]
        public async override Task Login(C2CH_Login request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var heroName = request.HeroName;
            var loginServer = Application.S.LoginServers.FirstOrDefault();
            if (loginServer == null)
            {
                Debuger.LogError($"No found login server");
                return;
            }

            Debuger.Log($"{request.AccountID} Join chat");
            var channel = new LogChannel(loginServer.ServicsHost);
            var client = channel.CreateClient<LoginBattleGameServerService.LoginBattleGameServerServiceClient>();
            var r = await client.CheckSessionAsync(new S2L_CheckSession { UserID = request.AccountID, Session = request.Token });
            await channel.ShutdownAsync();
            if (r.Code != ErrorCode.Ok) return;
            //if (!Server.TryCreateSession(request.AccountID, out string session)) return;
            await this.CloseChannel(request.AccountID);

            if (!(await DataBase.S.Online(request.AccountID, Application.S.Config.ChatServerID, request.HeroName, request.Token))) return;

            var account = request.AccountID;
            var streamchannel = new AsyncStreamBuffer<Any>(200);
            if (!NotifyChannels.TryAdd(account, streamchannel)) throw new Exception($"Acount:[{account}] had join chat");

            if (!(await context.WriteSession(account, Server))) return;


            await Application.S.NotifyStateForAllFriends(new PlayerState
            {
                State = PlayerState.Types.StateType.Online,
                User = new ChatUser
                {
                    ChatServerId = Application.S.Config.ChatServerID,
                    Uuid = account,
                    UserName = heroName
                }
            });


            try
            {
                await foreach (var i in streamchannel.TryPullAsync(context.CancellationToken))
                {
                    await responseStream.WriteAsync(i).ConfigureAwait(false);
                }
            }
            catch { }

            await DataBase.S.Offline(account, request.Token);

            Debuger.Log($"{request.AccountID} exit chat");
            //notify offline
            await Application.S.NotifyStateForAllFriends(new PlayerState
            {
                State = PlayerState.Types.StateType.Offline,
                User = new ChatUser
                {
                    ChatServerId = Application.S.Config.ChatServerID,
                    Uuid = account,
                    UserName = heroName
                }
            });
            NotifyChannels.TryRemove(account, out _);
        }

        public async override Task<CH2C_Chat> SendChat(C2CH_Chat request, ServerCallContext context)
        {
            var account = context.GetAccountId();
            foreach (var chat in request.Mesg)
            {
                if (chat.Receiver.Uuid == chat.Sender.Uuid) continue;
                if (chat.Sender.Uuid != account) return new CH2C_Chat { Code = ErrorCode.Error };
                await Application.S.RouteChat(chat);
            }
            
            return new  CH2C_Chat { Code = ErrorCode.Ok };
        }

        public async override Task<CH2C_QueryPlayerState> QueryPlayerState(C2CH_QueryPlayerState request, ServerCallContext context)
        {
            var list = await DataBase.S.QueryFriend(context.GetAccountId());
            var res = new CH2C_QueryPlayerState();

            //ignore
            foreach (var i in list)
            {
                if (i.State == PlayerState.Types.StateType.Offline) continue;
                else res.States.Add(i);
            }
            return res;
        }

        public async override Task<CH2C_LinkFriend> LinkFrind(C2CH_LinkFriend request, ServerCallContext context)
        {
            var account = context.GetAccountId();
            if (await DataBase.S.LinkFriend(account, request.FriendId))
            {
                return new CH2C_LinkFriend { Code = ErrorCode.Ok };
            }
            return new CH2C_LinkFriend { Code = ErrorCode.Error };
        }

        public async override Task<CH2C_UnLinkFriend> UnLinkFrind(C2CH_UnLinkFriend request, ServerCallContext context)
        {

            var account = context.GetAccountId();

            return new CH2C_UnLinkFriend
            {
                Code = await DataBase.S.UnLinkFriend(account, request.FriendId) ? ErrorCode.Ok : ErrorCode.Error
            };
        }

        public async Task CloseChannel(string account)
        {
            if (NotifyChannels.TryRemove(account, out  StreamBuffer<Any> notify))
                notify.Close();

            await Task.CompletedTask;
        }

        public async Task<bool> NotifyFriendStateChange(string account, params Any[] state)
        {
            if (this.NotifyChannels.TryGetValue(account, out StreamBuffer<Any> chan))
            {
                foreach (var i in state) chan.Push(i);
                return await Task.FromResult(true);
            }
            else {
                Debuger.LogWaring($"Nofound {account} on chat server");
            }
            return await Task.FromResult(false);
        }

        public async override Task<CH2C_QueryFriend> QueryFriend(Proto.Void request, ServerCallContext context)
        {
            var list = await DataBase.S.QueryFriend(context.GetAccountId());
            return new CH2C_QueryFriend { Code = ErrorCode.Ok,  States = { list } };
        }
    }
}
