using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Flammingle;

public class FlammingleShot : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Flammingle>();
    }
    #endregion

    public FlammingleShot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = false;

        PaletteName = "flamminglePalette";
        SpriteSheetName = "Flammingle";

        SetAnimationNames("Shot");
        InitialAnimationName = "Shot";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return Flammingle.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = Flammingle.SHOT_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        Kill();
    }
}