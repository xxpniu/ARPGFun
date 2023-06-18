using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GServer.MongoTool;
using GServer.Utility;
using MongoDB.Driver;
using Proto;
using Proto.MongoDB;
using ServerUtility;
using XNet.Libs.Utility;
using static GServer.MongoTool.DataBase;
using static Proto.ItemsShop.Types;

namespace GServer.Managers
{


    /// <summary>
    /// 管理用户的数据 并且管理持久化
    /// </summary>
    public class UserDataManager :XSingleton<UserDataManager>
    {

        public async Task<GameHeroEntity> FindHeroByPlayerId(string player_uuid)
        {
           
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var query = await  DataBase.S.Heros.FindAsync(filter);

            return query.Single();
        }

        public async Task<GameHeroEntity> FindHeroByAccountId(string accountID)
        {
            var player = await FindPlayerByAccountId(accountID);
            return await FindHeroByPlayerId(player.Uuid);
        }

        public async Task<string> ProcessBattleReward(string account_uuid,IList<PlayerItem> modifyItems,  IList<PlayerItem> RemoveItems, int exp, int level, int gold,int hp, int mp)
        {
            var player = await FindPlayerByAccountId(account_uuid);
            if (player == null) return null;
            var pupdate = Builders<GamePlayerEntity>.Update.Inc(t =>t.Gold, gold);
            await DataBase.S.Playes.UpdateOneAsync(t => t.Uuid== player.Uuid, pupdate);
            var (ms,rs) = await ProcessRewardItem(player.Uuid, modifyItems,  RemoveItems);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var update = Builders<GameHeroEntity>.Update
                .Set(t => t.Exp, exp)
                .Set(t => t.Level, level)
                .Set(t=>t.HP, hp)
                .Set(t=>t.MP, mp);
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
            await DataBase.S.Heros.UpdateOneAsync(filter, update);
            return  player.Uuid;
        }

        internal async Task<bool> SyncToClient(string accountId, string playerUuid = null, bool syncPlayer = false, bool syncPackage = false)
        {
            if (!Application.S.StreamService.Exist(accountId)) return false;

            GamePlayerEntity player;
            if (string.IsNullOrEmpty(playerUuid))
            {
                player = await FindPlayerByAccountId(accountId);
                if (player == null) return false;
                playerUuid = player.Uuid;
            }
            else
            {
                 player = await FindPlayerById(playerUuid);
            }

            var p = await FindPackageByPlayerID(playerUuid);
            if (syncPackage)
            {
                //var p = await FindPackageByPlayerID(playerUuid);
                var pack = new Task_G2C_SyncPackage
                {
                    Coin = player.Coin,
                    Gold = player.Gold,
                    Package = p.ToPackage()
                };
                Application.S.StreamService.Push(accountId, pack);

            }

            if (!syncPlayer) return true;
            var h = (await FindHeroByPlayerId(playerUuid)).ToDHero(p);
            var hTask = new Task_G2C_SyncHero
            {
                Hero = h
            };
            Application.S.StreamService.Push(accountId, hTask);

            return true;
        }

