using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Game.Perceptions;
using ExcelConfig;
using System.Linq;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using EngineCore.Simulater;
using Proto;
using XNet.Libs.Utility;

namespace GameLogic.Game.LayoutLogics
{
    /// <summary>
    /// 处理layout
    /// </summary>
	public class HandleLayoutAttribute:Attribute
	{
		public HandleLayoutAttribute(Type handleType)
		{
			HandleType = handleType;
		}
        /// <summary>
        /// layout type
        /// </summary>
		public Type HandleType{set;get;}
	}
    /// <summary>
    /// Layout base logic.
    /// </summary>
    public static class LayoutBaseLogic
	{
        #region  EnableLayout
		static  LayoutBaseLogic ()
		{
			var type = typeof(LayoutBaseLogic);
			var methods=type.GetMethods (BindingFlags.Public | BindingFlags.Static);
			foreach (var i in methods)
			{
				var att = i.GetCustomAttribute<HandleLayoutAttribute>(false);
				if (att == null) continue;
				Handler.Add(att.HandleType, i);
			}
		}

		private static readonly Dictionary<Type,MethodInfo> Handler = new();

		public static void EnableLayout(LayoutBase layout, TimeLinePlayer player)
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

        #region LookAtTarget
		//LookAtTarget
		[HandleLayout(typeof(LookAtTarget))]
		public static void LookAtTargetActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			if (linePlayer.Releaser?.Releaser == linePlayer.Releaser?.Target) return;
			//var layout = layoutBase as LookAtTarget;
            linePlayer.Releaser?.Releaser?.LookAt(linePlayer.Releaser.Target,true);
		}
		#endregion

		#region MissileLayout
		[HandleLayout(typeof(MissileLayout))]
		public static void MissileActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var layout = layoutBase as MissileLayout;
			var per = linePlayer.Releaser.Controllor.Perception as BattlePerception;
			var missile = per!.CreateMissile(layout, linePlayer.Releaser);
			if (missile != null)
				linePlayer.Releaser.AttachElement(missile);
		}
		#endregion

		#region DamageLayout
		[HandleLayout(typeof(DamageLayout))]
		public static void DamageActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var releaser = linePlayer.Releaser;
			var layout = layoutBase as DamageLayout;

			UVector3? targetPos;
			var rotation = UnityEngine.Quaternion.identity;
			var deTarget = releaser.Target;
			switch (layout!.target)
			{
				case Layout.TargetType.Releaser:
					targetPos = releaser.Releaser.Position;
					rotation = releaser.Releaser.Rotation;
					deTarget = releaser.Releaser;
					break;
				case Layout.TargetType.Target:
					if (releaser.ReleaserTarget.ReleaserTarget == null) return;
					targetPos = releaser.Target.Position;
					rotation = releaser.Target.Rotation;
					deTarget = releaser.Target;
					break;
				case Layout.TargetType.EventTarget:
					targetPos = linePlayer.EventTarget.Position;
					rotation = linePlayer.EventTarget.Rotation;
					deTarget = linePlayer.EventTarget;
					break;
				case Layout.TargetType.ReleaseInstance:
					targetPos = linePlayer.Releaser.Position;
					rotation = linePlayer.Releaser.Rotation;
					break;
				default:
					targetPos = releaser.ReleaserTarget.TargetPosition;
					break;
			}

			if (targetPos == null)
			{
				throw new Exception("Do not have target of origin. type:" + layout.target.ToString());
			}

			var offsetPos = layout.RangeType.offsetPosition.ToUV3();
			var per = releaser.Controllor.Perception as BattlePerception;
			var targets = per!.DamageFindTarget(
				deTarget,
				targetPos.Value,
				rotation,
				layout.RangeType.fiterType,
				layout.RangeType.damageType,
				layout.RangeType.radius,
				layout.RangeType.angle,
				layout.RangeType.offsetAngle,
				offsetPos,
				releaser.Releaser.TeamIndex);

			releaser.ShowDamageRange(layout,targetPos.Value,rotation);

			if (string.IsNullOrEmpty(layout.effectKey))
			{
				return;
			}

			var group = linePlayer.TypeEvent.FindGroupByKey(layout.effectKey);
			if (group == null) return;
			foreach (var t in targets)
			{
				if (!t) continue;
				foreach (var i in group.effects)
				{
					EffectBaseLogic.EffectActive(t, i, releaser);
				}
			}

		}
		#endregion

		#region CallUnitLayout
		[HandleLayout(typeof(CallUnitLayout))]
		public static void CallUnitActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var unitLayout = layoutBase as CallUnitLayout;
			var releaser = linePlayer.Releaser;
			var character = releaser.ReleaserTarget.Releaser;
			var per = releaser.Controllor.Perception as BattlePerception;
			var level = unitLayout!.level.ProcessValue(linePlayer.Releaser);
			//判断是否达到上限
			if (unitLayout.maxNum <= releaser.UnitCount) return;
			var id = unitLayout.characterID.ProcessValue(releaser);
			var data = ExcelToJSONConfigManager.GetId<CharacterData>(id);
			if (data == null)
			{
				Debuger.LogError($"Not found call unit of {id}");
				return;
			}
			
			var levelConfig = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level ==level);
			var properties = data.CreatePlayerProperties(levelConfig);
			var magics = data.CreateHeroMagic();
			var unit = per!.CreateCharacter(per.AIControllor,
				level,
				data,
				magics,
				properties,
				character.TeamIndex,
				character.Position + character.Rotation * unitLayout.offset.ToUV3(),
				character.Rotation.eulerAngles,
				character.AccountUuid,
                data.Name,
                releaser.Releaser.Index
			);
			//unit.ResetHPMP();
			unit.LookAt(releaser.ReleaserTarget.ReleaserTarget);
			releaser.AttachElement(unit, false, unitLayout.time.ProcessValue(releaser)/1000f);
			releaser.OnEvent(Layout.EventType.EVENT_UNIT_CREATE,unit);
			var ai = unitLayout.AIPath;
			if (string.IsNullOrEmpty(ai)) ai = data.AIResourcePath;
			per.ChangeCharacterAI(ai, unit);
			unit.OnDead = (el) => 
			{
				releaser.OnEvent(Layout.EventType.EVENT_UNIT_DEAD,unit);
                GObject.Destroy(el, 3);
			};
		}
		#endregion

		#region LaunchSelfLayout

		[HandleLayout(typeof(LaunchSelfLayout))]
		public static void LaunchSelfActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var launch = layoutBase as LaunchSelfLayout;
			var releaser = linePlayer.Releaser;
			var character = releaser.ReleaserTarget.Releaser;
			var dis = launch!.distance;
			if (launch.reachType == TargetReachType.DistanceOfTaget)
			{
				dis = BattlePerception.Distance(character, releaser.ReleaserTarget.ReleaserTarget) + 2;
			}

			character.BeginLaunchSelf(character.Rotation,
				dis,
				launch.speed,
				(hit, obj) =>
				{
					if (hit.IsDeath) return;
					if (obj is not MagicReleaser r) return;
					if (hit.TeamIndex == r.ReleaserTarget.Releaser.TeamIndex) return;
					if (r.TryHit(hit)) r.OnEvent(Layout.EventType.EVENT_MISSILE_HIT,hit);
				},
				releaser);
		}
		#endregion

		#region RepeatTimeLine
		[HandleLayout(typeof(RepeatTimeLine))]
		public static void RepeatTimeLineActive(TimeLinePlayer player, LayoutBase layoutBase)
		{
			if (layoutBase is RepeatTimeLine r) player.Repeat(r.RepeatCount,r.ToTime);
		}
		#endregion

	}
}

