using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Tombot;

public enum TombotState
{
    LIFT_OFF = 0,
    CHASING = 1,
    TURNING = 2,
    LEAVING = 3
}

public class Tombot : Enemy, IFSMEntity<TombotState>
{
    #region StaticFields
    public static readonly Color[] PALETTE =
    [
        Color.Transparent,          // 0
        Color.FromBgra(0xFF283090), // 1
        Color.FromBgra(0xFF4858D8), // 2
        Color.FromBgra(0xFF88A8F0), // 3
        Color.FromBgra(0xFFB85020), // 4
        Color.FromBgra(0xFFF0A040), // 5           
        Color.FromBgra(0xFFF8E888), // 6
        Color.FromBgra(0xFF982830), // 7
        Color.FromBgra(0xFFD85050), // 8
        Color.FromBgra(0xFFE08090), // 9
        Color.FromBgra(0xFF585050), // A
        Color.FromBgra(0xFF989090), // B
        Color.FromBgra(0xFFE0D8D8), // C
        Color.FromBgra(0xFF801808), // D
        Color.FromBgra(0xFFF03808), // E
        Color.FromBgra(0xFF282830)  // F
    ];

    public const int HEALTH = 2;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;

    public static readonly FixedSingle LIFT_OFF_SPEED_Y = 1;
    public static readonly FixedSingle LIFT_OFF_ACCELERATION_X = 16 / 256.0;
    public static readonly FixedSingle LIFT_OFF_DECELERATION_X = 24 / 256.0;
    public const int LIFT_OFF_FRAMES_TO_START_DECELERATION = 48;
    public static readonly FixedSingle LIFT_OFF_TERMINAL_SPEED_X = 2;
    public const int LIFT_OFF_FRAMES = 64;
    public static readonly FixedSingle CHASING_SPEED = 1;
    public const int FRAMES_TO_UPDATE_CHASING_SPEED = 32;
    public static readonly FixedSingle CHASING_MAX_DISTANCE_FROM_SPAWN = 298;
    public const int TURNING_FRAMES = 11;
    public static readonly FixedSingle LEAVING_SPEED = 1;

    public static readonly Box HITBOX = ((0, 0), (-12, -7), (12, 7));
    public static readonly Box COLLISION_BOX = ((0, 0), (-7, -12), (7, 12));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 10;

    public static readonly FixedSingle LOAD_ORIGIN_OFFSET_Y = 9;
    public static readonly Box LOAD_HITBOX = ((0, 0), (-9, -5), (9, 5));
    #endregion

