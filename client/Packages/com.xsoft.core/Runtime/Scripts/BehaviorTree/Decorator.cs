namespace BehaviorTree
{
    public abstract class Decorator : GroupComposite
    {
        public Decorator(Composite child)
            : base(child)
        {
        }

        public Composite DecoratedChild => Children[0];
    }
}