using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataBase;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Proto;
using Proto.ServerConfig;
using Utility;
using XNet.Libs.Utility;

namespace MatchServer
{

    public class MatchServerService:Proto.MatchServices.MatchServicesBase
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private WatcherServer<string, BattleServerConfig> BattleWatcher { get; }
        private WatcherServer<string, NotifyServerConfig> NotifyServers { get; }

        private async Task<bool> SendNotify(IMessage notify, params string[] player)
        {
            var notifyServer = this.NotifyServers.FirstOrDefault();
            if (notifyServer == null)
            {
                Debuger.LogError($"not found notify server");
                return false;
            }

            var rNotify = new S2N_RouteSendNotify { }; 
            var any = Any.Pack(notify);
            foreach (var i in player)
            {
                var msg = new NotifyMsg { AccountID = i, AnyNotify = { any } };
                rNotify.Msg.Add(msg);
            }


            var chn = new LogChannel(notifyServer.ServicsHost);
            var query = await chn.CreateClientAsync<NotifyServices.NotifyServicesClient>();
            var res = await query.RouteSendNotifyAsync(rNotify);
            await chn.ShutdownAsync();
            if (res.Code == ErrorCode.Ok)
            {
                return true;
            }

            Debuger.LogError("Send notify error");
            return false;
        }

