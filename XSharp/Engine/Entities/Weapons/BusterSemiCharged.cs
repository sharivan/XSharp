using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Weapons;

public enum SemiChargedState
{
    FIRING = 0,
    SHOOTING = 1,
    HITTING = 2,
    EXPLODING = 3
}

public class BusterSemiCharged : Weapon, IFSMEntity<SemiChargedState>
{
    private EntityReference<Entity> hitEntity;

    new public Player Shooter
    {
        get => (Player) base.Shooter;
        internal set => base.Shooter = value;
    }

    public Entity HitEntity => hitEntity;

    public SemiChargedState State
    {
        get => GetState<SemiChargedState>();
        set => SetState(value);
    }

    public bool Firing => GetState<SemiChargedState>() == SemiChargedState.FIRING;

    public bool Exploding => GetState<SemiChargedState>() == SemiChargedState.EXPLODING;

    public bool Hitting => GetState<SemiChargedState>() == SemiChargedState.HITTING;

    public BusterSemiCharged()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X Weapons";

        SetupStateArray<SemiChargedState>();
        RegisterState(SemiChargedState.FIRING, OnStartFiring, "SemiChargedShotFiring");
        RegisterState(SemiChargedState.SHOOTING, OnStartShooting, OnShooting, null, "SemiChargedShot");
        RegisterState(SemiChargedState.HITTING, OnStartHitting, "SemiChargedShotHit");
        RegisterState(SemiChargedState.EXPLODING, OnStartExploding, "SemiChargedShotExplode");
    }

    public override FixedSingle GetGravity()
    {
        return FixedSingle.ZERO;
    }

    protected override FixedSingle GetBaseDamage()
    {
        return SEMI_CHARGED_DAMAGE;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Direction = Shooter.WallSliding ? Shooter.Direction.Oposite() : Shooter.Direction;
        CheckCollisionWithWorld = false;
        Velocity = Vector.NULL_VECTOR;

        SetState(SemiChargedState.FIRING);
    }

    private void OnStartFiring(EntityState state, EntityState lastState)
    {
        if (Engine.Boss != null && !Engine.Boss.Exploding)
            Engine.PlaySound(1, "X Semi Charged Shot");
    }

    private void OnStartShooting(EntityState state, EntityState lastState)
    {
        if (Direction == Direction.LEFT)
        {
            Origin += 14 * Vector.LEFT_VECTOR;
            Velocity = SEMI_CHARGED_INITIAL_SPEED * Vector.LEFT_VECTOR;
        }
        else
        {
            Origin += 14 * Vector.RIGHT_VECTOR;
            Velocity = SEMI_CHARGED_INITIAL_SPEED * Vector.RIGHT_VECTOR;
        }
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
        Velocity += new Vector(Velocity.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
        if (Velocity.X.Abs > LEMON_TERMINAL_SPEED)
            Velocity = new Vector(Velocity.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, Velocity.Y);
    }

    private void OnStartHitting(EntityState state, EntityState lastState)
    {
        if (HitEntity != null)
        {
            Box otherHitbox = HitEntity.Hitbox;
            Vector center = Hitbox.Center;
            FixedSingle x = Direction == Direction.RIGHT ? otherHitbox.Left : otherHitbox.Right;
            FixedSingle y = center.Y < otherHitbox.Top ? otherHitbox.Top : center.Y > otherHitbox.Bottom ? otherHitbox.Bottom : Origin.Y;
            Origin = (x, y);
        }

        Velocity = Vector.NULL_VECTOR;
    }

    private void OnStartExploding(EntityState state, EntityState lastState)
    {
        Velocity = Vector.NULL_VECTOR;
    }

    public override void Reflect()
    {
        Engine.PlaySound(1, "Enemy Helmet Hit");
        SetState(SemiChargedState.EXPLODING);
    }

    protected override void OnHit(Enemy enemy, FixedSingle damage)
    {
        if (!enemy.Broke && enemy.Health > damage)
        {
            Damage = 0;
            hitEntity = enemy;
            SetState(SemiChargedState.HITTING);

            base.OnHit(enemy, damage);
        }
    }

    protected override void OnDeath()
    {
        Shooter.shots--;
        Shooter.shootingCharged = false;

        base.OnDeath();
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        if (animation.Name != CurrentState.AnimationName)
            return;

        if (Firing)
            SetState(SemiChargedState.SHOOTING);
        else if (Hitting || Exploding)
            KillOnNextFrame();
    }
}
