using System;
using EConfig;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;

namespace GameLogic
{
	public interface IViewBase
	{
		IBattlePerception Create(ITimeSimulater simulater);

		ConstantValue GetConstant { get; }
	}
}

