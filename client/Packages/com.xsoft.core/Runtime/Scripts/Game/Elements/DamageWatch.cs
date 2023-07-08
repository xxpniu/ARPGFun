namespace GameLogic.Game.Elements
{
    public class DamageWatch
    {
        public int Index { set; get; }
        public int TotalDamage { set; get; }
        public float LastTime { set; get; }
        public float FirstTime { get; internal set; }
    }
}