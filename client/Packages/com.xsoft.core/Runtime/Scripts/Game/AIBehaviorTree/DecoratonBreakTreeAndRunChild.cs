using System;
using System.Collections.Generic;
using BehaviorTree;

namespace GameLogic.Game.AIBehaviorTree
{
	public class DecoratonBreakTreeAndRunChild: Decorator
    {
		public DecoratonBreakTreeAndRunChild(Composite comp):base(comp)
		{

		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{		
			context.Chanage(this.DecoratedChild);
			yield return RunStatus.Success;
		}
	}
}

