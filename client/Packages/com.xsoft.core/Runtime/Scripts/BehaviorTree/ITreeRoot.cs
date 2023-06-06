using EngineCore.Simulater;
using System;
namespace BehaviorTree
{
	public interface ITreeRoot
	{
		GTime Time { get; }
        object UserState { get; }
		void Chanage(Composite cur);
		bool IsDebug { get; }
	}

}

