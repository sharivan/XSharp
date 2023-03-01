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
        public BigAmmoRecover()
        {
            SpriteSheetName = "X Weapons";

            SetAnimationNames("BigAmmoRecover");
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