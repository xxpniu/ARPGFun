using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;

namespace GameLogic.Game.Controllors
{
    public class BattleItemControllor : GControllor
    {
        public BattleItemControllor(GPerception p) : base(p) { }

        public override GAction GetAction(GTime time, GObject current)
        {
            if (current is not BattleItem item) return GAction.Empty;
            item.AliveTime += time.DeltaTime;
            item.LockTime -= time.DeltaTime;
            if (item.LockTime < 0)
            {
                if (item.GroupIndex > 0)
                    item.ChangeIndex(-1);
            }
            if (item.AliveTime > 60)
            {
                GObject.Destroy(current);
            }
            return GAction.Empty;
        }
    }
}
