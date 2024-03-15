#define USEGM

#pragma warning disable CS1998
using System;
using System.Linq;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GServer.Managers;
using GServer.MongoTool;
using GServer.Utility;
using MongoDB.Driver;
using Proto;
using Proto.MongoDB;
using XNet.Libs.Utility;
using static GServer.MongoTool.DataBase;
using static Proto.ItemsShop.Types;

// ReSharper disable once CheckNamespace
// ReSharper disable once IdentifierTypo
namespace GServer.RPCResponsor
{
    public class GateServerService : Proto.GateServerService.GateServerServiceBase
    {

       
        
        public override async Task<G2C_BuyPackageSize> BuyPackageSize(C2G_BuyPackageSize req , ServerCallContext context)
        {
            var accountUuid = context.GetAccountId();
            return await UserDataManager.S.BuyPackageSize(accountUuid, req.SizeCurrent);
        }

        public override async Task<G2C_CreateHero> CreateHero(C2G_CreateHero request, ServerCallContext context)
        {
            var accountUuid = context.GetAccountId();
            
            return await UserDataManager.S
                .TryToCreateUser( accountUuid, request.HeroID, request.HeroName);
        }

        public override async Task<G2C_EquipmentLevelUp> EquipmentLevelUp(C2G_EquipmentLevelUp request, ServerCallContext context)
        {
            var accountUuid = context.GetAccountId();
             return await UserDataManager.S.EquipLevel( accountUuid, request.Guid, request.Level);
        }

        public override  async Task<G2C_GMTool> GMTool(C2G_GMTool request, ServerCallContext context)
        {
            var account = context.GetAccountId();
#if USEGM
            if (!Application.S.Config.EnableGM) return new G2C_GMTool { Code = ErrorCode.Error };
            var args = request.GMCommand.Split(' ');
            if (args.Length == 0) return new G2C_GMTool { Code = ErrorCode.Error };
            var player = await UserDataManager.S.FindPlayerByAccountId(account);
            switch (args[0].ToLower())
            {
                case "level":
                {
                    if (int.TryParse(args[1], out var level))
                    {
                        var update = Builders<GameHeroEntity>.Update.Set(t => t.Level, level);
                        await DataBase.S.Heros.FindOneAndUpdateAsync(t => t.PlayerUuid == player.Uuid, update);
                    }
                }
                    break;
                case "make":
                {
                    int id = int.Parse(args[1]);
                    var num = 1;
                    if (args.Length > 2) num = int.Parse(args[2]);
                    await UserDataManager.S.AddItems(player.Uuid, new PlayerItem {ItemID = id, Num = num});
                }
                    break;
                case "addgold":
                {
                    var gold = int.Parse(args[1]);
                    await UserDataManager.S.AddGoldAndCoin(player.Uuid, 0, gold); //.Wait();
                }
                    break;
                case "addcoin":
                {
                    var coin = int.Parse(args[1]);
                    await UserDataManager.S.AddGoldAndCoin(player.Uuid, coin, 0); //.Wait();
                }
                    break;
                case "addexp":
                {
                    var exp = int.Parse(args[1]);
                    await UserDataManager.S.HeroGetExperiences(player.Uuid, exp);
                }

                    break;
                case "addtp":
                {
                    var tp = int.Parse(args[1]);
                    await UserDataManager.S.AddTalentPoint(player.Uuid, tp);
                }
                    break;
                case "actp":
                {
                    var tp = int.Parse(args[1]);
                    await UserDataManager.S.ActiveTalent(tp, account);
                }
                    break;

            }

            await UserDataManager.S.SyncToClient(account,player.Uuid, true, true);
            return new G2C_GMTool
            {
                Code = ErrorCode.Ok
            };
#else
            return new G2C_GMTool { Code = ErrorCode.Error };
#endif
        }

        [Auth]
        public override async Task<G2C_Login> Login(C2G_Login request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Session)) return new G2C_Login { Code = ErrorCode.Error };

            var check = new S2L_CheckSession
            {
                Session = request.Session,
                UserID = request.UserID
            };

            //random 修改策略
            var loginServer = Application.S.LoginServers.NextServer();
            if (loginServer == null)
            {
                Debuger.LogError($"no found login server");
                return new G2C_Login();
            }

            var req = await C<LoginBattleGameServerService.LoginBattleGameServerServiceClient>
                .RequestOnceAsync(loginServer.ServicsHost,
                    async (c) =>
                        await c.CheckSessionAsync(check, cancellationToken: context.CancellationToken,
                            headers: context.GetTraceMeta()));

