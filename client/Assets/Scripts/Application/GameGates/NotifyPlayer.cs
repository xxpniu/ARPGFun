using System;
using Proto;
using UnityEngine;
using System.Collections.Generic;
using GameLogic.Game.Perceptions;
using GameLogic.Game.Elements;
using Google.Protobuf;
using System.Reflection;
using BattleViews.Views;
using GameLogic.Utility;
using Google.Protobuf.WellKnownTypes;
using XNet.Libs.Utility;
using UGameTools;

/// <summary>
/// 游戏中的通知播放者
/// </summary>
public class NotifyPlayer
{
    private struct NotifyMapping
    {
        public NeedNotifyAttribute Attr { set; get; }
        public MethodInfo Method { set; get; }
        public System. Type Type { set; get; }
    }
    private readonly Dictionary<string, NotifyMapping> PerceptionInvokes = new Dictionary<string, NotifyMapping>();
    private readonly Dictionary<string, NotifyMapping> ElementInvokes = new Dictionary<string, NotifyMapping>();

    public IBattlePerception PerView { set; get; }
 
    #region Events
    public Action<Notify_CharacterExp> OnAddExp;
    public Action<IBattleCharacter> OnCreateUser;
    public Action<Notify_PlayerJoinState> OnJoined;
    public Action<Notify_DropGold> OnDropGold;
    public Action<Notify_SyncServerTime> OnSyncServerTime;
    public Action<Notify_BattleEnd> OnBattleEnd;
    #endregion

    public NotifyPlayer(UPerceptionView view)
    {
        PerView = view;
        var invokes = typeof(IBattlePerception).GetMethods();
        foreach (var i in invokes)
        {
            var att = i.GetCustomAttribute<NeedNotifyAttribute>();
            if (att == null) continue;
            PerceptionInvokes.Add(att.NotifyType.FullName, new NotifyMapping
            {
                Type = att.NotifyType,
                Method = i,
                Attr = att
            });
        }

        AddType<IBattleElement>();
        AddType<IBattleCharacter>();
        AddType<IBattleMissile>();
        AddType<IMagicReleaser>();
        AddType<IBattleItem>();
    }

    private void AddType<T>()
    {
        var invokes = typeof(T).GetMethods();
        foreach (var i in invokes)
        {
            var att = i.GetCustomAttribute<NeedNotifyAttribute>();
            if (att == null) continue;
            if (ElementInvokes.ContainsKey(att.NotifyType.FullName))
            {
                Debug.LogError($"{att.NotifyType} had add");
                continue;
            }
            ElementInvokes.Add(att.NotifyType.FullName, new NotifyMapping { Method = i, Attr = att, Type = att.NotifyType });
            Debug.Log($"{ typeof(T)} handle notify {att.NotifyType}");
        }
    }

    private const string INDEX = "Index";
    
    /// <summary>
    /// 处理网络包的解析
    /// </summary>
    /// <param name="notify">Notify.</param>
    public void Process(Any any)
    {

        //Debuger.Log(any);
        var type = any.TypeUrl.Split('/')[1];
        //Debug.Log($"{notify.GetType().Name}->{notify}");
        //优先处理 perception 创建元素
        if (PerceptionInvokes.TryGetValue(type, out NotifyMapping m))
        {
            var notify = Activator.CreateInstance(m.Type) as IMessage;
            notify.MergeFrom(any.Value.ToByteArray());
            var ps = new List<object>();
            foreach (var i in m.Attr.FieldNames)
            {
                ps.Add(m.Type.GetProperty(i).GetValue(notify));
            }
            var go = m.Method.Invoke(PerView, ps.ToArray());

            if (go is UElementView el)
            {
                el.SetPerception(PerView as UPerceptionView);
                if((el is IBattleElement b)) b.JoinState((int)notify.GetType().GetProperty(INDEX).GetValue(notify));
            }

            if (go is UCharacterView c)
            {
                c.TryAdd<HpTipShower>();
                OnCreateUser?.Invoke(c);
            }

            if (go is UBattleItem item)
            {
                Debug.Log($"Drop: {item}");
                item.TryAdd<HpItemNameShower>();
            }
            Debuger.Log(notify);
            return;
        }

        if (ElementInvokes.TryGetValue(type, out NotifyMapping em))
        {
            var notify = Activator.CreateInstance(em.Type) as IMessage;
            notify.MergeFrom(any.Value.ToByteArray());
            var property = notify.GetType().GetProperty(INDEX);
            var index = (int)property.GetValue(notify);
            var per = PerView as UPerceptionView;
            var v = per.GetViewByIndex<UElementView>(index);
            if (v == null)
            {
                Debug.LogError($"No found index {index} by {notify.GetType()} -> {notify}");
                return;
            }
            var ps = new List<object>();
            foreach (var f in em.Attr.FieldNames)
            {
                ps.Add(notify.GetType().GetProperty(f).GetValue(notify));
            }
            em.Method.Invoke(v, ps.ToArray());
            Debuger.Log(notify);
            return;

        }

        //处理特别消息
        if (any.TryUnpack(out Notify_PlayerJoinState p))
        {
            OnJoined?.Invoke(p);
        }
        else if (any.TryUnpack(out Notify_DropGold dropGold))
        {
            OnDropGold?.Invoke(dropGold);
        }
        else if (any.TryUnpack(out Notify_CharacterExp exp))
        {
            OnAddExp?.Invoke(exp);
        }
        else if (any.TryUnpack(out Notify_SyncServerTime sTime))
        {
            OnSyncServerTime?.Invoke(sTime);
        }
        else if (any.TryUnpack(out Notify_BattleEnd end))
        {
            OnBattleEnd?.Invoke(end);
        }
        else
        {
            Debug.LogError($"NO Handle:{any}");
        }
    }

}


