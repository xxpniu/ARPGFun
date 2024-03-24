using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Grpc.Core;
using GServer.Managers;
using GServer.Utility;
using Proto;
using ServerUtility;
using XNet.Libs.Utility;

namespace GServer.RPCResponser
{
    public class GateServerLevelService : Proto.GateServerLevelService.GateServerLevelServiceBase
    {

        private (int gold, List<PlayerItem> items) DoDrop(int groupId)
        {

            var dropConfig = ExcelToJSONConfigManager.GetId<DropGroupData>(groupId);
            if (dropConfig == null) return (0, null);

            if (!GRandomer.Probability10000(dropConfig.DropPro)) return (0,null);
            var items = dropConfig.DropItem.SplitToInt();
            var pros = dropConfig.Pro.SplitToInt();
            var nums = dropConfig.DropNum.SplitToInt();

            var gold = GRandomer.RandomMinAndMax(dropConfig.GoldMin, dropConfig.GoldMax);
   
            var count = GRandomer.RandomMinAndMax(dropConfig.DropMinNum, dropConfig.DropMaxNum);
            var dropItems = new Dictionary<int, PlayerItem>();
            while (count > 0)
            {
                count--;
                //var offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                var index = GRandomer.RandPro(pros.ToArray());
                var id = items[index];
                var num = nums[index];
                if (!dropItems.TryGetValue(id, out var pt))
                {
                    pt = new PlayerItem
                    {
                        ItemID = id,
                        Num = 0
                    };
                    dropItems.Add(id,pt);
                }

                pt.Num += num;

                //var item = new PlayerItem { ItemID = items[index], Num = nums[index] };
                // Per.CreateItem(it.Pos + offset, item, it.OwnerIndex, it.TeamIndex);
            }

            return (gold, dropItems.Values.ToList());

        }

        public override async Task<G2C_UseItem> UseItem(C2G_UseItem request, ServerCallContext context)
        {
            var playerUuid = await UserDataManager.S.FindPlayerByAccountId(context.GetAccountId());
            var (res, _, _) = await UserDataManager.S.UseItem(playerUuid.Uuid, request.ItemId, request.Num);
            return new G2C_UseItem { Code = res };
        }

        public override async Task<G2C_LocalBattleFinished> LocalBattleFinished(C2G_LocalBattleFinished request,
            ServerCallContext context)
        {

            if (request.Damages.Count == 0) return new G2C_LocalBattleFinished { Code = ErrorCode.Exception };
            //no damage

            var level = ExcelToJSONConfigManager.GetId<BattleLevelData>(request.LevelId);
            if (level == null || level.DropIndex <= 0)
                return new G2C_LocalBattleFinished { Code = ErrorCode.Exception };


            var (gold, items) = DoDrop(level.DropIndex);

            var (code, _, _, _, _) = await UserDataManager.S.ProcessDrop(context.GetAccountId(), gold, 0, items);

            return new G2C_LocalBattleFinished { Code = code, Gold = gold, AddItems = { items } };

        }



    }
}