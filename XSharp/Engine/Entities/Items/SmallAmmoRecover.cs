using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public enum SmallAmmoRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class SmallAmmoRecover : Item
    {
        public SmallAmmoRecover(GameEngine engine, string name, Vector origin, int durationFrames = 0) : base(engine, name, origin, durationFrames, 1, false, "SmallAmmoRecover")
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
            player.ReloadAmmo(SMALL_AMMO_RECOVER_AMOUNT);
        }
    }
}