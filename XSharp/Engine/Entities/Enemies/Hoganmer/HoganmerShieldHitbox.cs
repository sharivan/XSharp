using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Hoganmer;

public class HoganmerShieldHitbox : Enemy
{
    public HoganmerShieldHitbox()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Invincible = true;
        SpawnFacedToPlayer = false;
        HitResponse = HitResponse.REFLECT;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return Hoganmer.SHIELD_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        AutoAdjustOnTheFloor = false;
        ContactDamage = 0;
    }
}