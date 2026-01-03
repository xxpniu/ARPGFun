using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public abstract class ActionComposite<T> : Composite where T : TreeNode
    {
        public ActionComposite(T Node)
        {
            this.Node = Node;
        }

        public T Node { private set; get; }
    }
}