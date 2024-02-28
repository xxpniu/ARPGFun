using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ChatTool;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using ServerUtility;
using Utility;
using XNet.Libs.Utility;

namespace ChatServer.Services
{
    public class ChatService:Proto.ChatService.ChatServiceBase
    {
        
        private ConcurrentDictionary<string, StreamBuffer<Any>> NotifyChannels { get; } = new ConcurrentDictionary<string, StreamBuffer<Any>>();

        private LogServer Server { get; }

        public ChatService(LogServer server)
        {
            this.Server = server;
        }

        [Auth]
        public override async Task Login(C2CH_Login request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var heroName = request.HeroName;
            var loginServer = Application.S.LoginServers.FirstOrDefault();
            if (loginServer == null)
            {
                Debuger.LogError($"No found login server");
                return;
            }

            //login server 
            Debuger.Log($"{request.AccountID} Join chat");
            
            var r = await C<LoginBattleGameServerService.LoginBattleGameServerServiceClient>.RequestOnceAsync(
                loginServer.ServicsHost,
                async ( c) => 
                    await c.CheckSessionAsync(
                        new S2L_CheckSession { UserID = request.AccountID, Session = request.Token },
                        headers: context.GetTraceMeta()));
            //check login session token
            if (r.Code != ErrorCode.Ok) return;
            //if (!Server.TryCreateSession(request.AccountID, out string session)) return;
            await CloseChannel(request.AccountID);

            //update database states
            if (!await DataBase.S
                    .Online(
                        request.AccountID, 
                        Application.S.Config.ChatServerID,
                        request.HeroName,
                        request.Token)) return;
            

            var account = request.AccountID;
            var accountChannel = new AsyncStreamBuffer<Any>(200);
            if (!NotifyChannels.TryAdd(account, accountChannel))
                throw new Exception($"Account:[{account}] had join chat");

            if (!await context.WriteSession(account, Server)) return;
            
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
                await foreach (var i in accountChannel.TryPullAsync(context.CancellationToken))
                {
                    await responseStream.WriteAsync(i).ConfigureAwait(false);
                }
            }
            catch
            {
                Debuger.Log($"Response Close");
            }

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

        public override async Task<CH2C_Chat> SendChat(C2CH_Chat request, ServerCallContext context)
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

        public override async Task<CH2C_QueryPlayerState> QueryPlayerState(C2CH_QueryPlayerState request, ServerCallContext context)
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

        public override async Task<CH2C_LinkFriend> LinkFriend(C2CH_LinkFriend request, ServerCallContext context)
        {
            var account = context.GetAccountId();
            if (await DataBase.S.LinkFriend(account, request.FriendId))
            {
                return new CH2C_LinkFriend { Code = ErrorCode.Ok };
            }
            return new CH2C_LinkFriend { Code = ErrorCode.Error };
        }

        public override async Task<CH2C_UnLinkFriend> UnLinkFriend(C2CH_UnLinkFriend request, ServerCallContext context)
        {
            var account = context.GetAccountId();
            return new CH2C_UnLinkFriend
            {
                Code = await DataBase.S.UnLinkFriend(account, request.FriendId) ? ErrorCode.Ok : ErrorCode.Error
            };
        }

        private async Task CloseChannel(string account)
        {
            if (NotifyChannels.TryRemove(account, out var notify)) notify.Close();
            await Task.CompletedTask;
        }

        public async Task<bool> NotifyFriendStateChange(string account, params Any[] state)
        {
            if (this.NotifyChannels.TryGetValue(account, out var chan))
            {
                foreach (var i in state) chan.Push(i);
                return await Task.FromResult(true);
            }

            Debuger.LogWaring($"Not found {account} on chat server");
            return await Task.FromResult(false);
        }

        public override async Task<CH2C_QueryFriend> QueryFriend(Proto.Void request, ServerCallContext context)
        {
            var list = await DataBase.S.QueryFriend(context.GetAccountId());
            return new CH2C_QueryFriend { Code = ErrorCode.Ok,  States = { list } };
        }
    }
}
