﻿using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;

namespace GameLogic.Game.Controllors
{
	public class AIControllor:GControllor
	{
		public AIControllor(GPerception per) : base(per) { }

		public override GAction GetAction(GTime time, GObject current)
		{
			if (current is not BattleCharacter character) return GAction.Empty;
			if (character.IsDeath) return GAction.Empty;
			character?.TickAi();
			return GAction.Empty;
		}
	}
}

