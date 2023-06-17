using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;

namespace GameLogic.Game.Controllors
{
	public class MagicReleaserControllor : GControllor
	{
		public MagicReleaserControllor(GPerception per) : base(per)
		{

		}

		private bool IsBindAlive(MagicReleaser releaser)
		{
			if (releaser.BindLifeCharacter == null) return true;
			return releaser.BindLifeCharacter.IsAliveAble;
		}

		public override GAction GetAction(GTime time, GObject current)
		{
			var releaser = current as MagicReleaser;
            releaser!.Tick(time);
			switch (releaser.State)
			{
				case ReleaserStates.NOStart:
					{
						releaser.OnEvent(Layout.EventType.EVENT_START);
						releaser.SetState(ReleaserStates.Starting);
					}
					break;
				case ReleaserStates.Starting:
					if (releaser.IsLayoutStartFinish)
					{
						releaser.SetState(ReleaserStates.Releasing);
						releaser.TickTime = 0;
					}

					if (!IsBindAlive(releaser))
					{
						releaser.SetState(ReleaserStates.ToComplete);
					}

					break;
				case ReleaserStates.Releasing:
					{
						if (releaser.Durtime > 0)
						{
							releaser.Durtime -= time.DeltaTime;
							if (releaser.TickTime >= releaser.Magic.triggerTicksTime )
							{
								releaser.TickTime = 0;
								releaser.OnEvent(Layout.EventType.EVENT_TRIGGER);
							}
							releaser.TickTime += time.DeltaTime;
							break;
						}
						if (releaser.IsCompleted)
						{
							releaser.SetState(ReleaserStates.ToComplete);
						}
						
						if (!IsBindAlive(releaser))
						{
							releaser.SetState(ReleaserStates.ToComplete);
						}
					}
					break;
                case ReleaserStates.ToComplete:
                    { 
                        releaser.OnEvent(Layout.EventType.EVENT_END);
                        releaser.SetState(ReleaserStates.Completing);
                    }
                    break;
				case ReleaserStates.Completing:
					{
						if (releaser.IsCompleted) releaser.SetState(ReleaserStates.Ended);
					}
					break;
				case ReleaserStates.Ended:
					{
						if (releaser.IsCompleted) GObject.Destroy(releaser,0.1f);
					}
					break;
			}
			return GAction.Empty;
		}
	}
}

