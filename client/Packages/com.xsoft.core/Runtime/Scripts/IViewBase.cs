using EConfig;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;

namespace GameLogic
{
    public interface IViewBase
    {
        ConstantValue GetConstant { get; }
        IBattlePerception Create(ITimeSimulator simulator);
    }
}