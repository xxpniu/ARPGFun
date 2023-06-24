using System;
using System.Collections.Generic;
using System.Reflection;
using App.Core.UICore.Utility;
using BattleViews.Views;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using GameLogic.Utility;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto;
using UGameTools;
using UnityEngine;
using XNet.Libs.Utility;

namespace UApp.GameGates
{
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
        private readonly Dictionary<string, NotifyMapping> _perceptionInvokes = new();
        private readonly Dictionary<string, NotifyMapping> _elementInvokes = new();

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
                _perceptionInvokes.Add(att.NotifyType.FullName!, new NotifyMapping
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
                if (_elementInvokes.ContainsKey(att.NotifyType.FullName!))
                {
                    Debug.LogError($"{att.NotifyType} had add");
                    continue;
                }
                _elementInvokes.Add(att.NotifyType.FullName, new NotifyMapping { Method = i, Attr = att, Type = att.NotifyType });
                Debug.Log($"{ typeof(T)} handle notify {att.NotifyType}");
            }
        }

        private const string Index = "Index";
    
        /// <summary>
        /// 处理网络包的解析
        /// </summary>
        /// <param name="any">Notify.</param>
        public void Process(Any any)
        {

            var type = any.TypeUrl.Split('/')[1];
            //Debug.Log($"{notify.GetType().Name}->{notify}");
            //优先处理 perception 创建元素
            if (_perceptionInvokes.TryGetValue(type, out var m))
            {
                var notify = Activator.CreateInstance(m.Type) as IMessage;
                notify.MergeFrom(any.Value.ToByteArray());
                var ps = new List<object>();
                foreach (var i in m.Attr.FieldNames)
                {
                    ps.Add(m.Type.GetProperty(i)!.GetValue(notify));
                }
                var go = m.Method.Invoke(PerView, ps.ToArray());

                if (go is UElementView el)
                {
                    el.SetPerception(PerView as UPerceptionView);
                    if((el is IBattleElement b)) b.JoinState((int)notify.GetType().GetProperty(Index).GetValue(notify));
                }

                switch (go)
                {
                    case UCharacterView c:
                        c.TryAdd<HpTipShower>();
                        OnCreateUser?.Invoke(c);
                        break;
                    case UBattleItem item:
                        Debug.Log($"Drop: {item}");
                        item.TryAdd<HpItemNameShower>();
                        break;
                }

                Debuger.Log(notify);
                return;
            }

            if (_elementInvokes.TryGetValue(type, out var em)) 
            {
                var notify = Activator.CreateInstance(em.Type) as IMessage;
                notify.MergeFrom(any.Value.ToByteArray());
                var property = notify!.GetType().GetProperty(Index);
                var index = (int)property!.GetValue(notify);
                var per = PerView as UPerceptionView;
                var v = per!.GetViewByIndex<UElementView>(index);
                if (v == null)
                {
                    Debug.LogError($"No found index {index} by {notify.GetType()} -> {notify}");
                    return;
                }
                var ps = new List<object>();
                foreach (var f in em.Attr.FieldNames)
                {
                    ps.Add(notify.GetType().GetProperty(f)!.GetValue(notify));
                }
                em.Method.Invoke(v, ps.ToArray());
                Debuger.Log($"{notify.GetType()} - {notify}");
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
}


