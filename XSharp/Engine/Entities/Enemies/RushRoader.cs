using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies;

public enum RushRoaderState
{
    CHASING,
    TURNING,
    POST_TURNING,
    BUMPING,
    OUT_OF_CONTROL
}

public class RushRoader : Enemy, IFSMEntity<RushRoaderState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF181818), // 1
        Color.FromBgra(0xFF500000), // 2
        Color.FromBgra(0xFFA02800), // 3
        Color.FromBgra(0xFFC06000), // 4
        Color.FromBgra(0xFFD8A000), // 5           
        Color.FromBgra(0xFF0068A8), // 6
        Color.FromBgra(0xFF0098D8), // 7
        Color.FromBgra(0xFF005000), // 8
        Color.FromBgra(0xFF008800), // 9
        Color.FromBgra(0xFF00B800), // A
        Color.FromBgra(0xFF505050), // B
        Color.FromBgra(0xFF787878), // C
        Color.FromBgra(0xFF909090), // D
        Color.FromBgra(0xFFB0B0B0), // E
        Color.FromBgra(0xFFE0E0E0)  // F
    };

    public static readonly FixedSingle CHASING_MAX_SPEED = 448 / 256.0;
    public static readonly FixedSingle CHASING_DECELERATION = 8 / 256.0;
    public static readonly FixedSingle CHASING_ACCELERATION = 16 / 256.0;
    public static readonly FixedSingle OUT_OF_CONTROL_SPEED = 341 / 256.0;
    public static readonly FixedSingle BUMP_SPEED_X = 224 / 256.0;
    public static readonly FixedSingle BUMP_SPEED1_Y = -704 / 256.0;
    public static readonly FixedSingle BUMP_SPEED2_Y = -576 / 256.0;
    public static readonly FixedSingle BUMP_SPEED3_Y = -320 / 256.0;
    public static readonly FixedSingle[] BUMP_SPEED_Y = { BUMP_SPEED1_Y, BUMP_SPEED2_Y, BUMP_SPEED3_Y };

    public const int HEALTH = 12;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((2, 0), (-14, -14), (14, 14));
    public static readonly Box COLLISION_BOX = ((0, 1), (-16, -13), (16, 13));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 10;

    public const int END_TURNING_FRAMES = 8;
    public const int POST_TURNING_FRAMES = 7;
    public const int OUT_OF_CONTROL_FRAMES = 12;
    public const int MAX_BUMP_COUNT = 3;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("RushRoaderPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("RushRoader", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Rush Roader.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Chasing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(12, 2, 2, 16, 48, 32, 4, true);
        sequence.AddFrame(12, 1, 55, 16, 42, 31, 4);

        sequence = spriteSheet.AddFrameSquence("Turning");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(11, 0, 203, 17, 53, 30, 6);
        sequence.AddFrame(8, -3, 151, 18, 50, 27, 1, true);

        sequence = spriteSheet.AddFrameSquence("PostTurning");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 1, 105, 16, 41, 32, 7);

        sequence = spriteSheet.AddFrameSquence("OutOfControl");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(11, 0, 203, 17, 53, 30, 3, true);
        sequence.AddFrame(8, -3, 151, 18, 50, 27, 4);
        sequence.AddFrame(6, 1, 105, 16, 41, 32, 4);
        sequence.AddFrame(11, 0, 203, 17, 53, 30, 1); // total of 12 frames

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private long endTurningFrameCounter;
    private int bumpCounter;

    public RushRoaderState State
    {
        get => GetState<RushRoaderState>();
        set => SetState(value);
    }

    public RushRoader()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = true;

        PaletteName = "RushRoaderPalette";
        SpriteSheetName = "RushRoader";

        SetAnimationNames("Chasing", "Turning", "PostTurning", "OutOfControl");

        SetupStateArray<RushRoaderState>();
        RegisterState(RushRoaderState.CHASING, OnStartChasing, OnChasing, null, "Chasing");
        RegisterState(RushRoaderState.TURNING, OnStartTurning, OnTurning, null, "Turning");
        RegisterState(RushRoaderState.POST_TURNING, OnPostTurning, "PostTurning");
        RegisterState(RushRoaderState.BUMPING, OnStartBumping, OnBumping, null, "Chasing");
        RegisterState(RushRoaderState.OUT_OF_CONTROL, OnOutOfControl, "OutOfControl");
    }

    private void OnStartChasing(EntityState state, EntityState lastState)
    {
        Velocity = Velocity.YVector;
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player != null && GetHorizontalDirection(player) != Direction)
        {
            State = RushRoaderState.TURNING;
            return;
        }

        var vx = Velocity.X;
        vx += CHASING_ACCELERATION * Direction.GetHorizontalSignal();

        if (vx.Abs > CHASING_MAX_SPEED)
            vx = CHASING_MAX_SPEED * Direction.GetHorizontalSignal();

        Velocity = (vx, Velocity.Y);

        if (Direction == Direction.RIGHT && BlockedRight || Direction == Direction.LEFT && BlockedLeft)
            State = RushRoaderState.BUMPING;
    }

    private void OnStartTurning(EntityState state, EntityState lastState)
    {
        Direction = Direction.Oposite();
        endTurningFrameCounter = 0;
    }

    private void OnTurning(EntityState state, long frameCounter)
    {
        var vx = Velocity.X;
        var lastVX = vx;
        vx += CHASING_DECELERATION * Direction.GetHorizontalSignal();

        if (vx.Signal != lastVX.Signal)
        {
            vx = 0;
            endTurningFrameCounter++;

            if (endTurningFrameCounter >= END_TURNING_FRAMES)
                State = RushRoaderState.POST_TURNING;
        }

        Velocity = (vx, Velocity.Y);
    }

    private void OnPostTurning(EntityState state, long frameCounter)
    {
        Velocity = Velocity.YVector;

        if (frameCounter >= POST_TURNING_FRAMES)
            State = RushRoaderState.CHASING;
    }

    private void OnStartBumping(EntityState state, EntityState lastState)
    {
        bumpCounter = 0;
        Velocity = (-BUMP_SPEED_X * Direction.GetHorizontalSignal(), BUMP_SPEED1_Y);
    }

    private void OnBumping(EntityState state, long frameCounter)
    {
        Velocity = (-BUMP_SPEED_X * Direction.GetHorizontalSignal(), Velocity.Y);

        if (Velocity.Y > 0 && Landed)
        {
            bumpCounter++;

            if (bumpCounter == MAX_BUMP_COUNT)
                State = RushRoaderState.CHASING;
            else
                Velocity = (Velocity.X, BUMP_SPEED_Y[bumpCounter]);
        }
    }

    private void OnOutOfControl(EntityState state, long frameCounter)
    {
        if (frameCounter % OUT_OF_CONTROL_FRAMES == 0)
            Direction = Direction.Oposite();

        Velocity = (OUT_OF_CONTROL_SPEED * Direction.GetHorizontalSignal(), Velocity.Y);
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

        Velocity = Vector.NULL_VECTOR;
        State = RushRoaderState.CHASING;
    }

    protected override void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
    {
        if (damage > 2)
            State = RushRoaderState.OUT_OF_CONTROL;

        base.OnTakeDamagePost(attacker, damage);
    }
}