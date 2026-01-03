using EngineCore.Simulater;
using GameLogic.Utility;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IBattleElement
    {
        int Index { set; get; }
        void JoinState(int index);

        [NeedNotify(typeof(Notify_ElementExitState), "Index")]
        void ExitState(int index);

        void AttachElement(GObject el);
    }
}