using XSharp.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items
{
    public enum BigAmmoRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class BigAmmoRecover : Item
    {
        public BigAmmoRecover(string name, Vector origin, int durationFrames = 0)
            : base(name, origin, durationFrames, 1, false, "BigAmmoRecover")
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