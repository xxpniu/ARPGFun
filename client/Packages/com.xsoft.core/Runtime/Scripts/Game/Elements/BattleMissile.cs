using System;
using EngineCore.Simulater;
using Layout.LayoutElements;

namespace GameLogic.Game.Elements
{

	public enum MissileState
	{
		NoStart,
		Moving,
		Hit,
		Death
	}

	public class BattleMissile:BattleElement<IBattleMissile>
	{
		public BattleMissile(GControllor controllor,MagicReleaser releaser,
		                     IBattleMissile view, 
		                     MissileLayout layout, BattleCharacter target) : base(controllor, view)
		{
			State = MissileState.NoStart;
			Releaser = releaser;
			Layout = layout;
			Target = target;
		}
			
		public MagicReleaser Releaser { private set; get; }

		public MissileLayout Layout { private set; get; }

		public MissileState State { set; get; }

		public float TotalTime { get; internal set; } = -1f;

        public BattleCharacter Target { get; }

		//public GTime TimeStart { set; get; }
	}
}

