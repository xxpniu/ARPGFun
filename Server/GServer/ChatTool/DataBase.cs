using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Proto;
using ServerUtility;

namespace ChatTool
{
    public class DataBase : XSingleton<DataBase>
    {
        public class UserEntity
        {
            public string AcountID { set; get; }
            public bool Active { set; get; }
        }

        public class FriendEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }

            public UserEntity User1 { set; get; }
            public UserEntity User2 { set; get; }
        }

        public class OnlineStateEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }
            public string AccountID { set; get; }
            public string HeroName { set; get; }
            public int ChatServerID { set; get; }
            public bool IsOnline { set; get; }
            public string Token { set; get; }

        }

        public const string FRIEND = "Friend";
        public const string PLAYERSTATE = "PlayerState";


        public IMongoCollection<FriendEntity> Friends { get; private set; }
        public IMongoCollection<OnlineStateEntity> States { get; private set; }

        public void Init(string connectString, string dbName)
        {
            var mongo = new MongoClient(connectString);
            var db = mongo.GetDatabase(dbName);
            Friends = db.GetCollection<FriendEntity>(FRIEND);
            States = db.GetCollection<OnlineStateEntity>(PLAYERSTATE);
            
        }

        public async Task<bool> Online(string uuid, int ServerID,string heroname,string token)
        {
            var update1 = Builders<OnlineStateEntity>.Update
                .Set(t => t.IsOnline, true)
                .Set(t => t.ChatServerID, ServerID)
                .Set(t => t.HeroName, heroname)
                .Set(t=>t.Token, token);
             
            var u = await this.States.FindOneAndUpdateAsync(t => t.AccountID == uuid, update1);
            if (u == null)
            {
                u = new OnlineStateEntity { AccountID = uuid, ChatServerID = ServerID, IsOnline = true,HeroName = heroname };
                await States.InsertOneAsync(u);
            }
            return u !=null;
        }

        public async Task<bool> Offline(string uuid,string token)
        {
            var update1 = Builders<OnlineStateEntity>.Update
                .Set(t => t.IsOnline, false)
                .Set(t => t.ChatServerID, -1);
            var u = await States.FindOneAndUpdateAsync((t) => t.AccountID == uuid && t.Token == token, update1);
            return u!=null;
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="friend"></param>
        /// <returns></returns>
        public async Task<bool> LinkFriend(string owner, string friend)
        {
            var fitler = Builders<FriendEntity>.Filter;

            var query = (fitler.Eq(t => t.User1.AcountID, owner) & fitler.Eq(t => t.User2.AcountID, friend)) |
                (fitler.Eq(t => t.User2.AcountID, owner) & fitler.Eq(t => t.User1.AcountID, friend));

            var u = await Friends.FindAsync(query);
            var en = await u.FirstOrDefaultAsync();
            if (en != null)
            {
                if (en.User1.AcountID == owner)
                {
                    var update = Builders<FriendEntity>.Update.Set(t => t.User1.Active, true);
                    await Friends.UpdateOneAsync(t => t.User1.AcountID == owner, update);
                }
                if (en.User2.AcountID == owner)
                {
                    var update = Builders<FriendEntity>.Update.Set(t => t.User2.Active, true);
                    await Friends.UpdateOneAsync(t => t.User2.AcountID == owner, update);
                }
                return true;
            }
            else
            {
                var entity = new FriendEntity
                {
                    User1 = new UserEntity { AcountID = owner, Active = true },
                    User2 = new UserEntity { AcountID = friend, Active = false }
                };
                await Friends.InsertOneAsync(entity);
                return true;
            }
        }

        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="friend"></param>
        /// <returns></returns>
        public async Task<bool> UnLinkFriend(string owner, string friend)
        {
            var fitler = Builders<FriendEntity>.Filter;

            var query = (fitler.Eq(t => t.User1.AcountID, owner) & fitler.Eq(t => t.User2.AcountID, friend)) |
                (fitler.Eq(t => t.User2.AcountID, owner) & fitler.Eq(t => t.User1.AcountID, friend));

            var u = await Friends.FindAsync(query);
            var en = await u.FirstOrDefaultAsync();
            if (en != null)
            {

                //todo:当双方解绑可以删除记录
                if (en.User1.AcountID == owner)
                {
                    var update = Builders<FriendEntity>.Update.Set(t => t.User1.Active, false);
                    await Friends.UpdateOneAsync(t => t.Uuid == en.Uuid, update);
                }

                if (en.User2.AcountID == owner)
                {
                    var update = Builders<FriendEntity>.Update.Set(t => t.User2.Active, false);
                    await Friends.UpdateOneAsync(t => t.Uuid == en.Uuid, update);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前好友
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public async Task<IList<PlayerState>> QueryFriend(string owner)
        {
            var fitler = Builders<FriendEntity>.Filter;
            var query = (fitler.Eq(t => t.User1.AcountID, owner) & fitler.Eq(t=>t.User1.Active,true))
                |( fitler.Eq(t => t.User2.AcountID, owner) & fitler.Eq(t => t.User2.Active, true));

            var u = await Friends.FindAsync(query);
            var list = new List<string>();

            await u.ForEachAsync(t =>
            {
                if (t.User1.AcountID == owner)
                {
                    list.Add(t.User2.AcountID);
                }
                else
                {
                    list.Add(t.User1.AcountID);
                }
            });
            return  await FindPlayersByUuid(list);
        }

        public async Task<IList<PlayerState>> QueryNotifyFriend(string owner)
        {
            var fitler = Builders<FriendEntity>.Filter;
            var query = (fitler.Eq(t => t.User1.AcountID, owner) & fitler.Eq(t => t.User2.Active, true))
                | (fitler.Eq(t => t.User2.AcountID, owner) & fitler.Eq(t => t.User1.Active, true));

            var u = await Friends.FindAsync(query);
            var list = new List<string>();

            await u.ForEachAsync(t =>
            {
                if (t.User1.AcountID == owner)
                {
                    list.Add(t.User2.AcountID);
                }
                else
                {
                    list.Add(t.User1.AcountID);
                }
            });
            return await FindPlayersByUuid(list);
        }


        public async Task<IList<PlayerState>> QueryFriendOnServer(string owner, int serverID)
        {

            var friends = await QueryFriend(owner);
            return friends.Where(t => t.ServerID == serverID).ToList();

        }

        public async Task<PlayerState> FindPlayerByUuid(string uuid)
        {
            var query = from t in this.States.AsQueryable()
                        where (t.AccountID == uuid)
                        select new PlayerState
                        {
                            ServerID = t.ChatServerID,
                            State = t.IsOnline ? PlayerState.Types.StateType.Online : PlayerState.Types.StateType.Offline,
                            User = new ChatUser
                            {
                                ChatServerId = t.ChatServerID,
                                UserName = t.HeroName,
                                Uuid = t.AccountID
                            }
                        }
                        ;

            return await Task.FromResult(query.FirstOrDefault());
        }

        public async Task<IList<PlayerState>> FindPlayersByUuid(IList<string> uuid)
        {
            var query = from t in this.States.AsQueryable()
                        where uuid.Contains(t.AccountID)
                        select new PlayerState
                        {
                            ServerID = t.ChatServerID,
                            State = t.IsOnline ? PlayerState.Types.StateType.Online : PlayerState.Types.StateType.Offline,
                            User = new ChatUser
                            {
                                ChatServerId = t.ChatServerID,
                                UserName = t.HeroName,
                                Uuid = t.AccountID
                            }
                        };

             return await Task.FromResult( query.ToList());

        }
    }
}
