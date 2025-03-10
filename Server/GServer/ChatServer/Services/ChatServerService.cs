﻿using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;

namespace ChatServer.Services
{

    //消息转发
    public class ChatServerService:Proto.ChatServerService.ChatServerServiceBase
    {
        public override async Task<PlayerState> ChatRoute(Chat request, ServerCallContext context)
        {

            var success = await Application.S.ChatService
                  .NotifyFriendStateChange(request.Receiver.Uuid, Any.Pack(request));

            return new PlayerState
            {
                State = success? PlayerState.Types.StateType.Online: PlayerState.Types.StateType.Offline,
                User= new ChatUser { ChatServerId = Application.S.Config.ChatServerID, Uuid = request.Receiver.Uuid }
            };
           
            //send player state
        }

        public override async Task<Void> CreateNotify(NotifyMsg request, ServerCallContext context)
        {
            await Application.S.ChatService.NotifyFriendStateChange(request.AccountID, request.AnyNotify.ToArray());
            return new Void();
        }
    }
}
