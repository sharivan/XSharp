using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.MetallC15;

public enum MetallC15State
{
    IDLE,
    CHASING,
    SHOOTING
}

public class MetallC15 : Enemy, IStateEntity<MetallC15State>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF004000), // 1
        Color.FromBgra(0xFF006020), // 2
        Color.FromBgra(0xFF208040), // 3
        Color.FromBgra(0xFF702008), // 4
        Color.FromBgra(0xFFB85828), // 5           
        Color.FromBgra(0xFFE8A840), // 6
        Color.FromBgra(0xFFA81030), // 7
        Color.FromBgra(0xFFD02040), // 8
        Color.FromBgra(0xFFF06898), // 9
        Color.FromBgra(0xFF685858), // A
        Color.FromBgra(0xFFA09090), // B
        Color.FromBgra(0xFFD0D0D0), // C
        Color.FromBgra(0xFF387038), // D
        Color.FromBgra(0xFF281010), // E
        Color.FromBgra(0xFF382020)  // F
    };

    public const int HEALTH = 2;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((0, -1), (-7, -6), (7, 6));

    public static readonly FixedSingle ATTACK_DISTANCE = 80;

    public const int FRAMES_TO_SHOW = 28;
    public const int FRAMES_TO_SHOOT_AGAIN = 116;
    public const int SHOOTING_FRAMES = 113;
    public const int FRAMES_TO_CHASE = 4;

    public static readonly FixedSingle SHOT_SPEED = 2;
    public static readonly FixedSingle SHOT_OFFSET_X = 3;
    public static readonly FixedSingle SHOT_OFFSET_Y = 1;
    public static readonly Box SHOT_HITBOX = ((0, 0), (-3, -3), (3, 3));
    public static readonly FixedSingle SHOT_DAMAGE = 1;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("MetallC15Palette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("MetallC15", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Metall C-15.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(3, 0, 0, 0, 21, 12, 1, true);

        sequence = spriteSheet.AddFrameSquence("Chasing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, 0, 323, 1, 70, 48, 24, true);

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 15, 323, 1, 70, 48, 24);

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(1, 1, 120, 0, 8, 8, 1, true);
        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private bool shotAlive;

    public MetallC15State State
    {
        get => GetState<MetallC15State>();
        set => SetState(value);
    }

    public MetallC15()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.RIGHT;
        SpawnFacedToPlayer = true;
        AlwaysFaceToPlayer = false;

        PaletteName = "MetallC15Palette";
        SpriteSheetName = "MetallC15";

        SetAnimationNames("Idle", "Chasing", "Shooting");

        SetupStateArray<MetallC15State>();
        RegisterState(MetallC15State.IDLE, OnStartIdle, OnIdle, null, "Idle");
        RegisterState(MetallC15State.CHASING, OnStartChasing, OnChasing, null, "Idle");
        RegisterState(MetallC15State.SHOOTING, OnStartShooting, OnShooting, null, "Shooting");
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.REFLECT;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
    }

    private void OnStartChasing(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.ACCEPT;
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
    }

    private void OnStartShooting(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.ACCEPT;
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
    }

    private EntityReference<MetallC15Shot> Shoot()
    {
        var player = Engine.Player;
        if (player == null)
            return null;

        var signal = Direction.GetHorizontalSignal() * DefaultDirection.GetHorizontalSignal();
        var shotOrigin = Origin + (SHOT_OFFSET_X * signal, SHOT_OFFSET_Y);
        var delta = player.Origin - shotOrigin;
        var velocity = (SHOT_SPEED * delta.Versor()).TruncFracPart();

        MetallC15Shot shot = Engine.Entities.Create<MetallC15Shot>(new
        {
            Origin = shotOrigin,
            Velocity = velocity,
            Direction
        });

        shotAlive = true;
        shot.shooter = this;
        shot.Spawn();
        return shot;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Health = HEALTH;
        ContactDamage = CONTACT_DAMAGE;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        shotAlive = false;

        State = MetallC15State.IDLE;
    }

    internal void NotifyShotDeath()
    {
        shotAlive = false;
    }
}