using EngineCore.Simulater;

namespace GameLogic.Game.Elements
{
    public delegate void HanlderEvent<T>(T el) where T : GObject;

    public class BattleElement<T> : GObject where T : IBattleElement
    {
        public HanlderEvent<GObject> OnExitedState;

        public HanlderEvent<GObject> OnJoinedState;

        public BattleElement(GControllor controller, T view) : base(controller)
        {
            View = view;
        }

        protected T View { get; }

        protected override void OnJoinState()
        {
            base.OnJoinState();
            View?.AttachElement(this);
            View?.JoinState(Index);
            OnJoinedState?.Invoke(this);
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            View?.ExitState(Index);
            OnExitedState?.Invoke(this);
        }
    }
}