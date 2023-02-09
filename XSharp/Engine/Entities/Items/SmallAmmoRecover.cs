using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items
{
    public enum SmallAmmoRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class SmallAmmoRecover : Item
    {
        public SmallAmmoRecover()
        {
            SpriteSheetIndex = 1;

            SetAnimationNames("SmallAmmoRecover");
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