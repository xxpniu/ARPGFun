using System;
using System.Threading.Tasks;
using Grpc.Core;
using Proto;

namespace  Server
{
    public class BattleInnerServices:Proto.BattleInnerServices.BattleInnerServicesBase
    {
        public BattleInnerServices()
        {
        }

        public override async Task<B2M_StartBattle> StartBatle(M2B_StartBattle request, ServerCallContext context)
        {
            var result = await BattleServerApp.S.BeginSimulator(request.Players, request.LevelID);
            return new B2M_StartBattle { Code = !result? ErrorCode.Error: ErrorCode.Ok };
             
        }

        public override async Task<B2M_KillUer> KillUser(M2B_KillUser request, ServerCallContext context)
        {
            BattleServerApp.S.KillUser(request.UserID);
            return await Task.FromResult(new B2M_KillUer { Code = ErrorCode.Ok });
        }
    }
}
