using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Proto.MongoDB;
using ServerUtility;

namespace LoginServer.MongoTool
{
    public class DataBase : XSingleton<DataBase>
    {
        public const string ACCOUNT = "Account";
        public const string SESSION = "Session";

        public MongoClient Client { get; set; }

        public IMongoDatabase Data { set; get; }

        public IMongoCollection<AccountEntity> Account { private set; get; }
        public IMongoCollection<UserSessionInfoEntity> Session { private set; get; }

        static DataBase()
        {
            BsonClassMap.RegisterClassMap<UserSessionInfoEntity>(
             cm =>
             {
                 cm.AutoMap();
                 _ = cm.MapIdMember(c => c.Uuid).SetIdGenerator(StringObjectIdGenerator.Instance);
             });
        }

        public async Task Init(string connectString, string db, CancellationToken token = default)
        {
            Client = new MongoClient(connectString);
            Data = Client.GetDatabase(db);
            Account = Data.GetCollection<AccountEntity>(ACCOUNT);
            Session = Data.GetCollection<UserSessionInfoEntity>(SESSION);
            await Session.DeleteManyAsync(Builders<UserSessionInfoEntity>
                .Filter.Exists(t => t.AccountUuid, true), token);
        }

        public  bool GetSessionInfo(string userID, out UserSessionInfoEntity serverInfo)
        {
            var filter = Builders<UserSessionInfoEntity>.Filter.Eq(t => t.AccountUuid, userID);
            var res = S.Session.Find(filter);
            serverInfo = res.FirstOrDefault();
            return serverInfo != null;
        }
    }
}
