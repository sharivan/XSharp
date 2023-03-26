using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.GunVolt;

public class GunVoltSpark : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<GunVolt>();
    }
    #endregion

    public GunVoltSpark()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "GunVoltPalette";
        SpriteSheetName = "GunVolt";
        Layer = 1;

        SetAnimationNames("Spark");
        InitialAnimationName = "Spark";
    }

    protected override Box GetHitbox()
    {
        return GunVolt.SPARK_HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return GunVolt.SPARK_COLLISION_BOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = true;
        ContactDamage = GunVolt.SPARK_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;

        Velocity = (GunVolt.SPARK_SPEED * Direction.GetHorizontalSignal(), GunVolt.SPARK_SPEED);
    }
}