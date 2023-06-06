using System;
using Layout.LayoutEffects;
using GameLogic.Game.Elements;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using P = Proto.HeroPropertyType;
using Layout;

namespace GameLogic.Game.LayoutLogics
{

	public class EffectHandleAttribute : Attribute
	{
		public EffectHandleAttribute(Type handleType)
		{
			HandleType = handleType;
		}

		public Type HandleType { set; get; }
	}

    public class EffectBaseLogic
    {
        static EffectBaseLogic()
        {
            _handlers = new Dictionary<Type, MethodInfo>();
            var methodInfos = typeof(EffectBaseLogic).GetMethods();
            foreach (var i in methodInfos)
            {
                var attrs = i.GetCustomAttributes(typeof(EffectHandleAttribute), false) as EffectHandleAttribute[];
                if (attrs.Length == 0) continue;
                _handlers.Add(attrs[0].HandleType, i);
            }
        }

        private static readonly Dictionary<Type, MethodInfo> _handlers;

        /// <summary>
        /// Effects the active.
        /// </summary>
        /// <param name="effectTarget">成熟效果的目标</param>
        /// <param name="effect">效果类型</param>
        /// <param name="releaser">魔法释放者</param>
        public static void EffectActive(BattleCharacter effectTarget, EffectBase effect, MagicReleaser releaser)
        {
            if (_handlers.TryGetValue(effect.GetType(), out MethodInfo handle))
            {
                handle.Invoke(null, new object[] { effectTarget, effect, releaser });
            }
            else
            {
                throw new Exception(string.Format("Effect [{0}] no handler!!!", effect.GetType()));
            }
        }

        private static int GetVauleBy(BattleCharacter owner, BattleCharacter target, ValueOf vOf, int value)
        {
            switch (vOf)
            {
                case ValueOf.HPMaxPro: return (int)(target.MaxHP * (value / 10000f));
                case ValueOf.HPPro: return (int)(target.HP * (value / 10000f));
                case ValueOf.MPMaxPro: return (int)(target.MaxHP * (value / 10000f));
                case ValueOf.MPPro: return (int)(target.MP * (value / 10000f));
                case ValueOf.NormalAttack: return (int)(BattleAlgorithm.CalFinalDamage( BattleAlgorithm.CalNormalDamage(owner),owner.TDamage, target.TDefance) *(1f + value/10000f));
                case ValueOf.FixedValue:
                default: return value;

            }
        }

        [EffectHandle(typeof(NormalDamageEffect))]
        public static void NormalDamage(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var per = releaser.Controllor.Perception as BattlePerception;
            var effect = e as NormalDamageEffect;
            int damage = GetVauleBy(releaser.Releaser, effectTarget, effect.valueOf, effect.DamageValue.ProcessValue(releaser));
            var result = BattleAlgorithm.GetDamageResult(releaser.Releaser, damage, releaser.Releaser.TDamage, effectTarget);
            if (releaser.ReleaserTarget.Releaser.TDamage != Proto.DamageType.Magic)
            {
                if (!result.IsMissed)
                {
                    var cureHP = (int)(result.Damage * releaser.Releaser[P.Hpdrain] / 10000f);
                    if (cureHP > 0) releaser.Releaser.AddHP(cureHP);
                    var cureMp = (int)(result.Damage * releaser.Releaser[P.MpDrain] / 10000f);
                    if (cureMp > 0) releaser.Releaser.AddMP(cureMp);
                }
            }

            if (!result.IsMissed) effectTarget.FireEvent(BattleEventType.Hurt, releaser.Releaser);
            per.ProcessDamage(releaser.Releaser, effectTarget, result);
        }

        //CureEffect
        [EffectHandle(typeof(CureEffect))]
        public static void Cure(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as CureEffect;
            int cure =  GetVauleBy(releaser.Releaser, effectTarget, effect.valueType, effect.value.ProcessValue(releaser));
            if (cure > 0)
            {
                effectTarget.AddHP(cure);
            }
        }
        //CureEffect
        [EffectHandle(typeof(CureMPEffect))]
        public static void CureMP(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as CureMPEffect;
            int cure = GetVauleBy(releaser.Releaser, effectTarget, effect.valueType, effect.value.ProcessValue(releaser));
            if (cure > 0) effectTarget.AddMP(cure);
        }


        [EffectHandle(typeof(AddBufEffect))]
        public static void AddBuff(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as AddBufEffect;
            var per = releaser.Controllor.Perception as BattlePerception;

            var rT = new ReleaseAtTarget(releaser.Releaser, effectTarget);
            var r= per.CreateReleaser(effect.buffMagicKey, releaser.Releaser, rT, ReleaserType.Buff, Proto.ReleaserModeType.RmtBuff, effect.durationTime.ProcessValue(releaser)/1000f);
            if (effect.CopyParams) r.SetParam(releaser.Params);
            r.DisposeValue = effect.DiType;
        }

        [EffectHandle(typeof(BreakReleaserEffect))]
        public static void BreakAction(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as BreakReleaserEffect;
            var per = releaser.Controllor.Perception as BattlePerception;
            per.BreakReleaserByCharacter(effectTarget, effect.breakType);
        }

        [EffectHandle(typeof(AddPropertyEffect))]
        public static void AddProperty(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as AddPropertyEffect;
            effectTarget.ModifyValueAdd(effect.property, effect.addType, effect.addValue.ProcessValue(releaser));
            if (effect.revertType == RevertType.ReleaserDeath)  releaser.RevertProperty(effectTarget, effect.property, effect.addType, effect.addValue.ProcessValue(releaser));
        }

        [EffectHandle(typeof(ModifyLockEffect))]
        public static void ModifyLockEffect(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as ModifyLockEffect;
            effectTarget.LockAction(effect.lockType);
            if (effect.revertType == RevertType.ReleaserDeath) releaser.RevertLock(effectTarget, effect.lockType);
        }

    
        //CharmEffect
        [EffectHandle(typeof(CharmEffect))]
        public static void CharmEffect(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            var effect = e as CharmEffect;
            if (effectTarget.Level > effect.Level.ProcessValue(releaser)) return;
            if (GRandomer.Probability10000(effect.ProValue.ProcessValue(releaser)))
            {
                effectTarget.Clear();
                var re = releaser.Releaser;
                effectTarget.SetTeamIndex(re.TeamIndex, re.Index);
                releaser.AttachElement(effectTarget, false, effect.Time.ProcessValue(releaser) / 1000f);
            }
        }

        [EffectHandle(typeof(TransportEffect))]
        public static void Transport(BattleCharacter effectTarget, EffectBase e, MagicReleaser releaser)
        {
            if (e is TransportEffect transport)
            {
                UnityEngine.Vector3 tar;
                switch (transport.ValueOf)
                {
                    
                    case TransportEffect.TranportValueOf.Value:
                        tar = transport.TargetPos.ToUV3();
                        break;
                    default:
                    case TransportEffect.TranportValueOf.ReleaseTargetPos:
                        tar = releaser.ReleaserTarget.TargetPosition;
                        break;
                }

                effectTarget.TryToSetPosition(tar, 0);
            }
            return;
        }
    }
}

