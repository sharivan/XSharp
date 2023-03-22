using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Entities.Enemies.Bosses.Penguin.Penguin;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public class PenguinSculpture : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }
    #endregion

    private int frameCounter;
    private bool gravity;
    private EntityReference<Penguin> shooter;

    public Penguin Shooter
    {
        get => shooter;
        internal set => shooter = Engine.Entities.GetReferenceTo(value);
    }

    public bool Exploding
    {
        get;
        private set;
    }

    public PenguinSculpture()
    {
        Directional = true;
        DefaultDirection = Direction.LEFT;
        SpriteSheetName = "Penguin";

        SetAnimationNames("Sculpture");
    }

    public override FixedSingle GetGravity()
    {
        return gravity ? base.GetGravity() : 0;
    }

    protected override Box GetHitbox()
    {
        return PENGUIN_SCULPTURE_HITBOX;
    }

    public void Explode()
    {
        if (Exploding)
            return;

        Exploding = true;
        Engine.PlaySound(4, "Ice Freeze");

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

    protected override void OnBlockedLeft()
    {
        base.OnBlockedLeft();
        Break();
    }

    protected override void OnBlockedRight()
    {
        base.OnBlockedRight();
        Break();
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        if (!gravity)
            return false;

        switch (attacker)
        {
            case BusterLemon lemon:
                damage = lemon.DashLemon ? 2 : 1;
                break;

            case BusterSemiCharged:
                damage = 2;
                break;

            case BusterCharged:
                damage = 3;
                break;
        }

        return base.OnTakeDamage(attacker, ref damage);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Direction = Shooter.Direction;
        Health = 8;
        Invincible = true;

        Exploding = false;
        frameCounter = 0;
        gravity = false;

        SetCurrentAnimationByName("Sculpture");
    }

    protected override void OnThink()
    {
        base.OnThink();

        frameCounter++;
        if (frameCounter == PENGUIN_SCULPTURE_FRAMES_TO_GRAVITY)
        {
            ContactDamage = 4;
            Invincible = false;
            gravity = true;
        }
    }

    protected override void OnBroke()
    {
        Explode();
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (entity is Penguin)
            Break();
    }
}