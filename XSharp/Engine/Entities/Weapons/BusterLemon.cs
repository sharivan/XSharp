using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Weapons;

public enum LemonState
{
    SHOOTING = 0,
    EXPLODING = 1
}

public class BusterLemon : Weapon, IStateEntity<LemonState>
{
    private bool reflected;

    new public Player Shooter
    {
        get => (Player) base.Shooter;
        internal set => base.Shooter = value;
    }

    public LemonState State
    {
        get => GetState<LemonState>();
        set => SetState(value);
    }

    public bool DashLemon
    {
        get;
        internal set;
    }

    public BusterLemon()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X Weapons";

        SetupStateArray(typeof(LemonState));
        RegisterState(LemonState.SHOOTING, OnStartShot, OnShooting, null, "LemonShot");
        RegisterState(LemonState.EXPLODING, null, OnExploding, OnExploded, "LemonShotExplode");
    }

    public override FixedSingle GetGravity()
    {
        return reflected && DashLemon && GetState<LemonState>() != LemonState.EXPLODING ? GRAVITY : FixedSingle.ZERO;
    }

    protected override Box GetHitbox()
    {
        return LEMON_HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return LEMON_HITBOX;
    }

    protected override FixedSingle GetBaseDamage()
    {
        return DashLemon ? 2 * LEMON_DAMAGE : LEMON_DAMAGE;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Direction = Shooter.WallSliding ? Shooter.Direction.Oposite() : Shooter.Direction;
        CheckCollisionWithWorld = false;
        Velocity = new Vector(Direction == Direction.LEFT ? (DashLemon ? -LEMON_TERMINAL_SPEED : -LEMON_INITIAL_SPEED) : (DashLemon ? LEMON_TERMINAL_SPEED : LEMON_INITIAL_SPEED), 0);

        SetState(LemonState.SHOOTING);
    }

    private void OnStartShot(EntityState state, EntityState lastState)
    {
        if (!Shooter.shootingCharged)
            Engine.PlaySound(1, "X Regular Shot");
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
        Velocity += new Vector(Velocity.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
        if (Velocity.X.Abs > LEMON_TERMINAL_SPEED)
            Velocity = new Vector(Velocity.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, Velocity.Y);
    }

    private void OnExploding(EntityState state, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;
    }

    private void OnExploded(EntityState state)
    {
        KillOnNextFrame();
    }

    protected override void OnHit(Enemy enemy, FixedSingle damage)
    {
        base.OnHit(enemy, damage);

        Explode(enemy);
    }

    public void Explode(Entity entity)
    {
        if (GetState<LemonState>() != LemonState.EXPLODING)
        {
            Damage = 0;

            if (entity != null)
            {
                Box otherHitbox = entity.Hitbox;
                Vector center = Hitbox.Center;
                FixedSingle x = Direction == Direction.RIGHT ? otherHitbox.Left : otherHitbox.Right;
                FixedSingle y = center.Y < otherHitbox.Top ? otherHitbox.Top : center.Y > otherHitbox.Bottom ? otherHitbox.Bottom : Origin.Y;
                Origin = (x, y);
            }

            Velocity = Vector.NULL_VECTOR;
            SetState(LemonState.EXPLODING);
        }
    }

    public override void Reflect()
    {
        if (!reflected && GetState<LemonState>() != LemonState.EXPLODING)
        {
            Damage = 0;
            reflected = true;
            Velocity = new Vector(-Velocity.X, LEMON_REFLECTION_VSPEED);
            Engine.PlaySound(1, "Enemy Helmet Hit");
        }
    }

    protected override void OnDeath()
    {
        Shooter.shots--;
        base.OnDeath();
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        if (animation.Name != CurrentState.AnimationName)
            return;

        if (GetState<LemonState>() == LemonState.EXPLODING)
            KillOnNextFrame();
    }
}