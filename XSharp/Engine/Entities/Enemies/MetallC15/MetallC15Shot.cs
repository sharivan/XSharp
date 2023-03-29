using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.MetallC15;

public class MetallC15Shot : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<MetallC15>();
    }
    #endregion

    internal EntityReference<MetallC15> shooter;

    public MetallC15 Shooter => shooter;

    public MetallC15Shot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.RIGHT;
        SpawnFacedToPlayer = false;

        PaletteName = "MetallC15Palette";
        SpriteSheetName = "MetallC15";

        SetAnimationNames("Shot");
        InitialAnimationName = "Shot";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return MetallC15.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = MetallC15.SHOT_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;
    }

    protected override void OnDeath()
    {
        Shooter?.NotifyShotDeath();

        base.OnDeath();
    }
}