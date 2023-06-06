using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using Layout.AITree;
using Proto;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeSelectCanReleaseMagic))]
    public class ActionSelectCanReleaseMagic : ActionComposite<TreeNodeSelectCanReleaseMagic>
    {
        public ActionSelectCanReleaseMagic(TreeNodeSelectCanReleaseMagic node) : base(node) { }

        private readonly HashSet<int> releaseHistorys = new HashSet<int>();

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            var key = string.Empty;
            var list = new List<BattleCharacterMagic>();
            root.Character.EachActiveMagicByType(Node.MTpye, root.Time,
            (item) =>
            {
                list.Add(item);
                return false;
            });


            if (list.Count == 0)
            {
                if (root.IsDebug) Attach("failure", $"nofound {list.Count}");
                yield return RunStatus.Failure;
                yield break;
            }
            BattleCharacterMagic result = null;
            switch (Node.resultType)
            {
                case MagicResultType.Random:
                    result = GRandomer.RandomList(list);
                    break;
                case MagicResultType.Frist:
                    result = list[0];
                    break;
                case MagicResultType.Sequence:
                    foreach (var i in list)
                    {
                        if (releaseHistorys.Contains(i.ConfigId)) continue;
                        result = i;
                    }
                    if (result == null)
                    {
                        releaseHistorys.Clear();
                        result = list[0];
                    }
                    releaseHistorys.Add(result.ConfigId);
                    break;
            }
            if (result == null)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            root[AITreeRoot.SELECT_MAGIC_ID] = result.ConfigId;
            var config = result.Config;
            if (config != null) key = config.MagicKey;
            if (context.IsDebug) Attach("select result", $"{result.ConfigId}-{key}");
            yield return RunStatus.Success;
            yield break;
        }
    }
}