        private async Task<(string id, BattleServerConfig config)> StartBattleServer(IList<string> player, int levelId)
        {
            //only one in time
            await _semaphoreSlim.WaitAsync();
            try
            {
                BattleServerConfig config = null;
                foreach (var c in BattleWatcher)
                {
                    if (await DataBaseTool.S.ExitsMatchByServerId(c.ServerID))
                    {
                        continue;
                    }

                    config = c;
                    break;
                }


                if (config == null)
                {
                    Debuger.Log("NO found Free battle server!");
                    //no found free server 
                    return (null, null);
                }

                var re = new M2B_StartBattle {LevelID = levelId};
                foreach (var i in player)
                {
                    re.Players.Add(i);
                }

                var channel = new LogChannel(config.ServicsHost);
                var client = await channel.CreateClientAsync<BattleInnerServices.BattleInnerServicesClient>();
                var rs = await client.StartBatleAsync(re);
                try
                {
                    await channel.ShutdownAsync();
                }
                catch
                {
                    // ignored
                }

                if (rs.Code != ErrorCode.Ok) return (null, null);
                var notify = new N_Notify_BattleServer
                {
                    ServerUUID = config.ServerID,
                    Server = new GameServerInfo
                    {
                        CurrentPlayerCount = 0,
                        Host = config.ListenHost.IpAddress,
                        Port = config.ListenHost.Port,
                        MaxPlayerCount = config.MaxPlayer,
                        ServerId = -1
                    },
                    LevelID = levelId
                };
                if (!await SendNotify(notify, player.ToArray())) return (null, null);
                await DataBaseTool.S.CreateMatch(player, config, levelId);
                return (config.ServerID, config);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public MatchServerService(WatcherServer<string, BattleServerConfig> battleWatcher, WatcherServer<string, NotifyServerConfig> notifyClient) 
        {
            this.NotifyServers = notifyClient;
            this.BattleWatcher = battleWatcher;
            this.BattleWatcher.OnRefreshed = () =>
            {
                _ = RemoveMatch();
            };
        }

        private async Task RemoveMatch()
        {
            foreach (var i in BattleWatcher)
            {
                await DataBaseTool.S.RemoveMatchByServerId(i.ServerID);
            }
        }

        public override async Task<M2S_CreateMatchGroup> CreateMatchGroup(S2M_CreateMatchGroup request, ServerCallContext context)
        {
            var (res, group) = await DataBaseTool.S.TryToCreateGroup(request.Level, request.Player);
            if (!res)
                return new M2S_CreateMatchGroup
                {
                    Code = ErrorCode.InMatch,
                    GroupID = group?.Uuid ?? string.Empty
                };
            var ntf = new N_Notify_MatchGroup
            {
                LevelID = group.LevelID,
                Players = { group.Players },
                Id = group.Uuid
            };
            await SendNotify(ntf, request.Player.AccountID);
            return new M2S_CreateMatchGroup
            {
                Code = ErrorCode.Ok,
                GroupID = group?.Uuid??string.Empty
            };
        }

        public override async Task<M2S_StartMatch> StartMatch(S2M_StartMatch request, ServerCallContext context)
        {
            var group = await DataBaseTool.S.QueryMatchGroup(request.GroupID);
            if (group == null) return new M2S_StartMatch { Code = ErrorCode.NoFoundMatch };
            var (_, config) = await StartBattleServer(group.Players.Select(t=>t.AccountID).ToList(), group.LevelID);
            return new M2S_StartMatch { Code = config == null ? ErrorCode.NofreeBattleServer: ErrorCode.Ok };
        }

        public override async Task<M2S_TryToReJoinMatch> TryToReJoinMatch(S2M_TryToReJoinMatch request, ServerCallContext context)
        {
            var (res, group) = await DataBaseTool.S.QueryMatchGroupByPlayer(request.Account);
            if (res)
            {
                var ntf = new N_Notify_MatchGroup
                {
                    LevelID = group.LevelID,
                    Players = { group.Players },
                    Id = group.Uuid
                };
                await SendNotify(ntf, request.Account);
            }
            var( config, level) = await DataBaseTool.S.QueryMatchByPlayer(request.Account);
            if (config == null) return new M2S_TryToReJoinMatch {Code = ErrorCode.Ok};
            var notify = new N_Notify_BattleServer
            {
                ServerUUID = config.ServerID,
                Server = new GameServerInfo
                {
                    CurrentPlayerCount = 0,
                    Host = config.ListenHost.IpAddress,
                    Port = config.ListenHost.Port,
                    MaxPlayerCount = config.MaxPlayer,
                    ServerId = -1,
                    
                },
                ReTry = true,
                LevelID = level
            };

            await SendNotify(notify, request.Account);

            return new M2S_TryToReJoinMatch { Code = ErrorCode.Ok };
        }

        public override async Task<M2S_JoinMatchGroup> JoinMatchGroup(S2M_JoinMatchGroup request, ServerCallContext context)
        {
            var (res, group) = await DataBaseTool.S.TryToJoinGroup(request.GroupID, request.Player);
            if (!res) return new M2S_JoinMatchGroup {Code = ErrorCode.InMatch};
            var ntf = new N_Notify_MatchGroup
            {
                LevelID = @group.LevelID,
                Players = { @group.Players },
                Id = @group.Uuid
            };
            Debuger.Log(ntf);
            await SendNotify(ntf, @group.Players.Select(t=>t.AccountID).ToArray());

            return new M2S_JoinMatchGroup { Code = ErrorCode.Ok };
        }

        public override async Task<M2S_LeaveMatchGroup> LeaveMatchGroup(S2M_LeaveMatchGroup request, ServerCallContext context)
        {
            var (res, group) = await DataBaseTool.S.QuitMatchGroupByPlayer(request.AccountID);
            if (!res) return new M2S_LeaveMatchGroup {Code = ErrorCode.Error};
            var ntf = new N_Notify_MatchGroup
            {
                LevelID = @group.LevelID,
                Players = { @group.Players },
                Id = @group.Uuid
            };
            var pls = @group.Players.Select(t => t.AccountID).ToList();
            pls.Add(request.AccountID);
            await SendNotify(ntf,pls.ToArray());
            return new M2S_LeaveMatchGroup { Code = ErrorCode.Ok };
        }

        public override async Task<M2S_FinishBattle> FinishBattle(S2M_FinishBattle request, ServerCallContext context)
        {
            var data = await DataBaseTool.S.RemoveMatchByServerId(request.BattleServerID);
            return new M2S_FinishBattle { Code = data ? ErrorCode.Ok : ErrorCode.Error };
        }

        public override async Task<M2S_KillUser> KllUser(S2M_KillUser request, ServerCallContext context)
        {
            var (config, _) = await DataBaseTool.S.QueryMatchByPlayer(request.UserID);
            if (config == null) return new M2S_KillUser {Code = ErrorCode.NofoundUserOnBattleServer};

            var chn = new LogChannel(config.ServicsHost);
            var client = await chn.CreateClientAsync<BattleInnerServices.BattleInnerServicesClient>();
            await client.KillUserAsync(new M2B_KillUser {UserID = request.UserID});
            await chn.ShutdownAsync();

            return new M2S_KillUser {Code = ErrorCode.Ok};
        }

        public override async Task<M2S_ExitBattle> ExitBattle(S2M_ExitBattle request, ServerCallContext context)
        {
            if (await DataBaseTool.S.ExitBattleServer(request.UserID))
            {
                return new M2S_ExitBattle { Code = ErrorCode.Ok };
            }
            return new M2S_ExitBattle { };
        }
    }
}
