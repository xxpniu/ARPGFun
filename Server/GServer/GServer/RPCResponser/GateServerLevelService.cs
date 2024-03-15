using System.Threading.Tasks;
using Grpc.Core;
using GServer.Managers;
using Proto;
using XNet.Libs.Utility;

namespace GServer.RPCResponser
{
    public class GateServerLevelService : Proto.GateServerLevelService.GateServerLevelServiceBase
    {
        
        public override async Task<G2C_UseItem> UseItem(C2G_UseItem request, ServerCallContext context)
        {
            var playerUuid = await UserDataManager.S.FindPlayerByAccountId(context.GetAccountId());
            var (res, _, _) = await UserDataManager.S.UseItem(playerUuid.Uuid, request.ItemId, request.Num);
            return new G2C_UseItem { Code = res };
        }

        public override async Task<G2C_LocalBattleFinished> LocalBattleFinished(C2G_LocalBattleFinished request, ServerCallContext context)
        {
            
            return new G2C_LocalBattleFinished
            {
                Code = ErrorCode.Exception
            };
        }

    }
}