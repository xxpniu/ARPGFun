using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using org.vxwo.csharp.json;
using Proto;
using Proto.ServerConfig;
using XNet.Libs.Utility;

namespace DataBase
{
    public class BattleMatchInfo
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        public string Uuid { set; get; }

        public List<string> Players { set; get; }

        public BattleServerConfig BattleServer { set; get; }

        public string ServerID { set; get; }

        public int LevelID { set; get; }
    }

    public class MatchGroupEntity
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        public string Uuid { set; get; }

        public int LevelID { set; get; }

        public List<MatchPlayer> Players { set; get; }

    }

    public class DataBaseTool:ServerUtility.XSingleton<DataBaseTool>
    {
        public const string BATTLEMATCH = "BattleMatch";
        public const string MATCHGROUP = "MatchGroup";

        public IMongoCollection<BattleMatchInfo> MatchInfos { private set; get; }
        public IMongoCollection<MatchGroupEntity> MatchGroups { private set; get; }

        public async void Init(string connectString, string dbName)
        {
            var mongo = new MongoClient(connectString);
           
            var db = mongo.GetDatabase(dbName);
            await db.DropCollectionAsync(BATTLEMATCH);
            await db.DropCollectionAsync(MATCHGROUP);
            MatchInfos = db.GetCollection<BattleMatchInfo>(BATTLEMATCH);
            MatchGroups = db.GetCollection<MatchGroupEntity>(MATCHGROUP);
        }

        public async Task<(bool, MatchGroupEntity)> TryToCreateGroup(int level, MatchPlayer player)
        {
            var filter = Builders<MatchGroupEntity>.Filter.ElemMatch(t => t.Players, i => i.AccountID == player.AccountID);
            var query = await MatchGroups.FindAsync(filter);
            if (query.Any())
            {
                return (false, null);
            }

            var group = new MatchGroupEntity
            {
                LevelID = level,
                Players = new List<MatchPlayer> { player }
            };

            await MatchGroups.InsertOneAsync(group);
            Debuger.Log($"uuid:{group.Uuid}");
            return (true, group);
        }


        public async Task<(bool res, MatchGroupEntity entity)> TryToJoinGroup(string groupId, MatchPlayer player)
        {
            var filter = Builders<MatchGroupEntity>.Filter.ElemMatch(t => t.Players, i => i.AccountID == player.AccountID);
            var query = await MatchGroups.FindAsync(filter);
            if (query.Any())
            {
                return (false, null);
            }
            await MatchGroups.FindOneAndUpdateAsync(t=>t.Uuid == groupId,Builders<MatchGroupEntity>.Update.Push(t => t.Players, player));

            var group =( await MatchGroups.FindAsync(t => t.Uuid == groupId)).FirstOrDefault();

            return (true, group);
        }

        public async Task<(BattleServerConfig config, int levelID)> QueryMatchByPlayer(string player)
        {
            var fiter = Builders<BattleMatchInfo>.Filter.AnyEq(t => t.Players, player);
            var rest = await MatchInfos.FindAsync(fiter);
            var res = await rest.FirstOrDefaultAsync();

            return (res?.BattleServer, res?.LevelID ?? 0);
        }

        public async Task<BattleMatchInfo> CreateMatch(IList<string> players, BattleServerConfig config,int levelID)
        {
            await RemoveMatchByServerID(config.ServerID);
            var entity = new BattleMatchInfo
            {
                BattleServer = config,
                ServerID = config.ServerID,
                Players = new List<string>(players),
                LevelID = levelID
            };
            await MatchInfos.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> RemoveMatchByServerID(string serverID)
        {
            var rs = await MatchInfos.FindOneAndDeleteAsync(t => t.ServerID == serverID);
            return true;
        }

        public async Task<bool> ExitsMatchByServerId(string serverID)
        {
            var fiter = Builders<BattleMatchInfo>.Filter.Eq(t => t.ServerID, serverID);
            return (await MatchInfos.FindAsync(fiter)).Any();
        }

        public async Task<MatchGroupEntity> QueryMatchGroup(string groupID)
        {
            var query = await MatchGroups.FindAsync(t => t.Uuid == groupID);
            return query.FirstOrDefault();
        }

        public async Task<(bool,MatchGroupEntity)> QueryMatchGroupByPlayer(string account)
        {
            var fiter = Builders<MatchGroupEntity>.Filter.ElemMatch(t => t.Players, i => i.AccountID == account);
            var query = await MatchGroups.FindAsync(fiter);

            var group = query.FirstOrDefault();
            if (group ==null) return (false, null);
            return (true,group);
        }

        internal async Task<(bool, MatchGroupEntity)> QuitMatchGroupByPlayer(string accountID)
        {
            var b = Builders<MatchGroupEntity>.Filter;
            var fiter = b.ElemMatch(t => t.Players, x => x.AccountID == accountID);
            var query = (await MatchGroups.FindAsync(fiter)).FirstOrDefault();

            if (query == null)
            {
                return (false, null);
            }

            var update = Builders<MatchGroupEntity>.Update.PullFilter(t => t.Players, t => t.AccountID == accountID);
            query = await MatchGroups.FindOneAndUpdateAsync(fiter, update);

            if (query != null) Debuger.Log($"{query.Uuid}:{query.Players}");

            if (query != null && query.Players.Count == 0)
            {
                await MatchGroups.DeleteOneAsync(t => t.Uuid == query.Uuid);
            }
            var matchgroup = (await MatchGroups.FindAsync(t => t.Uuid == query.Uuid)).FirstOrDefault();
            await ExitBattleServer(accountID);
            return (matchgroup != null, matchgroup);
        }

        public async Task<bool> ExitBattleServer(string accountID)
        {
            try
            {
                var filter = Builders<BattleMatchInfo>.Filter.AnyEq(t => t.Players, accountID);
                var match =( await MatchInfos.FindAsync(filter)).FirstOrDefault();
                Debuger.Log($"{accountID} Exit {match?.LevelID} by {match?.Uuid}");

                var bUpdate = Builders<BattleMatchInfo>.Update.Pull(t => t.Players,accountID);
                var modify = await MatchInfos.UpdateOneAsync(t=>t.Uuid == match.Uuid, bUpdate);
                return modify.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }
            return false;
        }

    }
}
