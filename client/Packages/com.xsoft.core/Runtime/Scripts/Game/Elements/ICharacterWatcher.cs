using Layout.AITree;

namespace GameLogic.Game.Elements
{
    public interface ICharacterWatcher
    {
        void OnFireEvent(BattleEventType eventType, object args);
    }
}