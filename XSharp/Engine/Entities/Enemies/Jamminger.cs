using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies;

public enum JammingerState
{
    CHASING = 0,
    LAUGHING = 1,
    SMOOTH_CHASING = 2,
    LEAVING = 3
}

public enum JammingerSubState
{
    IDLE = 0,
    LAUGHING1 = 1,
    LAUGHING2 = 2,
}

public class Jamminger : Enemy, IFSMEntity<JammingerState, JammingerSubState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFFA82040), // 1
        Color.FromBgra(0xFFE05070), // 2
        Color.FromBgra(0xFFF080A0), // 3
        Color.FromBgra(0xFFB85820), // 4
        Color.FromBgra(0xFFE8A040), // 5           
        Color.FromBgra(0xFFF8D888), // 6
        Color.FromBgra(0xFF603880), // 7
        Color.FromBgra(0xFF9060C8), // 8
        Color.FromBgra(0xFFD0A0F8), // 9
        Color.FromBgra(0xFF705870), // A
        Color.FromBgra(0xFFA090A0), // B
        Color.FromBgra(0xFFE0D0E0), // C
        Color.FromBgra(0xFF286890), // D
        Color.FromBgra(0xFF60C0E0), // E
        Color.FromBgra(0xFF302020)  // F
    };

    public static readonly FixedSingle CHASING_DECELERATION_DISTANCE = 64;
    public static readonly FixedSingle CHASING_SPEED = 768 / 256.0;
    public static readonly FixedSingle SMOOTH_CHASING_SPEED = 256 / 256.0;
    public static readonly FixedSingle LAUGHING_ESCAPE_SPEED = 1024 / 256.0;
    public static readonly FixedSingle LEAVING_SPEED = 768 / 256.0;
    public static readonly FixedSingle CHASING_DECELERATION = 32 / 256.0;
    public static readonly FixedSingle CHASING_DECELERATION_STOP_SPEED = 1;
    public const int PRE_SMOOTH_CHASING_FRAMES = 30;
    public const int LAUGHING_ESCAPE_FRAMES = 12;
    public const int HEALTH = 4;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((1, 4), (-11, -9), (11, 9));
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("jammingerPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Jamminger", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Jamminger.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 11, 7, 5, 31, 32, 1, true);
        sequence.AddFrame(4, 11, 108, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 58, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 158, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 208, 5, 29, 32, 1);

        sequence = spriteSheet.AddFrameSquence("Laughing1");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 11, 7, 48, 31, 32, 1, true);
        sequence.AddFrame(4, 11, 108, 48, 29, 32, 1);
        sequence.AddFrame(4, 11, 58, 48, 29, 32, 1);
        sequence.AddFrame(4, 11, 158, 48, 29, 32, 1);
        sequence.AddFrame(4, 11, 208, 48, 29, 32, 1);

        sequence = spriteSheet.AddFrameSquence("Laughing2");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 11, 7, 92, 31, 32, 1, true);
        sequence.AddFrame(4, 11, 108, 92, 29, 32, 1);
        sequence.AddFrame(4, 11, 58, 92, 29, 32, 1);
        sequence.AddFrame(4, 11, 158, 92, 29, 32, 1);
        sequence.AddFrame(4, 11, 208, 92, 29, 32, 1);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private Vector playerOrigin;
    private Vector chasingDirection;
    private FixedSingle lastDistance;

    public JammingerState State
    {
        get => GetState<JammingerState>();
        set => SetState(value);
    }

    public JammingerSubState SubState
    {
        get => GetSubState<JammingerSubState>();
        set => SetSubState(value);
    }

    public Jamminger()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = false;

        PaletteName = "jammingerPalette";
        SpriteSheetName = "Jamminger";

        SetAnimationNames("Idle", "Laughing1", "Laughing2");

        SetupStateArray<JammingerState>();
        var state = (SpriteState) RegisterState<JammingerState, JammingerSubState>(JammingerState.CHASING, OnStartChasing, OnChasing, null);
        state.RegisterSubState(JammingerSubState.IDLE, "Idle");
        state.RegisterSubState(JammingerSubState.LAUGHING1, "Laughing1");
        state.RegisterSubState(JammingerSubState.LAUGHING2, "Laughing2");

        state = (SpriteState) RegisterState<JammingerState, JammingerSubState>(JammingerState.LAUGHING, OnLaughing);
        state.RegisterSubState(JammingerSubState.IDLE, "Idle");
        state.RegisterSubState(JammingerSubState.LAUGHING1, "Laughing1");
        state.RegisterSubState(JammingerSubState.LAUGHING2, "Laughing2");

        state = (SpriteState) RegisterState<JammingerState, JammingerSubState>(JammingerState.SMOOTH_CHASING, OnSmoothChasing);
        state.RegisterSubState(JammingerSubState.IDLE, "Idle");
        state.RegisterSubState(JammingerSubState.LAUGHING1, "Laughing1");
        state.RegisterSubState(JammingerSubState.LAUGHING2, "Laughing2");

        state = (SpriteState) RegisterState<JammingerState, JammingerSubState>(JammingerState.LEAVING, OnStartLeaving);
        state.RegisterSubState(JammingerSubState.IDLE, "Idle");
        state.RegisterSubState(JammingerSubState.LAUGHING1, "Laughing1");
        state.RegisterSubState(JammingerSubState.LAUGHING2, "Laughing2");
    }

    private void OnStartChasing(EntityState state, EntityState lastState)
    {
        var player = Engine.Player;
        if (player == null)
        {
            SetState(JammingerState.LEAVING, JammingerSubState.IDLE);
            return;
        }

        playerOrigin = player.Origin;
        chasingDirection = playerOrigin - Origin;
        Velocity = CHASING_SPEED * chasingDirection.Versor();
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
        var distance = Origin.DistanceTo(playerOrigin, Metric.MAX);
        if (distance <= CHASING_DECELERATION_DISTANCE)
        {
            Velocity -= CHASING_DECELERATION * chasingDirection.Versor();

            if (chasingDirection.X.Signal * Velocity.X.Signal <= 0
                && chasingDirection.Y.Signal * Velocity.Y.Signal <= 0
                && Velocity.Length >= CHASING_DECELERATION_STOP_SPEED)
                SetState(JammingerState.SMOOTH_CHASING, JammingerSubState.IDLE);
        }
    }

    private void OnLaughing(EntityState state, long frameCounter)
    {
        if (frameCounter >= 80)
        {
            SetState(JammingerState.SMOOTH_CHASING, JammingerSubState.IDLE);
        }
        else
        {
            Velocity = frameCounter < LAUGHING_ESCAPE_FRAMES ? (Vector) (0, -LAUGHING_ESCAPE_SPEED) : Vector.NULL_VECTOR;

            long frame = frameCounter % 10;
            SubState = frame switch
            {
                >= 0 and < 2 => JammingerSubState.IDLE,
                >= 2 and < 4 => JammingerSubState.LAUGHING1,
                >= 4 and < 8 => JammingerSubState.LAUGHING2,
                _ => JammingerSubState.LAUGHING1,
            };
        }
    }

    private void OnSmoothChasing(EntityState state, long frameCounter)
    {
        if (frameCounter < PRE_SMOOTH_CHASING_FRAMES)
        {
            Velocity = Vector.NULL_VECTOR;
        }
        else if (frameCounter == PRE_SMOOTH_CHASING_FRAMES)
        {
            var player = Engine.Player;
            if (player == null)
            {
                SetState(JammingerState.LEAVING, JammingerSubState.IDLE);
                return;
            }

            playerOrigin = player.Origin;
            chasingDirection = playerOrigin - Origin;
            lastDistance = chasingDirection.Length;
            Velocity = SMOOTH_CHASING_SPEED * chasingDirection.Versor();
        }
        else
        {
            var distance = Origin.DistanceTo(playerOrigin, Metric.MAX);
            if (distance >= lastDistance)
                SetState(JammingerState.LEAVING, JammingerSubState.IDLE);

            lastDistance = distance;
        }
    }

    private void OnStartLeaving(EntityState state, EntityState lastState)
    {
        Velocity = (0, -LEAVING_SPEED);
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;

        Health = HEALTH;
        ContactDamage = CONTACT_DAMAGE;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        SetState(JammingerState.CHASING, JammingerSubState.IDLE);
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        if (State is JammingerState.CHASING or JammingerState.SMOOTH_CHASING)
            SetState(JammingerState.LAUGHING, JammingerSubState.IDLE);
    }
}