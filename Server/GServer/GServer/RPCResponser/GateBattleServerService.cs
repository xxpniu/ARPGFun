using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GServer.Managers;
using GServer.Utility;
using Proto;
using XNet.Libs.Utility;

namespace GServer.RPCResponser
{

    public class GateBattleServerService : GateServerInnerService.GateServerInnerServiceBase
    {
        
     
        public override async Task<G2B_GetPlayerInfo> GetPlayerInfo(B2G_GetPlayerInfo request, ServerCallContext context)
        {
            return await GetPlayer(request.AccountUuid);
        }

        private static async Task<G2B_GetPlayerInfo> GetPlayer(string accountId)
        {
            var player = await UserDataManager.S.FindPlayerByAccountId(accountId);

            if (player == null)
            {
                return new G2B_GetPlayerInfo
                {
                    Code = ErrorCode.NoGamePlayerData
                };
            }

            var package = await UserDataManager.S.FindPackageByPlayerId(player.Uuid);
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

            var match = Application.S.MatchServers.FirstOrDefault();
            if (match == null) return await Task.FromResult(new Void());

            await C<MatchServices.MatchServicesClient>.RequestOnceAsync(match.ServicsHost,
                async (c) =>
                    await c.KllUserAsync(new S2M_KillUser { UserID = request.Uuid }, headers: context.GetTraceMeta()));
            
            return new Void();
        }


        public override async Task<G2B_RewardItem> RewardItem(B2G_RewardItem request, ServerCallContext context)
        {
            var player =await UserDataManager.S.FindPlayerByAccountId(request.AccountId);
            var (modifies, adds) = await UserDataManager.S.AddItems(player.Uuid,
                new PlayerItem { ItemID = request.ItemId, Num = request.Num });
            if (modifies == null || adds == null)
            {
                return new G2B_RewardItem
                {
                    Code = ErrorCode.Error
                };
            }

            return new G2B_RewardItem
            {
                Code = ErrorCode.Ok ,
                AddItems = { adds.Select(t=>t.ToPlayerItem()) },
                ModifyItems = { modifies.Select(t=>t.ToPlayerItem())  }
            };
        }

        public override async Task<G2B_UseItem> UseItem(B2G_UseItem request, ServerCallContext context)
        {
            var player =await UserDataManager.S.FindPlayerByAccountId(request.AccountId);
            var (error, mod, remove) = await  UserDataManager.S.UseItem(player.Uuid, request.ItemId, request.Num);

            return new G2B_UseItem
            {
                Code = error,
                ModifyItems =  {mod.Select(t=>t.ToPlayerItem()).ToArray()},
                RemoveItems = { remove.Select(t=>t.ToPlayerItem()).ToArray() }
            };
        }
    }
}
