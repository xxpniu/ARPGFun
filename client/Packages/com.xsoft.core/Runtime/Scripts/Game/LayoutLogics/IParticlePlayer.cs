namespace GameLogic.Game.LayoutLogics
{
    public interface IParticlePlayer
    {
        bool CanDestroy { get; }
        void DestroyParticle();
        void AutoDestroy(float time);
    }
}