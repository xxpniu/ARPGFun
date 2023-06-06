using System;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;

namespace GameLogic.Game.Elements
{
    public delegate void HanlderEvent<T>(T el) where T  : GObject;

	public class BattleElement<T> : GObject where T : IBattleElement
	{
		public BattleElement(GControllor controllor, T view) : base(controllor)
		{
			View = view;
		}

		protected T View { private set; get; }

		protected override void OnJoinState()
		{
			base.OnJoinState();
			View?.AttachElement(this);
			View?.JoinState(this.Index);
			OnJoinedState?.Invoke(this);


		}

		protected override void OnExitState()
		{
			base.OnExitState();
			View?.ExitState(this.Index);
			OnExitedState?.Invoke(this);

		}

		public HanlderEvent<GObject> OnJoinedState;
		public HanlderEvent<GObject> OnExitedState;
	}
}

