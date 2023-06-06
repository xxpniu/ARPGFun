using Layout.AITree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Game.Elements
{
    public interface ICharacterWatcher
    {
        void OnFireEvent(BattleEventType eventType,object args);
    }
}
