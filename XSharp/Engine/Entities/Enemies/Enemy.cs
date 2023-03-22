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
        Engine.PrecacheSound("Small Hit", @"resources\sounds\mmx\30 - MMX - Small Hit.wav");
        Engine.PrecacheSound("Big Hit", @"resources\sounds\mmx\31 - MMX - Big Hit.wav");
        Engine.PrecacheSound("Enemy Die (1)", @"resources\sounds\mmx\56 - MMX - Enemy Die (1).wav");
        Engine.PrecacheSound("Enemy Die (2)", @"resources\sounds\mmx\57 - MMX - Enemy Die (2).wav");
        Engine.PrecacheSound("Enemy Die (3)", @"resources\sounds\mmx\58 - MMX - Enemy Die (3).wav");
        Engine.PrecacheSound("Enemy Die (4)", @"resources\sounds\mmx\59 - MMX - Enemy Die (4).wav");
        Engine.PrecacheSound("Armadillo Laser", @"resources\sounds\mmx\40 - MMX - Armadillo Laser.wav");

        Engine.PrecachePalette("flashingPalette", FLASHING_PALETTE);
    }

    private string lastPaletteName;
    private bool flashing;

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
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        CanGoOutOfMapBounds = true;
        KillOnOffscreen = true;
        Health = 1;
        ContactDamage = 1;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        lastPaletteName = null;
        flashing = false;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%
    }

    protected override bool PreThink()
    {
        if (flashing && lastPaletteName != null)
        {
            flashing = false;
            PaletteName = lastPaletteName;
            lastPaletteName = null;
        }

        return base.PreThink();
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
        if (!flashing && PaletteName != null)
        {
            lastPaletteName = PaletteName;
            flashing = true;
            PaletteName = "flashingPalette";
        }

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
                weapon.NotifyHit(this, damage);

            damage = 0;
            return false;
        }

        if (base.OnTakeDamage(attacker, ref damage))
        {
            if (attacker is Weapon weapon)
                weapon.NotifyHit(this, damage);

            return true;
        }

        return false;
    }

    protected override void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
    {
        OnDamaged(attacker, damage);
        base.OnTakeDamagePost(attacker, damage);
    }

    protected virtual void OnExplode()
    {
        Engine.CreateExplosionEffect(Origin);
    }

    protected override void OnBroke()
    {
        base.OnBroke();

        OnExplode();

        var random = Engine.RNG.NextLong(TotalDropOdd);
        if (random > TotalDropOdd - NothingDropOdd)
            return;

        if (random < LifeUpDropOdd)
        {
            Engine.DropLifeUp(Origin, ITEM_DURATION_FRAMES);
            return;
        }

        random -= LifeUpDropOdd;
        if (random < BigHealthDropOdd)
        {
            Engine.DropBigHealthRecover(Origin, ITEM_DURATION_FRAMES);
            return;
        }

        random -= BigHealthDropOdd;
        if (random < BigAmmoDropOdd)
        {
            Engine.DropBigAmmoRecover(Origin, ITEM_DURATION_FRAMES);
            return;
        }

        random -= BigAmmoDropOdd;
        if (random < SmallHealthDropOdd)
        {
            Engine.DropSmallHealthRecover(Origin, ITEM_DURATION_FRAMES);
            return;
        }

        random -= SmallHealthDropOdd;
        if (random < SmallAmmoDropOdd)
            Engine.DropSmallAmmoRecover(Origin, ITEM_DURATION_FRAMES);
    }

    protected override void OnDeath()
    {
        if (flashing && lastPaletteName != null)
        {
            flashing = false;
            PaletteName = lastPaletteName;
            lastPaletteName = null;
        }

        base.OnDeath();
    }
}