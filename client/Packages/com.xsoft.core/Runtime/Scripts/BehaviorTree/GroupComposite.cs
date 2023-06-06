using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    public abstract class GroupComposite : Composite
	{
		protected GroupComposite(params Composite[] children)
        {
			Children = new List<Composite>(children);
        }

        public List<Composite> Children { get; private set; }

        public override void Start(ITreeRoot context)
		{
            base.Start(context);
        }

        public override void Stop(ITreeRoot context)
        {
            foreach (var i in Children)
            {
                if (i.LastStatus == RunStatus.Running)
                    i.Stop(context);
            }
            base.Stop(context);
        }

		public override Composite FindGuid(string id)
		{
			if (Guid == id) return this;
			foreach (var i in Children)
			{
				var t = i.FindGuid(id);
				if (t != null) return t;
			}
			return null;
		}
    }
}
