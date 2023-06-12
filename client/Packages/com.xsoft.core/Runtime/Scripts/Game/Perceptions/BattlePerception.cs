using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;
using GameLogic.Game.Controllors;
using Layout.LayoutElements;
using System.Collections.Generic;
using GameLogic.Game.LayoutLogics;
using Layout;
using Layout.AITree;
using GameLogic.Game.AIBehaviorTree;
using Proto;
using Layout.LayoutEffects;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using UnityEngine;
using System.Linq;
using GameLogic.Game.States;
using DamageType = Layout.LayoutElements.DamageType;

namespace GameLogic.Game.Perceptions
{
    /// <summary>
    /// 战斗感知器
    /// </summary>
	public class BattlePerception : GPerception
    {
        public class EmptyControllor : GControllor
        {
            public EmptyControllor(BattlePerception p) : base(p) { }
            public override GAction GetAction(GTime time, GObject current)
            {
                return GAction.Empty;
            }
        }

        public static float Distance(BattleCharacter c1, BattleCharacter c2)
        {
            var size = c2.Radius + c1.Radius;
            return Math.Max(0, (c1.Position - c2.Position).magnitude - size);
        }

        public static float Distance(BattleCharacter c1, UVector3 c2)
        {
            return Math.Max(0, (c1.Position - c2).magnitude - c1.Radius);
        }

        public static bool InviewSide(BattleCharacter ower , BattleCharacter target, float viewDistance, float angle)
        {
            if (Distance(ower, target) > viewDistance) return false;
            var forward = target.Position - ower.Position;
            if (angle / 2 < UVector3.Angle(forward, ower.Forward)) return false;
            return true;
        }

        public BattlePerception(GState state, IBattlePerception view) : base(state)
        {
            View = view;
            Empty = new EmptyControllor(this);
            ReleaserControllor = new MagicReleaserControllor(this);
            BattleMissileControllor = new BattleMissileControllor(this);
            AIControllor = new AIControllor(this);
            BattleItemControllor = new BattleItemControllor(this);
            StateControllor = new ControllorState(this);
        }

        public IBattlePerception View { private set; get; }


        #region controllor
        public BattleMissileControllor BattleMissileControllor { private set; get; }
        public MagicReleaserControllor ReleaserControllor { private set; get; }
        public AIControllor AIControllor { private set; get; }
        public BattleItemControllor BattleItemControllor { private set; get; }
        public EmptyControllor Empty { private set; get; }
        public ControllorState StateControllor { private set; get; }
        #endregion

        #region create Elements 
        public MagicReleaser CreateReleaser(string key, BattleCharacter owner, 
            IReleaserTarget target, ReleaserType ty,
            ReleaserModeType rmType, float durTime,bool canMoveCancel = false, string[] magicParams = default)
        {
            var magic = View.GetMagicByKey(key);
            if (magic == null)
            {
                Debug.LogError($"{key} no found!");
                return null;
            }
            var releaser = CreateReleaser(key, owner, magic, target, ty, rmType, durTime,canMoveCancel,magicParams);
            
            return releaser;
        }

        public MagicReleaser CreateReleaser(string key, BattleCharacter owner, MagicData magic, IReleaserTarget target, ReleaserType ty, 
            ReleaserModeType rmType, float durTime, bool canMoveCancel = false, string[] magicParams = default)
        {
            if (magic.unique)
            {
                var have = false;
                State.Each<MagicReleaser>(t =>
                {
                    if (t.Releaser.Index != owner.Index) return false;
                    if (t.MagicKey != key) return false;
                    have = true;
                    return true;
                });
                if (have) return null;
            }

            var forward = target.TargetPosition - target.Releaser.Position;
            var r = owner.Rotation;
            if (forward.magnitude > 0)
            {
                r = Quaternion.LookRotation(forward);
            }

            var view = View.CreateReleaserView(owner.Transform.position.ToPV3(),
                r.eulerAngles.ToPV3(), target.Releaser.Index,
                target.ReleaserTarget.Index, key, target.TargetPosition.ToPV3(), rmType);
            view.MagicData = magic;
            var mReleaser = new MagicReleaser(key, magic, owner, target, this.ReleaserControllor, view, ty, durTime,canMoveCancel, magicParams);
            switch (rmType)
            {
                case ReleaserModeType.RmtMagic:
                    owner.FireEvent(BattleEventType.Skill, mReleaser);
                    break;
                case ReleaserModeType.RmtNormalAttack:
                    owner.FireEvent(BattleEventType.NomarlAttack, mReleaser);
                    break;
            }

            this.JoinElement(mReleaser);
            return mReleaser;
        }