            if (req.Code != ErrorCode.Ok)
            {
                return new G2C_Login { Code = req.Code };
            }

            var player = await UserDataManager.S.FindAndUpdateLastLogin(request.UserID, context.Peer);
            if (!await context.WriteSession(request.UserID, Application.S.ListenServer)) return new G2C_Login();

            DHero dHero = null;
            PlayerPackage playerPackage = null;
            if (player != null)
            {
                var hero = await UserDataManager.S.FindHeroByPlayerId(player.Uuid);
                var package = await UserDataManager.S.FindPackageByPlayerId(player.Uuid);
                dHero = hero.ToDHero(package);
                playerPackage = package.ToPackage();
            }

            return new G2C_Login
            {
                Code = ErrorCode.Ok,
                HavePlayer = player != null,
                Hero = dHero,
                Package = playerPackage,
                Coin = player?.Coin ?? 0,
                Gold = player?.Gold ?? 0
            };
        }

        public override async Task<G2C_MagicLevelUp> MagicLevelUp(C2G_MagicLevelUp req, ServerCallContext context)
        {
            return await UserDataManager.S.MagicLevelUp( req.MagicId, req.Level, context.GetAccountId());
        }

        public  override async Task<G2C_TalentActive> TalentActive(C2G_TalentActive request, ServerCallContext context)
        {
            return await UserDataManager.S.ActiveTalent(request.MagicId, context.GetAccountId());
        }

   
        public override async Task< G2C_OperatorEquip> OperatorEquip(C2G_OperatorEquip request, ServerCallContext context)
        {
            var accountUuid = context.GetAccountId();
            var player = await UserDataManager.S.FindPlayerByAccountId(accountUuid);
            if (player == null)  return new G2C_OperatorEquip { Code = ErrorCode.NoGamePlayerData };
            var result = await UserDataManager.S.OperatorEquip(player.Uuid, request.Guid, request.Part, request.IsWear);
            if (result) await UserDataManager.S.SyncToClient(accountUuid, player.Uuid,true);
            return new G2C_OperatorEquip
            {
                Code = !result ? ErrorCode.Error : ErrorCode.Ok,
            };
        }

        public override async Task<G2C_Shop> QueryShop(C2G_Shop req, ServerCallContext context)
        {
            Debuger.Log(req);
            var shops = ExcelToJSONConfigManager.Find<ItemShopData>();
            if (shops.Length == 0) return new G2C_Shop { Code = ErrorCode.NoItemsShop };
            var res = new G2C_Shop { Code = ErrorCode.Ok };
            foreach (var i in shops)
            {
                res.Shops.Add(i.ToItemShop());
            }
            return res;
        }

        public override async Task<G2C_BuyItem> BuyItem(C2G_BuyItem req, ServerCallContext context)
        {
            var shop =  ExcelToJSONConfigManager.GetId<ItemShopData>(req.ShopId);
            if (shop == null)
            {
                return new G2C_BuyItem { Code = ErrorCode.NoItemsShop };
            }

            var itemShop = shop.ToItemShop();
            ShopItem item = null;
            foreach (var i in itemShop.Items)
            {
                if (i.ItemId != req.ItemId) continue;
                item = i;
                break;
            }
            if (item == null) return new G2C_BuyItem { Code = ErrorCode.NoFoundItemInShop };

            return new G2C_BuyItem
            {
                Code = await UserDataManager.S.BuyItem( context.GetAccountId(), item)
            };
        }

        public override async Task<G2C_SaleItem> SaleItem(C2G_SaleItem req,ServerCallContext context)
        {
                return await UserDataManager .S
               .SaleItem(context.GetAccountId(), req.Items);
        }

        public override async Task<G2C_BuyGold> BuyGold(C2G_BuyGold req,ServerCallContext context)
        {
                return await UserDataManager
                .S
                .BuyGold(context.GetAccountId(), req.ShopId);
        }

        public override async Task<G2C_RefreshEquip> RefreshEquip(C2G_RefreshEquip req,ServerCallContext context)
        {
            return await UserDataManager.S.RefreshEquip(context.GetAccountId(), req.EquipUuid, req.CoustomItem);
        }

        public override async Task<G2C_ActiveMagic> ActiveMagic(C2G_ActiveMagic req,ServerCallContext context)
        {
            return await UserDataManager.S.ActiveMagic(context.GetAccountId(), req.MagicId);
        }

  }
}
