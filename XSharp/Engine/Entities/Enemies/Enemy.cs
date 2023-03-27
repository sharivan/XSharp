using SharpDX;

using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies;

public enum HitResponse
{
    ACCEPT = 0,
    IGNORE = 1,
    REFLECT = 2
}

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
        Engine.PrecacheSound("Small Hit", @"X1\30 - MMX - Small Hit.wav");
        Engine.PrecacheSound("Big Hit", @"X1\31 - MMX - Big Hit.wav");
        Engine.PrecacheSound("Enemy Die (1)", @"X1\56 - MMX - Enemy Die (1).wav");
        Engine.PrecacheSound("Enemy Die (2)", @"X1\57 - MMX - Enemy Die (2).wav");
        Engine.PrecacheSound("Enemy Die (3)", @"X1\58 - MMX - Enemy Die (3).wav");
        Engine.PrecacheSound("Enemy Die (4)", @"X1\59 - MMX - Enemy Die (4).wav");
        Engine.PrecacheSound("Armadillo Laser", @"X1\40 - MMX - Armadillo Laser.wav");

        Engine.PrecachePalette("flashingPalette", FLASHING_PALETTE);
    }

    private string lastPaletteName;
    private bool flashing;

    public bool SpawnFacedToPlayer
    {
        get;
        set;
    } = true;

    public bool AlwaysFaceToPlayer
    {
        get;
        set;
    } = false;

    public HitResponse HitResponse
    {
        get;
        set;
    } = HitResponse.ACCEPT;

    public bool AcceptShots => HitResponse == HitResponse.ACCEPT;

    public bool IgnoreShots => HitResponse == HitResponse.IGNORE;

    public bool ReflectShots => HitResponse == HitResponse.REFLECT;

    public FixedSingle ContactDamage
    {
        get;
        set;
    } = 1;

    public ulong SmallHealthDropOdd
    {
        get;
        set;
    } = 0;

    public ulong BigHealthDropOdd
    {
        get;
        set;
    } = 0;

    public ulong SmallAmmoDropOdd
    {
        get;
        set;
    } = 0;

    public ulong BigAmmoDropOdd
    {
        get;
        set;
    } = 0;

    public ulong LifeUpDropOdd
    {
        get;
        set;
    } = 0;

    public ulong NothingDropOdd
    {
        get;
        set;
    } = 100;

    public ulong TotalDropOdd => SmallHealthDropOdd + BigHealthDropOdd + SmallAmmoDropOdd + BigAmmoDropOdd + LifeUpDropOdd + NothingDropOdd;

    protected Enemy()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

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

        if (SpawnFacedToPlayer && Engine.Player != null)
            Direction = GetHorizontalDirection(Engine.Player);
    }

    protected override bool OnPreThink()
    {
        if (flashing && lastPaletteName != null)
        {
            flashing = false;
            PaletteName = lastPaletteName;
            lastPaletteName = null;
        }

        return base.OnPreThink();
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
        if (IgnoreShots)
            return false;

        if (ReflectShots)
        {
            if (attacker is Weapon weapon)
                weapon.Reflect();

            damage = 0;
            return false;
        }

        if (AcceptShots && !flashing && PaletteName != null)
        {
            lastPaletteName = PaletteName;
            flashing = true;
            PaletteName = "flashingPalette";
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

        var random = Engine.RNG.NextLong((ulong) TotalDropOdd);
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

    protected override void OnThink()
    {
        base.OnThink();

        if (AlwaysFaceToPlayer)
            FaceToPlayer();
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