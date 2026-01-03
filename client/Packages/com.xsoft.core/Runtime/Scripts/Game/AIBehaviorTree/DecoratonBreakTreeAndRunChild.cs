using System.Collections.Generic;
using BehaviorTree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratonBreakTreeAndRunChild : Decorator
    {
        public DecoratonBreakTreeAndRunChild(Composite comp) : base(comp)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            context.Change(DecoratedChild);
            yield return RunStatus.Success;
        }
    }
}