        internal async Task<G2C_BuyPackageSize> BuyPackageSize( string accountUuid, int size)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var package = await FindPackageByPlayerID(player.Uuid);
            if (package.PackageSize != size) return new G2C_BuyPackageSize { Code = ErrorCode.Error };
            if (player.Coin < Application.Constant.PACKAGE_BUY_COST) return new G2C_BuyPackageSize { Code = ErrorCode.NoEnoughtCoin };
            if (package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE >
                Application.Constant.PACKAGE_SIZE_LIMIT) return new G2C_BuyPackageSize { Code = ErrorCode.PackageSizeLimit };
            {

                await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                    Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -Application.Constant.PACKAGE_BUY_COST));
                await SyncCoinAndGold(accountUuid, player.Coin - Application.Constant.PACKAGE_BUY_COST, player.Gold);
            }

            {
                await DataBase.S.Packages.FindOneAndUpdateAsync(t => t.Uuid == package.Uuid,
                    Builders<GamePackageEntity>.Update.Inc(t => t.PackageSize, Application.Constant.PACKAGE_BUY_SIZE)
                    );
            }



            Application.S.StreamService.Push(accountUuid,
                new Task_PackageSize { Size = package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE });
           

            return new G2C_BuyPackageSize
            {
                Code = ErrorCode.Ok,
                OldCount = size,
                PackageCount = package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE
            };
        }

        private async Task<GamePlayerEntity> FindPlayerById(string player_uuid)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);
            var query = await DataBase.S.Playes.FindAsync(filter);
            return query.SingleOrDefault();
        }

        public async Task<GamePlayerEntity> FindPlayerByAccountId(string account_uuid)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, account_uuid);
            var query = await DataBase.S.Playes.FindAsync(filter);
            return query.SingleOrDefault();
        }

        public async Task<GamePackageEntity> FindPackageByPlayerID(string player_uuid)
        {
            var filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var query = await DataBase.S.Packages.FindAsync(filter);
            return query.Single();
        }

        public async Task<G2C_CreateHero> TryToCreateUser( string userID, int heroID, string heroName)
        {

            if (heroName.Length > 12) return  new G2C_CreateHero { Code = ErrorCode.NameOrPwdLeghtIncorrect };

            var character = ExcelToJSONConfigManager.First<CharacterPlayerData>(t => t.CharacterID == heroID);
            if (character == null) return new G2C_CreateHero { Code = ErrorCode.NoHeroInfo };

            Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userID);
            Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName);
            if(
               /*user create*/ (await DataBase.S.Playes.FindAsync(Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userID))).Any() ||
               /*hero name  */ (await DataBase.S.Heros.FindAsync(Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName))).Any()
            )
            {

                return new G2C_CreateHero { Code = ErrorCode.RegExistUserName };
            }

            var player = new GamePlayerEntity
            {
                AccountUuid = userID,
                Coin = Application.Constant.PLAYER_COIN,
                Gold = Application.Constant.PLAYER_GOLD,
                LastIp = string.Empty
            };
            await DataBase.S.Playes.InsertOneAsync(player);
            var hero = new GameHeroEntity
            {
                Exp = 0,
                HeroName = heroName,
                Level = 1,
                PlayerUuid = player.Uuid,
                HeroId = heroID
            };
            await DataBase.S.Heros.InsertOneAsync(hero);

           

            var package = new GamePackageEntity
            {
                PackageSize = Application.Constant.PACKAGE_SIZE,
                PlayerUuid = player.Uuid
            };
            await DataBase.S.Packages.InsertOneAsync(package);

            //init equip
            var data =  ExcelToJSONConfigManager.GetId<CharacterData>(heroID);
            var item =  ExcelToJSONConfigManager.GetId<ItemData>(data.InitEquip);
            if (item != null)
            {

                var (modify, add) = await AddItems(player.Uuid, new PlayerItem { ItemID = item.ID, Num = 1 });
                var equip =  ExcelToJSONConfigManager.GetId<EquipmentData>(data.InitEquip);
                if (equip != null)
                {
                    await OperatorEquip(player.Uuid, add.FirstOrDefault().Uuid, (EquipmentType)equip.PartType, true);
                }
                else
                {
                    Debuger.LogError($"Nofound equip {data.InitEquip}");
                }
            }
            else
            {
                Debuger.LogError($"Nofound Item {data.InitEquip}");
            }
            
            await SyncToClient(userID, player.Uuid, true, true);
            return new G2C_CreateHero { Code = ErrorCode.Ok };

        }

        public async Task<G2C_EquipmentLevelUp> EquipLevel(string account_uuid, string item_uuid, int level)
        {
            var player = await FindPlayerByAccountId(account_uuid);
            var player_uuid = player.Uuid;
            var p_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = await FindPackageByPlayerID(player_uuid);
            if (package == null) return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };

            if (!package.TryGetItem(item_uuid, out PackageItem item))
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NofoundItem };

            var itemconfig =  ExcelToJSONConfigManager.GetId<ItemData>(item.Id);
            if ((ItemType)itemconfig.ItemType != ItemType.ItEquip)
            {
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NeedEquipmentItem };
            }

            //装备获取失败
            var equipconfig =  ExcelToJSONConfigManager.GetId<EquipmentData>(itemconfig.ID);
            if (equipconfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoFoundEquipmentConfig };

            //等级不一样
            if (item.Level != level)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };


            var levelconfig =  ExcelToJSONConfigManager.First<EquipmentLevelUpData>(t =>
                {
                    return t.Level == level + 1 && t.Quality == equipconfig.Quality;
                });

            if (levelconfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error }; ;

            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);


            if (levelconfig.CostGold > 0 && levelconfig.CostGold > player.Gold)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtGold };

            if (levelconfig.CostCoin > 0 && levelconfig.CostCoin > player.Coin)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtCoin };

            if (levelconfig.CostGold > 0)
            {
                player.Gold -= levelconfig.CostGold;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -levelconfig.CostGold);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (levelconfig.CostCoin > 0)
            {
                player.Coin -= levelconfig.CostCoin;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -levelconfig.CostCoin);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (GRandomer.Probability10000(levelconfig.Pro))
            {
                item.Level += 1;
                var update = Builders<GamePackageEntity>.Update.Set(t => t.Items[-1].Level, item.Level);
                var f = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player.Uuid)
                    & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid);
                await DataBase.S.Packages.FindOneAndUpdateAsync(f, update);
                await SyncModifyItems(account_uuid, new[] { item.ToPlayerItem() });
            }

            await SyncCoinAndGold(account_uuid, player.Coin, player.Gold);
            return new G2C_EquipmentLevelUp { Code = ErrorCode.Ok, Level = item.Level };
        }

        internal async Task<IList<GamePlayerEntity>> QueryPlayers(string account)
        {
            var player = await DataBase.S.Playes.FindAsync(t => t.AccountUuid != account);
            return await player.ToListAsync();
        }

        private async Task<bool> SyncModifyItems(string accountID, PlayerItem[] modifies, PlayerItem[] removes = null)
        {
            var task = new Task_ModifyItem();
            if (modifies != null)
            {
                foreach (var i in modifies)
                {
                    task.ModifyItems.Add(i);
                }
            }
            if (removes != null)
            {
                foreach (var i in removes)
                {
                    task.RemoveItems.Add(i);
                }
            }

            return await Task.FromResult(Application.S.StreamService.Push(accountID, task));

        }


        private async Task<bool> SyncCoinAndGold(string accountId, int coin, int gold)
        {
            var task = new Task_CoinAndGold { Coin = coin, Gold = gold };

            return await Task.FromResult( Application.S.StreamService.Push(accountId, task));
        }

        internal async Task<G2C_BuyGold> BuyGold(string accountUuid, int shopId)
        {
            var item = ExcelToJSONConfigManager.GetId<GoldShopData>(shopId);
            if (item == null) return new G2C_BuyGold {Code = ErrorCode.NofoundItem};
            var player = await FindPlayerByAccountId(accountUuid);
            if (player.Coin < item.Prices) return new G2C_BuyGold {Code = ErrorCode.NoEnoughtCoin};

            var update = Builders<GamePlayerEntity>.Update
                .Inc(t => t.Coin, -item.Prices)
                .Inc(t => t.Gold, item.ReceiveGold);
            await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid, update);

            await SyncCoinAndGold(accountUuid, player.Coin - item.Prices, player.Gold + item.ReceiveGold);

            return new G2C_BuyGold
            {
                Code = ErrorCode.Ok,
                Coin = player.Coin - item.Prices,
                Gold = player.Gold + item.ReceiveGold,
                ReceivedGold = item.ReceiveGold
            };
        }

        internal async Task<G2C_ActiveMagic> ActiveMagic( string accountUuid, int magicId)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var config =  ExcelToJSONConfigManager.GetId<CharacterMagicData>(magicId);
            if (config.CharacterID != hero.HeroId) return new G2C_ActiveMagic { Code = ErrorCode.Error };
            if (!hero.Magics.TryGetValue(magicId, out var magic))
            {
                magic = new DBHeroMagic { Actived = true, Exp = 0, Level = 1 };
                hero.Magics.Add(magicId, magic);
                var update = Builders<GameHeroEntity>.Update.Set(t => t.Magics, hero.Magics);
                await DataBase.S.Heros.UpdateOneAsync(t => t.Uuid == hero.Uuid, update);
                await SyncToClient(accountUuid, player.Uuid, true);
                return new G2C_ActiveMagic { Code = ErrorCode.Ok };
            }
            else {
                return new G2C_ActiveMagic { Code = ErrorCode.Error };
            }

        }

        internal async Task<G2C_MagicLevelUp> MagicLevelUp(int magicId, int level, string accountUuid)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var config =  ExcelToJSONConfigManager.GetId<CharacterMagicData>(magicId);
            if (config.CharacterID != hero.HeroId) return new G2C_MagicLevelUp { Code = ErrorCode.Error };
            var levelConfig = ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.Level == level && t.MagicID == magicId);
            if (levelConfig == null)
            {
                return new G2C_MagicLevelUp { Code = ErrorCode.Error };
            }
            if (levelConfig.NeedLevel > hero.Level) return new G2C_MagicLevelUp { Code = ErrorCode.NeedHeroLevel };
            if (levelConfig.NeedGold > player.Gold) return new G2C_MagicLevelUp { Code = ErrorCode.NoEnoughtGold };

            
            if (!hero.Magics.TryGetValue(magicId, out var magic))
            {
                return new G2C_MagicLevelUp { Code = ErrorCode.MagicNoActicted };
            }

            if (levelConfig.NeedGold > 0)
            {
                player.Gold -= levelConfig.NeedGold;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -levelConfig.NeedGold);
                await DataBase.S.Playes.UpdateOneAsync(t => t.Uuid == player.Uuid, update);
                await SyncCoinAndGold(accountUuid, player.Coin, player.Gold);
            }

            magic.Level += 1;

            {
               
                var update = Builders<GameHeroEntity>.Update.Set(t=>t.Magics, hero.Magics);
                await DataBase.S.Heros.UpdateOneAsync(t=>t.Uuid == hero.Uuid, update);
            }

            await SyncToClient(accountUuid, player.Uuid,true);

            return new G2C_MagicLevelUp
            {
                Code = ErrorCode.Ok
            };
        }

        internal async Task<ErrorCode> BuyItem( string acount_id, ShopItem item)
        {
            var player = await FindPlayerByAccountId(acount_id);
            var id = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);

            if (item.CType == CoinType.Coin)
            {
                if (item.Prices > player.Coin) return ErrorCode.NoEnoughtCoin;

                await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                    Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -item.Prices));
                player.Coin -= item.Prices;
            }
            else if (item.CType == CoinType.Gold)
            {
                if (item.Prices > player.Gold) return ErrorCode.NoEnoughtGold;
                await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                  Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -item.Prices));
                player.Gold -= item.Prices;
            }

            var (modifies, add) = await AddItems(player.Uuid, new PlayerItem { ItemID = item.ItemId, Num = item.PackageNum });

            var items = new List<PlayerItem>();
            foreach (var i in modifies) items.Add(i.ToPlayerItem());
            foreach (var i in add) items.Add(i.ToPlayerItem());

            await SyncModifyItems(acount_id, items.ToArray());
            await SyncCoinAndGold(acount_id, player.Coin, player.Gold);
            return ErrorCode.Ok;

        }

        internal async Task<int> HeroGetExprise(string uuid, int exp)
        {

            var hero = await FindHeroByPlayerId(uuid);

            var res = await AddExp(exp + hero.Exp, hero.Level);

            if (res.succes)
            {
                var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
                var update = Builders<GameHeroEntity>.Update.Set(t => t.Level, res.level).Set(t => t.Exp, res.exp);

                await DataBase.S.Heros.UpdateOneAsync(filter,update);
            }

            return  res.level;

        }

        //leveup, ex, exlevel
        private async Task<(bool succes, int exp, int level)> AddExp(int totalExp, int level)
        {
            int exLevel = level;
            int exExp = totalExp;
            var herolevel =  ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == level + 1);
            if (herolevel == null) return (true, exExp, exLevel);

            if (exExp >= herolevel.NeedExp)
            {
                exLevel += 1;
                exExp -= herolevel.NeedExp;
                if (exExp > 0)
                {
                    await AddExp(exExp, exLevel);
                }
            }
            return (true, exExp, exLevel);
        }

        public async Task<G2C_SaleItem> SaleItem( string account, IList<C2G_SaleItem.Types.SaleItem> items)
        {
            var pl = await FindPlayerByAccountId(account);

            var fiterHero = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);
            var fiterPackage = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);

            var h = (await DataBase.S.Heros.FindAsync(fiterHero)).Single();
            var p = (await DataBase.S.Packages.FindAsync(fiterPackage)).Single();


            foreach (var i in items)
            {
                foreach (var e in h.Equips)
                {
                    if (i.Guid == e.Value) return new G2C_SaleItem { Code = ErrorCode.IsWearOnHero };
                }

                if (!p.TryGetItem(i.Guid, out PackageItem item))
                    return new G2C_SaleItem { Code = ErrorCode.NofoundItem };
                if (item.Num < i.Num)
                    return new G2C_SaleItem { Code = ErrorCode.NoenoughItem };
                if (item.IsLock)
                    return new G2C_SaleItem { Code = ErrorCode.Error };
            }

            var total = 0;
            var removes = new List<PackageItem>();
            var modify = new List<PackageItem>();

            var models = new List<WriteModel<GamePackageEntity>>();

            foreach (var i in items)
            {
                if (p.TryGetItem(i.Guid, out PackageItem item))
                {
                    var config = ExcelToJSONConfigManager.GetId<ItemData>(item.Id);
                    if (config.SalePrice > 0) total += config.SalePrice * i.Num;
                    item.Num -= i.Num;
                    if (item.Num == 0)
                    {
                        removes.Add(item);
                        models.Add(new UpdateOneModel<GamePackageEntity>(
                            Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, p.Uuid),
                            Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, x => x.Uuid == item.Uuid))
                            );
                    }
                    else
                    {
                        modify.Add(item);
                        models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, p.Uuid) &
                        Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid),
                        Builders<GamePackageEntity>.Update.Set(t => t.Items[-1].Num, item.Num))
                        );
                    }
                }
            }

            pl.Gold += total;


            var u_player = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, total);
            await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == pl.Uuid, u_player);
            await DataBase.S.Packages.BulkWriteAsync(models);

            await SyncModifyItems(account, modify.Select(t => t.ToPlayerItem()).ToArray(), removes.Select(t => t.ToPlayerItem()).ToArray());
            await SyncCoinAndGold(account, pl.Coin, pl.Gold);
            return new G2C_SaleItem { Code = ErrorCode.Ok, Coin = pl.Coin, Gold = pl.Gold };

        }

        internal async Task<bool> OperatorEquip(string player_uuid, string equip_uuid, EquipmentType part, bool isWear)
        {
            var h_filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);

            var hero = (await DataBase.S.Heros.FindAsync(h_filter)).SingleOrDefault();
            if (hero == null) return false;

            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();

            if (!package.TryGetItem(equip_uuid, out var item)) return false;

            var config =  ExcelToJSONConfigManager.GetId<ItemData>(item.Id);
            if (config == null) return false;

            var equipConfig =  ExcelToJSONConfigManager.GetId<EquipmentData>(item.Id);
            if (equipConfig == null) return false;
            if (equipConfig.PartType != (int)part) return false;

            hero.Equips.Remove((int)part);
            if (isWear)
            {
                hero.Equips.Add((int)part, item.Uuid);
            }

            var update = Builders<GameHeroEntity>.Update.Set(t => t.Equips, hero.Equips);
            await DataBase.S.Heros.UpdateOneAsync(h_filter, update);

            return true;
        }

        private async Task<Tuple<IList<PlayerItem>, IList<PlayerItem>>> ProcessRewardItem(string player_uuid, IList<PlayerItem> modify, IList<PlayerItem> removes)
        {
            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();

            var models = new List<WriteModel<GamePackageEntity>>();
            foreach (var i in modify)
            {
                if (package.TryGetItem(i.GUID, out PackageItem item))
                {
                    var m = i.ToPackageItem();
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid)
                        & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid),
                        Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], m)
                        ));
                }
                else 
                {
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                       Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                       Builders<GamePackageEntity>.Update.Push(t => t.Items, i.ToPackageItem())
                       ));
                }
            }

            foreach (var i in removes)
            {
                models.Add(new UpdateOneModel<GamePackageEntity>(
                          Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                          Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, t=>t.Uuid == i.GUID)));
            }
            if(models.Count>0) await DataBase.S.Packages.BulkWriteAsync(models);

            return Tuple.Create(modify,removes);
        }

        private async Task<PackageItem> GetCanStackItem(int itemID, GamePackageEntity package)
        {
            var it = ExcelToJSONConfigManager.GetId<ItemData>(itemID);
            foreach (var i in package.Items)
            {
                if (i.Id == itemID)
                {
                    if (i.Num < it.MaxStackNum)

                        return await Task.FromResult(i);
                }
            }
            return null;
        }

        public async Task<Tuple<List<PackageItem>,List< PackageItem>>> AddItems(string uuid, PlayerItem i)
        {
            if (i.Num <= 0) return null;
            GamePackageEntity package = await FindPackageByPlayerID(uuid);
            var it =  ExcelToJSONConfigManager.GetId<ItemData>(i.ItemID);
            if (it == null) return null;
            var num = i.Num;
            var modifies = new List<PackageItem>();
            var adds = new List<PackageItem>();
           
            while (num > 0)
            {
                var cItem = await GetCanStackItem(i.ItemID, package);
                if (cItem != null)
                {
                    var remainNum = it.MaxStackNum - cItem.Num;
                    var add = Math.Min(remainNum, num);
                    cItem.Num += add;
                    num -= add;
                    modifies.Add(cItem);
                }
                else
                {
                    var add = Math.Min(num, it.MaxStackNum);
                    num -= add;
                    var itemNum = new PackageItem
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        Id = i.ItemID,
                        IsLock = i.Locked,
                        Level = i.Level,
                        Num = add,
                        CreateTime = DateTime.UtcNow
                    };
                    adds.Add(itemNum);
                }
            }


            var models = new List<WriteModel<GamePackageEntity>>();
            foreach (var a in adds) 
            {
                var push = new UpdateOneModel<GamePackageEntity>(
                    Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, uuid),
                    Builders<GamePackageEntity>.Update.Push(t => t.Items, a));
                models.Add(push);
            }

            foreach (var m in modifies)
            {
                var b = Builders<GamePackageEntity>.Filter;
                models.Add(new UpdateOneModel<GamePackageEntity>(
                    b.Eq(t => t.PlayerUuid, uuid) & b.ElemMatch(t => t.Items, x => x.Uuid == m.Uuid),
                    Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], m))
                    );
            }

            await DataBase.S.Packages.BulkWriteAsync(models);

            return  Tuple.Create(modifies, adds);
        }

        public async Task<bool> AddGoldAndCoin(string player_uuid, int coin, int gold)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);
            var player = (await DataBase.S.Playes.FindAsync(filter)).Single();
            var up = Builders<GamePlayerEntity>.Update;
            UpdateDefinition<GamePlayerEntity> update = null;

            if (coin > 0)
            {
                update = up.Inc(t => t.Coin,coin);
            }

            if (gold > 0)
            {
                update = up.Inc(t => t.Gold,gold);
            }

            var result = await DataBase.S.Playes.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        internal async Task<G2C_RefreshEquip> RefreshEquip( string accountId, string equipUuid, IList<string> customItem)
        {
            var player = await FindPlayerByAccountId(accountId);
            var package = await FindPackageByPlayerID(player.Uuid);

            if (!package.TryGetItem(equipUuid, out var equip)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };

            var config =  ExcelToJSONConfigManager.GetId<ItemData>(equip.Id);
            if (config == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
            var refreshData =  ExcelToJSONConfigManager.GetId<EquipRefreshData>(config.Quality);
            int refreshCount = equip.EquipData?.RefreshCount ?? 0;
            if (refreshData.MaxRefreshTimes <= refreshCount) return new G2C_RefreshEquip { Code = ErrorCode.RefreshTimeLimit };
            if (refreshData.NeedItemCount > customItem.Count) return new G2C_RefreshEquip { Code = ErrorCode.NoenoughItem };
            Dictionary<HeroPropertyType, int> values = new Dictionary<HeroPropertyType, int>();
            var removes = new List<PackageItem>();
            foreach (var i in customItem)
            {
                if (!package.TryGetItem(i, out PackageItem custom)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
                var itemConfig =  ExcelToJSONConfigManager.GetId<ItemData>(custom.Id);
                if ((ItemType)itemConfig.ItemType != ItemType.ItEquip) return new G2C_RefreshEquip { Code = ErrorCode.NeedEquipmentItem };
                if (custom.Uuid == equipUuid) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
                if (itemConfig.Quality < refreshData.NeedQuality) return new G2C_RefreshEquip { Code = ErrorCode.NeedItemQuality };
                var equipConfig =  ExcelToJSONConfigManager.GetId<EquipmentData>(itemConfig.ID);
                if (equipConfig == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
                var pre = equipConfig.Properties.SplitToInt();
                var vals = equipConfig.PropertyValues.SplitToInt();
                for (var index = 0; index < pre.Count; index++)
                {
                    var p = (HeroPropertyType)pre[index];
                    var d = ExcelToJSONConfigManager.GetId<RefreshPropertyValueData>(pre[index]);
                    if (d == null) continue;
                    if (values.ContainsKey(p))
                    {
                        values[p] += vals[index];
                    }
                    else
                    {
                        values.Add(p, vals[index]);
                    }
                }
                removes.Add(custom);
            }

            if (values.Count == 0) return new G2C_RefreshEquip { Code = ErrorCode.NoPropertyToRefresh };

            if (refreshData.CostGold > player.Gold) return new G2C_RefreshEquip { Code = ErrorCode.NoEnoughtGold };
            var appendCount = GRandomer.RandomMinAndMax(refreshData.PropertyAppendCountMin, refreshData.PropertyAppendCountMax);

            while (appendCount > 0)
            {
                appendCount--;
                var property = GRandomer.RandomMinAndMax(refreshData.PropertyAppendMin, refreshData.PropertyAppendMax);
                var selected = GRandomer.RandomList(values.Keys.ToList());
                var val =  ExcelToJSONConfigManager.GetId<RefreshPropertyValueData>((int)selected);
                var appendValue = val.Value * property;
                //Debug.Assert(equip.EquipData != null, "equip.EquipData != null");
                if (equip.EquipData.Properties.ContainsKey(selected))
                {
                    equip.EquipData.Properties[selected] += appendValue;
                }
                else
                {
                    equip.EquipData.Properties.Add(selected, appendValue);
                }
            }

            equip.EquipData.RefreshCount++;

            var models = new List<WriteModel<GamePackageEntity>>();

            var modify = new UpdateOneModel<GamePackageEntity>(
                     Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid)
                     & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, c=>c.Uuid == equip.Uuid),
                      Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], equip)
                );
            models.Add(modify);

            foreach (var i in removes)
            {
                var delete = new UpdateOneModel<GamePackageEntity>(
                    Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                    Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, t=>t.Uuid == i.Uuid)
                    );
                models.Add(delete);
            }


            await DataBase.S.Packages.BulkWriteAsync(models);
            await DataBase.S.Playes.FindOneAndUpdateAsync(p => p.Uuid == player.Uuid,
                 Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -refreshData.CostGold));

            await SyncCoinAndGold(accountId, player.Coin, player.Gold - refreshData.CostGold);
            await SyncModifyItems(accountId, new[] { equip.ToPlayerItem() }, removes.Select(t => t.ToPlayerItem()).ToArray());
            return new G2C_RefreshEquip { Code = ErrorCode.Ok };
        }

        public async Task<G2C_TalentActive> ActiveTalent(int talentId, string accountId)
        {
            var player = await FindHeroByAccountId(accountId);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var config = ExcelToJSONConfigManager.GetId<TalentData>(talentId);
            var ids = config.NeedTalentID.SplitToInt();
            foreach (var id in ids)
            {
                if (id <= 0) continue;
                if (!hero.Talents.ContainsKey(id))
                {
                    return new G2C_TalentActive
                    {
                        Code = ErrorCode.NoActiveNeedTp
                    };
                }
            }
            
            if (hero.TalentPoint < config.TpCost)
            {
                return new G2C_TalentActive
                {
                    Code = ErrorCode.NoenoughTp
                };
            }
            
            hero.Talents.Add(config.ID,new DBHeroTalent
            {
                Activied =  true,
                Exp =  0, Level = 0
            });
            

            var update = Builders<GameHeroEntity>.Update;
            var inc = update
                .Inc(t => t.TalentPoint, -config.TpCost)
                .Set(h=>h.Talents,hero.Talents);
            //need todo 
            var doc = await DataBase.S.Heros.FindOneAndUpdateAsync(
                h => h.Uuid == hero.Uuid && h.TalentPoint == hero.TalentPoint,
                inc);

            return new G2C_TalentActive
            {
                Code = ErrorCode.Ok
            };
        }

        public async Task<bool> AddTalentPoint(string playerUuid, int tp)
        {
            var hero = await FindHeroByPlayerId(playerUuid);
            var update = Builders<GameHeroEntity>.Update;
            var inc = update
                .Inc(t => t.TalentPoint, tp);
            var doc = await DataBase.S.Heros.FindOneAndUpdateAsync(
                h => h.Uuid == hero.Uuid,
                inc);
            return doc != null;
        }
    }
}

