using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Core;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using Utility;
using XNet.Libs.Utility;
using static UApp.Utility.Stream;

namespace UApp
{
    public class ChatManager : XSingleton<ChatManager>
    {

        public Action<Chat> OnReceivedChat;
        public Action<PlayerState> OnReceivePlayerStateChange;
        public Action<N_Notify_BattleServer> OnStartBattle;
        
        public Action<N_Notify_MatchGroup> OnMatchGroup;
        public Action<N_Notify_InviteJoinMatchGroup> OnInviteJoinMatchGroup;

        private ResponseChannel<Any> ChatHandleChannel { get; set; }
        private string HeroName { get; set; }
        private LogChannel ChatChannel { get; set; }

        public Dictionary<string, PlayerState> Friends { get; } = new();

        public string Host;
        public int Port;
        
        private  void ShowConnect()
        {
            Windows.UUIPopup.ShowConfirm("Chat_Disconnect".GetLanguageWord(),
                "Chat_Disconnect_content".GetLanguageWord(),
                 async () => { await TryConnectChatServer(UApplication.S.ChatServer, HeroName); });

        }

        public ChatService.ChatServiceClient ChatClient { private set; get; }
        private AsyncServerStreamingCall<Any> LoginCall { get; set; }

        public async Task<bool> TryConnectChatServer(ServiceAddress serviceAddress, string heroName)
        {

            if (ChatHandleChannel?.IsWorking == true) return true;

            Host = serviceAddress.IpAddress;
            Port = serviceAddress.Port;
        
            if (ChatChannel != null)
            {
                await ChatHandleChannel?.ShutDownAsync(false)!;
                LoginCall?.Dispose();
                LoginCall = null;
                await ChatChannel?.ShutdownAsync()!;
            }


            this.HeroName = heroName;
            ChatChannel = new LogChannel(serviceAddress);
            var chat = ChatChannel.CreateClient<ChatService.ChatServiceClient>();// (ChatChannel.CreateLogCallInvoker());
            ChatClient = chat;
            LoginCall = chat.Login(new C2CH_Login
            {
                AccountID = UApplication.S.accountUuid,
                HeroName = HeroName ?? string.Empty,
                Token = UApplication.S.sessionKey
            }, cancellationToken: ChatChannel.ShutdownToken);
            var header = await LoginCall.ResponseHeadersAsync;
            ChatChannel.SessionKey = header.Get("session-key")?.Value ?? string.Empty;
            Debuger.Log($"ChatChannel.SessionKey:{ChatChannel.SessionKey }");

            if (string.IsNullOrEmpty(ChatChannel.SessionKey))
            {
                await ChatChannel.ShutdownAsync();
                ChatChannel = null;
                return false;
            }

            var friend = await chat.QueryFriendAsync(new Proto.Void());

            Friends.Clear();

            foreach (var i in friend.States)
            {
                Friends.Add(i.User.Uuid, i);
            }

            if (!friend.Code.IsOk())
            {
                await ChatChannel.ShutdownAsync();
                ChatChannel = null;
                UApplication.S.ShowError(friend.Code);
                return false;
            }

            ChatHandleChannel = new ResponseChannel<Any>(LoginCall.ResponseStream, true, tag: "ChatHandle")
            {
                OnReceived = OnReceived,
                OnDisconnect = TryConnected
            };

            await Task.Delay(1000);

            var (s, gate) =GateManager.TryGet();
            if(s)  await gate.GateFunction.ReloadMatchStateAsync(new C2G_ReloadMatchState { });
   
            return true;
        }

        private async void OnReceived(Any any)
        {
            await UniTask.SwitchToMainThread();
            Debuger.Log($"{any}");
            if (any.TryUnpack(out Chat msg))
            {
                Debuger.Log($"State:{msg}");
                OnReceivedChat?.Invoke(msg);
            }
            else if (any.TryUnpack(out PlayerState stat))
            {
                Debuger.Log($"State:{stat}");

                Friends.Remove(stat.User.Uuid);
                Friends.Add(stat.User.Uuid, stat);

                OnReceivePlayerStateChange?.Invoke(stat);

                UApplication.S.ShowNotify(stat.State == PlayerState.Types.StateType.Online
                    ? "User_Online".GetAsKeyFormat(stat.User.UserName)
                    : "User_Offline".GetAsKeyFormat(stat.User.UserName));
            }
            else if (any.TryUnpack(out N_Notify_BattleServer battleServer))
            {
                OnStartBattle?.Invoke(battleServer);
                if (!battleServer.ReTry)
                {
                    UApplication.S.GotoBattleGate(battleServer.Server, battleServer.LevelID);
                }
                else
                {
                    Windows.UUIPopup.ShowConfirm("BattleJoinTitle".GetLanguageWord(), "BattleJoinContent".GetLanguageWord(), () => UApplication.S.GotoBattleGate(battleServer.Server, battleServer.LevelID), async () =>
                    {
                        var (b, g) = GateManager.TryGet();
                        if (b) await g.GateFunction.LeaveMatchGroupAsync(new C2G_LeaveMatchGroup());
                    });
                }

                Debuger.Log(battleServer);
            }
            else if (any.TryUnpack(out N_Notify_MatchGroup group))
            {
                OnMatchGroup?.Invoke(group);
                //add player
            }
            else if (any.TryUnpack(out N_Notify_InviteJoinMatchGroup invite))
            {
                //start
                OnInviteJoinMatchGroup?.Invoke(invite);
            }
            else
            {
                Debuger.LogError($"Need handle:{any}");
            }
        }

        private void TryConnected()
        {
            //ChatPushChannel.OnDisconnect = null;
            ChatHandleChannel.OnDisconnect = null;
            ShowConnect();
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            if (ChatHandleChannel != null)
            {
                await ChatHandleChannel.ShutDownAsync(false)!;
                ChatHandleChannel = null;
            }
            if (ChatChannel != null)
            {
                try
                {
                    await ChatChannel.ShutdownAsync();
                }
                catch
                {
                    //ignore
                }

                ChatChannel = null;
            }

            LoginCall?.Dispose();
            LoginCall = null;
            
        }

        public async void SendChat(params Chat[] msg)
        {
            var res =  await ChatClient.SendChatAsync(new C2CH_Chat { Mesg = { msg } });
            if(!res.Code.IsOk()) UApplication.S.ShowError(res.Code);
        }
    }
}

