using XSharp.Geometry;

namespace XSharp.Engine.Entities.Items
{
    public class HeartTank : Item
    {
        public HeartTank(string name, Vector origin) : base(name, origin, -1, 1, false, "HeartTank")
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CurrentAnimationIndex = 0;
            CurrentAnimation.StartFromBegin();
        }

        protected override void OnCollecting(Player player)
        {
            Engine.StartHeartTankAcquiring();
        }
    }
}