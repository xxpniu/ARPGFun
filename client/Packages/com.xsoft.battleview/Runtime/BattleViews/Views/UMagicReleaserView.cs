using System.Collections.Generic;
using BattleViews.Components;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Google.Protobuf;
using Layout;
using Layout.LayoutElements;
using Proto;
using UnityEngine;
using DamageType = Layout.LayoutElements.DamageType;
using EventType = Layout.EventType;
using UVector3 = UnityEngine.Vector3;

namespace BattleViews.Views
{
    public class UMagicReleaserView : UElementView, IMagicReleaser
    {
        public UVector3 TargetPos;

        private readonly LinkedList<TimeLineViewPlayer> _players = new();


        private readonly List<IParticlePlayer> pPlayers = new();


        private MagicData Magic;
        private int RIndex;
        private int TIndex;

        public UCharacterView CharacterTarget { private set; get; }
        public UCharacterView CharacterReleaser { private set; get; }

        public MagicReleaseType RMType { private set; get; }

        public string MagicKey { get; private set; }

        private void Update()
        {
            TickTimeLine(PerView.GetTime());
        }

        private void OnDestroy()
        {
            foreach (var i in pPlayers) i.DestroyParticle();
            pPlayers.Clear();
            foreach (var i in _players) i.Destory();
            _players.Clear();
        }

        UVector3 IMagicReleaser.Position => transform.position;

        Quaternion IMagicReleaser.Rotation => transform.rotation;

        MagicData IMagicReleaser.MagicData
        {
            get => Magic;
            set => Magic = value;
        }

        void IMagicReleaser.PlayTimeLine(int pIndex, int pathIndex, int targetIndex, int type)
        {
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_PlayTimeLine
            {
                PathIndex = pathIndex,
                Index = Index,
                TargetIndex = targetIndex,
                Type = type,
                PlayIndex = pIndex
            });
#endif
#if !UNITY_SERVER

            if (Magic == null)
            {
                Debug.LogError($"Not found magic key {MagicKey}");
                return;
            }

            var eType = (EventType)type;
            var tar = PerView.GetViewByIndex<UCharacterView>(targetIndex);
            if (PerView is IBattlePerception per)
            {
                if (pathIndex < 0 || Magic.Containers.Count <= pathIndex)
                {
                    Debug.LogError($"Index out of bounds {pathIndex} magic containers {Magic?.Containers?.Count}");
                    return;
                }

                var e = Magic.Containers[pathIndex];
                PlayLine(pIndex, per?.GetTimeLineByPath(e.layoutPath), tar, eType);
            }
#endif
        }

        void IMagicReleaser.CancelTimeLine(int pIndex)
        {
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_CancelTimeLine
            {
                Index = Index,
                PlayIndex = pIndex
            });
            ;
#endif
#if !UNITY_SERVER
            foreach (var i in _players)
                if (i.Index == pIndex)
                {
                    _players.Remove(i);
                    i.Destory();
                    break;
                }
#endif
        }

        void IMagicReleaser.PlayTest(int pIndex, TimeLine line)
        {
            PlayLine(pIndex, line, CharacterTarget, EventType.EVENT_START);
        }


        void IMagicReleaser.ShowDamageRanger(DamageLayout layout, UVector3 tar, Quaternion rototion)
        {
#if UNITY_EDITOR
            if (layout.RangeType.damageType == DamageType.Area)
            {
                var pos = tar + rototion * layout.RangeType.offsetPosition.ToUV3();
                DamageRangeDebuger.TryGet(gameObject).AddDebug(layout, pos, rototion);
            }
#endif
        }

        public void SetData(int releaser, int target, UVector3 targetPos, MagicReleaseType rmType, string magicKey)
        {
            CharacterTarget = PerView.GetViewByIndex<UCharacterView>(target);
            CharacterReleaser = PerView.GetViewByIndex<UCharacterView>(releaser);
            RIndex = releaser;
            TIndex = target;
            TargetPos = targetPos;
            RMType = rmType;
            if (PerView is IBattlePerception per)
                if (Magic == null && !string.IsNullOrEmpty(magicKey))
                    Magic = per?.GetMagicByKey(magicKey);
            MagicKey = magicKey;
        }

        private TimeLineViewPlayer PlayLine(int pIndex, TimeLine timeLine, UCharacterView eventTarget, EventType type)
        {
            if (timeLine == null) return null;
            var player = new TimeLineViewPlayer(pIndex, timeLine, this, eventTarget, type);
            _players.AddLast(player);
            return player;
        }

        private void TickTimeLine(GTime time)
        {
            var current = _players.First;
            while (current != null)
            {
                if (current.Value.Tick(time))
                {
                    current.Value.Destory();
                    _players.Remove(current);
                }

                current = current.Next;
            }
        }

        internal void AttachParticle(IParticlePlayer particle)
        {
            pPlayers.Add(particle);
        }

        public override IMessage ToInitNotify()
        {
            var createNotify = new Notify_CreateReleaser
            {
                Index = Index,
                ReleaserIndex = RIndex,
                TargetIndex = TIndex,
                MagicKey = MagicKey,
                Position = TargetPos.ToPV3(),
                //RMType = RMType,
                OPosition = transform.position.ToPV3(),
                ORotation = transform.rotation.eulerAngles.ToPV3()
            };
            return createNotify;
        }
    }
}