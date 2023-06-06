using System;
using System.Threading.Tasks;
using Grpc.Core;
using LoginServer;
using MongoDB.Driver;
using MongoTool;
using Proto;
using Proto.MongoDB;
using Utility;

namespace RPCResponsers
{
   
    public class LoginBattleGameServerService :  Proto.LoginBattleGameServerService.LoginBattleGameServerServiceBase
    {
        public override async Task<L2S_CheckSession> CheckSession(S2L_CheckSession req, ServerCallContext context)
        {
            if (!DataBase.S.GetSessionInfo(req.UserID, out UserSessionInfoEntity info)) return new L2S_CheckSession();
            var gate = Appliaction.S.FindGateServer(info.GateServerId);
            if (gate != null) {
                return await Task.FromResult(new L2S_CheckSession
                {
                    Code = info?.Token == req.Session ? ErrorCode.Ok : ErrorCode.Error,
                    GateServerInnerHost =  gate.ServicsHost 
                });
            }
            return new L2S_CheckSession();
        }
    }
}
