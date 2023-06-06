#pragma warning disable CS1998

using System.Threading.Tasks;
using Proto;
using Grpc.Core;
using XNet.Libs.Utility;
using Utility;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using Server.ServiceHandlers;
using System.Linq;
using System;
using Core;
using Google.Protobuf;

namespace Server
{
    public class BattleServerService : Proto.BattleServerService.BattleServerServiceBase
    {
        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> PushChannels = new ConcurrentDictionary<string, StreamBuffer<Any>>();
        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> RequestChannels = new ConcurrentDictionary<string, StreamBuffer<Any>>();

        public LogServer Server { get; }

        public BattleServerService(LogServer server)
        {
            Server = server;
        }

        public override async Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle req, ServerCallContext context)
        {
            var accountUuid = context.GetAccountId(); 

            var matchsever = BattleServerApp.S.MatchServer.FirstOrDefault();
            if (matchsever != null)
            {
                var chn = new LogChannel(matchsever.ServicsHost);
                try
                {
                    var query = chn.CreateClient<MatchServices.MatchServicesClient>();

                    if (BattleServerApp.S.KillUser(accountUuid))
                    {
                        await query.LeaveMatchGroupAsync(new S2M_LeaveMatchGroup { AccountID = accountUuid });
                    }
                    else
                    {
                        await query.ExitBattleAsync(new S2M_ExitBattle { UserID = accountUuid });
                    }
                }
                catch (Exception ex)
                {
                    Debuger.Log(ex);
                }
                await chn.ShutdownAsync();
            }
            return new B2C_ExitBattle { Code = ErrorCode.Ok };
        }

        [Auth]
        public override async Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request, ServerCallContext context)
        {

            var simulator = BattleServerApp.S.BattleSimulater;
            if (!simulator) return new B2C_JoinBattle();
            if (!simulator.HavePlayer(request.AccountUuid)) return new B2C_JoinBattle();

            var re = new S2L_CheckSession
            {
                UserID = request.AccountUuid,
                Session = request.Session,
            };
            var loginServer = BattleServerApp.S.LoginServer.FirstOrDefault();
            if (loginServer == null)
            {
                Debuger.LogError($"no found login");
                return new B2C_JoinBattle { Code = ErrorCode.NofoundServerId };
            }
            L2S_CheckSession seResult;
            {
                var ch = new LogChannel(loginServer.ServicsHost);
                var client = ch.CreateClient<LoginBattleGameServerService.LoginBattleGameServerServiceClient>();
                seResult = await client.CheckSessionAsync(re);
                await ch.ConnectAsync();
            }

            if (!seResult.Code.IsOk()) return new B2C_JoinBattle();

            {
                var ch = new LogChannel(seResult.GateServerInnerHost);
                var client = ch.CreateClient<Proto.GateServerInnerService.GateServerInnerServiceClient>();
                var pack = await client.GetPlayerInfoAsync(new B2G_GetPlayerInfo { AccountUuid = request.AccountUuid });
                await ch.ShutdownAsync();
                if (!pack.Code.IsOk()) return new B2C_JoinBattle();

                if (!Server.TryCreateSession(request.AccountUuid, out string key)) return new B2C_JoinBattle();
                //session-key
                await context.WriteResponseHeadersAsync(new Metadata { { "session-key", key } });
                Debuger.Log($"Add:{request.AccountUuid} -> {key}");
                var battlePlayer = new BattlePlayer(request.AccountUuid, pack.Package, pack.Hero, pack.Gold, seResult);
                simulator.AddPlayer(request.AccountUuid, battlePlayer);

            }

            return new B2C_JoinBattle { Code = ErrorCode.Ok };
        }

        internal void CloseAllChannel()
        {
            foreach (var i in PushChannels)
                i.Value.Close();

            foreach (var i in RequestChannels)
                i.Value.Close();
        }

        public override async Task<B2C_ViewPlayerHero> ViewPlayerHero(C2B_ViewPlayerHero req,ServerCallContext context)
        {
            var simulator = BattleServerApp.S.BattleSimulater;
            if (!simulator) return new B2C_ViewPlayerHero { Code = ErrorCode.Error };

            if (!simulator.TryGetPlayer(req.AccountUuid, out BattlePlayer player))
            {
                return new B2C_ViewPlayerHero { Code = ErrorCode.NoGamePlayerData };
            }
            var hero = player.GetHero();
            var response = new B2C_ViewPlayerHero
            {
                Code = ErrorCode.Ok,
                HeroID = hero.HeroID,
                Level = hero.Level,
                Name = hero.Name
            };
            foreach (var i in hero.Equips)
            {
                var e = player.GetEquipByGuid(i.GUID);
                if (e == null) continue;
                response.WaerEquips.Add(e);
            }
            foreach (var i in hero.Magics)
            {
                response.Magics.Add(i);
            }
            return response;

        }

        public override async Task BattleChannel(IAsyncStreamReader<Any> requestStream, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            var simulator = BattleServerApp.S.BattleSimulater;
            var account = context.GetAccountId();
            var puschannel = new ServerPushChannel<Any>(500);
            PushChannels.TryAdd(account, puschannel);
            var channel = new StreamBuffer<Any>(100);
            RequestChannels.TryAdd(account, channel);
            simulator.BindUserChannel(account, pushChannel: puschannel, requestChannel: channel);

            var push = Task.Factory.StartNew(async () =>
            {
                await puschannel.ProcessAsync(responseStream).ConfigureAwait(false);
            }, context.CancellationToken);

            try
            {
                while (!context.CancellationToken.IsCancellationRequested
                    && channel.IsWorking
                    && await requestStream.MoveNext(context.CancellationToken))
                {
                    channel.Push(requestStream.Current);
                }
            }
            catch
            {
                // ignored
            }

            await push.ConfigureAwait(false);

            Debuger.Log($"Exit user {account}");

            PushChannels.TryRemove(account, out _);
            puschannel.Close();
            RequestChannels.TryRemove(account, out _);
            channel.Close();
        }

        public void CloseChannel(string userID)
        {
            if (PushChannels.TryRemove(userID, out StreamBuffer<Any> push)) push.Close();
            if (RequestChannels.TryRemove(userID, out StreamBuffer<Any> req)) req.Close();
        }

        public void PushToAll(IMessage msg)
        {
            var any = Any.Pack(msg);
            foreach (var i in PushChannels)
            {
                i.Value.Push(any);
            }
        }
    }


}