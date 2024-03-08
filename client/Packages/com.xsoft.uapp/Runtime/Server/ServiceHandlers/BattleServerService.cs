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
using App.Core.Core;
using Cysharp.Threading.Tasks;
using Google.Protobuf;

namespace Server
{
    public class BattleServerService : Proto.BattleServerService.BattleServerServiceBase
    {
        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> _pushChannels = new();
        private readonly ConcurrentDictionary<string, StreamBuffer<Any>> _requestChannels = new();

        private LogServer Server { get; }

        public BattleServerService(LogServer server)
        {
            Server = server;
        }

        public override async Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle req, ServerCallContext context)
        {
            await UniTask.SwitchToMainThread();
            var accountUuid = context.GetAccountId();
            var matchServer = BattleServerApp.S.MatchServer.FirstOrDefault();
            
            if (matchServer == null) return new B2C_ExitBattle { Code = ErrorCode.Ok };
            try
            {
                if (BattleServerApp.S.KillUser(accountUuid))
                {
                    await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                        matchServer.ServicsHost, 
                        async (c) => await c.LeaveMatchGroupAsync(
                            new S2M_LeaveMatchGroup { AccountID = accountUuid },
                            headers: context.GetTraceMeta()
                            ));
                }
                else
                {
                    await C<MatchServices.MatchServicesClient>.RequestOnceAsync(
                        matchServer.ServicsHost,
                        async (c) => await c.ExitBattleAsync(
                            new S2M_ExitBattle { UserID = accountUuid },
                            headers: context.GetTraceMeta()));
                }
            }
            catch (Exception ex)
            {
                Debuger.Log(ex);
            }
            return new B2C_ExitBattle { Code = ErrorCode.Ok };
        }

        [Auth]
        public override async Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request, ServerCallContext context)
        {
            await UniTask.SwitchToMainThread();
            var simulator = BattleServerApp.S.BattleSimulator;
            if (!simulator) return new B2C_JoinBattle{ Code = ErrorCode.ServerHostOffLine };
            if (!simulator.HavePlayer(request.AccountUuid)) return new B2C_JoinBattle{ Code = ErrorCode.LoginFailure};
            var loginServer = BattleServerApp.S.LoginServer.FirstOrDefault();
            if (loginServer == null)
            {
                Debuger.LogError($"no found login");
                return new B2C_JoinBattle { Code = ErrorCode.NofoundServerId };
            }

            await UniTask.SwitchToTaskPool();
            try
            {
                var seResult = await C<LoginBattleGameServerService.LoginBattleGameServerServiceClient>
                    .RequestOnceAsync(loginServer.ServicsHost,
                        expression: async c =>
                            await c.CheckSessionAsync(
                                request: new S2L_CheckSession
                                {
                                    UserID = request.AccountUuid,
                                    Session = request.Session,
                                },
                                headers: context.GetTraceMeta()
                            ));

                if (!seResult.Code.IsOk()) return new B2C_JoinBattle();
                {
                    var pack = await C<GateServerInnerService.GateServerInnerServiceClient>
                        .RequestOnceAsync(seResult.GateServerInnerHost,
                            expression: async c =>
                                await c.GetPlayerInfoAsync(
                                    request: new B2G_GetPlayerInfo { AccountUuid = request.AccountUuid },
                                    headers: context.GetTraceMeta())
                                );

                    if (!pack.Code.IsOk()) return new B2C_JoinBattle{ Code = pack.Code};
                    
                    if (!Server.TryCreateSession(request.AccountUuid, out var key)) 
                        return new B2C_JoinBattle{ Code = ErrorCode.Exception };
                    //session-key
                    await context.WriteResponseHeadersAsync(new Metadata { { "session-key", key } });
                    Debuger.Log($"Add:{request.AccountUuid} -> {key}");
                    var battlePlayer =
                        new BattlePlayer(request.AccountUuid,pack.Package, pack.Hero, pack.Gold, seResult);
                    simulator.AddPlayer(request.AccountUuid, battlePlayer);
                }
                return new B2C_JoinBattle { Code = ErrorCode.Ok };
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex);
                return new B2C_JoinBattle { Code = ErrorCode.Error };
            }
        }

        internal void CloseAllChannel()
        {
            foreach (var i in _pushChannels)
                i.Value.Close();

            foreach (var i in _requestChannels)
                i.Value.Close();
        }

        public override async Task<B2C_ViewPlayerHero> ViewPlayerHero(C2B_ViewPlayerHero req,ServerCallContext context)
        {
            await UniTask.SwitchToMainThread();
            var simulator = BattleServerApp.S.BattleSimulator;
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
            await UniTask.SwitchToMainThread();
            var simulator = BattleServerApp.S.BattleSimulator;
            var account = context.GetAccountId();
            var pushChannel = new ServerPushChannel<Any>(500);
            _pushChannels.TryAdd(account, pushChannel);
            var channel = new StreamBuffer<Any>(100);
            _requestChannels.TryAdd(account, channel);
            simulator.BindUserChannel(account, pushChannel: pushChannel, requestChannel: channel);
            await UniTask.SwitchToTaskPool();
            
            var push = Task.Factory.StartNew(async () =>
            {
                await pushChannel.ProcessAsync(responseStream).ConfigureAwait(false);
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

            _pushChannels.TryRemove(account, out _);
            pushChannel.Close();
            _requestChannels.TryRemove(account, out _);
            channel.Close();
        }

        public void CloseChannel(string userID)
        {
            if (_pushChannels.TryRemove(userID, out var push)) push.Close();
            if (_requestChannels.TryRemove(userID, out var req)) req.Close();
        }

        public void PushToAll(IMessage msg)
        {
            var any = Any.Pack(msg);
            foreach (var i in _pushChannels)
            {
                i.Value.Push(any);
            }
        }
    }


}