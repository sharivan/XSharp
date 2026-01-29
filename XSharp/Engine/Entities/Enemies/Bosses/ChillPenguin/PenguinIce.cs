using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;
using static XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin.ChillPenguin;

namespace XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin;

public class PenguinIce : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<ChillPenguin>();
    }
    #endregion

    private FixedSingle speed;
    private bool bumped;
    private EntityReference<ChillPenguin> shooter;

    public ChillPenguin Shooter
    {
        get => shooter;
        internal set => shooter = Engine.Entities.GetReferenceTo(value);
    }

    public bool Bump
    {
        get;
        internal set;
    } = false;

    public bool Exploding
    {
        get;
        private set;
    }

    public PenguinIce()
    {
        SpriteSheetName = "ChillPenguin";
        ContactDamage = 2;

        SetAnimationNames("Ice");
    }

    protected override Box GetHitbox()
    {
        return PENGUIN_ICE_HITBOX;
    }

    public void Explode()
    {
        if (Exploding)
            return;

        Exploding = true;
        Engine.PlaySound(4, "Ice");

        PenguinIceExplosionEffect fragment = Engine.Entities.Create<PenguinIceExplosionEffect>(new
        {
            Origin,
            InitialVelocity = (-PENGUIN_ICE_FRAGMENT_SPEED, -PENGUIN_ICE_FRAGMENT_SPEED)
        });

        fragment.Spawn();

        fragment = Engine.Entities.Create<PenguinIceExplosionEffect>(new
        {
            Origin,
            InitialVelocity = (PENGUIN_ICE_FRAGMENT_SPEED, -PENGUIN_ICE_FRAGMENT_SPEED)
        });

        fragment.Spawn();

        fragment = Engine.Entities.Create<PenguinIceExplosionEffect>(new
        {
            Origin,
            InitialVelocity = (-PENGUIN_ICE_FRAGMENT_SPEED, -PENGUIN_ICE_FRAGMENT_SPEED * FixedSingle.HALF)
        });

        fragment.Spawn();

        fragment = Engine.Entities.Create<PenguinIceExplosionEffect>(new
        {
            Origin,
            InitialVelocity = (PENGUIN_ICE_FRAGMENT_SPEED, -PENGUIN_ICE_FRAGMENT_SPEED * FixedSingle.HALF)
        });

        fragment.Spawn();

        Kill();
    }

    public override FixedSingle GetGravity()
    {
        return Bump ? base.GetGravity() : 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Direction = Shooter.Direction;
        Origin = Shooter.Origin + (Shooter.Direction == Shooter.DefaultDirection ? -PENGUIN_SHOT_ORIGIN_OFFSET.X : PENGUIN_SHOT_ORIGIN_OFFSET.X, PENGUIN_SHOT_ORIGIN_OFFSET.Y);
        HitResponse = HitResponse.REFLECT;

        Exploding = false;
        bumped = false;
        speed = Bump ? PENGUIN_ICE_SPEED2_X : PENGUIN_ICE_SPEED;

        SetCurrentAnimationByName("Ice");
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        if (!bumped)
        {
            Velocity = (Velocity.X, -PENGUIN_ICE_SPEED2_X);
            bumped = true;
        }
        else
        {
            Velocity = Velocity.XVector;
        }
    }

    protected override void OnBlockedLeft()
    {
        base.OnBlockedLeft();

        Explode();
    }

    protected override void OnBlockedRight()
    {
        base.OnBlockedRight();

        Explode();
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        Explode();
    }

    protected override void OnThink()
    {
        base.OnThink();

        Velocity = (Direction == Direction.RIGHT ? speed : -speed, Velocity.Y);
    }

    protected override void OnBroke()
    {
        Explode();
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (entity is PenguinSculpture)
            Break();
    }
}