        public BattleMissile CreateMissile(MissileLayout layout, MagicReleaser releaser)
        {
           // int targetIndex = -1;
            var target = releaser.Target;

            if (layout.movementType == MovementType.AutoTarget)
            {
                target = FindTarget(releaser.Releaser, TargetTeamType.Enemy, layout.maxDistance, 60, true, TargetSelectType.Random, TargetFilterType.None);
            }

            if (target == null) return null;

            var view = this.View.CreateMissile(releaser.Index,target.Index,
                layout.resourcesPath,
                layout.offset.ToV3(),
                layout.fromBone,
                layout.toBone,
                layout.speed,
                (int)layout.movementType,
                layout.maxDistance,
                layout.maxLifeTime);
            var missile = new BattleMissile(BattleMissileControllor, releaser, view, layout, target);
            this.JoinElement(missile);
            return missile;
        }

        #endregion

        #region Character
        public BattleCharacter CreateCharacter(
            GControllor controllor,
            int level,
            CharacterData data,
            IList<BattleCharacterMagic> magics,
            Dictionary<HeroPropertyType, ComplexValue > properties,
            int teamIndex,
            UVector3 position,
            UVector3 forward,
            string accountUuid,
            string name,int ownerIndex = -1, int hp = -1,int mp =-1)
        {
            
            var now = this.View.GetTimeSimulater().Now.Time;
            var cds = magics.Select(t => t.ToHeroMagic(now)) .ToList();

            var view = View.CreateBattleCharacterView(accountUuid, data.ID,
                teamIndex, position.ToPV3(), forward.ToPV3(), level, name, cds,
                ownerIndex,properties.ToHeroProperty(),
                properties[HeroPropertyType.MaxHp],
                properties[HeroPropertyType.MaxMp]
                );

            var battleCharacter = new BattleCharacter(data,magics, controllor,
                view, accountUuid,teamIndex,  properties, ownerIndex);

            battleCharacter.EachMagicByType(MagicType.MtMagic, (m) =>
            {
                m.CdTime = battleCharacter.NormalCdTime;
                return false;
            });

            battleCharacter.Level = level;
            battleCharacter.TDamage =Proto.DamageType.Confusion;
            battleCharacter.TDefance = DefanceType.Normal;
            battleCharacter.Category = (HeroCategory)data.Category;
            battleCharacter.Name = data.Name;
            battleCharacter.ResetHPMP(hp, mp);
            view.SetPriorityMove(data.PriorityMove);
            this.JoinElement(battleCharacter);
            return battleCharacter;
        }

        internal void ProcessDamage(BattleCharacter sources, BattleCharacter effectTarget, DamageResult result)
        {
            View.ProcessDamage(sources.Index, effectTarget.Index, result.Damage, result.IsMissed, result.CrtMult);
            NotifyHurt(effectTarget);
            if (result.IsMissed) return;
            effectTarget.AttachDamage(sources.Index, result.Damage, View.GetTimeSimulater().Now.Time);
            if (!effectTarget.SubHP(result.Damage, out var dead)) return;
            if (dead) sources.FireEvent(BattleEventType.Killed, effectTarget);
        }



        public BattleItem CreateItem(UVector3 ps, PlayerItem item, int groupIndex, int teamIndex)
        {
            var view = View.CreateDropItem(ps.ToPV3(), item, teamIndex, groupIndex);
            var dItem = new BattleItem(this.BattleItemControllor, view, item);
            JoinElement(dItem); 
            return dItem;
        }

