using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.SnowShooter;

public class SnowShooterShot : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<SnowShooter>();
    }
    #endregion

    public SnowShooterShot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = false;

        PaletteName = "snowShooterPalette";
        SpriteSheetName = "SnowShooter";

        SetAnimationNames("Shot");
        InitialAnimationName = "Shot";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return SnowShooter.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = SnowShooter.SHOT_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;
    }
}