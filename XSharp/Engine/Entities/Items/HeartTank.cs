using XSharp.Geometry;

namespace XSharp.Engine.Entities.Items
{
    public class HeartTank : Item
    {
        public HeartTank()
        {
            DurationFrames = 0;
            SpriteSheetIndex = 1;

            SetAnimationNames("HeartTank");
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