using System;
using Proto;
using Proto.MongoDB;
using LoginServer;
using MongoDB.Driver;
using MongoTool;
using XNet.Libs.Utility;
using System.Threading.Tasks;
using Grpc.Core;
using System.Diagnostics;
using Utility;

namespace RPCResponsers
{
    /// <summary>
    /// Login server handle client request
    /// </summary>

    public class LoginServerService : Proto.LoginServerService.LoginServerServiceBase
    {
        [Auth]
        public override async Task<L2C_Login> Login(C2L_Login request, ServerCallContext context)
        {
            var users = DataBase.S.Account;

            var pwd = request.Password;
            var nameFilter = Builders<AccountEntity>.Filter.Eq(t=>t.UserName, request.UserName);
            var user = (await users.FindAsync(nameFilter)).FirstOrDefault() ;

            if (user == null) return new L2C_Login { Code = ErrorCode.NofoundUserAccount };
            if (user.Password != pwd) return new L2C_Login { Code = ErrorCode.LoginFailure };

            if (DataBase.S.GetSessionInfo(user.Uuid, out UserSessionInfoEntity info))
            {
                try
                {
                    var s = Appliaction.S.FindGateServer(info.GateServerId);
                    var channel = new LogChannel($"{s.ServicsHost.IpAddress}:{s.ServicsHost.Port}", ChannelCredentials.Insecure);
                    var client = channel.CreateClient<GateServerInnerService.GateServerInnerServiceClient>();
                    await client.KillUserAsync(new L2G_KillUser { Uuid = user.Uuid });
                    await channel.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debuger.LogError(ex);
                }
            }
            
            //清空之前的登陆信息
            var s_filter = Builders<UserSessionInfoEntity>.Filter.Eq(t => t.AccountUuid, user.Uuid);
            await DataBase.S.Session.DeleteManyAsync(s_filter);

            Debuger.Log($"Server:{user.ServerID}");
            var gate = Appliaction.S.FindGateServer(user.ServerID);
            if (gate == null)  return new L2C_Login { Code = ErrorCode.NofoundServerId };

            //创建session
            var session = SaveSession(user.Uuid, user.ServerID);
            user.LoginCount += 1;
            var update = Builders<AccountEntity>.Update
                .Set(u => u.LastLoginTime, DateTime.Now)
                .Set(t => t.LoginCount, user.LoginCount);

            var upFilter = Builders<AccountEntity>.Filter.Eq(t => t.Uuid, user.Uuid);
            await users.UpdateOneAsync(upFilter, update);


            var chat = Appliaction.S.FindFreeChatServer();
            GameServerInfo chats = null;
            if (chat != null)
            {
                chats = new GameServerInfo
                {
                    ServerId = chat.ChatServerID,
                    CurrentPlayerCount = chat.Player,
                    MaxPlayerCount =
                    chat.MaxPlayer,
                    Host = chat.ListenHost.IpAddress,
                    Port = chat.ListenHost.Port
                };
            }
            return new L2C_Login
            {
                Code = ErrorCode.Ok,
                GateServer = new GameServerInfo
                {
                    CurrentPlayerCount = gate.Player,
                    Host = gate.ListenHost.IpAddress,
                    Port = gate.ListenHost.Port,
                    MaxPlayerCount = gate.MaxPlayer,
                    ServerId = gate.ServerID
                },
                ChatServer = chats,
                Session = session,
                UserID = user.Uuid
            };
        }

        private string SaveSession(string uuid,int gateServer)
        {
            var session = Md5Tool.GetMd5Hash(DateTime.UtcNow.Ticks.ToString());
            var us = new UserSessionInfoEntity
            {
                AccountUuid = uuid,
                Token = session,
                GateServerId = gateServer,
            };
            DataBase.S.Session.InsertOne(us);
            return session;
        }

        [Auth]
        public override async Task<L2C_Reg> Reg(C2L_Reg request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return new L2C_Reg { Code = ErrorCode.RegInputEmptyOrNull };
            if (request.UserName.Length > 100 || request.UserName.Length < 2)
                return new L2C_Reg { Code = ErrorCode.NameOrPwdLeghtIncorrect };
            if (request.Password.Length > 100 || request.Password.Length < 2)
                return new L2C_Reg { Code = ErrorCode.NameOrPwdLeghtIncorrect };

            var users = DataBase.S.Account;
            var filter = Builders<AccountEntity>.Filter.Eq(t=>t.UserName, request.UserName);
            if (await (await users.FindAsync(filter)).AnyAsync()) return new L2C_Reg{Code = ErrorCode.RegExistUserName };
            var data = Appliaction.S.FindFreeGateServer();
            if (data == null) return new L2C_Reg() { Code = ErrorCode.NoFreeGateServer };
            var serverID = data.ServerID;
            var pwd = Md5Tool.GetMd5Hash(request.Password);
            var acc = new AccountEntity
            {
                UserName = request.UserName,
                Password = pwd,
                CreateTime = DateTime.Now,
                LoginCount = 0,
                LastLoginTime =DateTime.Now,
                ServerID = serverID
            };

            await users.InsertOneAsync(acc);
            var session = SaveSession(acc.Uuid, acc.ServerID);

            var chat = Appliaction.S.FindFreeChatServer();
            GameServerInfo chats = null;
            if (chat != null)
            {
                chats = new GameServerInfo
                {
                    ServerId = chat.ChatServerID,
                    CurrentPlayerCount = chat.Player,
                    MaxPlayerCount =
                    chat.MaxPlayer,
                    Host = chat.ListenHost.IpAddress,
                    Port = chat.ListenHost.Port
                };
            }
            return new L2C_Reg
            {
                Code = ErrorCode.Ok,
                Session = session,
                UserID = acc.Uuid,
                GateServer = new GameServerInfo
                {
                    CurrentPlayerCount = data.Player,
                    Host = data.ListenHost.IpAddress,
                    Port = data.ListenHost.Port,
                    MaxPlayerCount = data.MaxPlayer,
                    ServerId = data.ServerID
                },
                ChatServer = chats
            };
        }
    }
}
