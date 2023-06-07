using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GServer.Managers;
using GServer.Utility;
using Proto;
using Utility;

namespace GServer.RPCResponsor
{

    public class GateBattleServerService : GateServerInnerService.GateServerInnerServiceBase
    {

        public override async Task<G2B_BattleReward> BattleReward(B2G_BattleReward request, ServerCallContext context)
        {
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
            return new G2B_BattleReward { Code = ErrorCode.Ok };
        }
     
        public override async Task<G2B_GetPlayerInfo> GetPlayerInfo(B2G_GetPlayerInfo request, ServerCallContext context)
        {
            return await GetPlayer(request.AccountUuid);
        }

        private  async Task<G2B_GetPlayerInfo> GetPlayer(string accountId)
        {
            var player = await UserDataManager.S.FindPlayerByAccountId(accountId);

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
                Hero = hero.ToDHero(package)
            };
        }

        public override async Task<Void> KillUser(L2G_KillUser request, ServerCallContext context)
        {
            Application.S.StreamService.TryClose(request.Uuid);

            {
                var match = Application.S.MatchServers.FirstOrDefault();
                if (match == null) return await Task.FromResult(new Void());
                var chn = new LogChannel(match.ServicsHost);
                var client = await chn.CreateClientAsync<MatchServices.MatchServicesClient>();
                client.KllUserAsync(new S2M_KillUser {UserID = request.Uuid});
                await chn.ShutdownAsync();
            }

            return await Task.FromResult(new Void());
        }
    }
}
