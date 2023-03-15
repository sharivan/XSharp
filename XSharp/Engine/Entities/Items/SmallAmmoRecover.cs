using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public class SmallAmmoRecover : Item
{
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }

    public SmallAmmoRecover()
    {
        SpriteSheetName = "X Weapons";

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