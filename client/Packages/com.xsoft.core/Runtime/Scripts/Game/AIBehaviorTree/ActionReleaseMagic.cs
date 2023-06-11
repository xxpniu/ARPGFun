using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeReleaseMagic))]
	public class ActionReleaseMagic : ActionComposite<TreeNodeReleaseMagic>
	{

        public ActionReleaseMagic(TreeNodeReleaseMagic node) : base(node) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            
            bool atPos = Node.ReleaseATPos;
            IReleaserTarget releaserTarget;
            if (!atPos)
            {
                if (!root!.TryGetTarget(out var target))
                {
                    if (root.IsDebug) { Attach("failure", "not found target"); }

                    yield return RunStatus.Failure;
                    yield break;
                }

                releaserTarget = new ReleaseAtTarget(root.Character, target);
            }
            else
            {
                if (!root!.TryGetTargetPos(out var tar))
                {
                    if (root.IsDebug) { Attach("failure", "not found target pos"); }

                    yield return RunStatus.Failure;
                    yield break;
                }
                releaserTarget = new ReleaseAtPos(root.Character, tar);
            }

            string key = Node.magicKey;
            switch (Node.valueOf)
            {
                case MagicValueOf.BlackBoard:
                    {
                        if (!root.TryGetMagic(out var magicData)) 
                        {
                            yield return RunStatus.Failure;
                            yield break;
                        }
                        key = magicData.MagicKey;
                        root.Character.AttachMagicHistory(magicData.ID, root.Time);
                    }
                    break;
                case MagicValueOf.MagicKey:
                    {
                        key = Node.magicKey;
                    }
                    break;
            }

            if (!root.Perception.View.ExistMagicKey(key))
            {
                if (context.IsDebug)
                {
                    Attach("failure", $"not found key {key}");
                }
                yield return RunStatus.Failure;
                yield break;
            }

            _releaser = root.Perception
                .CreateReleaser(key, root.Character,releaserTarget , ReleaserType.Magic, Proto.ReleaserModeType.RmtMagic, -1);

            while (!_releaser.IsLayoutStartFinish)
            {
                yield return RunStatus.Running;
            }

            yield return RunStatus.Success;
            yield break;

        }

        private MagicReleaser _releaser;

        public override void Stop(ITreeRoot context)
        {
            if (LastStatus == RunStatus.Running)
            {
                _releaser?.Cancel();
            }
            base.Stop(context);
        }
	}
}

