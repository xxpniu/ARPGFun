using System;
using GameLogic.Utility;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IBattleItem : IBattleElement
    {
        int TeamIndex { get; }
        int GroupIndex { get; }
        [NeedNotify(typeof(Notify_BattleItemChangeGroupIndex),"GroupIndex")]
        void ChangeGroupIndex(int groupIndex);
    }

}
