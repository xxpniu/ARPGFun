using System.Collections.Generic;
using EngineCore.Simulater;
using Proto;
using UnityEngine;
using System.Collections.Concurrent;
using GameLogic.Game.Elements;
using XNet.Libs.Utility;
using Google.Protobuf;
using EConfig;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using System.Threading.Tasks;
using GameLogic.Game.LayoutLogics;
using Server;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Utility;
using System;
using System.Collections;
using App.Core.Core.Components;
using BattleViews.Views;


public enum RunState
{
     NoStart,
     Waiting,
     Running,
     Ending
}

public class BattleSimulater : ComponentAsync
{
    private readonly ConcurrentQueue<BindPlayer> _addTemp = new ConcurrentQueue<BindPlayer>();
    private readonly ConcurrentQueue<string> _kickUsers = new ConcurrentQueue<string>();
    private readonly Dictionary<string, BattlePlayer> BattlePlayers = new Dictionary<string, BattlePlayer>();
    private readonly ConcurrentDictionary<string, BattlePlayer> _tempPlayers = new ConcurrentDictionary<string, BattlePlayer>();

    public BattleLevelSimulater Simulater;
    public RunState StateOfRun = RunState.NoStart;

    public HashSet<string> Players { get; } = new HashSet<string>();

    public async  Task<BattleLevelSimulater> Begin(BattleLevelData level, IList<string> players)
    {
        var per = UPerceptionView.Create(BattleServerApp.S.Constant);
        var simulater = BattleLevelSimulater.Create(level);
        foreach (var i in players) Players.Add(i);
          await  simulater.Init(this, level, per);
        Simulater = simulater;
        StateOfRun = RunState.Waiting;
        WaitingTime = BattleServerApp.S.Constant.WAITING_TIME;
        return simulater;
    }

    private float WaitingTime = 60;

    volatile bool exited = false;

    private void StopAll()
    {
        if (exited) return;
        exited = true;
        Simulater?.Stop();
        Simulater = null;
    }

    internal bool HavePlayer(string accountUuid)
    {
        return Players.Contains(accountUuid);
    }

    protected override void Update()
    {
        base.Update();
        if (StateOfRun == RunState.NoStart) return;

        if (StateOfRun == RunState.Waiting)
        {
            WaitingTime -= Time.deltaTime;
            if (WaitingTime <= 0) EndCall(true);
        }

        ProcessJoinClient();
        ProcessAction();
        var (isEnd, msgs) = Simulater.Tick();

        if (StateOfRun == RunState.Running)
        {
            if (isEnd) EndCall();
        }
        SendNotify(msgs);
    }

    private void EndCall(bool force = false)
    {
        StateOfRun = RunState.Ending;
        OnEnd?.Invoke(force);
    }

    public Action<bool> OnEnd;

    public Action OnExited;

    private void OnDestroy()
    {
        StopAll();
        OnExited?.Invoke();
    }

    private void ProcessJoinClient()
    {
        if (_addTemp.Count == 0 && _kickUsers.Count ==0) return;

        while (_addTemp.TryDequeue(out BindPlayer client))
        {
            if (this.StateOfRun == RunState.Waiting)
                this.StateOfRun = RunState.Running;

            Debuger.Log($"Add Client:{client.Account}");
            BattlePlayers.Remove(client.Account);
            var createNotify = Simulater.GetInitNotify();
            var c = Simulater.CreateUser(client.Player);
            if (c != null)
            {
                client.Player.HeroCharacter = c;
                BattlePlayers.Add(client.Account, client.Player);
                var package = client.Player.GetNotifyPackage();
                package.TimeNow = Simulater.TimeNow.Time;
                client.Player.PushChannel?.Push(Any.Pack(package));
                foreach (var i in createNotify)
                {
                    client.Player.PushChannel?.Push(Any.Pack(i));
                }
            }
            else
            {
                Debuger.LogError($"Create character failure!");
            }
        }

        while (_kickUsers.TryDequeue(out string u))
        {
            if (BattlePlayers.TryGetValue(u, out BattlePlayer p))
            {
                ExitPlayer(u, p);
            }
        }

    }

    private void ExitPlayer(string uid, BattlePlayer p)
    {
        if (p.HeroCharacter) GObject.Destroy(p.HeroCharacter);
        BattlePlayers.Remove(p.AccountId);
        if (!p.Dirty) return;
        _ = SendReword(uid, p);
    }

    private async Task<bool> SendReword(string uid, BattlePlayer p)
    {
        try
        {
            BattleServerApp.S.Services.CloseChannel(uid);
            var req = new B2G_BattleReward
            {
                AccountUuid = p.AccountId,
                MapID = Simulater.LevelData.ID,
                DiffGold = p.DiffGold,
                Exp = p.GetHero().Exprices,
                Level = p.GetHero().Level,
                HP = p.HeroCharacter?.HP ?? 0,
                MP = p.HeroCharacter?.MP ?? 0
            };

            var removeItesm = p.Package.Removes.Select(t => t.Item).ToList();
            var modify = p.Package.Items.Where(t => t.Value.Dirty).Select(t => t.Value.Item).ToList();
            foreach (var i in removeItesm)
            {
                req.RemoveItems.Add(i);
            }

            foreach (var i in modify)
            {
                req.ModifyItems.Add(i);
            }

            var channel = new LogChannel(p.GateServer.GateServerInnerHost);
            var client = channel.CreateClient<GateServerInnerService.GateServerInnerServiceClient>();
            await client.BattleRewardAsync(req);
            await channel.ShutdownAsync();
            return true;
        }
        catch(Exception ex)
        {
            Debuger.LogError(ex);
            return false;
        }
    }

