using EngineCore.Simulater;

namespace BehaviorTree
{
    public interface ITreeRoot
    {
        GTime Time { get; }
        object UserState { get; }
        bool IsDebug { get; }
        void Change(Composite cur);
    }
}