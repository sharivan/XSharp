using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies;

public abstract class Enemy : Sprite
{
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