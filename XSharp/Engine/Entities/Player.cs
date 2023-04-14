using System;

using SharpDX;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities;

public enum PlayerState
{
    NONE = -1,
    SPAWN = 0,
    SPAWN_END = 1,
    STAND = 2,
    PRE_WALK = 3,
    WALK = 4,
    JUMP = 5,
    GOING_UP = 6,
    FALL = 7,
    LAND = 8,
    PRE_DASH = 9,
    DASH = 10,
    POST_DASH = 11,
    WALL_SLIDE = 12,
    WALL_JUMP = 13,
    PRE_LADDER_CLIMB = 14,
    LADDER = 15,
    TOP_LADDER_CLIMB = 16,
    TOP_LADDER_DESCEND = 17,
    TAKING_DAMAGE = 18,
    DYING = 19,
    VICTORY = 20,
    PRE_TELEPORTING = 21,
    TELEPORTING = 22
}

// TODO : This class needs a huge refactor
public class Player : Sprite, IFSMEntity<PlayerState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        new Color(224, 224, 224, 255), // 1
        new Color(232, 224, 64, 255), // 2
        new Color(240, 64, 16, 255), // 3
        new Color(120, 216, 240, 255), // 4
        new Color(80, 160, 240, 255), // 5
        new Color(24, 88, 176, 255), // 6
        new Color(0, 128, 248, 255), // 7
        new Color(0, 64, 240, 255), // 8
        new Color(32, 48, 128, 255), // 9
        new Color(248, 176, 128, 255), // 10
        new Color(184, 96, 72, 255), // 11
        new Color(128, 64, 32, 255), // 12
        new Color(240, 240, 240, 255), // 13
        new Color(152, 152, 152, 255), // 14
        new Color(24, 24, 24, 255), // 15
        Color.Transparent, // 16
        new Color(248, 248, 248, 255), // 17
        new Color(240, 248, 248, 255), // 18
        new Color(232, 248, 248, 255), // 19
        new Color(224, 248, 248, 255), // 20
        new Color(216, 248, 248, 255), // 21
        new Color(208, 248, 248, 255), // 22
        new Color(200, 248, 248, 255), // 23
        new Color(192, 248, 248, 255), // 24
        new Color(184, 248, 248, 255), // 25
        new Color(176, 248, 248, 255), // 26
        new Color(168, 248, 248, 255), // 27
        new Color(160, 248, 248, 255), // 28
        new Color(152, 248, 248, 255), // 29
        new Color(144, 248, 248, 255), // 30
        new Color(136, 248, 248, 255) // 31
    };
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.PrecacheSound("X Regular Shot", @"X1\01 - MMX - X Regular Shot.wav");
        Engine.PrecacheSound("X Semi Charged Shot", @"X2\X Semi Charged Shot.wav");
        Engine.PrecacheSound("X Charge Shot", @"X1\02 - MMX - X Charge Shot.wav");
        Engine.PrecacheSound("X Charge", @"X1\04 - MMX - X Charge.wav");
        Engine.PrecacheSound("X Dash", @"X1\07 - MMX - X Dash.wav");
        Engine.PrecacheSound("X Jump", @"X1\08 - MMX - X Jump.wav");
        Engine.PrecacheSound("X Land", @"X1\09 - MMX - X Land.wav");
        Engine.PrecacheSound("X Fade In", @"X1\17 - MMX - X Fade In.wav");
        Engine.PrecacheSound("X Hurt", @"X1\10 - MMX - X Hurt.wav");
        Engine.PrecacheSound("X Die", @"X1\11 - MMX - X Die.wav");
        Engine.PrecacheSound("X Upgrade Complete", @"X1\16 - MMX - X Upgrade Complete.wav");
        Engine.PrecacheSound("X Fade Out", @"X1\18 - MMX - X Fade Out.wav");
        Engine.PrecacheSound("X Life Gain", @"X1\12 - MMX - X Life Gain.wav");
        Engine.PrecacheSound("X Extra Life", @"X1\13 - MMX - X Extra Life.wav");
        Engine.PrecacheSound("X Sub Tank-Heart Powerup", @"X1\14 - MMX - X Sub Tank-Heart Powerup.wav");

        var palette = Engine.PrecachePalette("x1NormalPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("X", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.X.X[small].png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Spawn");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(-4, 17, 5, 15, 8, 48);

        sequence = spriteSheet.AddFrameSquence("SpawnEnd");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(-4, 17, 5, 15, 8, 48);
        sequence.AddFrame(3, -2, 19, 34, 22, 29, 2);
        sequence.AddFrame(8, 11, 46, 21, 30, 42);
        sequence.AddFrame(8, 8, 84, 24, 30, 39);
        sequence.AddFrame(8, 5, 120, 27, 30, 36);
        sequence.AddFrame(8, 3, 156, 28, 30, 34);
        sequence.AddFrame(8, 1, 191, 31, 30, 32, 3);

        sequence = spriteSheet.AddFrameSquence("Stand");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 3, 226, 29, 30, 34, 80, true);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 295, 29, 30, 34, 8);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 226, 29, 30, 34, 48);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 295, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 226, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 295, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);

        sequence = spriteSheet.AddFrameSquence("Tired");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 4, 166, 331, 30, 35, 10, true);
        sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
        sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
        sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
        sequence.AddFrame(8, 4, 166, 331, 30, 35, 10);
        sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
        sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
        sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
        sequence.AddFrame(8, 4, 166, 331, 30, 35, 10);
        sequence.AddFrame(8, 3, 262, 332, 30, 34, 2);
        sequence.AddFrame(8, 3, 294, 332, 30, 34, 6);
        sequence.AddFrame(8, 3, 262, 332, 30, 34, 2);
        sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
        sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 3, 365, 29, 30, 34, 4);
        sequence.AddFrame(8, 3, 402, 29, 29, 34, 12, true);

        sequence = spriteSheet.AddFrameSquence("PreWalking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 3, 5, 67, 30, 34, 5);

        sequence = spriteSheet.AddFrameSquence("Walking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 3, 50, 67, 20, 34, 1, true);
        sequence.AddFrame(3, 4, 75, 67, 23, 35, 2);
        sequence.AddFrame(7, 3, 105, 68, 32, 34, 3);
        sequence.AddFrame(10, 2, 145, 68, 34, 33, 3);
        sequence.AddFrame(5, 2, 190, 68, 26, 33, 3);
        sequence.AddFrame(3, 3, 222, 67, 22, 34, 2);
        sequence.AddFrame(5, 4, 248, 67, 25, 35, 2);
        sequence.AddFrame(5, 3, 280, 67, 30, 34, 3);
        sequence.AddFrame(8, 2, 318, 68, 34, 33, 3);
        sequence.AddFrame(7, 2, 359, 68, 29, 33, 3);
        sequence.AddFrame(1, 3, 50, 67, 20, 34);

        sequence = spriteSheet.AddFrameSquence("ShootWalking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 3, 41, 107, 29, 34, 1, true);
        sequence.AddFrame(3, 4, 76, 107, 32, 35, 2);
        sequence.AddFrame(7, 3, 115, 108, 35, 34, 3);
        sequence.AddFrame(10, 2, 159, 108, 38, 33, 3);
        sequence.AddFrame(5, 2, 204, 108, 34, 33, 3);
        sequence.AddFrame(3, 3, 246, 107, 31, 34, 2);
        sequence.AddFrame(5, 4, 284, 107, 33, 35, 2);
        sequence.AddFrame(5, 3, 326, 107, 35, 34, 3);
        sequence.AddFrame(8, 2, 369, 108, 37, 33, 3);
        sequence.AddFrame(7, 2, 413, 108, 35, 33, 3);
        sequence.AddFrame(1, 3, 41, 107, 29, 34);

        sequence = spriteSheet.AddFrameSquence("Jumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 0, 6, 148, 25, 37, 3);
        sequence.AddFrame(-5, 1, 37, 148, 15, 41);

        sequence = spriteSheet.AddFrameSquence("ShootJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 0, 201, 148, 29, 37, 3);
        sequence.AddFrame(-5, 1, 240, 148, 24, 41);

        sequence = spriteSheet.AddFrameSquence("GoingUp");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(-1, 5, 56, 146, 19, 46, 1, true);

        sequence = spriteSheet.AddFrameSquence("ShootGoingUp");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(-1, 5, 271, 146, 27, 46, 1, true);

        sequence = spriteSheet.AddFrameSquence("Falling");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 5, 80, 150, 23, 41, 4);
        sequence.AddFrame(5, 6, 108, 150, 27, 42, 1, true);

        sequence = spriteSheet.AddFrameSquence("ShootFalling");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 5, 304, 150, 31, 41, 4);
        sequence.AddFrame(5 - 3, 6, 341, 150, 31, 42, 1, true);

        sequence = spriteSheet.AddFrameSquence("Landing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 2, 139, 151, 24, 38, 2);
        sequence.AddFrame(8, 1, 166, 153, 30, 32, 2);

        sequence = spriteSheet.AddFrameSquence("ShootLanding");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 2, 378, 151, 30, 38, 2);
        sequence.AddFrame(8, 1, 413, 153, 36, 32, 2);

        sequence = spriteSheet.AddFrameSquence("PreDashing");
        sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
        sequence.Hitbox = DASHING_HITBOX;
        sequence.AddFrame(4, 12, 4, 335, 28, 31, 3);

        sequence = spriteSheet.AddFrameSquence("ShootPreDashing");
        sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
        sequence.Hitbox = DASHING_HITBOX;
        sequence.AddFrame(4, 12, 76, 335, 37, 31, 3);

        sequence = spriteSheet.AddFrameSquence("Dashing");
        sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
        sequence.Hitbox = DASHING_HITBOX;
        sequence.AddFrame(14, 7, 34, 341, 38, 26, 1, true);

        sequence = spriteSheet.AddFrameSquence("ShootDashing");
        sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
        sequence.Hitbox = DASHING_HITBOX;
        sequence.AddFrame(14, 7, 115, 341, 48, 26, 1, true);

        sequence = spriteSheet.AddFrameSquence("PostDashing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 0, 4, 335, 28, 31, 8);

        sequence = spriteSheet.AddFrameSquence("ShootPostDashing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 0, 76, 335, 37, 31, 8);

        sequence = spriteSheet.AddFrameSquence("WallSliding");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 5, 5, 197, 25, 42, 5);
        sequence.AddFrame(9, 7, 33, 196, 27, 43, 6);
        sequence.AddFrame(9, 8, 64, 196, 28, 42, 1, true);

        sequence = spriteSheet.AddFrameSquence("ShootWallSliding");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 2 - 3, 158, 200, 31, 39, 5);
        sequence.AddFrame(9 + 5, 7, 201, 196, 32, 43, 6);
        sequence.AddFrame(9 + 4, 8, 240, 196, 32, 42, 1, true);

        sequence = spriteSheet.AddFrameSquence("WallJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(7, 2, 95, 199, 30, 39, 3);
        sequence.AddFrame(5, 10, 128, 195, 27, 44);

        sequence = spriteSheet.AddFrameSquence("ShootWallJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(7, 1, 276, 200, 31, 38, 3);
        sequence.AddFrame(5, 5, 315, 200, 32, 39);

        sequence = spriteSheet.AddFrameSquence("PreLadderClimbing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(3, 4, 7, 267, 21, 36, 8);

        sequence = spriteSheet.AddFrameSquence("LadderMoving");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 10, 111, 261, 18, 49, 8, true);
        sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);
        sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
        sequence.AddFrame(5, 14, 36, 261, 18, 49, 8);
        sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
        sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);

        sequence = spriteSheet.AddFrameSquence("ShootLadder");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 14, 137, 261, 26, 48, 16, true);

        sequence = spriteSheet.AddFrameSquence("TopLadderClimbing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.AddFrame(5, -11, 169, 281, 21, 32, 4);
        sequence.AddFrame(2, -4, 195, 274, 18, 34, 4);

        sequence = spriteSheet.AddFrameSquence("TopLadderDescending");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, -4, 195, 274, 18, 34, 4);
        sequence.AddFrame(5, -11, 169, 281, 21, 32, 4);

        sequence = spriteSheet.AddFrameSquence("TakingDamage");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 1, 478, 157, 25, 36, 4);
        sequence.AddFrame(10, -1, 509, 159, 29, 34, 1);
        sequence.AddFrame(10, -1, 543, 159, 29, 34, 1);
        sequence.AddFrame(11, 9, 578, 149, 32, 48, 2);
        sequence.AddFrame(10, 1, 616, 159, 31, 34, 2);
        sequence.AddFrame(11, 11, 654, 149, 32, 48, 2);
        sequence.AddFrame(10, 1, 692, 159, 29, 34, 2);
        sequence.AddFrame(11, 11, 727, 149, 32, 48, 2);
        sequence.AddFrame(12, -1, 768, 159, 31, 34, 1);
        sequence.AddFrame(10, 1, 804, 158, 29, 35, 1);
        sequence.AddFrame(10, -1, 509, 159, 29, 34, 8);
        sequence.AddFrame(6, 1, 478, 157, 25, 36, 2);

        sequence = spriteSheet.AddFrameSquence("Dying");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 1, 478, 157, 25, 36, 30);

        sequence = spriteSheet.AddFrameSquence("Victory");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(7, 14, 221, 256, 28, 45, 10);
        sequence.AddFrame(7, 14, 259, 256, 29, 45, 1);
        sequence.AddFrame(7, 17, 297, 253, 34, 48, 2);
        sequence.AddFrame(7, 14, 335, 256, 29, 45, 2);
        sequence.AddFrame(7, 14, 373, 256, 31, 45, 3);
        sequence.AddFrame(7, 14, 221, 256, 28, 45, 40);

        sequence = spriteSheet.AddFrameSquence("PreTeleporting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 1, 191, 31, 30, 32, 3);
        sequence.AddFrame(8, 4, 156, 28, 30, 34);
        sequence.AddFrame(8, 5, 120, 27, 30, 36);
        sequence.AddFrame(8, 8, 84, 24, 30, 39);
        sequence.AddFrame(8, 11, 46, 21, 30, 42);
        sequence.AddFrame(3, -3, 19, 34, 22, 29, 2);
        sequence.AddFrame(-4, 32, 5, 15, 8, 48);

        sequence = spriteSheet.AddFrameSquence("DyingExplosion");
        sequence.AddFrame(396, 344, 6, 6, 8, false, OriginPosition.CENTER);
        sequence.AddFrame(406, 343, 8, 8, 8, false, OriginPosition.CENTER);
        sequence.AddFrame(417, 342, 9, 9, 8, false, OriginPosition.CENTER);
        sequence.AddFrame(429, 341, 11, 11, 8, true, OriginPosition.CENTER);
        sequence.AddFrame(443, 339, 15, 15, 8, false, OriginPosition.CENTER);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private int lives;

    private readonly Keys[] keyBuffer = new Keys[KEY_BUFFER_COUNT];
    protected bool death;

    private readonly AnimationReference[,] animations = new AnimationReference[Enum.GetNames(typeof(PlayerState)).Length, 3];

    private long frameCounter = 0;

    private bool jumping;
    private int jumpingFrames;
    private bool dashReleased;
    private bool teleporting;

    private FixedSingle baseHSpeed = WALKING_SPEED;
    private long lastMovingFrame = 0;
    private int dashFrameCounter;
    private bool spawning;
    private int wallJumpFrameCounter;
    private int wallSlideFrameCounter;

    private PlayerState state = PlayerState.NONE;
    private PlayerState forcedState = PlayerState.NONE;
    private Direction stateDirection;
    internal int shots;
    private int shotFrameCounter;
    private bool charging;
    private int chargingFrameCounter;
    private int chargingFrameCounter2;
    internal bool shootingCharged;

    private bool spawnSoundPlayed;

    private EntityReference<DashSparkEffect> dashSparkEffect = null;
    private EntityReference<ChargingEffect> chargingEffect = null;

    internal DashSparkEffect DashSparkEffect => dashSparkEffect;

    internal ChargingEffect ChargingEffect => chargingEffect;

    public bool CanWallJump => GetWallJumpDir() != Direction.NONE;

    public bool CrossingBossDoor
    {
        get;
        private set;
    }

    public bool CanGoOutOfCameraBounds
    {
        get;
        set;
    } = false;

    public PlayerState State
    {
        get => state;
        set => SetState(value);
    }

    public PlayerState ForcedState
    {
        get => forcedState;
        set
        {
            forcedState = value;
            SetState(forcedState, 0);
        }
    }

    public PlayerState ForcedStateException
    {
        get;
        set;
    } = PlayerState.NONE;

    public bool Shooting
    {
        get;
        private set;
    }

    public bool Dashing => state is PlayerState.PRE_DASH or PlayerState.DASH;

    public bool DashingOnly => state == PlayerState.DASH;

    public bool DashingLeft => (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.LEFT;

    public bool DashingRight => (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.RIGHT;

    public bool PostDashing => state == PlayerState.POST_DASH;

    public bool GoingUp => state == PlayerState.GOING_UP;

    public bool Falling => state == PlayerState.FALL;

    public bool WallSliding => state == PlayerState.WALL_SLIDE;

    public bool WallJumping
    {
        get;
        private set;
    }

    public bool WallJumpingToLeft => state == PlayerState.WALL_JUMP && stateDirection == Direction.LEFT;

    public bool WallJumpingToRight => state == PlayerState.WALL_JUMP && stateDirection == Direction.RIGHT;

    public bool NormalJumping => state == PlayerState.JUMP;

    public bool Jumping => state is PlayerState.JUMP or PlayerState.WALL_JUMP;

    public bool Landing => state == PlayerState.LAND;

    public bool Walking => state is PlayerState.PRE_WALK or PlayerState.WALK;

    public bool PreWalking => state == PlayerState.PRE_WALK;

    public bool WalkingOnly => state == PlayerState.WALK;

    public bool WalkingLeft => (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.LEFT;

    public bool PreWalkingLeft => state == PlayerState.PRE_WALK && stateDirection == Direction.LEFT;

    public bool WalkingLeftOnly => state == PlayerState.WALK && stateDirection == Direction.LEFT;

    public bool WalkingRight => (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.RIGHT;

    public bool PreWalkingRight => state == PlayerState.PRE_WALK && stateDirection == Direction.RIGHT;

    public bool WalkingRightOnly => state == PlayerState.WALK && stateDirection == Direction.RIGHT;

    public bool PlayerSpawning => state == PlayerState.SPAWN;

    public bool Teleporting => state == PlayerState.TELEPORTING;

    public bool VictoryPosing => state == PlayerState.VICTORY;

    public bool Standing => state == PlayerState.STAND;

    public bool PreLadderClimbing => state == PlayerState.PRE_LADDER_CLIMB;

    public bool TopLadderClimbing => state == PlayerState.TOP_LADDER_CLIMB;

    public bool TopLadderDescending => state == PlayerState.TOP_LADDER_DESCEND;

    public bool OnLadder => OnLadderOnly || TopLadderDescending || TopLadderClimbing || PreLadderClimbing;

    public bool OnLadderOnly => state == PlayerState.LADDER;

    public bool LadderMoving => Velocity.Y != 0 && OnLadder;

    public bool LadderClimbing => Velocity.Y < 0 && OnLadder;

    public bool LadderDescending => Velocity.Y > 0 && OnLadder;

    public bool TakingDamage => state == PlayerState.TAKING_DAMAGE;

    public int TakingDamageFrameCounter
    {
        get;
        private set;
    } = 0;

    public bool Dying => state == PlayerState.DYING;

    public bool Freezed
    {
        get;
        private set;
    }

    public Keys Keys => GetKeys(0);

    public Keys LastKeys => GetLastKeys(0);

    public Keys LastKeysWithoutLatency => GetLastKeys(0);

    public bool InputLocked
    {
        get;
        set;
    }

    public int Lives
    {
        get => lives;
        set
        {
            if (value is < MIN_LIVES or > MAX_LIVES)
                return;

            lives = value;
        }
    }

    public bool Tired => Health / Engine.HealthCapacity < X_TIRED_PERCENTAGE;

    public bool PressingNothing => InputLocked || Keys == 0;

    public bool PressingNoLeftRight => !InputLocked && !PressingLeft && !PressingRight;

    public bool PressingLeft => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

    public bool WasPressingLeft => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

    public bool PressedLeft => !WasPressingLeft && PressingLeft;

    public bool PressingRight => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

    public bool WasPressingRight => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

    public bool PressedRight => !WasPressingRight && PressingRight;

    public bool PressingDown => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

    public bool WasPressingDown => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

    public bool PressedDown => !WasPressingDown && PressingDown;

    public bool PressingUp => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

    public bool WasPressingUp => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

    public bool PressedUp => !WasPressingUp && PressingUp;

    public bool PressingShot => !InputLocked && Keys.HasFlag(Keys.SHOT);

    public bool WasPressingShot => !InputLocked && LastKeys.HasFlag(Keys.SHOT);

    public bool PressedShot => !WasPressingShot && PressingShot;

    public bool PressingWeapon => !InputLocked && Keys.HasFlag(Keys.WEAPON);

    public bool WasPressingWeapon => !InputLocked && LastKeys.HasFlag(Keys.WEAPON);

    public bool PressedWeapon => !WasPressingWeapon && PressingWeapon;

    public bool PressingJump => !InputLocked && Keys.HasFlag(Keys.JUMP);

    public bool WasPressingJump => !InputLocked && LastKeys.HasFlag(Keys.JUMP);

    public bool PressedJump => !WasPressingJump && PressingJump;

    public bool PressingDash => !InputLocked && Keys.HasFlag(Keys.DASH);

    public bool WasPressingDash => !InputLocked && LastKeys.HasFlag(Keys.DASH);

    public bool PressedDash => !WasPressingDash && PressingDash;

    public bool PressingLWeaponSwitch => !InputLocked && Keys.HasFlag(Keys.LWS);

    public bool WasPressingLWeaponSwitch => !InputLocked && LastKeys.HasFlag(Keys.LWS);

    public bool PressedLWeaponSwitch => !WasPressingLWeaponSwitch && PressingLWeaponSwitch;

    public bool PressingRWeaponSwitch => !InputLocked && Keys.HasFlag(Keys.RWS);

    public bool WasPressingRWeaponSwitch => !InputLocked && LastKeys.HasFlag(Keys.RWS);

    public bool PressedRWeaponSwitch => !WasPressingRWeaponSwitch && PressingRWeaponSwitch;

    public bool PressingStart => !InputLocked && Keys.HasFlag(Keys.START);

    public bool WasPressingStart => !InputLocked && LastKeys.HasFlag(Keys.START);

    public bool PressedStart => !WasPressingStart && PressingStart;

    public bool PressingSelect => !InputLocked && Keys.HasFlag(Keys.SELECT);

    public bool WasPressingSelect => !InputLocked && LastKeys.HasFlag(Keys.SELECT);

    public bool PressedSelect => !WasPressingSelect && PressingSelect;

    public bool DeadByAbiss
    {
        get;
        private set;
    }

    public Player()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X";
        Respawnable = true;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override Box GetHitbox()
    {
        return Dashing ? DASHING_HITBOX : HITBOX;
    }

    protected override FixedSingle GetTerminalDownwardSpeed()
    {
        return spawning || teleporting
            ? TELEPORT_SPEED
            : WallSliding
            ? (Underwater ? UNDERWATER_WALL_SLIDE_SPEED : WALL_SLIDE_SPEED)
            : base.GetTerminalDownwardSpeed();
    }

    protected void SetState(PlayerState state, int startAnimationIndex = -1)
    {
        SetState(state, Direction, startAnimationIndex);
    }

    protected void SetState(PlayerState state, Direction direction, int startAnimationIndex = -1)
    {
        if (state == PlayerState.NONE || forcedState != PlayerState.NONE && state != ForcedStateException && state != forcedState)
            return;

        if (state == PlayerState.STAND)
            TakingDamageFrameCounter = 0;

        this.state = state;
        stateDirection = direction;

        var animation = GetAnimation(state, Shooting);
        if (animation != CurrentAnimation)
        {
            CurrentAnimation = animation;
            CurrentAnimation?.Start(startAnimationIndex);
        }
    }

    protected AnimationReference GetAnimation(PlayerState state, bool shooting)
    {
        return animations[(int) state, shooting && !(spawning || teleporting || TopLadderClimbing || TopLadderDescending || PreLadderClimbing || PreWalking || TakingDamage || Dying || VictoryPosing) ? 1 : state == PlayerState.STAND && Tired ? 2 : 0];
    }

    public Keys GetKeys(int latency)
    {
        return keyBuffer[latency];
    }

    public Keys GetLastKeys(int latency)
    {
        return keyBuffer[latency + 1];
    }

    protected override void OnStartMoving()
    {
    }

    protected override void OnStopMoving()
    {
    }

    public void SetStandState(Direction direction, int startAnimationIndex = 0, bool stop = true)
    {
        if (LandedOnTopLadder && !BlockedUp && AutoAdjustOnTheFloor)
            AdjustOnTheFloor();

        if (stop)
            Velocity = Vector.NULL_VECTOR;

        SetState(PlayerState.STAND, direction, startAnimationIndex);
    }

    public void SetStandState(int startAnimationIndex = 0, bool stop = true)
    {
        SetStandState(Direction, startAnimationIndex, stop);
    }

    public void StopMoving()
    {
        WallJumping = false;
        jumping = false;
        Velocity = Vector.NULL_VECTOR;

        if (Landed)
            SetStandState();
        else
            SetAirStateAnimation();
    }

    protected override void OnBlockedUp()
    {
        base.OnBlockedUp();

        if (CrossingBossDoor || teleporting || Dying)
            return;

        if (TakingDamage && Velocity.Y < 0)
        {
            Velocity = Velocity.XVector;
            return;
        }

        if (!Invincible)
        {
            if (TouchingLethalSpikeUp)
            {
                Die();
                return;
            }

            if (TouchingNonLethalSpikeUp)
            {
                Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                return;
            }
        }

        if (WallJumping && wallJumpFrameCounter >= 7)
        {
            Velocity = Vector.NULL_VECTOR;
            WallJumping = false;
            jumping = false;
            SetAirStateAnimation(true);
        }
        else if (Velocity.Y < 0)
        {
            //Velocity = Velocity.XVector;

            if (Landed)
                SetStandState();
            else
                SetAirStateAnimation();
        }
    }

    protected override void OnBlockedLeft()
    {
        base.OnBlockedLeft();

        if (CrossingBossDoor || teleporting || Dying)
            return;

        if (TakingDamage)
        {
            if (PressingLeft && TakingDamageFrameCounter >= 4)
                OnKnockbackEnd();
        }
        else if (Velocity.X < 0)
        {
            if (!Invincible)
            {
                if (TouchingLethalSpikeLeft)
                {
                    Die();
                    return;
                }

                if (TouchingNonLethalSpikeLeft)
                {
                    Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                    return;
                }
            }

            if (Landed)
                SetStandState(0, false);
        }
    }

    protected override void OnBlockedRight()
    {
        base.OnBlockedRight();

        if (CrossingBossDoor || teleporting || Dying)
            return;

        if (TakingDamage)
        {
            if (PressingRight && TakingDamageFrameCounter >= 4)
                OnKnockbackEnd();
        }
        else if (Velocity.X > 0)
        {
            if (!Invincible)
            {
                if (TouchingLethalSpikeRight)
                {
                    Die();
                    return;
                }

                if (TouchingNonLethalSpikeRight)
                {
                    Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                    return;
                }
            }

            if (Landed)
                SetStandState(0, false);
        }
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        if (Velocity.Y < 0)
            return;

        WallJumping = false;
        jumping = false;
        baseHSpeed = WALKING_SPEED;

        if (CrossingBossDoor || teleporting || Dying)
            return;

        if (!spawning)
        {
            if (!TakingDamage)
            {
                if (!Invincible)
                {
                    if (LandedOnLethalSpike)
                    {
                        Die();
                        return;
                    }

                    if (LandedOnNonLethalSpike)
                    {
                        Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                        return;
                    }
                }

                if (state == PlayerState.FALL && !ContainsAnimation(PlayerState.LAND, CurrentAnimation, true))
                    PlaySound("X Land");

                if (PressingLeft)
                    TryMoveLeft();
                else if (PressingRight)
                    TryMoveRight();
                else if (state != PlayerState.STAND)
                    SetState(PlayerState.LAND, 0);
            }
            else
            {
                Velocity = Velocity.XVector;
            }
        }
        else
        {
            SetState(PlayerState.SPAWN_END, 0);
        }
    }

    protected override FixedSingle GetCollisionBoxHeadHeight()
    {
        return 8;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return 8;
    }

    protected override FixedSingle GetHitboxHeadHeight()
    {
        return 5;
    }

    protected override FixedSingle GetHitboxLegsHeight()
    {
        return 5;
    }

    protected override bool IsUsingCollisionPlacements()
    {
        return true;
    }

    protected override void OnOutOfLiveArea()
    {
        if (teleporting)
            Engine.OnPlayerTeleported();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        spawning = true;

        frameCounter = 0;
        CheckCollisionWithWorld = false;
        CheckCollisionWithSolidSprites = false;
        PaletteName = "x1NormalPalette";
        Velocity = TELEPORT_SPEED * Vector.DOWN_VECTOR;
        Lives = X_INITIAL_LIVES;
        Health = Engine.HealthCapacity;
        Freezed = false;
        CrossingBossDoor = false;
        CanGoOutOfCameraBounds = false;
        DeadByAbiss = false;
        TakingDamageFrameCounter = 0;

        ResetKeys();

        SetState(PlayerState.SPAWN, 0);
    }

    private void ResetKeys()
    {
        for (int i = 0; i < keyBuffer.Length; i++)
            keyBuffer[i] = 0;
    }

    protected override void OnDeath()
    {
        // TODO : Implement the remaining

        Lives--;

        base.OnDeath();
    }

    private void TryMoveLeft(bool standOnly = false, bool startWithNullHorizontalSpeed = false)
    {
        if (CrossingBossDoor || teleporting || TakingDamage || Dying)
            return;

        if (!Invincible)
        {
            if (TouchingLethalSpikeLeft)
            {
                Die();
                return;
            }

            if (TouchingNonLethalSpikeLeft)
            {
                Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                return;
            }
        }

        Velocity = !startWithNullHorizontalSpeed && !standOnly && (!BlockedLeft || WasPressingLeft) ? (-baseHSpeed, Velocity.Y) : (0, Velocity.Y);

        if (Landed)
        {
            if (standOnly || BlockedLeft)
            {
                bool wasStanding = Standing;
                SetStandState(Direction.LEFT, !wasStanding ? 0 : -1, !WalkingLeft);
            }
            else
            {
                if (!Shooting && baseHSpeed == PRE_WALKING_SPEED)
                {
                    bool wasPreWalkingLeft = PreWalkingLeft;
                    SetState(PlayerState.PRE_WALK, Direction.LEFT, !wasPreWalkingLeft ? 0 : -1);
                }
                else
                {
                    baseHSpeed = GetWalkingSpeed();
                    bool wasWalkingLeftOnly = WalkingLeftOnly;
                    SetState(PlayerState.WALK, Direction.LEFT, !wasWalkingLeftOnly ? 0 : -1);
                }
            }
        }
        else if (!OnLadder)
        {
            if (!Jumping && !GoingUp)
            {
                if (BlockedLeft && GetWallJumpDir() == Direction.LEFT)
                {
                    if (!WallSliding)
                    {
                        Velocity = (PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, 0);
                        wallSlideFrameCounter = 0;
                        SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
                        PlaySound("X Land");
                    }
                }
                else if (!WallJumping)
                {
                    SetAirStateAnimation();
                }
            }
            else if (!WallJumping)
            {
                SetAirStateAnimation();
            }
        }
    }

    private void TryMoveRight(bool standOnly = false, bool startWithNullHorizontalSpeed = false)
    {
        if (CrossingBossDoor || teleporting || TakingDamage || Dying)
            return;

        if (!Invincible)
        {
            if (TouchingLethalSpikeRight)
            {
                Die();
                return;
            }

            if (TouchingNonLethalSpikeRight)
            {
                Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                return;
            }
        }

        Velocity = !startWithNullHorizontalSpeed && !standOnly && (!BlockedRight || WasPressingRight) ? (baseHSpeed, Velocity.Y) : (0, Velocity.Y);

        if (Landed)
        {
            if (standOnly || BlockedRight)
            {
                bool wasStanding = Standing;
                SetStandState(Direction.RIGHT, !wasStanding ? 0 : -1, !WalkingRight);
            }
            else
            {
                if (!Shooting && baseHSpeed == PRE_WALKING_SPEED)
                {
                    bool wasPreWalkingRight = PreWalkingRight;
                    SetState(PlayerState.PRE_WALK, Direction.RIGHT, !wasPreWalkingRight ? 0 : -1);
                }
                else
                {
                    baseHSpeed = GetWalkingSpeed();
                    bool wasWalkingRightOnly = WalkingRightOnly;
                    SetState(PlayerState.WALK, Direction.RIGHT, !wasWalkingRightOnly ? 0 : -1);
                }
            }
        }
        else if (!OnLadder)
        {
            if (!Jumping && !GoingUp)
            {
                if (BlockedRight && GetWallJumpDir() == Direction.RIGHT)
                {
                    if (!WallSliding)
                    {
                        Velocity = (PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, 0);
                        wallSlideFrameCounter = 0;
                        SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
                        PlaySound("X Land");
                    }
                }
                else if (!WallJumping)
                {
                    SetAirStateAnimation();
                }
            }
            else if (!WallJumping)
            {
                SetAirStateAnimation();
            }
        }
    }

    private FixedSingle GetWalkingSpeed()
    {
        if (Walking && LandedOnSlope && LandedSlope.HCathetusSign == Velocity.X.Signal)
        {
            RightTriangle slope = LandedSlope;
            FixedSingle ratio = slope.HCathetus / slope.VCathetus;

            if (ratio == 4)
                return SLOPE_DOWNWARD_WALKING_SPEED_1;

            if (ratio == 2)
                return SLOPE_DOWNWARD_WALKING_SPEED_2;
        }

        return WALKING_SPEED;
    }

    private FixedSingle GetInitialJumpSpeed()
    {
        if (Walking)
        {
            if (baseHSpeed == SLOPE_DOWNWARD_WALKING_SPEED_1)
                return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1;

            if (baseHSpeed == SLOPE_DOWNWARD_WALKING_SPEED_2)
                return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2;
        }

        return INITIAL_UPWARD_SPEED_FROM_JUMP;
    }

    internal void PlaySound(string name, bool ignoreUpdatesUntilFinished = false)
    {
        Engine.PlaySound(0, name, ignoreUpdatesUntilFinished);
    }

    protected override void OnBeforeMove(ref Vector origin)
    {
        Box limit = Engine.World.ForegroundLayout.BoundingBox;
        if (!CanGoOutOfCameraBounds && !CrossingBossDoor && !Engine.NoCameraConstraints && !Engine.Camera.NoConstraints)
            limit &= Engine.CameraConstraintsBox;

        Clamp(limit.ClipTop(-2 * BLOCK_SIZE).ClipBottom(-2 * BLOCK_SIZE), ref origin);
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (Engine.Paused)
        {
            if (PressedStart)
                Engine.ContinueGame();
        }
        else if (!CrossingBossDoor && !Dying && !teleporting && !VictoryPosing)
        {
            frameCounter++;

            if (Velocity.X != 0)
                lastMovingFrame = frameCounter;

            if (spawning)
            {
                if (!spawnSoundPlayed)
                {
                    PlaySound("X Fade In");
                    spawnSoundPlayed = true;
                }

                if (!CheckCollisionWithWorld)
                {
                    if (Engine.CurrentCheckpoint != null)
                    {
                        if (Origin.Y >= Engine.CurrentCheckpoint.Hitbox.Top + SCENE_SIZE * 0.5)
                        {
                            CheckCollisionWithWorld = true;
                            CheckCollisionWithSolidSprites = true;
                        }
                        else
                        {
                            CheckCollisionWithWorld = false;
                            CheckCollisionWithSolidSprites = false;
                        }
                    }
                }
            }
            else
            {
                CheckCollisionWithWorld = true;
                CheckCollisionWithSolidSprites = true;
            }

            if (NoClip)
            {
                spawning = false;

                bool mirrored = false;
                Direction direction = PressingLeft ? Direction.LEFT : PressingRight ? Direction.RIGHT : Direction.NONE;
                if (direction != Direction.NONE && direction != Direction)
                {
                    mirrored = true;
                    Direction = direction;
                    RefreshAnimation();
                }

                baseHSpeed = PressingDash ? NO_CLIP_SPEED_BOOST : NO_CLIP_SPEED;
                Velocity = new Vector(mirrored ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, PressingUp ? -baseHSpeed : PressingDown ? baseHSpeed : 0);
                SetAirStateAnimation();
            }
            else if (!spawning)
            {
                if (Origin.Y >= Engine.Camera.Top + SCENE_SIZE)
                {
                    DeadByAbiss = true;
                    Die();
                    return;
                }

                Direction lastDirection = Direction;

                if (baseHSpeed == PRE_WALKING_SPEED)
                    baseHSpeed = WALKING_SPEED;

                if (TakingDamage)
                {
                    TakingDamageFrameCounter++;
                }
                else
                {
                    if (!Invincible)
                    {
                        if (LandedOnLethalSpike)
                        {
                            Die();
                            return;
                        }

                        if (LandedOnNonLethalSpike)
                        {
                            Hurt(this, NON_LETHAN_SPIKE_DAMAGE);
                            return;
                        }
                    }

                    if (!WallJumping)
                    {
                        if (!OnLadder)
                        {
                            if (PressingLeft)
                            {
                                Direction = Direction.LEFT;

                                bool mirrored = false;
                                if (lastDirection == Direction.RIGHT && !WalkingRight && !DashingRight)
                                {
                                    mirrored = true;
                                    RefreshAnimation();
                                }

                                if (Standing || PostDashing || WalkingRight || DashingRight)
                                {
                                    baseHSpeed = Standing && frameCounter - lastMovingFrame > MAX_FRAMES_TO_PRESERVE_WALKING_SPEED ? PRE_WALKING_SPEED : WALKING_SPEED;
                                    TryMoveLeft(mirrored);
                                }
                                else if (WalkingLeftOnly && Landed)
                                {
                                    baseHSpeed = GetWalkingSpeed();
                                    TryMoveLeft();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedLeft && !Jumping && !GoingUp && GetWallJumpDir() == Direction.LEFT)
                                    {
                                        if (!WallSliding)
                                        {
                                            Velocity = (PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, 0);
                                            wallSlideFrameCounter = 0;
                                            SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
                                            PlaySound("X Land");
                                        }
                                        else if (!WallJumping)
                                        {
                                            TryMoveLeft(mirrored);
                                        }
                                    }
                                    else if (!WallJumping)
                                    {
                                        TryMoveLeft(mirrored);
                                    }
                                }
                            }
                            else if (PressingRight)
                            {
                                Direction = Direction.RIGHT;

                                bool mirrored = false;
                                if (lastDirection == Direction.LEFT && !WalkingLeft && !DashingLeft)
                                {
                                    mirrored = true;
                                    RefreshAnimation();
                                }

                                if (Standing || PostDashing || WalkingLeft || DashingLeft)
                                {
                                    baseHSpeed = Standing && frameCounter - lastMovingFrame > MAX_FRAMES_TO_PRESERVE_WALKING_SPEED ? PRE_WALKING_SPEED : WALKING_SPEED;
                                    TryMoveRight(mirrored);
                                }
                                else if (WalkingRightOnly && Landed)
                                {
                                    baseHSpeed = GetWalkingSpeed();
                                    TryMoveRight();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedRight && !Jumping && !GoingUp && GetWallJumpDir() == Direction.RIGHT)
                                    {
                                        if (!WallSliding)
                                        {
                                            Velocity = (PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, 0);
                                            wallSlideFrameCounter = 0;
                                            SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
                                            PlaySound("X Land");
                                        }
                                        else if (!WallJumping)
                                        {
                                            TryMoveRight(mirrored);
                                        }
                                    }
                                    else if (!WallJumping)
                                    {
                                        TryMoveRight(mirrored);
                                    }
                                }
                            }
                            else
                            {
                                if (Landed)
                                {
                                    if (!Standing && !Dashing)
                                    {
                                        if (!WallJumping)
                                            Velocity = new Vector(0, Velocity.Y);

                                        if (!Landing && !PostDashing)
                                            SetStandState();
                                    }
                                }
                                else
                                {
                                    if (!WallJumping)
                                        Velocity = new Vector(0, Velocity.Y);

                                    SetAirStateAnimation();
                                }
                            }
                        }

                        if (PressingUp)
                        {
                            if (OnLadderOnly)
                            {
                                if (!Shooting)
                                {
                                    Box ladderCollisionBox = Hitbox.ClipTop(HITBOX.Height - 5);
                                    CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, this);
                                    if (flags.HasFlag(CollisionFlags.TOP_LADDER))
                                    {
                                        if (!TopLadderClimbing && !TopLadderDescending)
                                        {
                                            Box collisionBox = CollisionBox;
                                            worldCollider.Box = collisionBox + LADDER_MOVE_OFFSET * Vector.UP_VECTOR;
                                            worldCollider.AdjustOnTheFloor(MAP_SIZE);
                                            Vector delta = worldCollider.Box.Origin - collisionBox.Origin;
                                            worldCollider.Box = collisionBox;
                                            Origin += delta;
                                            Velocity = Vector.NULL_VECTOR;
                                            SetState(PlayerState.TOP_LADDER_CLIMB, 0);
                                        }
                                    }
                                    else if (!TopLadderClimbing && !TopLadderDescending)
                                    {
                                        Velocity = new Vector(0, -LADDER_CLIMB_SPEED);
                                        CurrentAnimation?.Start();
                                    }
                                }
                            }
                            else if (!OnLadder)
                            {
                                Box ladderCollisionBox = Hitbox.ClipTop(15);
                                CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, this);
                                if (flags.IsLadder())
                                {
                                    SpriteCollider collider = WorldCollider;
                                    var lastOrigin = collider.Box.Origin;
                                    if (collider.AdjustOnTheLadder())
                                    {
                                        Vector delta = collider.Box.Origin - lastOrigin;
                                        Origin += delta;

                                        SetState(PlayerState.PRE_LADDER_CLIMB, 0);

                                        Velocity = Vector.NULL_VECTOR;
                                    }
                                }
                            }
                        }
                        else if (PressingDown)
                        {
                            if (OnLadderOnly)
                            {
                                if (!Shooting)
                                {
                                    SpriteCollider collider = WorldCollider;
                                    if (collider.Landed)
                                    {
                                        if (!Standing)
                                            SetState(PlayerState.LAND, 0);
                                    }
                                    else
                                    {
                                        Box ladderCollisionBox = Hitbox.ClipTop(15);
                                        CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, this);
                                        if (!flags.HasFlag(CollisionFlags.LADDER))
                                        {
                                            if (Landed)
                                            {
                                                if (!Standing)
                                                    SetState(PlayerState.LAND, 0);
                                            }
                                            else if (!TopLadderClimbing && !TopLadderDescending)
                                            {
                                                Velocity = Vector.NULL_VECTOR;
                                                SetAirStateAnimation();
                                            }
                                        }
                                        else if (!TopLadderClimbing && !TopLadderDescending)
                                        {
                                            Velocity = new Vector(0, LADDER_CLIMB_SPEED);
                                            CurrentAnimation?.Start();
                                        }
                                    }
                                }
                            }
                            else if (LandedOnTopLadder && !TopLadderDescending && !TopLadderClimbing)
                            {
                                Velocity = Vector.NULL_VECTOR;

                                SpriteCollider collider = WorldCollider;
                                var lastOrigin = collider.Box.Origin;
                                collider.AdjustOnTheLadder();
                                Vector delta = collider.Box.Origin - lastOrigin;
                                Origin += delta;

                                SetState(PlayerState.TOP_LADDER_DESCEND, 0);
                            }
                        }
                        else if (OnLadderOnly)
                        {
                            Velocity = Vector.NULL_VECTOR;
                            CurrentAnimation.Stop();
                        }
                    }

                    if (PressedJump && !TopLadderClimbing && !TopLadderDescending)
                    {
                        SpriteCollider worldCollider = WorldCollider;
                        SpriteCollider spriteCollider = SpriteCollider;

                        if (worldCollider.Landed || spriteCollider.Landed || (worldCollider.TouchingWaterSurface || spriteCollider.TouchingWaterSurface) && !CanWallJump)
                        {
                            if (PressingDash)
                                baseHSpeed = DASH_SPEED;

                            if (!BlockedUp)
                            {
                                jumping = true;
                                jumpingFrames = 0;
                            }
                            else if (Velocity.Y < 0)
                            {
                                Velocity = Velocity.XVector;
                            }
                        }
                        else if (OnLadder)
                        {
                            Velocity = Vector.NULL_VECTOR;
                            SetAirStateAnimation();
                        }
                        else if (BlockedUp && Velocity.Y < 0)
                        {
                            Velocity = Velocity.XVector;
                        }
                        else if (!WallJumping || wallJumpFrameCounter >= 3)
                        {
                            Direction wallJumpDir = GetWallJumpDir();
                            if (wallJumpDir != Direction.NONE)
                            {
                                if (!Invincible)
                                {
                                    if (wallJumpDir == Direction.RIGHT)
                                    {
                                        if (TouchingLethalSpikeRight)
                                        {
                                            Die();
                                            return;
                                        }

                                        if (TouchingNonLethalSpikeRight)
                                        {
                                            if (!PressingRight)
                                                DoHurtAnimation();
                                            else
                                                Hurt(this, NON_LETHAN_SPIKE_DAMAGE);

                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (TouchingLethalSpikeLeft)
                                        {
                                            Die();
                                            return;
                                        }

                                        if (TouchingNonLethalSpikeLeft)
                                        {
                                            if (!PressingLeft)
                                                DoHurtAnimation();
                                            else
                                                Hurt(this, NON_LETHAN_SPIKE_DAMAGE);

                                            return;
                                        }
                                    }
                                }

                                WallJumping = true;
                                wallJumpFrameCounter = 0;
                                Direction = wallJumpDir;
                                baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;

                                jumping = true;
                                jumpingFrames = 0;
                                Velocity = Vector.NULL_VECTOR;
                            }
                        }
                    }

                    if (!OnLadder && !WallJumping)
                    {
                        if (PressingDash)
                        {
                            if (!jumping)
                            {
                                if (!WasPressingDash)
                                {
                                    dashReleased = false;
                                    if (Landed && (Direction == Direction.LEFT ? !BlockedLeft : !BlockedRight))
                                    {
                                        baseHSpeed = DASH_SPEED;
                                        dashFrameCounter = 0;
                                        Velocity = new Vector(Direction == Direction.LEFT ? -DASH_SPEED : DASH_SPEED, Velocity.Y);
                                        SetState(PlayerState.PRE_DASH, 0);
                                        PlaySound("X Dash");
                                    }
                                }
                                else if (!Landed && !WallJumping && !WallSliding && !OnLadder)
                                {
                                    SetAirStateAnimation();
                                }
                            }
                        }
                        else if (Dashing)
                        {
                            if (!dashReleased)
                            {
                                dashReleased = true;

                                if (Landed)
                                {
                                    baseHSpeed = WALKING_SPEED;

                                    if (Dashing)
                                    {
                                        DashSparkEffect effect = dashSparkEffect;
                                        if (effect != null)
                                        {
                                            effect.KillOnNextFrame();
                                            dashSparkEffect = null;
                                        }

                                        if (PressingLeft && !BlockedLeft)
                                        {
                                            Velocity = new Vector(-baseHSpeed, Velocity.Y);
                                            SetState(PlayerState.WALK, 0);
                                        }
                                        else if (PressingRight && !BlockedRight)
                                        {
                                            Velocity = new Vector(baseHSpeed, Velocity.Y);
                                            SetState(PlayerState.WALK, 0);
                                        }
                                        else
                                        {
                                            Velocity = new Vector(0, Velocity.Y);
                                            SetState(PlayerState.POST_DASH, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Dashing && !jumping)
                    {
                        if (dashFrameCounter == 0)
                            dashSparkEffect = Engine.StartDashSparkEffect(this);

                        dashFrameCounter++;

                        if (dashFrameCounter % 4 == 0 && !Underwater)
                            Engine.StartDashSmokeEffect(this);

                        if (dashFrameCounter > DASH_DURATION)
                        {
                            DashSparkEffect effect = dashSparkEffect;
                            if (effect != null)
                            {
                                effect.KillOnNextFrame();
                                dashSparkEffect = null;
                            }

                            baseHSpeed = WALKING_SPEED;
                            if (PressingLeft && !BlockedLeft)
                            {
                                Velocity = new Vector(-baseHSpeed, Velocity.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else if (PressingRight && !BlockedRight)
                            {
                                Velocity = new Vector(baseHSpeed, Velocity.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else
                            {
                                Velocity = Velocity.YVector;
                                SetState(PlayerState.POST_DASH, 0);
                            }
                        }
                    }
                    else
                    {
                        DashSparkEffect effect = dashSparkEffect;
                        if (effect != null)
                        {
                            effect.KillOnNextFrame();
                            dashSparkEffect = null;
                        }
                    }

                    if (jumping)
                    {
                        jumpingFrames++;

                        if (WallJumping)
                        {
                            wallJumpFrameCounter++;

                            if (wallJumpFrameCounter < 7)
                            {
                                if (wallJumpFrameCounter == 3)
                                {
                                    SetState(PlayerState.WALL_JUMP, 0);
                                }
                                else if (wallJumpFrameCounter == 4)
                                {
                                    PlaySound("X Jump");
                                    Engine.StartWallKickEffect(this);
                                }

                                Velocity = Vector.NULL_VECTOR;
                            }
                            else if (wallJumpFrameCounter == 7)
                            {
                                baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;
                                Velocity = new Vector(WallJumpingToLeft ? baseHSpeed : -baseHSpeed, -INITIAL_UPWARD_SPEED_FROM_JUMP);
                            }
                            else if (wallJumpFrameCounter > WALL_JUMP_DURATION)
                            {
                                WallJumping = false;
                                FixedSingle vy;
                                if (!PressingJump)
                                {
                                    jumping = false;
                                    vy = !Landed && !WallSliding && Velocity.Y < 0 ? 0 : Velocity.Y;
                                }
                                else
                                {
                                    vy = Velocity.Y;
                                }

                                Velocity = new Vector(PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, vy);
                                SetState(PlayerState.GOING_UP, 0);
                            }
                        }
                        else if (jumpingFrames > 1)
                        {
                            if (jumpingFrames == 2)
                            {
                                bool hspeedNull = false;
                                if (PressingDash)
                                {
                                    baseHSpeed = DASH_SPEED;
                                }
                                else if (PreWalking)
                                {
                                    baseHSpeed = WALKING_SPEED;
                                    hspeedNull = true;
                                }

                                Velocity = (hspeedNull ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, -GetInitialJumpSpeed());
                                SetState(PlayerState.JUMP, 0);
                                PlaySound("X Jump");
                            }
                            else if (!PressingJump)
                            {
                                jumping = false;
                                WallJumping = false;

                                if (Velocity.Y < 0)
                                    Velocity = Velocity.XVector;
                            }
                        }
                    }

                    if (WallSliding)
                    {
                        wallSlideFrameCounter++;
                        baseHSpeed = WALKING_SPEED;
                        Velocity = (PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, wallSlideFrameCounter < 8 ? 0 : Underwater ? UNDERWATER_WALL_SLIDE_SPEED : WALL_SLIDE_SPEED);

                        if (wallSlideFrameCounter >= 11)
                        {
                            int diff = wallSlideFrameCounter - 11;
                            if (!Underwater && diff % 4 == 0)
                                Engine.StartWallSlideEffect(this);
                        }
                    }
                }
            }

            if (!TakingDamage && !InputLocked)
            {
                if (PressingShot)
                {
                    if (!WasPressingShot)
                    {
                        if (!spawning && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                        {
                            Shooting = true;
                            shotFrameCounter = 0;

                            if (OnLadderOnly)
                            {
                                Velocity = Vector.NULL_VECTOR;

                                if (PressingLeft)
                                    Direction = Direction.LEFT;
                                else if (PressingRight)
                                    Direction = Direction.RIGHT;

                                RefreshAnimation();
                            }
                            else if (Standing || PreWalking)
                            {
                                SetStandState();
                            }
                            else
                            {
                                RefreshAnimation();
                            }

                            ShootLemon();
                        }
                    }
                    else if (!Shooting && !charging && !shootingCharged && shots < MAX_SHOTS)
                    {
                        charging = true;
                        chargingFrameCounter = 0;
                        chargingFrameCounter2 = 0;
                    }
                }

                if (!InputLocked && charging && !PressingShot)
                {
                    bool charging = this.charging;
                    int chargingFrameCounter = this.chargingFrameCounter;
                    this.charging = false;
                    this.chargingFrameCounter = 0;
                    chargingFrameCounter2 = 0;

                    PaletteName = "x1NormalPalette";

                    ChargingEffect effect = ChargingEffect;
                    if (effect != null)
                    {
                        effect.KillOnNextFrame();
                        chargingEffect = null;
                    }

                    if (!spawning && charging && chargingFrameCounter >= 4 && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                    {
                        Shooting = true;
                        shootingCharged = true;
                        shotFrameCounter = 0;

                        if (OnLadderOnly)
                        {
                            Velocity = Vector.NULL_VECTOR;

                            if (PressingLeft)
                                Direction = Direction.LEFT;
                            else if (PressingRight)
                                Direction = Direction.RIGHT;

                            RefreshAnimation();
                        }
                        else if (Standing || PreWalking)
                        {
                            SetStandState();
                        }
                        else
                        {
                            RefreshAnimation();
                        }

                        if (chargingFrameCounter >= 60)
                            ShootCharged();
                        else
                            ShootSemiCharged();
                    }
                }
            }

            if (Shooting)
            {
                shotFrameCounter++;
                if (shotFrameCounter > SHOT_DURATION)
                {
                    Shooting = false;
                    RefreshAnimation();
                }
            }

            if (PressedStart)
                Engine.PauseGame();
        }

        if (charging)
        {
            if (!InputLocked)
                chargingFrameCounter++;

            chargingFrameCounter2++;

            if (chargingFrameCounter >= 4)
            {
                int frame = chargingFrameCounter2 - 4;
                PaletteName = (frame & 2) is 0 or 1 ? "chargeLevel1Palette" : "x1NormalPalette";

                ChargingEffect effect = ChargingEffect;
                if (effect == null)
                {
                    chargingEffect = Engine.StartChargingEffect(this);
                    effect = ChargingEffect;
                }

                if (chargingFrameCounter == 64)
                    effect.Level = 2;
            }
        }
    }

    private Vector GetShotOrigin()
    {
        return state switch
        {
            PlayerState.STAND or PlayerState.LAND => new Vector(7, 9),
            PlayerState.WALK => new Vector(16, 8),
            PlayerState.JUMP or PlayerState.WALL_JUMP or PlayerState.GOING_UP or PlayerState.FALL => new Vector(16, 9),
            PlayerState.PRE_DASH => new Vector(19, -3),
            PlayerState.DASH => new Vector(24, 1),
            PlayerState.POST_DASH => new Vector(22, 9),
            PlayerState.LADDER => new Vector(7, 6),
            PlayerState.WALL_SLIDE when wallSlideFrameCounter >= 11 => new Vector(7, 11),
            _ => new Vector(7, 9),
        };
    }

    public void ShootLemon()
    {
        shots++;

        Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
        Direction direction = Direction;
        if (state == PlayerState.WALL_SLIDE)
            direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

        Engine.ShootLemon(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), baseHSpeed == DASH_SPEED);
    }

    public void ShootSemiCharged()
    {
        shots++;

        Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
        Direction direction = Direction;
        if (state == PlayerState.WALL_SLIDE)
            direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

        Engine.ShootSemiCharged(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y));
    }

    public void ShootCharged()
    {
        shots++;

        Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
        Direction direction = Direction;
        if (state == PlayerState.WALL_SLIDE)
            direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

        Engine.ShootCharged(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y));
    }

    private bool CanWallJumpOnWorldLeft()
    {
        var collider = WorldCollider;
        var leftWallJumpBoxDetector = new Box(collider.Box.LeftTop - (8, 1), 8, collider.Box.Height - collider.LegsHeight - 1);
        return Engine.World.GetCollisionFlags(leftWallJumpBoxDetector, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE_WALL, true, false, this).IsClimbable();
    }

    private bool CanWallJumpOnSpritesLeft()
    {
        var collider = SpriteCollider;
        var leftWallJumpBoxDetector = new Box(collider.Box.LeftTop - (1, 0), 1, collider.Box.Height - collider.LegsHeight);
        return Engine.World.GetCollisionFlags(leftWallJumpBoxDetector, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE_WALL, false, true, this).IsClimbable();
    }

    private bool CanWallJumpLeft()
    {
        return CheckCollisionWithWorld && CanWallJumpOnWorldLeft() || CheckCollisionWithSolidSprites && CanWallJumpOnSpritesLeft();
    }

    private bool CanWallJumpOnWorldRight()
    {
        var collider = WorldCollider;
        var rightWallJumpBoxDetector = new Box(collider.Box.RightTop + (1, -1), 8, collider.Box.Height - collider.LegsHeight - 1);
        return Engine.World.GetCollisionFlags(rightWallJumpBoxDetector, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE_WALL, true, false, this).IsClimbable();
    }

    private bool CanWallJumpOnSpritesRight()
    {
        var collider = SpriteCollider;
        var rightWallJumpBoxDetector = new Box(collider.Box.RightTop + (1, 0), 1, collider.Box.Height - collider.LegsHeight);
        return Engine.World.GetCollisionFlags(rightWallJumpBoxDetector, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE_WALL, false, true, this).IsClimbable();
    }

    private bool CanWallJumpRight()
    {
        return CheckCollisionWithWorld && CanWallJumpOnWorldRight() || CheckCollisionWithSolidSprites && CanWallJumpOnSpritesRight();
    }

    public Direction GetWallJumpDir()
    {
        bool cwjl = CanWallJumpLeft();
        bool cwjr = CanWallJumpRight();

        /*return PressingLeft && cwjl
            ? Direction.LEFT
            : PressingRight && cwjr
            ? Direction.RIGHT
            : Direction == Direction.LEFT && cwjl
            ? Direction.LEFT 
            : Direction == Direction.RIGHT && cwjr
            ? Direction.RIGHT : cwjr
            ? Direction.RIGHT : cwjl
            ? Direction.LEFT : Direction.NONE;*/

        return cwjr ? Direction.RIGHT : cwjl ? Direction.LEFT : Direction.NONE;
    }

    private void SetAirStateAnimation(bool forceGoingUp = false)
    {
        if (Velocity.Y >= FALL_ANIMATION_MINIMAL_SPEED)
        {
            if (!Falling)
            {
                if (WallSliding)
                    baseHSpeed = WALKING_SPEED;

                SetState(PlayerState.FALL, 0);
            }
        }
        else if (forceGoingUp || !Jumping && !GoingUp)
        {
            SetState(PlayerState.GOING_UP, 0);
        }
    }

    public static Vector GetVectorDir(Direction direction)
    {
        return direction switch
        {
            Direction.LEFT => Vector.LEFT_VECTOR,
            Direction.UP => Vector.UP_VECTOR,
            Direction.RIGHT => Vector.RIGHT_VECTOR,
            Direction.DOWN => Vector.DOWN_VECTOR,
            _ => Vector.NULL_VECTOR,
        };
    }

    internal void PushKeys(Keys value)
    {
        if (death)
            return;

        Array.Copy(keyBuffer, 0, keyBuffer, 1, keyBuffer.Length - 1);
        keyBuffer[0] = value;
    }

    private void RefreshAnimation()
    {
        CurrentAnimation = GetAnimation(state, Shooting);
    }

    protected bool ContainsAnimation(PlayerState state, AnimationReference animation, bool checkShooting = false)
    {
        return animations[(int) state, 0] == animation || checkShooting && animations[(int) state, 1] == animation;
    }

    private void OnKnockbackEnd()
    {
        MakeInvincible(60, true);
        SetStandState(0, false);

        if (PressingLeft)
            TryMoveLeft();
        else if (PressingRight)
            TryMoveRight();
        else if (Landed)
            SetStandState();
        else
            SetAirStateAnimation();
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        if (CrossingBossDoor && !ContainsAnimation(PlayerState.PRE_DASH, animation, true))
            return;

        if (ContainsAnimation(PlayerState.SPAWN_END, animation))
        {
            spawning = false;
            SetStandState();
        }
        else if (ContainsAnimation(PlayerState.PRE_WALK, animation, true))
        {
            if (Landed && Walking)
            {
                baseHSpeed = GetWalkingSpeed();

                if (Direction == Direction.LEFT)
                    TryMoveLeft(false, PAUSE_AFTER_WALKING_SPEED_ENDS);
                else
                    TryMoveRight(false, PAUSE_AFTER_WALKING_SPEED_ENDS);
            }
        }
        else if (ContainsAnimation(PlayerState.JUMP, animation, true))
        {
            SetState(PlayerState.GOING_UP, 0);
        }
        else if (ContainsAnimation(PlayerState.LAND, animation, true))
        {
            SetStandState();
        }
        else if (ContainsAnimation(PlayerState.PRE_DASH, animation, true))
        {
            DashSparkEffect effect = dashSparkEffect;
            if (effect != null)
                effect.State = DashingSparkEffectState.DASHING;

            SetState(PlayerState.DASH, 0);
        }
        else if (ContainsAnimation(PlayerState.POST_DASH, animation, true))
        {
            if (Landed)
            {
                baseHSpeed = WALKING_SPEED;
                if (PressingLeft)
                    TryMoveLeft();
                else if (PressingRight)
                    TryMoveRight();
                else
                    SetStandState();
            }
            else
            {
                SetAirStateAnimation();
            }
        }
        else if (ContainsAnimation(PlayerState.PRE_LADDER_CLIMB, animation, true))
        {
            SetState(PlayerState.LADDER, 0);

            if (PressingUp)
                Velocity = new Vector(0, Shooting ? 0 : -LADDER_CLIMB_SPEED);
            else if (PressingDown)
                Velocity = new Vector(0, Shooting ? 0 : LADDER_CLIMB_SPEED);
            else
                CurrentAnimation.Stop();

            if (Shooting)
                CurrentAnimation.Stop();
        }
        else if (ContainsAnimation(PlayerState.TOP_LADDER_CLIMB, animation, true))
        {
            SetStandState();
        }
        else if (ContainsAnimation(PlayerState.TOP_LADDER_DESCEND, animation, true))
        {
            Origin += LADDER_MOVE_OFFSET * Vector.DOWN_VECTOR;

            SetState(PlayerState.LADDER, 0);

            if (PressingUp)
                Velocity = new Vector(0, Shooting ? 0 : -LADDER_CLIMB_SPEED);
            else if (PressingDown)
                Velocity = new Vector(0, Shooting ? 0 : LADDER_CLIMB_SPEED);
            else
                CurrentAnimation.Stop();

            if (Shooting)
                CurrentAnimation.Stop();
        }
        else if (ContainsAnimation(PlayerState.TAKING_DAMAGE, animation, false))
        {
            OnKnockbackEnd();
        }
        else if (ContainsAnimation(PlayerState.DYING, animation, false))
        {
            Freezed = false;
            Engine.PlaySound(2, "X Die");
            PaletteName = "flashingPalette";

            Engine.StartDyingEffect();
            KillOnNextFrame();
        }
        else if (ContainsAnimation(PlayerState.VICTORY, animation, false))
        {
            if (teleporting)
            {
                InputLocked = true;
                Invincible = true;
                SetState(PlayerState.PRE_TELEPORTING, 0);
                PlaySound("X Fade Out");
            }
            else
            {
                Invincible = false;
                InputLocked = false;
                SetStandState();
            }
        }
        else if (ContainsAnimation(PlayerState.PRE_TELEPORTING, animation, false))
        {
            Invincible = true;
            Velocity = TELEPORT_SPEED * Vector.UP_VECTOR;
            CheckCollisionWithSolidSprites = false;
            CheckCollisionWithWorld = false;
            SetState(PlayerState.TELEPORTING, 0);
        }
    }

    public void StartVictoryPosing()
    {
        Invincible = true;
        InputLocked = true;
        SetState(PlayerState.VICTORY, 0);
        PlaySound("X Upgrade Complete");
    }

    public void StartTeleporting(bool withVictoryPose)
    {
        CanGoOutOfCameraBounds = true;
        teleporting = true;

        if (withVictoryPose)
        {
            StartVictoryPosing();
        }
        else
        {
            Invincible = true;
            InputLocked = true;
            SetState(PlayerState.PRE_TELEPORTING, 0);
            PlaySound("X Fade Out");
        }
    }

    private void SetAnimation(PlayerState state, AnimationReference animation, bool shooting, bool tired = false)
    {
        animations[(int) state, shooting ? 1 : tired ? 2 : 0] = animation;
    }

    protected override void OnAnimationCreated(Animation animation)
    {
        switch (animation.Name)
        {
            case "Spawn":
                SetAnimation(PlayerState.SPAWN, animation, false);
                SetAnimation(PlayerState.TELEPORTING, animation, false);
                break;

            case "SpawnEnd":
                SetAnimation(PlayerState.SPAWN_END, animation, false);
                break;

            case "Stand":
                SetAnimation(PlayerState.STAND, animation, false);
                break;

            case "Shooting":
                SetAnimation(PlayerState.STAND, animation, true);
                break;

            case "Tired":
                SetAnimation(PlayerState.STAND, animation, false, true);
                break;

            case "PreWalking":
                SetAnimation(PlayerState.PRE_WALK, animation, false);
                break;

            case "Walking":
                SetAnimation(PlayerState.WALK, animation, false);
                break;

            case "ShootWalking":
                SetAnimation(PlayerState.WALK, animation, true);
                break;

            case "Jumping":
                SetAnimation(PlayerState.JUMP, animation, false);
                break;

            case "ShootJumping":
                SetAnimation(PlayerState.JUMP, animation, true);
                break;

            case "GoingUp":
                SetAnimation(PlayerState.GOING_UP, animation, false);
                break;

            case "ShootGoingUp":
                SetAnimation(PlayerState.GOING_UP, animation, true);
                break;

            case "Falling":
                SetAnimation(PlayerState.FALL, animation, false);
                break;

            case "ShootFalling":
                SetAnimation(PlayerState.FALL, animation, true);
                break;

            case "Landing":
                SetAnimation(PlayerState.LAND, animation, false);
                break;

            case "ShootLanding":
                SetAnimation(PlayerState.LAND, animation, true);
                break;

            case "PreDashing":
                SetAnimation(PlayerState.PRE_DASH, animation, false);
                break;

            case "ShootPreDashing":
                SetAnimation(PlayerState.PRE_DASH, animation, true);
                break;

            case "Dashing":
                SetAnimation(PlayerState.DASH, animation, false);
                break;

            case "ShootDashing":
                SetAnimation(PlayerState.DASH, animation, true);
                break;

            case "PostDashing":
                SetAnimation(PlayerState.POST_DASH, animation, false);
                break;

            case "ShootPostDashing":
                SetAnimation(PlayerState.POST_DASH, animation, true);
                break;

            case "WallSliding":
                SetAnimation(PlayerState.WALL_SLIDE, animation, false);
                break;

            case "ShootWallSliding":
                SetAnimation(PlayerState.WALL_SLIDE, animation, true);
                break;

            case "WallJumping":
                SetAnimation(PlayerState.WALL_JUMP, animation, false);
                break;

            case "ShootWallJumping":
                SetAnimation(PlayerState.WALL_JUMP, animation, true);
                break;

            case "PreLadderClimbing":
                SetAnimation(PlayerState.PRE_LADDER_CLIMB, animation, false);
                break;

            case "LadderMoving":
                SetAnimation(PlayerState.LADDER, animation, false);
                break;

            case "ShootLadder":
                SetAnimation(PlayerState.LADDER, animation, true);
                break;

            case "TopLadderClimbing":
                SetAnimation(PlayerState.TOP_LADDER_CLIMB, animation, false);
                break;

            case "TopLadderDescending":
                SetAnimation(PlayerState.TOP_LADDER_DESCEND, animation, false);
                break;

            case "TakingDamage":
                SetAnimation(PlayerState.TAKING_DAMAGE, animation, false);
                break;

            case "Dying":
                SetAnimation(PlayerState.DYING, animation, false);
                break;

            case "Victory":
                SetAnimation(PlayerState.VICTORY, animation, false);
                break;

            case "PreTeleporting":
                SetAnimation(PlayerState.PRE_TELEPORTING, animation, false);
                break;
        }
    }

    public override FixedSingle GetGravity()
    {
        return spawning || teleporting || WallJumping && wallJumpFrameCounter < 7 || WallSliding || OnLadder || Dying || CrossingBossDoor ? 0 : base.GetGravity();
    }

    public void DoHurtAnimation(Direction direction, bool knockback = true)
    {
        if (direction != Direction.NONE)
            Direction = direction;

        Engine.PlaySound(2, "X Hurt");

        WallJumping = false;
        jumping = false;

        if (knockback)
        {
            Velocity = (Direction == Direction.RIGHT ? INITIAL_DAMAGE_RECOIL_SPEED_X : -INITIAL_DAMAGE_RECOIL_SPEED_X, INITIAL_DAMAGE_RECOIL_SPEED_Y);
            SetState(PlayerState.TAKING_DAMAGE, 0);
        }
    }

    public void DoHurtAnimation(bool knockback = true)
    {
        DoHurtAnimation(Direction);
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        if (TakingDamage || Dying || Invincible || NoClip)
            return false;

        Direction direction = attacker == null || attacker == this ? Direction : GetHorizontalDirection(attacker);
        DoHurtAnimation(direction, attacker.KnockPlayerOnHurt);

        if (Health > 0 && Health - damage <= 0)
            Velocity = Vector.NULL_VECTOR;

        return true;
    }

    public void Die()
    {
        Health = 0;
    }

    protected override bool OnBreak()
    {
        Invincible = true;
        WallJumping = false;
        jumping = false;
        Velocity = Vector.NULL_VECTOR;
        Freezed = true;
        charging = false;

        ChargingEffect effect = ChargingEffect;
        if (effect != null)
        {
            effect.KillOnNextFrame();
            chargingEffect = null;
        }

        SetState(PlayerState.DYING, 0);

        return false;
    }

    public void Heal(int amount)
    {
        FixedSingle health = Health;
        health += amount;
        if (health > Engine.HealthCapacity)
            health = Engine.HealthCapacity;

        if (health > Health)
            Engine.StartHealthRecovering((int) (health - Health));
    }

    public void ReloadAmmo(int amount)
    {
        // TODO : Implement
    }

    internal void StartBossDoorCrossing()
    {
        CrossingBossDoor = true;
        CheckCollisionWithWorld = false;
        CheckCollisionWithSolidSprites = false;
        Invincible = true;
        Blinking = false;
        WallJumping = false;
        jumping = false;
        baseHSpeed = WALKING_SPEED;
        Velocity = Vector.NULL_VECTOR;
    }

    internal void StopBossDoorCrossing()
    {
        CrossingBossDoor = false;
        CheckCollisionWithWorld = true;
        CheckCollisionWithSolidSprites = true;
        Invincible = false;
        Velocity = Vector.NULL_VECTOR;

        if (Landed)
            SetStandState();
        else
            SetAirStateAnimation();
    }
}