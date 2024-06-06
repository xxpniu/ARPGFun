using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using Proto;
using Proto.MongoDB;
using ServerUtility;

namespace GServer.MongoTool
{
    public class DataBase : XSingleton<DataBase>
    {

        public class PackageEquip
        {
            public PackageEquip() { Properties = new Dictionary<HeroPropertyType, int>(); }
            public int RefreshCount { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<HeroPropertyType,int> Properties { set; get; }
        }

        public class PackageItem
        {

            public PackageItem() { this.EquipData = new PackageEquip(); }

            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }

            public int Id { set; get; }

            public int Level { set; get; }

            public int Num { set; get; }

            public bool IsLock { set; get; }
            
            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            public DateTime CreateTime { set; get; }
            
            public PackageEquip EquipData { set; get; }
        }

        public class GamePackageEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }
            [BsonElement("item")]
            public List<PackageItem> Items { set; get; } = new List<PackageItem>();

            [BsonElement("size")]
            public int PackageSize { set; get; }
            [BsonElement("puuid")]
            public string PlayerUuid { set; get; }

            public bool TryGetItem(string uuid, out PackageItem item)
            {
                foreach (var i in Items)
                {
                    if (i.Uuid != uuid) continue;
                    item = i;
                    return true;
                }
                item = null;
                return false;
            }

        }

        public class GameHeroEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("_id")]
            public string Uuid { set; get; }
            public string PlayerUuid { set; get; }
            public int Exp { set; get; }
            public int Level { set; get; }
            
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<int, DBHeroMagic> Magics { set; get; }
            
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<int, string> Equips { set; get; }
            
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<int,DBHeroTalent> Talents { set; get; }
            
            public string HeroName { set; get; }
            public int HeroId { set; get; }
            public int HP { set; get; }
            public int MP { set; get; }
            
            public int TalentPoint { set; get; }
            public GameHeroEntity()
            {
                Magics = new Dictionary<int, DBHeroMagic>();
                Equips = new Dictionary<int, string>();
                Talents = new Dictionary<int, DBHeroTalent>();
            }
        }

        public class GameWareroom
        {
            public int Size { set; get; }
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            [BsonElement("item")]
            public Dictionary<string, PackageItem> Items { set; get; }
            [BsonElement("puuid")]
            public string PlayerUuid { set; get; }
        }

        static DataBase()
        {
            BsonClassMap.RegisterClassMap<GamePlayerEntity>(
            (cm) =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Uuid).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }

        private const string PLAYER = "Player";
        private const string HERO = "Hero";
        private const string PACKAGE = "Package";
        private const string WAREROOM = "Wareroom";

        public IMongoCollection<GamePlayerEntity> Playes { get; private set; }
        public IMongoCollection<GameHeroEntity> Heros { get; private set; }
        public IMongoCollection<GamePackageEntity> Packages { get; private set; }
        public IMongoCollection<GameWareroom> Warerooms { get; private set; }

        public void Init(string connectString, string dbName)
        {
            var mongo = new MongoClient(connectString);
            var db = mongo.GetDatabase(dbName);
            Playes = db.GetCollection<GamePlayerEntity>(PLAYER);
            Heros = db.GetCollection<GameHeroEntity>(HERO);
            Packages = db.GetCollection<GamePackageEntity>(PACKAGE);
            Warerooms = db.GetCollection<GameWareroom>(WAREROOM);
        }
    }
}
