using System;
using EngineCore.Simulater;

namespace GameLogic.Game.Elements
{
   
    public class BattleItem:BattleElement<IBattleItem>
    {
        public BattleItem(GControllor controllor,IBattleItem view, Proto.PlayerItem item):base(controllor, view)
        {
            DropItem = item;
            LockTime = 15;//s
        }

        public Proto.PlayerItem DropItem { private set; get; }

        public float AliveTime { set; get; }

        public float LockTime { set; get; }

        public bool CanBecollect(BattleCharacter heroCharacter)
        {
            if (GroupIndex < 0 || View.GroupIndex == heroCharacter.Index)
                return true;
            return false;
        }

        internal void ChangeIndex(int groupIndex)
        {
            View.ChangeGroupIndex(groupIndex);
        }

        public int GroupIndex { get { return View.GroupIndex; } }
    }
}
