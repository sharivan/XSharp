using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.RayBit;

public class RayBitShot : Enemy
{
    public static readonly FixedSingle CONTACT_DAMAGE = 2;

    public static readonly Box HITBOX = ((0, 0), (-7, -7), (7, 7));

    public static readonly Vector SPEED = (576 / 256.0, 0);

    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(RayBit));
    }

    public EntityReference<RayBit> shooter;

    public RayBit Shooter
    {
        get => shooter;
        set => shooter = value;
    }

    public RayBitShot()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "RayBit";
        Directional = true;

        SetAnimationNames("Shot");
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = 0;

        SetCurrentAnimationByName("Shot");
    }

    protected override void Think()
    {
        base.Think();

        if (FrameCounter < 12)
            Velocity = Vector.NULL_VECTOR;
        else if (FrameCounter == 12)
        {
            ContactDamage = CONTACT_DAMAGE;
            Velocity = Direction == Direction.LEFT ? -SPEED : SPEED;
            Engine.PlaySound(4, "Armadillo Laser");
        }
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        Kill();
    }
}