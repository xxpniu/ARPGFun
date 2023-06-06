using System;
using EngineCore.Simulater;
using GameLogic.Utility;

namespace GameLogic.Game.Elements
{
    public interface IBattleElement
    {
        void JoinState(int index);
        [NeedNotify(typeof(Proto.Notify_ElementExitState), "Index")]
        void ExitState(int index);
        void AttachElement(GObject el);
        int Index { set; get; }
    }
}

