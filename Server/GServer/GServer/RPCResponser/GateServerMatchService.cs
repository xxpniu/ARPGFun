using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GServer.Managers;
using Proto;
using Proto.MongoDB;
using XNet.Libs.Utility;

namespace GServer.RPCResponser
{
    public class GateServerMatchService : Proto.GateServerMatchService.GateServerMatchServiceBase
    {
        private static async Task<bool> SendNotify(IMessage notify, params string[] player)
        {
            var notifyServer = Application.S.NotifyServers.NextServer();
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
            
            var res = await C<NotifyServices.NotifyServicesClient>.RequestOnceAsync(
                notifyServer.ServicsHost,
                async ( c)=> await c.RouteSendNotifyAsync(rNotify));
            if (res.Code == ErrorCode.Ok)
            {
                return true;
            }
            else
            {
                Debuger.LogError("Send notify error");
                return false;
            }
        }
        
         public override async Task<G2C_InviteJoinMatch> InviteJoinMatch(C2G_InviteJoinMatch request, ServerCallContext context)
        {
            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var ntf = new N_Notify_InviteJoinMatchGroup { GroupId = request.GroupID, Inviter = player, LevelID = request.LevelID };
            var res = await SendNotify(ntf, request.AccountUuid);
            return new G2C_InviteJoinMatch { Code = res ? ErrorCode.Ok : ErrorCode.Error };
        }

        public override async Task<G2C_JoinMatch> JoinMatch(C2G_JoinMatch request, ServerCallContext context)
        {

            var matchServer = Application.S.MatchServers.FirstOrDefault();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_JoinMatch();
            }

            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var match = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) =>
                    await c.JoinMatchGroupAsync(
                        new S2M_JoinMatchGroup { GroupID = request.GroupID, Player = player },
                        headers: context.GetTraceMeta()));

            return new G2C_JoinMatch { Code = match.Code };
        }

        public override async Task<G2C_CreateMatch> CreateMatch(C2G_CreateMatch request, ServerCallContext context)
        {
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_CreateMatch { Code = ErrorCode.Error };
            }
            
            var hero = await UserDataManager.S.FindHeroByAccountId(context.GetAccountId());
            var player = new MatchPlayer
            {
                AccountID = context.GetAccountId(),
                Hero = new MatchPlayer.Types.MatchHero
                {
                    HeroID = hero.HeroId,
                    Level = hero.Level
                },
                Name = hero.HeroName
            };

            var match = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) =>
                    await c.CreateMatchGroupAsync(new S2M_CreateMatchGroup
                    {
                        Level = request.LevelID,
                        Player = player
                    }, headers: context.GetTraceMeta())

            );


            return new G2C_CreateMatch { Code = match.Code, GroupID = match.GroupID };
        }

        public override async Task<G2C_LeaveMatchGroup> LeaveMatchGroup(C2G_LeaveMatchGroup request, ServerCallContext context)
        {
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new  G2C_LeaveMatchGroup { Code = ErrorCode.Error };
            }

            var quit = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchServer.ServicsHost,
                async (c) => 
                    await c.LeaveMatchGroupAsync(
                        new S2M_LeaveMatchGroup { AccountID = context.GetAccountId() },
                        headers: context.GetTraceMeta())
            );
            return new G2C_LeaveMatchGroup { Code = quit.Code };
        }

        public override async Task<G2C_BeginGame> BeginGame(C2G_BeginGame request, ServerCallContext context)
        {
            var userId = context.GetAccountId();
            var matchServer = Application.S.MatchServers.NextServer();
            if (matchServer == null)
            {
                Debuger.LogError($"No found match server");
                return new G2C_BeginGame { Code = ErrorCode.Ok };
            }

            try
            {
                var match = await C<MatchServices.MatchServicesClient>
                    .RequestOnceAsync(
                    matchServer.ServicsHost,
                    async (c)=>  
                        await c.StartMatchAsync(new S2M_StartMatch { GroupID = request.GroupID, Leader = userId },
                            headers: context.GetTraceMeta())
                    );
                return match.Code == ErrorCode.Ok ? new G2C_BeginGame { Code = ErrorCode.Ok } : new G2C_BeginGame { Code = match.Code };
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
            }

            return new G2C_BeginGame { };
        }
        
        public override async Task<G2C_SearchPlayer> SearchPlayer(C2G_SearchPlayer request, ServerCallContext context)
        {
            var players = await UserDataManager.S.QueryPlayers(context.GetAccountId());

            var res = new G2C_SearchPlayer
            {
                Code = ErrorCode.Ok
            };
            foreach (var i in players)
            {
                res.Players.Add(await GetPlayer(i));
            }
            return res;

            static async Task<G2C_SearchPlayer.Types.Player> GetPlayer(GamePlayerEntity entity)
            {
                var hero = (await UserDataManager.S.FindHeroByPlayerId(entity.Uuid));
                return new G2C_SearchPlayer.Types.Player
                {
                    AccountUuid = entity.AccountUuid,
                    HeroName = hero.HeroName,
                    Level = hero.Level
                };
            }
        }

        public override async Task<G2C_ReloadMatchState> ReloadMatchState(C2G_ReloadMatchState request,
            ServerCallContext context)
        {
            var matchSever = Application.S.MatchServers.NextServer();
            if (matchSever == null) return new G2C_ReloadMatchState { Code = ErrorCode.Error };
            var rejoin = await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                matchSever.ServicsHost,
                async (c) => 
                    await c.TryToReJoinMatchAsync(
                        new S2M_TryToReJoinMatch { Account = context.GetAccountId() },
                        headers: context.GetTraceMeta())
            );

            return new G2C_ReloadMatchState { Code = rejoin.Code };
        }


    }
}