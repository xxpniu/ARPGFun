using System;
using EConfig;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;

namespace GameLogic
{
	public interface IViewBase
	{
		IBattlePerception Create(ITimeSimulator simulator);

		ConstantValue GetConstant { get; }
	}
}

