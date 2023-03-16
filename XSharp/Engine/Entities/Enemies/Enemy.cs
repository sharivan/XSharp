using SharpDX;

using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies;

public abstract class Enemy : Sprite
{
    public static readonly Color[] FLASHING_PALETTE = new Color[]
    {
        Color.Transparent, // 0
        new Color(248, 248, 248, 255), // 1
        new Color(240, 248, 248, 255), // 2
        new Color(232, 248, 248, 255), // 3
        new Color(224, 248, 248, 255), // 4
        new Color(216, 248, 248, 255), // 5
        new Color(208, 248, 248, 255), // 6
        new Color(200, 248, 248, 255), // 7
        new Color(192, 248, 248, 255), // 8
        new Color(184, 248, 248, 255), // 9
        new Color(176, 248, 248, 255), // 10
        new Color(168, 248, 248, 255), // 11
        new Color(160, 248, 248, 255), // 12
        new Color(152, 248, 248, 255), // 13
        new Color(144, 248, 248, 255), // 14
        new Color(136, 248, 248, 255) // 15
    };

    [Precache]
    internal static void Precache()
    {
        Engine.CreatePalette("flashingPalette", FLASHING_PALETTE);
    }

    public bool ReflectShots
    {
        get;
        protected set;
    } = false;

    public FixedSingle ContactDamage
    {
        get;
        set;
    }

    public long SmallHealthDropOdd
    {
        get;
        set;
    }

    public long BigHealthDropOdd
    {
        get;
        set;
    }

    public long SmallAmmoDropOdd
    {
        get;
        set;
    }

    public long BigAmmoDropOdd
    {
        get;
        set;
    }

    public long LifeUpDropOdd
    {
        get;
        set;
    }

    public long NothingDropOdd
    {
        get;
        set;
    }

    public long TotalDropOdd => SmallHealthDropOdd + BigHealthDropOdd + SmallAmmoDropOdd + BigAmmoDropOdd + LifeUpDropOdd + NothingDropOdd;

    protected Enemy()
    {
        Directional = true;
        CanGoOutOfMapBounds = true;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = true;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%
    }

    protected virtual void OnContactDamage(Player player)
    {
        Hurt(player, ContactDamage);
    }

    protected override void OnTouching(Entity entity)
    {
        if (ContactDamage > 0 && entity is Player player)
            OnContactDamage(player);

        base.OnTouching(entity);
    }

    protected virtual void OnDamaged(Sprite attacker, FixedSingle damage)
    {
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        if (ReflectShots)
        {
            if (attacker is Weapon weapon)
                weapon.Reflect();

            damage = 0;
            return false;
        }

        if (Invincible)
        {
            if (attacker is Weapon weapon)
                weapon.OnHit(this, damage);

            damage = 0;
            return false;
        }

        if (base.OnTakeDamage(attacker, ref damage))
        {
            if (attacker is Weapon weapon)
                weapon.OnHit(this, damage);

            return true;
        }

        return false;
    }

    protected override void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
    {
        OnDamaged(attacker, damage);
        base.OnTakeDamagePost(attacker, damage);
    }

    protected override void OnBroke()
    {
        Engine.CreateExplosionEffect(Hitbox.Center);

        var random = Engine.RNG.NextLong(TotalDropOdd);
        if (random > TotalDropOdd - NothingDropOdd)
            return;

        if (random < LifeUpDropOdd)
        {
            Engine.DropLifeUp(Hitbox.Center, ITEM_DURATION_FRAMES);
            return;
        }

        random -= LifeUpDropOdd;
        if (random < BigHealthDropOdd)
        {
            Engine.DropBigHealthRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
            return;
        }

        random -= BigHealthDropOdd;
        if (random < BigAmmoDropOdd)
        {
            Engine.DropBigAmmoRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
            return;
        }

        random -= BigAmmoDropOdd;
        if (random < SmallHealthDropOdd)
        {
            Engine.DropSmallHealthRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
            return;
        }

        random -= SmallHealthDropOdd;
        if (random < SmallAmmoDropOdd)
            Engine.DropSmallAmmoRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
    }
}