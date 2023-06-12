using EngineCore.Simulater;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Layout.LayoutEffects;
using Proto;
using XNet.Libs.Utility;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.Controllors
{
    public class ControllorState : GControllor
    {
        private const string TimeKey = "__SkillTime__";
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
                    var lastSkillTime = character[TimeKey];
                    if (lastSkillTime != null &&  (float)lastSkillTime +0.2f > time.Time)
                    {
                        return GAction.Empty;
                    }

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
                            if (!TryReleaseMagic(target, character, key)) return true;
                            character.IsCoolDown(t.ConfigId, time.Time, true, character.NormalCdTime);
                            character[TimeKey] = time.Time;
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
                    if (!TryReleaseMagic(target, character, key, data.Params)) break;
                    character.IsCoolDown(skill.MagicId, time.Time, true);
                    character[TimeKey] = time.Time;
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
        
        private bool TryReleaseMagic(IReleaserTarget target, BattleCharacter character, 
            string key, string[] magicParams = default)
        {
            return BattlePerception.CreateReleaser(key, character, target,
                ReleaserType.Magic, ReleaserModeType.RmtMagic,
                -1, true, magicParams);
        }


        private bool TryGetReleaserTarget(EConfig.CharacterMagicData config, BattleCharacter character,
            out IReleaserTarget target)
        {
            target = null;
            var tCharacter = BattlePerception.FindTarget(character, config.GetTeamType(),
                config.RangeMax/100f, 360, ignoreHidden:false);
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
