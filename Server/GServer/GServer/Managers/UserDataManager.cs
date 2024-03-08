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

        public async Task<GameHeroEntity> FindHeroByPlayerId(string playerUuid)
        {
           
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, playerUuid);
            var query = await  DataBase.S.Heros.FindAsync(filter);

            return query.Single();
        }

        public async Task<GameHeroEntity> FindHeroByAccountId(string accountId)
        {
            var player = await FindPlayerByAccountId(accountId);
            return await FindHeroByPlayerId(player.Uuid);
        }

        public async Task<string> ProcessBattleReward(string accountUuid,IList<PlayerItem> modifyItems,  IList<PlayerItem> removeItems, int exp, int level, int gold,int hp, int mp)
        {
            return await Task.FromResult(string.Empty);
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

            var p = await FindPackageByPlayerId(playerUuid);
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
            var package = await FindPackageByPlayerId(player.Uuid);
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

        public async Task<GamePlayerEntity> FindPlayerByAccountId(string accountUuid)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, accountUuid);
            var query = await DataBase.S.Playes.FindAsync(filter);
            return query.SingleOrDefault();
        }

        public async Task<GamePackageEntity> FindPackageByPlayerId(string playerUuid)
        {
            var filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, playerUuid);
            var query = await DataBase.S.Packages.FindAsync(filter);
            return query.Single();
        }

        public async Task<G2C_CreateHero> TryToCreateUser( string userId, int heroId, string heroName)
        {

            if (heroName.Length > 12) return  new G2C_CreateHero { Code = ErrorCode.NameOrPwdLeghtIncorrect };

            var character = ExcelToJSONConfigManager.First<CharacterPlayerData>(t => t.CharacterID == heroId);
            if (character == null) return new G2C_CreateHero { Code = ErrorCode.NoHeroInfo };

            Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userId);
            Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName);
            if(
               /*user create*/ (await DataBase.S.Playes.FindAsync(Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userId))).Any() ||
               /*hero name  */ (await DataBase.S.Heros.FindAsync(Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName))).Any()
            )
            {

                return new G2C_CreateHero { Code = ErrorCode.RegExistUserName };
            }

            var player = new GamePlayerEntity
            {
                AccountUuid = userId,
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
                HeroId = heroId
            };
            await DataBase.S.Heros.InsertOneAsync(hero);

           

            var package = new GamePackageEntity
            {
                PackageSize = Application.Constant.PACKAGE_SIZE,
                PlayerUuid = player.Uuid
            };
            await DataBase.S.Packages.InsertOneAsync(package);

            //init equip
            var data =  ExcelToJSONConfigManager.GetId<CharacterData>(heroId);
            var item =  ExcelToJSONConfigManager.GetId<ItemData>(data.InitEquip);
            if (item != null)
            {

                var (modify, add) = await AddItems(player.Uuid, new PlayerItem { ItemID = item.ID, Num = 1 });
                var equip =  ExcelToJSONConfigManager.GetId<EquipmentData>(data.InitEquip);
                if (equip != null)
                {
                    await OperatorEquip(player.Uuid, add.FirstOrDefault()!.Uuid, (EquipmentType)equip.PartType, true);
                }
                else
                {
                    Debuger.LogError($"Not found equip {data.InitEquip}");
                }
            }
            else
            {
                Debuger.LogError($"Not found Item {data.InitEquip}");
            }
            
            await SyncToClient(userId, player.Uuid, true, true);
            return new G2C_CreateHero { Code = ErrorCode.Ok };

        }

        public async Task<G2C_EquipmentLevelUp> EquipLevel(string accountUuid, string itemUuid, int level)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var playerUuid = player.Uuid;
            //var pFilter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, playerUuid);
            var package = await FindPackageByPlayerId(playerUuid);
            if (package == null) return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };

            if (!package.TryGetItem(itemUuid, out var item))
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NofoundItem };

            var itemConfig =  ExcelToJSONConfigManager.GetId<ItemData>(item.Id);
            if ((ItemType)itemConfig.ItemType != ItemType.ItEquip)
            {
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NeedEquipmentItem };
            }

            //装备获取失败
            var equipConfig  =  ExcelToJSONConfigManager.GetId<EquipmentData>(itemConfig.ID);
            if (equipConfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoFoundEquipmentConfig };

            //等级不一样
            if (item.Level != level)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };


            var levelConfig =  ExcelToJSONConfigManager
                .First<EquipmentLevelUpData>(t => t.Level == level + 1 && t.Quality == equipConfig.Quality);

            if (levelConfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error }; ;

            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, playerUuid);


            if (levelConfig.CostGold > 0 && levelConfig.CostGold > player.Gold)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtGold };

            if (levelConfig.CostCoin > 0 && levelConfig.CostCoin > player.Coin)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtCoin };

            if (levelConfig.CostGold > 0)
            {
                player.Gold -= levelConfig.CostGold;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -levelConfig.CostGold);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (levelConfig.CostCoin > 0)
            {
                player.Coin -= levelConfig.CostCoin;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -levelConfig.CostCoin);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (GRandomer.Probability10000(levelConfig.Pro))
            {
                item.Level += 1;
                //Items
                var update = Builders<GamePackageEntity>.Update.Set("Items.$.Level", item.Level);
                //var update = Builders<GamePackageEntity>.Update.Set(t => t.Items[0].Level, item.Level);
                var f = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player.Uuid)
                    & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid);
                await DataBase.S.Packages.FindOneAndUpdateAsync(f, update);
                await SyncModifyItems(accountUuid, new[] { item.ToPlayerItem() });
            }

            await SyncCoinAndGold(accountUuid, player.Coin, player.Gold);
            return new G2C_EquipmentLevelUp { Code = ErrorCode.Ok, Level = item.Level };
        }

        internal async Task<IList<GamePlayerEntity>> QueryPlayers(string account)
        {
            var player = await DataBase.S.Playes.FindAsync(t => t.AccountUuid != account);
            return await player.ToListAsync();
        }

        private async Task<bool> SyncModifyItems(string accountId, PlayerItem[] modifies, PlayerItem[] removes = null)
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

            return await Task.FromResult(Application.S.StreamService.Push(accountId, task));

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

        internal async Task<ErrorCode> BuyItem( string accountId, ShopItem item)
        {
            var player = await FindPlayerByAccountId(accountId);
            //var id = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
            var package = await FindPackageByPlayerId(player.Uuid);
            //check package size or full?
            if (package.PackageSize <= package.Items.Count)
                return ErrorCode.PackageSizeLimit;
            
            switch (item.CType)
            {
                case CoinType.Coin when item.Prices > player.Coin:
                    return ErrorCode.NoEnoughtCoin;
                case CoinType.Coin:
                    await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                        Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -item.Prices));
                    player.Coin -= item.Prices;
                    break;
                case CoinType.Gold when item.Prices > player.Gold:
                    return ErrorCode.NoEnoughtGold;
                case CoinType.Gold:
                    await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                        Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -item.Prices));
                    player.Gold -= item.Prices;
                    break;
            }

            var (modifies, add) = await AddItems(player.Uuid, new PlayerItem
            {
                ItemID = item.ItemId,
                Num = item.PackageNum
            });

            var items = new List<PlayerItem>();
            foreach (var i in modifies) items.Add(i.ToPlayerItem());
            foreach (var i in add) items.Add(i.ToPlayerItem());

            await SyncModifyItems(accountId, items.ToArray());
            await SyncCoinAndGold(accountId, player.Coin, player.Gold);
            
            return ErrorCode.Ok;

        }

        internal async Task<int> HeroGetExperiences(string uuid, int exp)
        {
            var hero = await FindHeroByPlayerId(uuid);
            var (success, lExp, tLevel) = await AddExp(exp + hero.Exp, hero.Level);
            if (!success) return tLevel;
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
            var update = Builders<GameHeroEntity>.Update
                .Set(t => t.Level,tLevel)
                .Set(t => t.Exp, lExp);
            await DataBase.S.Heros.UpdateOneAsync(filter, update);
            return tLevel;
        }

        //leveup, ex, exlevel
        private static async Task<(bool succes, int exp, int level)> AddExp(int totalExp, int level)
        {
            //var exLevel = level;
            //var exExp = totalExp;
            var heroLevel =  ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == level + 1);
            if (heroLevel == null) return (true, totalExp, level);
            if (totalExp < heroLevel.NeedExp) return (true, totalExp, level);
            
            level += 1;
            totalExp -= heroLevel.NeedExp;
            if (totalExp > 0)
            {
                return await AddExp(totalExp, level);
            }
            
            return (true, totalExp, level);
        }


    
        public async Task<G2C_SaleItem> SaleItem( string account, IList<C2G_SaleItem.Types.SaleItem> items)
        {
            var pl = await FindPlayerByAccountId(account);

            var filterHero = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);
            var filterPackage = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);

            var h = (await DataBase.S.Heros.FindAsync(filterHero)).Single();
            var p = (await DataBase.S.Packages.FindAsync(filterPackage)).Single();


            foreach (var i in items)
            {
                foreach (var e in h.Equips)
                {
                    if (i.Guid == e.Value) return new G2C_SaleItem { Code = ErrorCode.IsWearOnHero };
                }

                if (!p.TryGetItem(i.Guid, out var item))
                    return new G2C_SaleItem { Code = ErrorCode.NofoundItem };
                if (item.Num < i.Num)
                    return new G2C_SaleItem { Code = ErrorCode.NoenoughItem };
                if (item.IsLock)
                    return new G2C_SaleItem { Code = ErrorCode.Error };
            }
            

            var models = new List<WriteModel<GamePackageEntity>>();
            
            var total = 0;
            var removes = new List<PackageItem>();
            var modify = new List<PackageItem>();
            foreach (var i in items)
            {
                if (p.TryGetItem(i.Guid, out var item))
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
                        Builders<GamePackageEntity>.Update.Set( "Items.$.Num", item.Num))
                        );
                    }
                }
            }

            pl.Gold += total;


            var uPlayer = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, total);
            await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == pl.Uuid, uPlayer);
            await DataBase.S.Packages.BulkWriteAsync(models);

            await SyncModifyItems(account, modify.Select(t => t.ToPlayerItem()).ToArray(), removes.Select(t => t.ToPlayerItem()).ToArray());
            await SyncCoinAndGold(account, pl.Coin, pl.Gold);
            return new G2C_SaleItem { Code = ErrorCode.Ok, Coin = pl.Coin, Gold = pl.Gold };

        }

        internal async Task<bool> OperatorEquip(string playerUuid, string equipUuid, EquipmentType part, bool isWear)
        {
            var hFilter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, playerUuid);

            var hero = (await DataBase.S.Heros.FindAsync(hFilter)).SingleOrDefault();
            if (hero == null) return false;

            var paFilter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, playerUuid);
            var package = (await DataBase.S.Packages.FindAsync(paFilter)).Single();

            if (!package.TryGetItem(equipUuid, out var item)) return false;

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
            await DataBase.S.Heros.UpdateOneAsync(hFilter, update);

            return true;
        }
        
        private async Task<PackageItem> GetCanStackItem(int itemId, GamePackageEntity package)
        {
            var it = ExcelToJSONConfigManager.GetId<ItemData>(itemId);
            foreach (var i in package.Items)
            {
                if (i.Id == itemId)
                {
                    if (i.Num < it.MaxStackNum)

                        return await Task.FromResult(i);
                }
            }
            return null;
        }

        /// <summary>
        /// return modifies adds
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public async Task<(List<PackageItem> modifies,List< PackageItem> adds)> AddItems(string uuid, PlayerItem i)
        {
            if (i.Num <= 0) return (null, null);
            var package = await FindPackageByPlayerId(uuid);
            var it =  ExcelToJSONConfigManager.GetId<ItemData>(i.ItemID);
            if (it == null) return (null,null);
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
                    Builders<GamePackageEntity>.Update.Set("Items.$", m))
                    );
            }

            await DataBase.S.Packages.BulkWriteAsync(models);

            return  (modifies, adds);
        }

        public async Task<bool> AddGoldAndCoin(string playerUuid, int coin, int gold)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, playerUuid);
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
            var package = await FindPackageByPlayerId(player.Uuid);

            if (!package.TryGetItem(equipUuid, out var equip)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };

            var config =  ExcelToJSONConfigManager.GetId<ItemData>(equip.Id);
            if (config == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
            var refreshData =  ExcelToJSONConfigManager.GetId<EquipRefreshData>(config.Quality);
            var refreshCount = equip.EquipData?.RefreshCount ?? 0;
            if (refreshData.MaxRefreshTimes <= refreshCount) return new G2C_RefreshEquip { Code = ErrorCode.RefreshTimeLimit };
            if (refreshData.NeedItemCount > customItem.Count) return new G2C_RefreshEquip { Code = ErrorCode.NoenoughItem };
            var values = new Dictionary<HeroPropertyType, int>();
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
                if (!equip.EquipData!.Properties.TryAdd(selected, appendValue))
                {
                    equip.EquipData.Properties[selected] += appendValue;
                }
            }

            equip.EquipData!.RefreshCount++;

            var models = new List<WriteModel<GamePackageEntity>>();

            var modify = new UpdateOneModel<GamePackageEntity>(
                     Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid)
                     & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, c=>c.Uuid == equip.Uuid),
                     Builders<GamePackageEntity>.Update.Set("Items.$", equip)     
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

        public async Task<GamePlayerEntity> FindAndUpdateLastLogin(string uuid, string peer)
        {
            var player = await DataBase.S.Playes
                .FindOneAndUpdateAsync(t => t.AccountUuid == uuid,
                    Builders<GamePlayerEntity>.Update.Set(t => t.LastIp,peer ?? string.Empty));

            return player;
        }

        public async Task<(ErrorCode code,  List<PackageItem> modifies,  List<PackageItem> removes)> UseItem(string pUuid, int itemId, int num = 1)
        {
            var package = await FindPackageByPlayerId(pUuid);
            var models = new List<WriteModel<GamePackageEntity>>();
            var acc = await FindPlayerById(pUuid);
            
            var removes = new List<PackageItem>();
            var modify = new List<PackageItem>();
            foreach (var item in package.Items)
            {
                if (item.Id != itemId) continue;
                //var config = ExcelToJSONConfigManager.GetId<ItemData>(item.Id);
                if (item.Num < num) return (ErrorCode.NoenoughItem, null, null);
                item.Num -= num;
                if (item.Num == 0)
                {
                    removes.Add(item);
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid,pUuid),
                        Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, x => x.Uuid == item.Uuid))
                    );
                }
                else
                {
                    modify.Add(item);
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, pUuid) &
                        Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid),
                        Builders<GamePackageEntity>.Update.Set( "Items.$.Num", item.Num))
                    );
                }
            }

            if (models.Count == 0) return (ErrorCode.NofoundItem, null, null);
            await DataBase.S.Packages.BulkWriteAsync(models);

            await SyncModifyItems(acc.AccountUuid, modify.Select(t => t.ToPlayerItem()).ToArray(), removes.Select(t => t.ToPlayerItem()).ToArray());
            //await SyncCoinAndGold(account, pl.Coin, pl.Gold);
            return (ErrorCode.Ok, modify, removes);

        }
    }
}

