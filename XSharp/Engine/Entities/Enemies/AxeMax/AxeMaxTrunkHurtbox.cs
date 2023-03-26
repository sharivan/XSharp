using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public class AxeMaxTrunkHurtbox : Enemy
{
    #region StaticFields
    public static readonly FixedSingle HP = 3;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;

    public static readonly Box HITBOX = ((0, 0), (-13, -6), (13, 6));
    public static readonly Box COLLISION_BOX = ((0, 0), (-13, -2), (13, 2));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 0;
    #endregion

    private EntityReference<AxeMaxTrunk> trunk;

    private string lastPaletteName;
    private bool flashing;

    public AxeMaxTrunk Trunk
    {
        get => trunk;
        internal set
        {
            trunk = value;
            Parent = value;
        }
    }

    public AxeMaxTrunkHurtbox()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Invincible = true;
        SpawnFacedToPlayer = false;

        trunk = (AxeMaxTrunk) Parent;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return COLLISION_BOX_LEGS_HEIGHT;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        AutoAdjustOnTheFloor = false;

        Health = HP;
        ContactDamage = 0;

        flashing = false;
        lastPaletteName = null;

        NothingDropOdd = 100; // 100%
        SmallHealthDropOdd = 0; // 0%
        BigHealthDropOdd = 0; // 0%
        SmallAmmoDropOdd = 0; // 0%
        BigAmmoDropOdd = 0; // 0%
        LifeUpDropOdd = 0; // 0%
    }

    protected override bool OnPreThink()
    {
        if (flashing && lastPaletteName != null)
        {
            flashing = false;
            Trunk.PaletteName = lastPaletteName;
            lastPaletteName = null;
        }

        return base.OnPreThink();
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        if (Trunk.PaletteName != null)
        {
            lastPaletteName = Trunk.PaletteName;
            flashing = true;
            Trunk.PaletteName = "flashingPalette";
        }

        return base.OnTakeDamage(attacker, ref damage);
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        if (Trunk.Thrown)
            Trunk.TrunkBase.AxeMax.MakeLumberjackLaugh();

        Break();
    }

    protected override void OnDeath()
    {
        if (Trunk != null)
        {
            if (lastPaletteName != null)
            {
                Trunk.PaletteName = lastPaletteName;
                lastPaletteName = null;
            }

            Trunk.Kill();
        }

        base.OnDeath();
    }
}