    private Vector spawnPos;

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("tombotPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Tombot", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Tombot.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("LiftOff");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(11, -2, 3, 23, 39, 17, 1, true);
        sequence.AddFrame(11, -2, 3, 46, 39, 17, 1);
        sequence.AddFrame(11, -2, 53, 46, 42, 19, 1);
        sequence.AddFrame(11, -2, 102, 22, 41, 19, 1);
        sequence.AddFrame(11, -2, 53, 22, 40, 19, 1);

        sequence = spriteSheet.AddFrameSquence("Chasing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(9, 5, 154, 23, 37, 18, 1, true);
        sequence.AddFrame(11, 2, 203, 24, 40, 15, 1);
        sequence.AddFrame(10, -1, 304, 25, 38, 13, 1);
        sequence.AddFrame(4, -3, 407, 16, 32, 11, 1);

        sequence = spriteSheet.AddFrameSquence("Turning");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(11, 4, 254, 22, 37, 18, 1);
        sequence.AddFrame(12, 2, 253, 46, 39, 16, 1);
        sequence.AddFrame(12, -2, 253, 72, 38, 11, 1);
        sequence.AddFrame(4, -3, 261, 92, 23, 11, 1);
        sequence.AddFrame(13, 6, 353, 22, 40, 20, 1);
        sequence.AddFrame(16, 1, 350, 47, 45, 16, 1);
        sequence.AddFrame(13, -2, 352, 72, 40, 14, 1);
        sequence.AddFrame(0, -3, 365, 92, 14, 11, 1);
        sequence.AddFrame(11, 4, 458, 22, 37, 18, 1);
        sequence.AddFrame(12, 2, 457, 46, 39, 16, 1);
        sequence.AddFrame(11, -2, 458, 72, 38, 11, 1); // total of 11 frames

        sequence = spriteSheet.AddFrameSquence("Load");
        sequence.OriginOffset = -LOAD_HITBOX.Origin - LOAD_HITBOX.Mins;
        sequence.Hitbox = LOAD_HITBOX;
        sequence.AddFrame(0, 1, 414, 49, 18, 9, 2);
        sequence.AddFrame(0, 0, 414, 37, 18, 10, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    internal EntityReference<Igloo> igloo;

    public TombotState State
    {
        get => GetState<TombotState>();
        set => SetState(value);
    }

    public Igloo Igloo => igloo;

    public Tombot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "tombotPalette";
        SpriteSheetName = "Tombot";

        SetAnimationNames("LiftOff", "Chasing");

        SetupStateArray<TombotState>();
        RegisterState(TombotState.LIFT_OFF, null, OnLiftOff, OnEndLiftOff, "LiftOff");
        RegisterState(TombotState.CHASING, OnChasing, "Chasing");
        RegisterState(TombotState.TURNING, OnTurning, "Turning");
        RegisterState(TombotState.LEAVING, OnLeaving, "Chasing");
    }

    private void OnLiftOff(EntityState state, long frameCounter)
    {
        if (frameCounter >= LIFT_OFF_FRAMES)
        {
            State = TombotState.CHASING;
            return;
        }

        if (frameCounter < 4)
        {
            Velocity = Vector.NULL_VECTOR;
        }
        else if (frameCounter < LIFT_OFF_FRAMES_TO_START_DECELERATION)
        {
            var signal = Direction.GetHorizontalSignal();
            var vx = Velocity.X + LIFT_OFF_ACCELERATION_X * signal;
            if (vx.Abs > LIFT_OFF_TERMINAL_SPEED_X)
                vx = LIFT_OFF_TERMINAL_SPEED_X * signal;

            Velocity = (vx, -LIFT_OFF_SPEED_Y);
        }
        else
        {
            var signal = Direction.GetHorizontalSignal();
            var vx = Velocity.X - LIFT_OFF_DECELERATION_X * signal;
            Velocity = (vx, -LIFT_OFF_SPEED_Y);
        }
    }

    private void OnEndLiftOff(EntityState state)
    {
        DropLoad();
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
        if (frameCounter % FRAMES_TO_UPDATE_CHASING_SPEED != 0)
            return;

        var player = Engine.Player;
        if (player == null)
        {
            State = TombotState.LEAVING;
            return;
        }

        var distanceFromSpawn = Origin.DistanceTo(spawnPos, Metric.MAX);
        if (distanceFromSpawn >= CHASING_MAX_DISTANCE_FROM_SPAWN)
        {
            State = TombotState.LEAVING;
            return;
        }

        var playerOrigin = player.Origin;
        var chasingDirection = playerOrigin - Origin;

        if (chasingDirection.X.Signal * Direction.GetHorizontalSignal() < 0)
        {
            State = TombotState.TURNING;
            return;
        }

        Velocity = CHASING_SPEED * chasingDirection.Versor();
    }

    private void OnTurning(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player == null)
        {
            State = TombotState.LEAVING;
            return;
        }

        var playerOrigin = player.Origin;
        var chasingDirection = playerOrigin - Origin;
        Velocity = CHASING_SPEED * chasingDirection.Versor();

        if (frameCounter >= TURNING_FRAMES)
        {
            Direction = Direction.Oposite();
            State = TombotState.CHASING;
        }
    }

    private void OnLeaving(EntityState state, long frameCounter)
    {
        Velocity = (0, -LEAVING_SPEED);
    }

    private EntityReference<TombotLoad> DropLoad()
    {
        TombotLoad load = Engine.Entities.Create<TombotLoad>(new
        {
            Origin = Origin + LOAD_ORIGIN_OFFSET_Y * Vector.DOWN_VECTOR,
            Direction
        });

        load.Spawn();
        return load;
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

        spawnPos = Origin;

        State = TombotState.LIFT_OFF;
    }
}