using System;
using System.Collections.Generic;
using System.Reflection;
using App.Core.Core;
using GameLogic.Game.LayoutLogics;
using Layout.LayoutElements;
using UnityEngine;

namespace BattleViews.Views
{
    public class TimeLineViewPlayer : TimeLinePlayerBase
    {

        #region  EnableLayout
        static TimeLineViewPlayer()
        {
            var type = typeof(TimeLineViewPlayer);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var i in methods)
            {
                if (!(i.GetCustomAttributes(typeof(HandleLayoutAttribute), false) is HandleLayoutAttribute[] atts) || atts.Length == 0)
                    continue;
                Handler.Add(atts[0].HandleType, i);
            }
        }

        private static readonly Dictionary<Type, MethodInfo> Handler = new Dictionary<Type, MethodInfo>();

        private static void ActiveLayout(LayoutBase layout, TimeLineViewPlayer player)
        {
            if (Handler.TryGetValue(layout.GetType(), out MethodInfo m))
            {
                m.Invoke(null, new object[] { player, layout });
            }
            else
            {
                throw new Exception("No Found handle Type :" + layout.GetType());
            }
        }

        #endregion

        #region RepeatTimeLine
        [HandleLayout(typeof(RepeatTimeLine))]
        public static void RepeatTimeLineActive(TimeLineViewPlayer player, LayoutBase layoutBase)
        {
            if (layoutBase is RepeatTimeLine r) player.Repeat(r.RepeatCount,r.ToTime);
        }
        #endregion

        #region ParticleLayout
        [HandleLayout(typeof(ParticleLayout))]
        public static void ParticleActive(TimeLineViewPlayer player, LayoutBase layoutBase)
        {
            var layout = layoutBase as ParticleLayout;
            var particle = player.RView.PerView.CreateParticlePlayer(player.RView, layout,player.EventTarget);
            if (particle == null) return;
            switch (layout!.destoryType)
            {
                case ParticleDestoryType.LayoutTimeOut:
                    player.AttachParticle(particle);
                    break;
                case ParticleDestoryType.Time:
                    particle.AutoDestory(layout.destoryTime);
                    break;
                case ParticleDestoryType.Normal:
                    player.RView.AttachParticle(particle);
                    break;
                
            }
        }


        #endregion

        #region LookAtTarget
        //LookAtTarget
        [HandleLayout(typeof(LookAtTarget))]
        public static void LookAtTargetActive(TimeLineViewPlayer linePlayer, LayoutBase layoutBase)
        {
            if (layoutBase is LookAtTarget)
            {
                if (linePlayer.RView.CharacterReleaser.Index == linePlayer.RView.CharacterTarget.Index) return;
                linePlayer.RView.CharacterReleaser.LookAtIndex(linePlayer.RView.CharacterTarget.Index,true);
            }
        }
        #endregion

        #region MotionLayout

        [HandleLayout(typeof(MotionLayout))]
        public static void MotionActive(TimeLineViewPlayer player, LayoutBase layoutBase)
        {
            var layout = layoutBase as MotionLayout;
            switch (layout!.targetType)
            {
                case Layout.TargetType.Releaser:
                    player.RView.CharacterReleaser.PlayMotion(layout.motionName);
                    break;
                case Layout.TargetType.Target:
                    player.RView.CharacterTarget.PlayMotion(layout.motionName);
                    break;
                case Layout.TargetType.EventTarget:
                    player.EventTarget.PlayMotion(layout.motionName);
                    break;
            }
        }

        #endregion

        #region PlaySoundLayout
        [HandleLayout(typeof(PlaySoundLayout))]
        public static async void PlaySoundLayout(TimeLineViewPlayer player, LayoutBase layoutBase)
        {
            var sound = layoutBase as PlaySoundLayout;
            var tar = sound!.target;
            UnityEngine.Vector3? pos = null;
            switch (tar)
            {
                case Layout.TargetType.EventTarget:
                {
                    if (player.EventTarget is { } p)
                    {
                        if (p) pos = p.GetBoneByName(sound.fromBone).position;
                    }
                }
                    break;
                case Layout.TargetType.Releaser:
                {
                    if (player.RView.CharacterReleaser is { } p)
                    {
                        if (p) pos = p.GetBoneByName(sound.fromBone).position;
                    }
                }
                    break;
                case Layout.TargetType.Target:
                {
                    if (player.RView.CharacterTarget is { } p)
                    {
                        if (p) pos = p.GetBoneByName(sound.fromBone).position;
                    }
                }
                    break;
                default:
                    pos = player.RView.TargetPos;
                    break;

            }

            if (!pos.HasValue) return;
        
            await ResourcesManager.S.LoadResourcesWithExName<AudioClip>(sound.resourcesPath, (clip) =>
            {
                AudioSource.PlayClipAtPoint(clip, pos.Value, sound.value);
            });

        }
        #endregion
 
        public TimeLineViewPlayer(int pIndex, TimeLine line, UMagicReleaserView view, UCharacterView eventTarget, Layout.EventType ty)
            : base(line, pIndex)
        {
            this.RView = view;
            this.EventTarget = eventTarget;
            this.EventType = ty;
            if (this.RView.CharacterReleaser is { } character)
            {
                character.AttachLayoutView(this);
            }
        }

        public UMagicReleaserView RView { get; }
        public UCharacterView EventTarget { get; private set; }
        public Layout.EventType EventType { get; }
        protected override void EnableLayout(LayoutBase layout)
        {
            if (LayoutBase.IsViewLayout(layout)) ActiveLayout(layout, this);
        }
        private readonly List<IParticlePlayer> _players = new();
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.RView.CharacterReleaser is { } character)
            {
                character.DeAttachLayoutView(this);
            }
            foreach (var i in _players)
            {
                i.DestoryParticle();
            }
        }
        private void AttachParticle(IParticlePlayer particle)
        {
            _players.Add(particle);
        }
    }
}
