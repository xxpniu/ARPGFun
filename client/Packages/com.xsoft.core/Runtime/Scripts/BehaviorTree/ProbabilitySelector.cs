using System;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class ProbabilitySelection
    {
        public Composite Branch;

        public int ChanceToExecute;

        public ProbabilitySelection(Composite branch, int chanceToExecute)
        {
            Branch = branch;
            ChanceToExecute = chanceToExecute;
        }
    }

    /// <summary>
    ///     从列表中随机选择一个节点执行，并返回这个节点的执行结果
    /// </summary>
    public class ProbabilitySelector : Composite
    {
        static ProbabilitySelector()
        {
            Randomizer = new Random();
        }

        public ProbabilitySelector(params ProbabilitySelection[] children)
        {
            PossibleBranches = children;
        }

        private ProbabilitySelection[] PossibleBranches { get; }

        protected static Random Randomizer { get; }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            //实现随机分布
            var total = 0;
            foreach (var i in PossibleBranches) total += i.ChanceToExecute;
            var rand = Randomizer.Next(total);
            var current = 0;
            for (var i = 0; i < PossibleBranches.Length; i++)
            {
                var before = current;
                current += PossibleBranches[i].ChanceToExecute;
                if (rand >= before && rand < current)
                {
                    PossibleBranches[i].Branch.Start(context);
                    while (PossibleBranches[i].Branch.Tick(context) == RunStatus.Running)
                        yield return RunStatus.Running;
                    yield return PossibleBranches[i].Branch.LastStatus.Value;
                    yield break;
                }
            }

            yield return RunStatus.Failure;
        }

        public override void Stop(ITreeRoot context)
        {
            foreach (var i in PossibleBranches)
                if (i.Branch.LastStatus == RunStatus.Running)
                    i.Branch.Stop(context);
            base.Stop(context);
        }

        public override Composite FindGuid(string id)
        {
            if (Guid == id) return this;
            foreach (var i in PossibleBranches)
            {
                var t = i.Branch.FindGuid(id);
                if (t != null) return t;
            }

            return null;
        }
    }
}