    public async Task<bool> ExitAllPlayer()
    {
        foreach (var i in BattlePlayers)
        {
            await SendReword(i.Key, i.Value);
        }
        return true;
    }

    private void SendNotify(IMessage[] notify)
    {
        if (notify == null || notify.Length == 0) return;

        var buffer = new List<Any>();
        var time = Any.Pack(new Notify_SyncServerTime { ServerNow = Simulater.TimeNow.Time });
        buffer.Add(time);
        foreach (var i in notify)
        {
            buffer.Add(Any.Pack(i));
        }

        foreach (var i in BattlePlayers)
        {
            if (!i.Value.PushChannel.IsWorking)
            {
                KickUser(i.Key);
                continue;
            }
            foreach (var m in buffer)
                i.Value.PushChannel?.Push(m);
        }

    }

    private void ProcessAction()
    {
        foreach (var i in BattlePlayers)
        {
            if (i.Value.RequestChannel?.IsWorking != true)
            {
                KickUser(i.Key);
                continue;
            }
            bool needNotifyPackage = false;
            var hero = i.Value.HeroCharacter;
            while (i.Value.RequestChannel.TryPull(out Any action))
            {


                if (i.Value.HeroCharacter.IsDeath)
                {
                    if (action.TryUnpack(out Action_Relive re))
                    {
                        hero.Relive(hero.MaxHP);//need cost item
                    }
                    continue;
                }
                if (action.TryUnpack(out Action_CollectItem collect))
                {
                    if (Simulater.TryGetElementByIndex(collect.Index, out BattleItem item))
                    {
                        if (item.IsAliveAble == true && item.CanBecollect(i.Value.HeroCharacter))
                        {
                            if (i.Value.AddDrop(item.DropItem))
                            {
                                needNotifyPackage = true;
                                GObject.Destroy(item);
                            }
                        }
                    }
                }
                else if (action.TryUnpack(out Action_UseItem useItem))
                {
                    if (i.Value.HeroCharacter.IsDeath) continue;
                    var config = CM.GetId<ItemData>(useItem.ItemId);
                    if (config == null) continue;
                    if (i.Value.GetItemCount(useItem.ItemId) == 0) continue;
                    switch ((ItemType)config.ItemType)
                    {
                        case ItemType.ItHpitem:
                        case ItemType.ItMpitem:
                            {
                                var rTarget = new ReleaseAtTarget(i.Value.HeroCharacter, i.Value.HeroCharacter);
                                if (Simulater.CreateReleaser(config.Params1, i.Value.HeroCharacter, rTarget, ReleaserType.Magic, ReleaserModeType.RmtNone, -1))
                                {
                                    i.Value.ConsumeItem(useItem.ItemId);
                                    needNotifyPackage = true;
                                }
                                break;
                            }
                    }
                }
                else if (action.TryUnpack(out Action_LookRotation look))
                {
                    i.Value.HeroCharacter.LookRotation(look.LookRotationY);
                }
                else if (action.TryUnpack(out Action_ClickSkillIndex clickSkillIndex))
                {
                    i.Value.HeroCharacter.AddNetAction(clickSkillIndex);
                }
                else if (action.TryUnpack(out Action_MoveJoystick moveJoystick))
                {
                    i.Value.HeroCharacter.AddNetAction(moveJoystick);
                }
                else if (action.TryUnpack(out Action_NormalAttack normalAttack))
                {
                    i.Value.HeroCharacter.AddNetAction(normalAttack);
                }
                else if (action.TryUnpack(out Action_StopMove stopMove))
                {
                    i.Value.HeroCharacter.AddNetAction(stopMove);
                }
                else
                {
                    Debuger.LogError($"Nofound Type:{action.TypeUrl} of {action}");
                }
            }

            if (needNotifyPackage)
            {
                var init = i.Value.GetNotifyPackage();
                init.TimeNow = Simulater.TimeNow.Time;
                i.Value.PushChannel?.Push(Any.Pack(init));
            }
        }    
    }

    public bool TryGetPlayer(string acccountUuid, out BattlePlayer player)
    {
        return BattlePlayers.TryGetValue(acccountUuid, out player);
    }

    public bool BindUserChannel(string accountUuid, StreamBuffer<Any> pushChannel = null, StreamBuffer<Any> requestChannel = null)
    {
        Debuger.Log($"Bind player:{accountUuid} of  push:{ pushChannel} request:{requestChannel}");

        if (!_tempPlayers.TryGetValue(accountUuid, out BattlePlayer player)) return false;
        if (pushChannel != null)
            player.PushChannel = pushChannel;
        if (requestChannel != null)
            player.RequestChannel = requestChannel;

        if (player.IsConnected)
        {
            Debuger.Log($"Add client into simulater:{player.AccountId}");
            _addTemp.Enqueue(new BindPlayer
            {
                Player = player,
                Account = accountUuid
            });

            _tempPlayers.TryRemove(accountUuid, out _);
        }
        return true;
    }

    public bool AddPlayer(string accountUuid, BattlePlayer battlePlayer)
    {
        Debuger.Log($"Add player:{accountUuid} connected:{battlePlayer.IsConnected}");
        _tempPlayers.TryRemove(accountUuid, out _);
        return _tempPlayers.TryAdd(accountUuid, battlePlayer);
    }

    public void KickUser(string account_uuid)
    {
        _kickUsers.Enqueue(account_uuid);
    }
}
