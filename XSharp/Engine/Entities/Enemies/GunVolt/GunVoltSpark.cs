using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

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

    private bool wasLanded;
    private FixedSingle hSpeed;

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

    public override FixedSingle GetGravity()
    {
        return wasLanded ? 0 : base.GetGravity();
    }

    protected override Box GetHitbox()
    {
        return GunVolt.SPARK_HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return GunVolt.SPARK_COLLISION_BOX;
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        if (!wasLanded)
        {
            wasLanded = true;
            CheckCollisionWithWorld = false;
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = true;
        ContactDamage = GunVolt.SPARK_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;

        wasLanded = false;
        hSpeed = GunVolt.SPARK_SPEED * Direction.GetHorizontalSignal();
        Velocity = (hSpeed, GunVolt.SPARK_SPEED);
    }

    protected override void OnThink()
    {
        base.OnThink();

        Velocity = (hSpeed, wasLanded ? 0 : Velocity.Y);
    }
}