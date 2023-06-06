using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using Layout;
using Layout.AITree;

namespace BattleViews
{
    public interface IAIRunner
    {
        AITreeRoot RunAI(TreeNode ai);
        bool IsRunning(Layout.EventType eventType);
        bool ReleaseMagic(MagicData data);
        void Attach(BattleCharacter character);
    }

    public abstract class AIRunner
    {
        public static IAIRunner Current { set; get; } 
    }
}