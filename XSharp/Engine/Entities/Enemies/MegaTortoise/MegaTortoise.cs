using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.MegaTortoise;

public enum MegaTortoiseState
{
    IDLE,
    ATTACKING
}

public class MegaTortoise : Enemy, IStateEntity<MegaTortoiseState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF602848), // 1
        Color.FromBgra(0xFFB02040), // 2
        Color.FromBgra(0xFFD86078), // 3
        Color.FromBgra(0xFF905810), // 4
        Color.FromBgra(0xFFD08010), // 5           
        Color.FromBgra(0xFFD8B058), // 6
        Color.FromBgra(0xFF289060), // 7
        Color.FromBgra(0xFF30D8A0), // 8
        Color.FromBgra(0xFFF8F8F8), // 9
        Color.FromBgra(0xFFC060D0), // A
        Color.FromBgra(0xFF7840B8), // B
        Color.FromBgra(0xFFB0A0A0), // C
        Color.FromBgra(0xFF807070), // D
        Color.FromBgra(0xFF504040), // E
        Color.FromBgra(0xFF382020)  // F
    };

    public const int HEALTH = 32;
    public static readonly FixedSingle CONTACT_DAMAGE = 4;
    public static readonly Box HITBOX = ((0, -1), (-30, -16), (30, 16));

    public const int ATTACKING_LOOP_POINT = 24;
    public const int ATTACKING_FRAMES = 113;
    public const int FRAME_FROM_LOOP_POINT_TO_LAUNCH_FIRST_BOMB = 45;
    public const int FRAME_FROM_LOOP_POINT_TO_LAUNCH_SECOND_BOMB = 81;

    public static readonly FixedSingle BOMB_INITIAL_SPEED_Y = -1542 / 256.0;
    public static readonly FixedSingle BOMB_LAUCHING_GRAVITY = 68 / 256.0;
    public static readonly FixedSingle BOMB_FALLING_SPEED = 384 / 256.0;
    public const int BOMB_FRAME_TO_OPEN_PARACHUTE = 27;
    public const int BOMB_PRE_FALLING_FRAMES = 4;
    public const int BOMB_FRAME_TO_START_TO_FALLING = BOMB_FRAME_TO_OPEN_PARACHUTE + BOMB_PRE_FALLING_FRAMES;
    public static readonly FixedSingle BOMB_START_FALLING_TARGET_OFFSET_FROM_PLAYER_X = 4;
    public static readonly Box BOMB_HITBOX = ((0, 0), (-3, -3), (3, 3));
    public static readonly FixedSingle FIRST_BOMB_SPAWN_OFFSET_X = -7;
    public static readonly FixedSingle SECOND_BOMB_SPAWN_OFFSET_X = 7;
    public static readonly FixedSingle BOMB_SPAWN_OFFSET_Y = -28;

    public const int BOMB_HEALTH = 4;
    public static readonly Box BOMB_EXPLOSION_HITBOX = ((0, 0), (-20, -20), (20, 20));
    public const int BOMB_EXPLOSION_DAMAGE = 3;
    public const int BOMB_EXPLOSION_FRAMES = 2;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.PrecacheSound("Torpedo", @"X1\25 - MMX - X Homing Torpedo.wav");

        var palette = Engine.PrecachePalette("MegaTortoisePalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("MegaTortoise", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Mega Tortoise.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 13, 85, 2, 70, 46, 6, true);
        sequence.AddFrame(5, 12, 5, 2, 70, 45, 6);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 15, 323, 1, 70, 48, 24);
        sequence.AddFrame(5, 16, 165, 0, 70, 49, 8, true); // loop here while the player is ahead
        sequence.AddFrame(5, 15, 323, 1, 70, 48, 8);
        sequence.AddFrame(5, 16, 165, 0, 70, 49, 8);
        sequence.AddFrame(5, 15, 323, 1, 70, 48, 8);
        sequence.AddFrame(5, 15, 5, 211, 70, 48, 12);
        sequence.AddFrame(5, 13, 85, 73, 70, 46, 1);
        sequence.AddFrame(5, 13, 165, 72, 70, 46, 2); // first bomb is launched here
        sequence.AddFrame(5, 17, 245, 70, 70, 50, 3);
        sequence.AddFrame(5, 17, 85, 140, 70, 50, 2);
        sequence.AddFrame(5, 19, 165, 139, 70, 52, 8);
        sequence.AddFrame(5, 21, 245, 138, 70, 54, 1);
        sequence.AddFrame(5, 15, 5, 71, 70, 48, 19);
        sequence.AddFrame(5, 13, 85, 213, 70, 46, 1);
        sequence.AddFrame(5, 13, 165, 212, 70, 46, 2); // second bomb is launched here
        sequence.AddFrame(5, 17, 245, 210, 70, 50, 3);
        sequence.AddFrame(5, 17, 85, 280, 70, 50, 2);
        sequence.AddFrame(5, 19, 165, 279, 70, 52, 8);
        sequence.AddFrame(5, 21, 245, 278, 70, 54, 1);
        sequence.AddFrame(5, 16, 165, 0, 70, 49, 8);
        sequence.AddFrame(5, 15, 323, 1, 70, 48, 8); // total of 113 frames from loop point

        sequence = spriteSheet.AddFrameSquence("BombLaunching");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(1, 1, 256, 21, 7, 8, 1, true);

        sequence = spriteSheet.AddFrameSquence("BombPreFalling");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(1, 13, 275, 15, 9, 19, 1, true);

        sequence = spriteSheet.AddFrameSquence("BombFalling");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(5, 20, 292, 13, 15, 24, 1);
        sequence.AddFrame(5, 16, 292, 13, 15, 24, 8);
        sequence.AddFrame(8, 17, 292, 13, 15, 24, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public MegaTortoiseState State
    {
        get => GetState<MegaTortoiseState>();
        set => SetState(value);
    }

    public MegaTortoise()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = true;
        AlwaysFaceToPlayer = false;

        PaletteName = "MegaTortoisePalette";
        SpriteSheetName = "MegaTortoise";

        SetAnimationNames("Idle", "Attacking");

        SetupStateArray<MegaTortoiseState>();
        RegisterState(MegaTortoiseState.IDLE, OnIdle, "Idle");
        RegisterState(MegaTortoiseState.ATTACKING, OnAttacking, "Attacking");
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player != null && GetHorizontalDirection(player) == Direction)
            State = MegaTortoiseState.ATTACKING;
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player == null || GetHorizontalDirection(player) != Direction)
        {
            State = MegaTortoiseState.IDLE;
            return;
        }

        if (frameCounter >= ATTACKING_LOOP_POINT)
        {
            long startFrame = (frameCounter - ATTACKING_LOOP_POINT) % ATTACKING_FRAMES;
            if (startFrame == FRAME_FROM_LOOP_POINT_TO_LAUNCH_FIRST_BOMB)
                LaunchBomb((FIRST_BOMB_SPAWN_OFFSET_X, BOMB_SPAWN_OFFSET_Y));
            else if (startFrame == FRAME_FROM_LOOP_POINT_TO_LAUNCH_SECOND_BOMB)
                LaunchBomb((SECOND_BOMB_SPAWN_OFFSET_X, BOMB_SPAWN_OFFSET_Y));
        }
    }

    private EntityReference<MegaTortoiseBomb> LaunchBomb(Vector offset)
    {
        var player = Engine.Player;
        if (player == null)
            return null;

        var signal = Direction.GetHorizontalSignal() * DefaultDirection.GetHorizontalSignal();
        var throwOrigin = Origin + (offset.X * signal, offset.Y);
        var targetOriginX = player.Origin.X + BOMB_START_FALLING_TARGET_OFFSET_FROM_PLAYER_X * (Origin.X - player.Origin.X).Signal;
        var velocity = ((targetOriginX - throwOrigin.X) / BOMB_FRAME_TO_START_TO_FALLING, BOMB_INITIAL_SPEED_Y);

        MegaTortoiseBomb bomb = Engine.Entities.Create<MegaTortoiseBomb>(new
        {
            Origin = throwOrigin,
            Velocity = velocity,
            Direction
        });

        bomb.launcher = this;
        bomb.Spawn();
        bomb.SendToBack();

        Engine.PlaySound(4, "Torpedo");

        return bomb;
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

        State = MegaTortoiseState.IDLE;
    }
}