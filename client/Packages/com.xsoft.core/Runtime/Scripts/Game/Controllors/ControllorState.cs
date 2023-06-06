using System;
using System.Threading.Tasks;
using EngineCore.Simulater;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Google.Protobuf;
using Layout.AITree;
using Layout.LayoutEffects;
using Proto;
using XNet.Libs.Utility;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.Controllors
{
    public class ControllorState : GControllor
    {
        public BattlePerception BattlePerception { get { return Perception as BattlePerception; } }

        public ControllorState(BattlePerception per) : base(per)
        {
           
        }

        public override GAction GetAction(GTime time, GObject current)
        {
            if (current is BattleCharacter character)
            {
                if (!character.IsDeath)
                {
                    if (character.TryDequeueNetAction(out IMessage action))
                    {
                        //Debuger.Log($"{action.GetType()}->{action}");
                        if (action is Action_MoveJoystick move)
                        {
                            if (character.MoveTo(move.WillPos.ToUV3(), out UVector3 _))
                            {
                                CancelStartingReleaser(character);
                            }
                            
                        }
                        else if (action is Action_NormalAttack normal)
                        {
                            character.EachActiveMagicByType(MagicType.MtNormal, time.Time,
                                (t) =>
                            {
                                //Debuger.Log($"{t.Config.MagicKey}");
                                var key = t.Config.MagicKey;
                                if (TryGetReleaserTraget(t.Config, character, out IReleaserTarget target))
                                {
                                    
                                    if (character.SubMP(t.MpCost))
                                    {
                                        if (TryRelaseMagic(target, character, key))
                                            character.IsCoolDown(t.ConfigId, time.Time, true, character.NormalCdTime);
                                    }
                                }
                                return true;
                            });
                        }
                        else if (action is Action_ClickSkillIndex skill)
                        {
                            if (character.TryGetActiveMagicById(skill.MagicId, time.Time, out BattleCharacterMagic data))
                            {
                                
                                var key = data.Config.MagicKey;
                                if (TryGetReleaserTraget(data.Config, character, out IReleaserTarget target))
                                {
                                    if (character.SubMP(data.MpCost))
                                    {
                                        if (TryRelaseMagic(target, character, key))
                                            character.IsCoolDown(skill.MagicId, time.Time, true);
                                    }
                                }
                            }
                        }
                        else if (action is Action_StopMove stop)
                        {
                            character.StopMove(stop.StopPos.ToUV3());
                        }
                        else
                        {
                            Debuger.LogError($"{action.GetType()}:{action}");
                        }
                    }
                }
            }
            return GAction.Empty;
        }

        private void CancelStartingReleaser(BattleCharacter character)
        {
            BattlePerception.BreakReleaserByCharacter(character, BreakReleaserType.InStartLayoutMagic, true);
        }


       // private const string LastReleaser = "_LAST_INDEX_";

        private bool TryRelaseMagic(IReleaserTarget target, BattleCharacter character, string key)
        {
            var r = BattlePerception.CreateReleaser(key, character, target,
                                           ReleaserType.Magic, ReleaserModeType.RmtMagic, -1,true);
           
            return r;
        }


        private bool TryGetReleaserTraget(EConfig.CharacterMagicData config, BattleCharacter character, out IReleaserTarget target)
        {
            target = null;
  
            var tCharacter = BattlePerception.FindTarget(character, config.GetTeamType(),
                config.RangeMax, 360, true, TargetSelectType.Nearest, TargetFilterType.None);
            if (tCharacter)
            {
                target = new ReleaseAtTarget(character, tCharacter);
            }
            else if(config.CanReleaseAtPos())
            {
                var pos = character.Position + character.Forward * config.RangeMax;
                target = new ReleaseAtPos(character, pos);
            }

            return target != null;
        }
    }
}
