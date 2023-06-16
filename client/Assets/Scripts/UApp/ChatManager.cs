using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using UApp.GameGates;
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

        public ResponseChannel<Any> ChatHandleChannel { get; private set; }
        public string HeroName { get; private set; }
        public LogChannel ChatChannel { get; private set; }

        public Dictionary<string, PlayerState> Friends { get; } = new Dictionary<string, PlayerState>();

        public string Host;
        public int Port;

 
        private  void ShowConnect()
        {
            Windows.UUIPopup.ShowConfirm("Chat_Disconnect".GetLanguageWord(),
                "Chat_Disconnect_content".GetLanguageWord(),
                 async () => { await TryConnectChatServer(UApplication.S.ChatServer, HeroName); });

        }

        public ChatService.ChatServiceClient ChatClient { private set; get; }
        public AsyncServerStreamingCall<Any> LoginCall { get; private set; }

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
                AccountID = UApplication.S.AccountUuid,
                HeroName = HeroName ?? string.Empty,
                Token = UApplication.S.SesssionKey
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
                OnReceived = (any) =>
                {
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
                        this.OnStartBattle?.Invoke(battleServer);
                        if (!battleServer.ReTry)
                        {
                            UApplication.S.GotoBattleGate(battleServer.Server, battleServer.LevelID);
                        }
                        else
                        {
                            Windows.UUIPopup.ShowConfirm("BattleJoinTitle".GetLanguageWord(), "BattleJoinContent".GetLanguageWord(),
                                    () => UApplication.S.GotoBattleGate(battleServer.Server, battleServer.LevelID),
                                    async () =>
                                    {
                                        var gate = UApplication.G<GMainGate>();
                                        if (gate==null) return;
                                        await gate.GateFunction.LeaveMatchGroupAsync(new C2G_LeaveMatchGroup());
                                    })
                                ;
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
                },
                OnDisconnect = TryConnected
            };

            await Task.Delay(1000);

            var g = UApplication.G<GMainGate>();
            if (g!=null) await  g.GateFunction.ReloadMatchStateAsync(new C2G_ReloadMatchState { });

            return true;
        }

        private void TryConnected()
        {
            //ChatPushChannel.OnDisconnect = null;
            ChatHandleChannel.OnDisconnect = null;
            ShowConnect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ChatHandleChannel?.ShutDownAsync(false);
            LoginCall?.Dispose();
            LoginCall = null;
            _ = ChatChannel?.ShutdownAsync()!;
        }
        public async void SendChat(params Chat[] msg)
        {
            var res =  await ChatClient.SendChatAsync(new C2CH_Chat { Mesg = { msg } });
            if(!res.Code.IsOk()) UApplication.S.ShowError(res.Code);
        }
    }
}

