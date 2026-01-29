using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.RayBit;

public class RayBitShot : Enemy
{
    #region StaticFields
    public static readonly FixedSingle CONTACT_DAMAGE = 2;

    public static readonly Box HITBOX = ((0, 0), (-7, -7), (7, 7));

    public static readonly Vector SPEED = (576 / 256.0, 0);
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<RayBit>();
    }
    #endregion

    public EntityReference<RayBit> shooter;

    public RayBit Shooter
    {
        get => shooter;
        set => shooter = value;
    }

    public RayBitShot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = false;

        SpriteSheetName = "RayBit";

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

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = 0;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;

        SetCurrentAnimationByName("Shot");
    }

    protected override void OnThink()
    {
        base.OnThink();

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