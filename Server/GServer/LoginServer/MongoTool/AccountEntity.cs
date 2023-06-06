using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoTool
{

    public class AccountEntity
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        public string Uuid { set; get; }
        public string UserName { set; get; }
        public  string Password { set; get; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreateTime { set; get; }
        public int LoginCount { set; get; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastLoginTime { set; get; }
        public int ServerID { set; get; }
    }
}
