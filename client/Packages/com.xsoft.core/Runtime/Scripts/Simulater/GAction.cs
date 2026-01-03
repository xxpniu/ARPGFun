namespace EngineCore.Simulater
{
    public abstract class GAction
    {
        static GAction()
        {
            Empty = new EmptyAction();
        }

        public GAction(GPerception perception)
        {
            Perceptipn = perception;
        }

        public static GAction Empty { private set; get; }

        public GPerception Perceptipn { private set; get; }

        public abstract void Execute(GTime time, GObject current);

        private class EmptyAction : GAction
        {
            public EmptyAction() : base(null)
            {
            }

            public override void Execute(GTime time, GObject current)
            {
            }
        }
    }
}