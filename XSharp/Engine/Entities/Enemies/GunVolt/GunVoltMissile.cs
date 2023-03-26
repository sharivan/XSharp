using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.GunVolt;

public class GunVoltMissile : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<GunVolt>();
    }
    #endregion

    public GunVoltMissile()
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

        SetAnimationNames("Missile");
        InitialAnimationName = "Missile";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return GunVolt.MISSILE_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = GunVolt.MISSILE_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;

        Velocity = (GunVolt.MISSILE_INITIAL_SPEED * Direction.GetHorizontalSignal(), 0);
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        Break();
    }

    protected override void OnBroke()
    {
        base.OnBroke();

        Engine.CreateExplosionEffect(Origin);
    }

    protected override void OnThink()
    {
        base.OnThink();

        Velocity += (GunVolt.MISSILE_ACCELERATION * Direction.GetHorizontalSignal(), 0);

        if (FrameCounter % GunVolt.MISSILE_SMOKE_SPAWN_INTERVAL == 0)
            CreateMissileSmoke();
    }

    private void CreateMissileSmoke()
    {
        GunVoltMissileSmoke smoke = Engine.Entities.Create<GunVoltMissileSmoke>(new
        {
            Origin
        });

        smoke.Spawn();
    }
}