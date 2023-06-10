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
        public BattlePerception BattlePerception => Perception as BattlePerception;

        public ControllorState(BattlePerception per) : base(per)
        {

        }

        public override GAction GetAction(GTime time, GObject current)
        {
            if (current is not BattleCharacter character) return GAction.Empty;
            if (character.IsDeath) return GAction.Empty;
            if (!character.TryDequeueNetAction(out var action)) return GAction.Empty;
            switch (action)
            {
                case Action_MoveJoystick move:
                {
                    if (character.MoveTo(move.WillPos.ToUV3(), out UVector3 _))
                    {
                        CancelStartingReleaser(character);
                    }

                    break;
                }
                case Action_NormalAttack normal:
                {
                    character.EachActiveMagicByType(MagicType.MtNormal, time.Time,
                        (t) =>
                        {
                            var key = t.Config.MagicKey;
                            if (!TryGetReleaserTarget(t.Config, character, out var target)) return true;
                            if (!character.SubMP(t.MpCost)) return true;
                            if (TryReleaseMagic(target, character, key))
                                character.IsCoolDown(t.ConfigId, time.Time, true, character.NormalCdTime);
                            return true;
                        });

                }
                    break;
                case Action_ClickSkillIndex skill:
                {
                    if (!character.TryGetActiveMagicById(skill.MagicId, time.Time, out var data)) break;
                    var key = data.Config.MagicKey;
                    if (!TryGetReleaserTarget(data.Config, character, out var target)) break;
                    if (!character.SubMP(data.MpCost)) break;
                    if (TryReleaseMagic(target, character, key)) break;
                    character.IsCoolDown(skill.MagicId, time.Time, true);
                }
                    break;

                case Action_StopMove stop:
                    character.StopMove(stop.StopPos.ToUV3());
                    break;
                default:
                    Debuger.LogError($"{action.GetType()}:{action}");
                    break;
            }

            return GAction.Empty;
        }

        private void CancelStartingReleaser(BattleCharacter character)
        {
            BattlePerception.BreakReleaserByCharacter(character, BreakReleaserType.InStartLayoutMagic, true);
        }


        // private const string LastReleaser = "_LAST_INDEX_";

        private bool TryReleaseMagic(IReleaserTarget target, BattleCharacter character, string key)
        {
            var r = BattlePerception.CreateReleaser(key, character, target,
                ReleaserType.Magic, ReleaserModeType.RmtMagic, -1, true);

            return r;
        }


        private bool TryGetReleaserTarget(EConfig.CharacterMagicData config, BattleCharacter character,
            out IReleaserTarget target)
        {
            target = null;

            var tCharacter = BattlePerception.FindTarget(character, config.GetTeamType(),
                config.RangeMax, 360, true, TargetSelectType.Nearest, TargetFilterType.None);
            if (tCharacter)
            {
                target = new ReleaseAtTarget(character, tCharacter);
            }
            else if (config.CanReleaseAtPos())
            {
                var pos = character.Position + character.Forward * config.RangeMax;
                target = new ReleaseAtPos(character, pos);
            }

            return target != null;
        }
    }
}
