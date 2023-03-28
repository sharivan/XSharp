using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Hoganmer;

public enum HoganmerState
{
    IDLE,
    ATTACKING,
    POST_ATTACKING
}

public class Hoganmer : Enemy, IStateEntity<HoganmerState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF006000), // 1
        Color.FromBgra(0xFF009020), // 2
        Color.FromBgra(0xFF30C060), // 3
        Color.FromBgra(0xFF30E080), // 4
        Color.FromBgra(0xFFA02000), // 5           
        Color.FromBgra(0xFFE00820), // 6
        Color.FromBgra(0xFFE05090), // 7
        Color.FromBgra(0xFFE06800), // 8
        Color.FromBgra(0xFFF09010), // 9
        Color.FromBgra(0xFFF0C060), // A
        Color.FromBgra(0xFF404040), // B
        Color.FromBgra(0xFF707070), // C
        Color.FromBgra(0xFFA0A0A0), // D
        Color.FromBgra(0xFFE0E0E0), // E
        Color.FromBgra(0xFF202020)  // F
    };

    public const int HEALTH = 16;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((1, 2), (-7, -14), (7, 14));
    public static readonly Box COLLISION_BOX = ((0, 1), (-7, -15), (7, 15));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 6;

    public static readonly FixedSingle ATTACK_DISTANCE = 85;

    public const int MINIMUM_IDLE_FRAMES_BEFORE_ATTACK = 60;
    public const int POST_ATTACKING_FRAMES = 13;
    public const int FRAME_TO_THROW_SPIKE_BALL = 18;
    public const int FRAME_TO_DISABLE_SHIELD = 19;

    public static readonly Box SHIELD_HITBOX = ((-15, 0), (-6, -16), (6, 16));

    public static readonly FixedSingle SPIKE_BALL_INITIAL_SPEED = 1024 / 256.0;
    public static readonly FixedSingle SPIKE_BALL_DESACELERATION = 16 / 256.0;
    public static readonly FixedSingle SPIKE_BALL_MAX_DISTANCE = 122;
    public static readonly Box SPIKE_BALL_HITBOX = ((0, 0), (-8, -8), (8, 8));
    public const int SPIKE_BALL_DAMAGE = 2;
    public static readonly FixedSingle SPIKE_BALL_SPAWN_OFFSET_X = 10;
    public static readonly FixedSingle SPIKE_BALL_SPAWN_OFFSET_Y = 0;
    public const int SPIKE_BALL_STOP_FRAMES_BEFORE_BACK = 16;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("HoganmerPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Hoganmer", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Hoganmer.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(10, 5, 0, 1, 36, 33, 1, true);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(23, 13, 36, 0, 49, 41, 1);
        sequence.AddFrame(23, 13, 85, 0, 46, 41, 7);
        sequence.AddFrame(24, 16, 131, 0, 47, 44, 1);
        sequence.AddFrame(24, 16, 0, 41, 45, 44, 7);
        sequence.AddFrame(9, 4, 45, 53, 30, 32, 1);
        sequence.AddFrame(9, 4, 75, 53, 30, 32, 1, true); // spike ball is spawned here

        sequence = spriteSheet.AddFrameSquence("PostAttacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(9, 4, 105, 53, 41, 32, 1);
        sequence.AddFrame(9, 4, 146, 53, 41, 32, 1);
        sequence.AddFrame(8, 4, 0, 85, 41, 32, 4);
        sequence.AddFrame(9, 4, 41, 85, 38, 32, 6);
        sequence.AddFrame(10, 5, 79, 85, 32, 33, 1); // total of 13 frames

        sequence = spriteSheet.AddFrameSquence("SpikeBall");
        sequence.OriginOffset = -SPIKE_BALL_HITBOX.Origin - SPIKE_BALL_HITBOX.Mins;
        sequence.Hitbox = SPIKE_BALL_HITBOX;
        sequence.AddFrame(1, 0, 239, 0, 17, 17, 1, true);

        sequence = spriteSheet.AddFrameSquence("SpikeBallChain");
        sequence.AddFrame(243, 19, 8, 8, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<HoganmerShieldHitbox> shield;
    private EntityReference<HoganmerSpikeBall> spikeBall;

    public HoganmerState State
    {
        get => GetState<HoganmerState>();
        set => SetState(value);
    }

    public HoganmerShieldHitbox Shield => shield;

    public HoganmerSpikeBall SpikeBall => spikeBall;

    public Hoganmer()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = true;
        DefaultDirection = Direction.RIGHT;

        PaletteName = "HoganmerPalette";
        SpriteSheetName = "Hoganmer";

        SetAnimationNames("Idle", "Attacking", "PostAttacking");

        SetupStateArray<HoganmerState>();
        RegisterState(HoganmerState.IDLE, OnStartIdle, OnIdle, null, "Idle");
        RegisterState(HoganmerState.ATTACKING, OnAttacking, "Attacking");
        RegisterState(HoganmerState.POST_ATTACKING, OnPostAttacking, "PostAttacking");

        shield = Engine.Entities.Create<HoganmerShieldHitbox>(new
        {
            KillOnOffscreen = false
        });
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        Shield.HitResponse = HitResponse.REFLECT;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player == null)
            return;

        FaceToPlayer();

        if (frameCounter < MINIMUM_IDLE_FRAMES_BEFORE_ATTACK)
            return;

        var distance = Origin.DistanceTo(player.Origin, Metric.MAX);
        if (distance <= ATTACK_DISTANCE)
            State = HoganmerState.ATTACKING;
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAME_TO_THROW_SPIKE_BALL)
            ThrowSpikeBall();
        else if (frameCounter == FRAME_TO_DISABLE_SHIELD)
            Shield.HitResponse = HitResponse.IGNORE;
    }

    private void OnPostAttacking(EntityState state, long frameCounter)
    {
        if (frameCounter >= POST_ATTACKING_FRAMES)
            State = HoganmerState.IDLE;
    }

    private EntityReference<HoganmerSpikeBall> ThrowSpikeBall()
    {
        var player = Engine.Player;
        if (player == null)
        {
            State = HoganmerState.POST_ATTACKING;
            return null;
        }

        var throwOrigin = Origin + (SPIKE_BALL_SPAWN_OFFSET_X * Direction.GetHorizontalSignal(), SPIKE_BALL_SPAWN_OFFSET_Y);
        var delta = player.Origin - throwOrigin;
        var velocity = (SPIKE_BALL_INITIAL_SPEED * delta.Versor()).TruncFracPart();

        spikeBall = Engine.Entities.Create<HoganmerSpikeBall>(new
        {
            Respawnable = true,
            Parent = this,
            Origin = throwOrigin,
            Direction,
            Velocity = velocity
        });

        SpikeBall.Spawn();
        return spikeBall;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return COLLISION_BOX_LEGS_HEIGHT;
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

        Shield.Origin = Origin;
        Shield.Direction = Direction;
        Shield.Parent = this;
        Shield.Spawn();

        State = HoganmerState.IDLE;
    }

    internal void NotifySpikeBallBack()
    {
        SpikeBall?.Kill();
        spikeBall = null;

        State = HoganmerState.POST_ATTACKING;
    }

    protected override void OnDeath()
    {
        SpikeBall?.Kill();
        spikeBall = null;

        Shield?.Kill();
        shield = null;

        base.OnDeath();
    }
}