        public AITreeRoot ChangeCharacterAI(string pathTree, BattleCharacter character)
        {
            var ai = View.GetAITree(pathTree);
            return ChangeCharacterAI(ai, character,pathTree);
        }

        public AITreeRoot ChangeCharacterAI(TreeNode ai, BattleCharacter character, string path = null)
        {
            var comp = AITreeParse.CreateFrom(ai,View);
            var root = new AITreeRoot(View.GetTimeSimulater(), character, comp, ai,path);
            character.SetAITreeRoot(root);
            character.SetControllor(AIControllor);
            return root;
        }

        #endregion

        public BattleCharacter FindTarget(int target)
        {
            return this.State[target] as BattleCharacter;
        }

        public BattleCharacter FindTarget(BattleCharacter character, TargetTeamType type,
            float distance,
            float view,
            bool igDead = true,
            TargetSelectType sType = TargetSelectType.Nearest,
            TargetFilterType filterType = TargetFilterType.None,
            bool ignoreHidden = true)
        {

            var list = new List<BattleCharacter>();
            State.Each<BattleCharacter>(t =>
            {
                //隐身的不进入目标查找
                if (ignoreHidden && t.IsLock(ActionLockType.NoInhiden)) return false;
                if (igDead && t.IsDeath) return false;
                switch (type)
                {
                    case TargetTeamType.Enemy:
                        if (character.TeamIndex == t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.OwnTeam:
                        if (character.TeamIndex != t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.OwnTeamWithOutSelf:
                        if (character.Index == t.Index) return false;
                        if (character.TeamIndex != t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.Own:
                        {
                            if (character.Index != t.Index) return false;
                        }
                        break;
                    case TargetTeamType.All:
                        break;
                    default:
                        return false;
                }

                if (!InviewSide(character, t, distance, view)) return false;
                switch (filterType)
                {
                    case TargetFilterType.Hurt:
                        if (t.HP == t.MaxHP) return false;
                        break;
                }
                list.Add(t);
                return false;
            });
            if (list.Count <= 0) return null;
            
            BattleCharacter target = null;
            switch (sType)
            {
                case TargetSelectType.Nearest:
                {
                    target = list[0];
                    var d = Distance(target, character);
                    foreach (var i in list)
                    {
                        var temp = Distance(i, character);
                        if (!(temp < d)) continue;
                        d = temp;
                        target = i;
                    }
                }
                    break;
                case TargetSelectType.ForwardNearest:
                {
                    var forward = character.Forward;
                    var dis = 6f ;
                    foreach (var i in list)
                    {
                        var temp = UVector3.Angle(i.Position - character.Position, forward);
                        if (!(temp < 15)) continue;
                        if (dis < Distance(i, character)) continue;
                        dis = Distance(i, character);
                        target = i;
                    }
                    //no nearest
                    if (target == null)
                    {
                        target = list[0];
                        var d = Distance(target, character);
                        foreach (var i in list)
                        {
                            var temp = Distance(i, character);
                            if (!(temp < d)) continue;
                            d = temp;
                            target = i;
                        }
                    }
                }
                    break;
                case TargetSelectType.Random:
                    target = GRandomer.RandomList(list);
                    break;
                case TargetSelectType.HPMax:
                {
                    target = list[0];
                    var d = target.HP;
                    foreach (var i in list)
                    {
                        var temp = i.HP;
                        if (temp > d)
                        {
                            d = temp;
                            target = i;
                        }
                    }
                }
                    break;
                case TargetSelectType.HPMin:
                {
                    target = list[0];
                    var d = target.HP;
                    foreach (var i in list)
                    {
                        var temp = i.HP;
                        if (temp < d)
                        {
                            d = temp;
                            target = i;
                        }
                    }
                }
                    break;
                case TargetSelectType.HPRateMax:
                {
                    target = list[0];
                    var d = (float)target.HP / target.MaxHP;
                    foreach (var i in list)
                    {
                        var temp = (float)i.HP / i.MaxHP; ;
                        if (!(temp > d)) continue;
                        d = temp;
                        target = i;
                    }
                }
                    break;
                case TargetSelectType.HPRateMin:
                {
                    target = list[0];
                    var d = (float)target.HP / target.MaxHP;
                    foreach (var i in list)
                    {
                        var temp = (float)i.HP / i.MaxHP;
                        if (!(temp < d)) continue;
                        d = temp;
                        target = i;
                    }
                } break;
            }

            return target;

        }

        public List<BattleCharacter> DamageFindTarget(BattleCharacter deTarget,
            UVector3 target,
            Quaternion rotation,
            FilterType filter,DamageType damageType,
            float radius, float angle, float offsetAngle,
            UVector3 offset, int teamIndex, bool igDeath = true)
        {
            switch (damageType)
            {
                case DamageType.Area:
                    {
                        var origin = target + rotation * offset;
                        var q = Quaternion.Euler(0, offsetAngle, 0);
                        var forward = q * rotation * UVector3.forward;
                        var list = new List<BattleCharacter>();
                        State.Each<BattleCharacter>((t) =>
                        {
                            if (igDeath && t.IsDeath) return false;//ig
                            //过滤
                            switch (filter)
                            {
                                case FilterType.Alliance:
                                case FilterType.OwnerTeam:
                                    if (teamIndex != t.TeamIndex) return false;
                                    break;
                                case FilterType.EmenyTeam:
                                    if (teamIndex == t.TeamIndex) return false;
                                    break;

                            }
                            if (Distance(t, origin) > radius) return false;
                            var len = t.Position - origin;
                            if (angle < 360)
                            {
                                var an = UVector3.Angle(len, forward);
                                if (an > angle / 2) return false;
                            }
                            list.Add(t);
                            return false;
                        });
                        return list;
                    }

                case DamageType.Single:
                default:
                {
                    switch (filter)
                    {
                        case FilterType.Alliance:
                        case FilterType.OwnerTeam:
                            if (teamIndex == deTarget.TeamIndex)
                                return new List<BattleCharacter> { deTarget };
                            break;
                        case FilterType.EmenyTeam:
                            if (teamIndex != deTarget.TeamIndex)
                                return new List<BattleCharacter> { deTarget };
                            break;
                        default:
                            return new List<BattleCharacter> { deTarget };

                    }
                    return new List<BattleCharacter> {  };
                }
                   

            }
        }

        public void StopAllReleaserByCharacter(BattleCharacter character)
        {
            State.Each<MagicReleaser>(t =>
            {
                if (t.ReleaserTarget.Releaser == character)
                {
                    t.SetState(ReleaserStates.Ended);//防止AI错误
                    GObject.Destroy(t);
                }
                return false;
            });
        }

        public void BreakReleaserByCharacter(BattleCharacter character, BreakReleaserType type, bool move = false)
        {
            State.Each<MagicReleaser>(t =>
            {
                if (t.Releaser != character) return false;
                if (move)
                {
                    if (!t.MoveCancel) return false;
                }

                switch (type)
                {
                    case BreakReleaserType.InStartLayoutMagic:
                    {
                        if (t.RType == ReleaserType.Magic)
                        {
                            if (!t.IsLayoutStartFinish)
                            {
                                t.StopAllPlayer();
                            }
                            t.SetState(ReleaserStates.ToComplete);
                        }
                    }
                        break;
                    case BreakReleaserType.Buff:
                    {
                        if (t.RType == ReleaserType.Buff)
                        {
                            t.SetState(ReleaserStates.ToComplete);
                        }
                    }
                        break;
                    case BreakReleaserType.ALL:
                    {
                        t.SetState(ReleaserStates.ToComplete);
                    }
                        break;
                }
                return false;
            });
        }

        public void NotifyHurt(BattleCharacter sources)
        {
            var constant = (State as BattleState)?.ViewBase.GetConstant;
            State.Each<BattleCharacter>((c) => 
            {
                if (c.IsDeath) return false;
                if (c.TeamIndex != sources.TeamIndex) return false;
                if (Distance(c, sources) < (constant!.BATTLE_NOTIFY_DISTANCE/100f))
                {
                    c.FireEvent(BattleEventType.TeamBeAttack, sources);
                }
                return false;
            });
        }
    }
}

