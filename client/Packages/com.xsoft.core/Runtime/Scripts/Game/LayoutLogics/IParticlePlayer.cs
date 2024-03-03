using System;

namespace GameLogic.Game.LayoutLogics
{
	public interface IParticlePlayer
	{
		void DestroyParticle();
		void AutoDestroy(float time);
		bool CanDestroy{ get;}
	}
}

