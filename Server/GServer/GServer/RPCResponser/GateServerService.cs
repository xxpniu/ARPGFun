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

        private static async Task<bool> SendNotify(IMessage notify, params string[] player)
        {
            var notifyServer = Application.S.NotifyServers.NextServer();
            if (notifyServer == null)
            {
                Debuger.LogError($"not found notify server");
                return false;
            }

            var rNotify = new S2N_RouteSendNotify { };
            var any = Any.Pack(notify);
            foreach (var i in player)
            {
                var msg = new NotifyMsg { AccountID = i, AnyNotify = { any } };
                rNotify.Msg.Add(msg);
            }
            
            var res = await C<NotifyServices.NotifyServicesClient>.RequestOnceAsync(
                notifyServer.ServicsHost,
                async ( c)=> await c.RouteSendNotifyAsync(rNotify));
            if (res.Code == ErrorCode.Ok)
            {
                return true;
            }
            else
            {
                Debuger.LogError("Send notify error");
                return false;
            }
        }

        public override async Task<G2C_InviteJoinMatch> InviteJoinMatch(C2G_InviteJoinMatch request, ServerCallContext context)
        {
            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var ntf = new N_Notify_InviteJoinMatchGroup { GroupId = request.GroupID, Inviter = player, LevelID = request.LevelID };
            var res = await SendNotify(ntf, request.AccountUuid);
            return new G2C_InviteJoinMatch { Code = res ? ErrorCode.Ok : ErrorCode.Error };
        }

        public override async Task<G2C_JoinMatch> JoinMatch(C2G_JoinMatch request, ServerCallContext context)
        {

            var matchServer = Application.S.MatchServers.FirstOrDefault();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_JoinMatch();
            }

            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var match = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) =>
                    await c.JoinMatchGroupAsync(
                        new S2M_JoinMatchGroup { GroupID = request.GroupID, Player = player },
                        headers: context.GetTraceMeta()));

            return new G2C_JoinMatch { Code = match.Code };
        }

        public override async Task<G2C_CreateMatch> CreateMatch(C2G_CreateMatch request, ServerCallContext context)
        {
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_CreateMatch { Code = ErrorCode.Error };
            }
            
            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var match = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) =>
                    await c.CreateMatchGroupAsync(new S2M_CreateMatchGroup
                    {
                        Level = request.LevelID,
                        Player = player
                    }, headers: context.GetTraceMeta())

            );


            return new G2C_CreateMatch { Code = match.Code, GroupID = match.GroupID };
        }

        public override async Task<G2C_LeaveMatchGroup> LeaveMatchGroup(C2G_LeaveMatchGroup request, ServerCallContext context)
        {
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new  G2C_LeaveMatchGroup { Code = ErrorCode.Error };
            }

            var quit = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) => 
                    await c.LeaveMatchGroupAsync(
                        new S2M_LeaveMatchGroup { AccountID = context.GetAccountId() },
                        headers: context.GetTraceMeta())
            );
            return new G2C_LeaveMatchGroup { Code = quit.Code };
        }

        public override async Task<G2C_BeginGame> BeginGame(C2G_BeginGame request, ServerCallContext context)
        {
            var userId = context.GetAccountId();
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_BeginGame { Code = ErrorCode.Ok };
            }

            try
            {
                var match = await C<MatchServices.MatchServicesClient>
                    .RequestOnceAsync(
                    matchServer.ServicsHost,
                    async (c)=>  
                        await c.StartMatchAsync(new S2M_StartMatch { GroupID = request.GroupID, Leader = userId },
                            headers: context.GetTraceMeta())
                    );
                return match.Code == ErrorCode.Ok ? new G2C_BeginGame { Code = ErrorCode.Ok } : new G2C_BeginGame { Code = match.Code };
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }

            return new G2C_BeginGame { };
        }

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

        public override async Task<G2C_SearchPlayer> SearchPlayer(C2G_SearchPlayer request, ServerCallContext context)
        {
            var players = await UserDataManager.S.QueryPlayers(context.GetAccountId());

            var res = new G2C_SearchPlayer
            {
                Code = ErrorCode.Ok
            };
            foreach (var i in players)
            {
                res.Players.Add(await GetPlayer(i));
            }
            return res;

            static async Task<G2C_SearchPlayer.Types.Player> GetPlayer(GamePlayerEntity entity)
            {
                var hero = (await UserDataManager.S.FindHeroByPlayerId(entity.Uuid));
                return new G2C_SearchPlayer.Types.Player
                {
                    AccountUuid = entity.AccountUuid,
                    HeroName = hero.HeroName,
                    Level = hero.Level
                };
            }
        }

        public override async Task<G2C_ReloadMatchState> ReloadMatchState(C2G_ReloadMatchState request,
            ServerCallContext context)
        {
            var matchSever = Application.S.MatchServers.NextServer();
            if (matchSever == null) return new G2C_ReloadMatchState { Code = ErrorCode.Error };
            var rejoin = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchSever.ServicsHost,
                async (c) => 
                    await c.TryToReJoinMatchAsync(
                        new S2M_TryToReJoinMatch { Account = context.GetAccountId() },
                        headers: context.GetTraceMeta())
            );

            return new G2C_ReloadMatchState { Code = rejoin.Code };
        }

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
