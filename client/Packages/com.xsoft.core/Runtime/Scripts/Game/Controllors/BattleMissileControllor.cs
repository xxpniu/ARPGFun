using EngineCore.Simulater;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using UVector3 = UnityEngine.Vector3;
namespace GameLogic.Game.Controllors
{
    public class BattleMissileControllor:GControllor
	{
		public BattleMissileControllor (GPerception per):base(per)
		{
			
		}

		public override GAction GetAction (GTime time, GObject current)
		{
			var missile = current as BattleMissile;
			var re = missile.Releaser;
			if (missile.TotalTime > 0) missile.TotalTime -= time.DeltaTime;
			switch (missile.State)
			{
				case MissileState.NoStart:
					{

						switch (missile.Layout.movementType)
						{

							case Layout.LayoutElements.MovementType.Line:
								{
									var distance = missile.Layout.maxDistance;
									missile.TotalTime = distance / missile.Layout.speed;
									//missile.TimeStart = time;
								}
								break;
							case Layout.LayoutElements.MovementType.Follow:
								{
									var distance = BattlePerception.Distance(re.Releaser, re.Target);
									missile.TotalTime = distance / missile.Layout.speed;
									//missile.TimeStart = time;
								}
								break;
							case Layout.LayoutElements.MovementType.AutoTarget:
							case Layout.LayoutElements.MovementType.Paracurve:
								{
                                    var dis = BattlePerception.Distance(re.Releaser, missile.Target) *1.25f;
									missile.TotalTime = dis / missile.Layout.speed;
								}
								break;

						}


						missile.State = MissileState.Moving;
						re.OnEvent(Layout.EventType.EVENT_MISSILE_CREATE, missile.Target);
					}
					
					break;
				case MissileState.Moving:
					{
						if (missile.TotalTime<=0)
						{
							re.OnEvent(Layout.EventType.EVENT_MISSILE_HIT, missile.Target);
							missile.State = MissileState.Hit;
						}
					}
					break;
				case MissileState.Hit:
					{
						re.OnEvent(Layout.EventType.EVENT_MISSILE_DEAD,missile.Target);
						missile.State = MissileState.Death;
					}
					break;
				case MissileState.Death:
					GObject.Destroy(missile);
					break;
			}
			return GAction.Empty;
		}
	}
}

