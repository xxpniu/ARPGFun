using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GServer;
using GServer.Managers;
using Proto;
using Utility;

namespace GateServer
{

    public class GateBattleServerService : GateServerInnerService.GateServerInnerServiceBase
    {

        public override async Task<G2B_BattleReward> BattleReward(B2G_BattleReward request, ServerCallContext context)
        {
            ErrorCode code = ErrorCode.Ok;

            var uuid = await UserDataManager.S.ProcessBattleReward(
                request.AccountUuid,
                request.ModifyItems,
                request.RemoveItems,
                request.Exp,
                request.Level,
                request.DiffGold,
                request.HP,
                request.MP);

            if (string.IsNullOrEmpty(uuid)) return new G2B_BattleReward { Code = ErrorCode.Error };
            await UserDataManager.S.SyncToClient(request.AccountUuid, uuid, true, true);
            return new G2B_BattleReward { Code = code };
        }
     
        public override async Task<G2B_GetPlayerInfo> GetPlayerInfo(B2G_GetPlayerInfo request, ServerCallContext context)
        {
            return await GetPlayer(request.AccountUuid);
        }

        private  async Task<G2B_GetPlayerInfo> GetPlayer(string accountID)
        {
            var player = await UserDataManager.S.FindPlayerByAccountId(accountID);

            if (player == null)
            {
                return new G2B_GetPlayerInfo
                {
                    Code = ErrorCode.NoGamePlayerData
                };
            }

            var package = await UserDataManager.S.FindPackageByPlayerID(player.Uuid);
            var hero = await UserDataManager.S.FindHeroByPlayerId(player.Uuid);
            return new G2B_GetPlayerInfo
            {
                Code = ErrorCode.Ok,
                Gold = player.Gold,
                Package = package.ToPackage(),
                Hero = hero.ToDhero(package)
            };
        }

        public override async Task<Void> KillUser(L2G_KillUser request, ServerCallContext context)
        {
            Application.S.StreamService.TryClose(request.Uuid);

            {
                var match = Application.S.MatchServers.FirstOrDefault();
                if (match != null)
                {
                    var chn = new LogChannel(match.ServicsHost);
                    await chn.CreateClient<MatchServices.MatchServicesClient>().KllUserAsync(new S2M_KillUser { UserID = request.Uuid });
                    await chn.ShutdownAsync();
                }
            }

            return await Task.FromResult(new Void());
        }
    }
}
