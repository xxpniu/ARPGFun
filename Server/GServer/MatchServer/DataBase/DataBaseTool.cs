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
        private const string BATTLEMATCH = "BattleMatch";
        private const string MATCHGROUP = "MatchGroup";

        private IMongoCollection<BattleMatchInfo> MatchInfos { set; get; }
        private IMongoCollection<MatchGroupEntity> MatchGroups { set; get; }

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
            if (await query.AnyAsync())
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
            if (await query.AnyAsync())
            {
                return (false, null);
            }
            await MatchGroups.FindOneAndUpdateAsync(t=>t.Uuid == groupId,Builders<MatchGroupEntity>.Update.Push(t => t.Players, player));

            var group =( await MatchGroups.FindAsync(t => t.Uuid == groupId)).FirstOrDefault();

            return (true, group);
        }

        public async Task<(BattleServerConfig config, int levelID)> QueryMatchByPlayer(string player)
        {
            var filter = Builders<BattleMatchInfo>.Filter.AnyEq(t => t.Players, player);
            var rest = await MatchInfos.FindAsync(filter);
            var res = await rest.FirstOrDefaultAsync();

            return (res?.BattleServer, res?.LevelID ?? 0);
        }

        public async Task<BattleMatchInfo> CreateMatch(IList<string> players, BattleServerConfig config,int levelId)
        {
            await RemoveMatchByServerId(config.ServerID);
            var entity = new BattleMatchInfo
            {
                BattleServer = config,
                ServerID = config.ServerID,
                Players = new List<string>(players),
                LevelID = levelId
            };
            await MatchInfos.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> RemoveMatchByServerId(string serverId)
        {
            var rs = await MatchInfos.FindOneAndDeleteAsync(t => t.ServerID == serverId);
            return true;
        }

        public async Task<bool> ExitsMatchByServerId(string serverId)
        {
            var filter = Builders<BattleMatchInfo>.Filter.Eq(t => t.ServerID, serverId);
            return await (await MatchInfos.FindAsync(filter)).AnyAsync();
        }

        public async Task<MatchGroupEntity> QueryMatchGroup(string groupId)
        {
            var query = await MatchGroups.FindAsync(t => t.Uuid == groupId);
            return query.FirstOrDefault();
        }

        public async Task<(bool,MatchGroupEntity)> QueryMatchGroupByPlayer(string account)
        {
            var filter = Builders<MatchGroupEntity>.Filter.ElemMatch(t => t.Players, i => i.AccountID == account);
            var query = await MatchGroups.FindAsync(filter);

            var group = query.FirstOrDefault();
            return @group ==null ? (false, null) : (true,@group);
        }

        internal async Task<(bool, MatchGroupEntity)> QuitMatchGroupByPlayer(string accountId)
        {
            var b = Builders<MatchGroupEntity>.Filter;
            var filter = b.ElemMatch(t => t.Players, x => x.AccountID == accountId);
            var query = (await MatchGroups.FindAsync(filter)).FirstOrDefault();

            if (query == null)
            {
                return (false, null);
            }

            var update = Builders<MatchGroupEntity>.Update.PullFilter(t => t.Players, t => t.AccountID == accountId);
            query = await MatchGroups.FindOneAndUpdateAsync(filter, update);

            if (query != null) Debuger.Log($"{query.Uuid}:{query.Players}");

            if (query != null && query.Players.Count == 0)
            {
                await MatchGroups.DeleteOneAsync(t => t.Uuid == query.Uuid);
            }
            var matchgroup = (await MatchGroups.FindAsync(t => t.Uuid == query.Uuid)).FirstOrDefault();
            await ExitBattleServer(accountId);
            return (matchgroup != null, matchgroup);
        }

        public async Task<bool> ExitBattleServer(string accountId)
        {
            try
            {
                var filter = Builders<BattleMatchInfo>.Filter.AnyEq(t => t.Players, accountId);
                var match =( await MatchInfos.FindAsync(filter)).FirstOrDefault();
                Debuger.Log($"{accountId} Exit {match?.LevelID} by {match?.Uuid}");

                var bUpdate = Builders<BattleMatchInfo>.Update.Pull(t => t.Players,accountId);
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
