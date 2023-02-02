using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public enum BigAmmoRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class BigAmmoRecover : Item
    {
        public BigAmmoRecover(GameEngine engine, string name, Vector origin, int durationFrames = 0) : base(engine, name, origin, durationFrames, 1, false, "BigAmmoRecover")
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
            player.ReloadAmmo(BIG_AMMO_RECOVER_AMOUNT);
        }
    }
}