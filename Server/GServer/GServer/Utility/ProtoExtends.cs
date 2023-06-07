using System.Collections.Generic;
using EConfig;
using Grpc.Core;
using Proto;
using Proto.MongoDB;
using static GateServer.DataBase;
using static Proto.ItemsShop.Types;

namespace GateServer
{
    public static class ProtoExtends
    {
        public static DHero ToDhero(this GameHeroEntity entity, GamePackageEntity package)
        {
            var h = new DHero
            {
                Exprices = entity.Exp,
                HeroID = entity.HeroId,
                Level = entity.Level,
                Name = entity.HeroName,
                HP = entity.HP,
                MP = entity.MP
            };


            foreach (var i in entity.Magics)
            {
                h.Magics.Add(new HeroMagic { MagicKey = i.Key, Level = i.Value.Level });
            }

            foreach (var i in entity.Equips)
            {
                if (package.TryGetItem(i.Value, out PackageItem item))
                {
                    h.Equips.Add(new WearEquip
                    {
                        Part = (EquipmentType)i.Key,
                        GUID = i.Value,
                        ItemID = item.Id
                    });
                }
            }

            return h;
        }

        public static PackageItem ToPackageItem(this PlayerItem item)
        {
            var pItem = new PackageItem
            {
                Id = item.ItemID,
                IsLock = item.Locked,
                Level = item.Level,
                Num = item.Num,
                Uuid = item.GUID
            };

            if (item.Data != null)
            {
                pItem.EquipData.RefreshCount = item.Data.RefreshTime;
                foreach (var i in item.Data.Values)
                {
                    pItem.EquipData.Properties.Add((HeroPropertyType)i.Key, i.Value);
                }
            }
            return pItem;
        }

        public static PlayerItem ToPlayerItem(this PackageItem i)
        {
            var item = new PlayerItem
            {
                GUID = i.Uuid,
                ItemID = i.Id,
                Locked = i.IsLock,
                Num = i.Num,
                Level = i.Level,
                Data = new EquipData
                {
                    RefreshTime = i.EquipData?.RefreshCount ?? 0
                }
            };

            foreach (var pro in i.EquipData!.Properties)
            {
                item.Data.Values.Add((int)pro.Key, pro.Value);
            }

            return item;
        }

        public static PlayerPackage ToPackage(this GamePackageEntity entity)
        {
            var p = new PlayerPackage { MaxSize = entity.PackageSize };
            if (entity.Items != null)
            {
                foreach (var i in entity.Items)
                {
                    p.Items.Add(i.Uuid,i.ToPlayerItem());
                }
            }

            return p;
        }

        public static IList<int> SplitToInt(this string str, char sKey = '|')
        {
            var arrs = str.Split(sKey);
            var list = new List<int>();
            foreach (var i in arrs) list.Add(int.Parse(i));
            return list;
        }

        public static ItemsShop ToItemShop(this ItemShopData config)
        {
            var shop = new ItemsShop { ShopId = config.ID };
            var items = config.ItemIds.SplitToInt();
            var nums = config.ItemNums.SplitToInt();
            var prices = config.ItemPrices.SplitToInt();
            var coinType = config.CoinTypes.SplitToInt();
            for (var index = 0; index < items.Count; index++)
            {
                shop.Items.Add(new ShopItem
                {
                    CType = (CoinType)coinType[index],
                    ItemId = items[index],
                    PackageNum = nums[index],
                    Prices = prices[index]
                });
            }

            return shop;
        }

      
    }
}
