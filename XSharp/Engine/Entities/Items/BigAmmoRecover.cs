using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public class BigAmmoRecover : Item
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction<Weapon>();
    }
    #endregion

    public BigAmmoRecover()
    {
        SpriteSheetName = "X Weapons";

        SetAnimationNames("BigAmmoRecover");
    }

    protected override void OnSpawn()
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