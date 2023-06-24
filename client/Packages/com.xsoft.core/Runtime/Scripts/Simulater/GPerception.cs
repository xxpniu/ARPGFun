using System;

namespace EngineCore.Simulater
{
	public abstract class GPerception
	{
		protected GPerception (GState state)
		{
			this.State = state;
		}

		public GState State{set;get;}

		protected void JoinElement(GObject el)
        {
            State.AddElement(el);
        }
	}
}

