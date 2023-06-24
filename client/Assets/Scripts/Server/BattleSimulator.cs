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
using UnityEngine.Serialization;


public enum RunState
{
     NoStart,
     Waiting,
     Running,
     Ending
}

public class BattleSimulator : ComponentAsync
{
    private readonly ConcurrentQueue<BindPlayer> _addTemp = new ConcurrentQueue<BindPlayer>();
    private readonly ConcurrentQueue<string> _kickUsers = new ConcurrentQueue<string>();
    private readonly Dictionary<string, BattlePlayer> _battlePlayers = new Dictionary<string, BattlePlayer>();
    private readonly ConcurrentDictionary<string, BattlePlayer> _tempPlayers = new ConcurrentDictionary<string, BattlePlayer>();

    public BattleLevelSimulator simulator;
    public RunState stateOfRun = RunState.NoStart;

    private HashSet<string> Players { get; } = new();

    public async Task<BattleLevelSimulator> Begin(BattleLevelData level, IList<string> players)
    {
        var per = UPerceptionView.Create(BattleServerApp.S.Constant);
        var levelSimulator = BattleLevelSimulator.Create(level);
        foreach (var i in players) Players.Add(i);
        await levelSimulator.Init(this, level, per);
        simulator = levelSimulator;
        stateOfRun = RunState.Waiting;
        _waitingTime = BattleServerApp.S.Constant.WAITING_TIME;
        return levelSimulator;
    }

    private float _waitingTime = 60;

    private volatile bool _exited;

    private void StopAll()
    {
        if (_exited) return;
        _exited = true;
        simulator?.Stop();
        simulator = null;
    }

    internal bool HavePlayer(string accountUuid)
    {
        return Players.Contains(accountUuid);
    }

    protected override void Update()
    {
        base.Update();
        switch (stateOfRun)
        {
            case RunState.NoStart:
                return;
            case RunState.Waiting:
            {
                _waitingTime -= Time.deltaTime;
                if (_waitingTime <= 0) EndCall(true);
                break;
            }
        }

        ProcessJoinClient();
        ProcessAction();
        var (isEnd, msg) = simulator.Tick();

        if (stateOfRun == RunState.Running)
        {
            if (isEnd) EndCall();
        }
        SendNotify(msg);
    }

    private void EndCall(bool force = false)
    {
        stateOfRun = RunState.Ending;
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

        while (_addTemp.TryDequeue(out var client))
        {
            if (this.stateOfRun == RunState.Waiting)
                this.stateOfRun = RunState.Running;

            Debuger.Log($"Add Client:{client.Account}");
            _battlePlayers.Remove(client.Account);
            var createNotify = simulator.GetInitNotify();
            var c = simulator.CreateUser(client.Player);
            if (c != null)
            {
                client.Player.HeroCharacter = c;
                _battlePlayers.Add(client.Account, client.Player);
                var package = client.Player.GetNotifyPackage();
                package.TimeNow = simulator.TimeNow.Time;
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

        while (_kickUsers.TryDequeue(out var u))
        {
            if (_battlePlayers.TryGetValue(u, out var p))
            {
                ExitPlayer(u, p);
            }
        }

    }

    private void ExitPlayer(string uid, BattlePlayer p)
    {
        if (p.HeroCharacter) GObject.Destroy(p.HeroCharacter);
        _battlePlayers.Remove(p.AccountId);
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
                MapID = simulator.LevelData.ID,
                DiffGold = p.DiffGold,
                Exp = p.GetHero().Exprices,
                Level = p.GetHero().Level,
                HP = p.HeroCharacter?.HP ?? 0,
                MP = p.HeroCharacter?.MP ?? 0
            };

            var removeItem = p.Package.Removes.Select(t => t.Item).ToList();
            var modify = p.Package.Items.Where(t => t.Value.Dirty).Select(t => t.Value.Item).ToList();
            foreach (var i in removeItem)
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
        foreach (var i in _battlePlayers)
        {
            await SendReword(i.Key, i.Value);
        }
        return true;
    }

    private void SendNotify(IMessage[] notify)
    {
        if (notify == null || notify.Length == 0) return;

        var buffer = new List<Any>();
        var time = Any.Pack(new Notify_SyncServerTime { ServerNow = simulator.TimeNow.Time });
        buffer.Add(time);
        foreach (var i in notify)
        {
            buffer.Add(Any.Pack(i));
        }

        foreach (var i in _battlePlayers)
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
        foreach (var i in _battlePlayers)
        {
            if (i.Value.RequestChannel?.IsWorking != true)
            {
                KickUser(i.Key);
                continue;
            }
            var needNotifyPackage = false;
            var hero = i.Value.HeroCharacter;
            while (i.Value.RequestChannel.TryPull(out var action))
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
                    if (!simulator.TryGetElementByIndex(collect.Index, out BattleItem item)) continue;
                    if (item.IsAliveAble != true || !item.CanBecollect(i.Value.HeroCharacter)) continue;
                    if (!i.Value.AddDrop(item.DropItem)) continue;
                    needNotifyPackage = true;
                    GObject.Destroy(item);
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
                                if (simulator.CreateReleaser(config.Params1, i.Value.HeroCharacter, rTarget, ReleaserType.Magic, ReleaserModeType.RmtNone, -1))
                                {
                                    i.Value.ConsumeItem(useItem.ItemId);
                                    needNotifyPackage = true;
                                }
                                break;
                            }
                        case ItemType.ItNone:
                        case ItemType.ItEquip:
                        case ItemType.ItConsume:
                        default:
                        {
                            Debuger.LogError($"type of {(ItemType)config.ItemType} can't be used!");
                        }
                            break;
                            
                    };
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
                    Debuger.LogError($"Not found Type:{action.TypeUrl} of {action}");
                }
            }
            if (!needNotifyPackage) continue;
            var init = i.Value.GetNotifyPackage();
            init.TimeNow = simulator.TimeNow.Time;
            i.Value.PushChannel?.Push(Any.Pack(init));
        }    
    }

    public bool TryGetPlayer(string accountUuid, out BattlePlayer player)
    {
        return _battlePlayers.TryGetValue(accountUuid, out player);
    }

    public bool BindUserChannel(string accountUuid, StreamBuffer<Any> pushChannel = null, StreamBuffer<Any> requestChannel = null)
    {
        Debuger.Log($"Bind player:{accountUuid} of  push:{ pushChannel} request:{requestChannel}");

        if (!_tempPlayers.TryGetValue(accountUuid, out BattlePlayer player)) return false;
        if (pushChannel != null)
            player.PushChannel = pushChannel;
        if (requestChannel != null)
            player.RequestChannel = requestChannel;

        if (!player.IsConnected) return true;
        Debuger.Log($"Add client into simulator:{player.AccountId}");
        _addTemp.Enqueue(new BindPlayer
        {
            Player = player,
            Account = accountUuid
        });

        _tempPlayers.TryRemove(accountUuid, out _);
        return true;
    }

    public bool AddPlayer(string accountUuid, BattlePlayer battlePlayer)
    {
        Debuger.Log($"Add player:{accountUuid} connected:{battlePlayer.IsConnected}");
        _tempPlayers.TryRemove(accountUuid, out _);
        return _tempPlayers.TryAdd(accountUuid, battlePlayer);
    }

    public void KickUser(string accountUuid)
    {
        _kickUsers.Enqueue(accountUuid);
    }
}
