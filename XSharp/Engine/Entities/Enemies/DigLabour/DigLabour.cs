using XSharp.Graphics;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Math.Physics;

namespace XSharp.Engine.Entities.Enemies.DigLabour;

public enum DigLabourState
{
    IDLE,
    ATTACKING,
    LAUGHING
}

public class DigLabour : Enemy, IFSMEntity<DigLabourState>
{
    #region StaticFields
    public static readonly Color[] PALETTE =
    [
        Color.Transparent, // 0
        Color.FromBgra(0xFF406830), // 1
        Color.FromBgra(0xFF70B888), // 2
        Color.FromBgra(0xFFD0E0B0), // 3
        Color.FromBgra(0xFFB85820), // 4
        Color.FromBgra(0xFFE8A040), // 5           
        Color.FromBgra(0xFFF8D888), // 6
        Color.FromBgra(0xFF405880), // 7
        Color.FromBgra(0xFF6098C8), // 8
        Color.FromBgra(0xFFA0D8F8), // 9
        Color.FromBgra(0xFF705870), // A
        Color.FromBgra(0xFFA090A0), // B
        Color.FromBgra(0xFFE0D0E0), // C
        Color.FromBgra(0xFF783830), // D
        Color.FromBgra(0xFFF87858), // E
        Color.FromBgra(0xFF302020)  // F
    ];

    public const int HEALTH = 8;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((0, 1), (-11, -17), (11, 17));
    public static readonly Box COLLISION_BOX = ((0, 0), (-11, -17), (11, 17));

    public const int IDLE_AFTER_ATTACKING_FRAMES = 90;
    public const int IDLE_AFTER_LAUGHING_FRAMES = 50;
    public const int ATTACKING_FRAMES = 38;
    public const int LAUGHING_FRAMES = 80;
    public const int FRAME_TO_THROW_PICKAXE = 18;

    public static readonly FixedSingle PICKAXE_INITIAL_SPEED = 1536 / 256.0;
    public static readonly Box PICKAXE_HITBOX = ((0, 0), (-11, -12), (11, 12));
    public const int PICKAXE_DAMAGE = 2;
    public static readonly FixedSingle PICKAXE_SPAWN_OFFSET_X = 20;
    public static readonly FixedSingle PICKAXE_SPAWN_OFFSET_Y = -12;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("DigLabourPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("DigLabour", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Dig Labour.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 2, 4, 13, 32, 37, 1, true);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 2, 4, 13, 32, 37, 4);
        sequence.AddFrame(3, 2, 150, 14, 32, 37, 5);
        sequence.AddFrame(2, 2, 199, 13, 34, 37, 4);
        sequence.AddFrame(6, 3, 247, 13, 38, 38, 4);
        sequence.AddFrame(9, 4, 296, 12, 40, 39, 4);
        sequence.AddFrame(13, 2, 345, 13, 41, 37, 17); // pickaxe spawn here, total of 38 frames

        sequence = spriteSheet.AddFrameSquence("Laughing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 2, 4, 13, 32, 37, 4, true);
        sequence.AddFrame(4, 2, 54, 13, 32, 37, 4);
        sequence.AddFrame(5, 3, 104, 13, 32, 38, 4);
        sequence.AddFrame(4, 2, 54, 13, 32, 37, 4); // this cycle is repeated 5 times, totalizing 80 frames

        sequence = spriteSheet.AddFrameSquence("Pickaxe");
        sequence.OriginOffset = -PICKAXE_HITBOX.Origin - PICKAXE_HITBOX.Mins;
        sequence.Hitbox = PICKAXE_HITBOX;
        sequence.AddFrame(1, 0, 403, 34, 24, 24, 3, true);
        sequence.AddFrame(2, -2, 456, 9, 21, 20, 3);
        sequence.AddFrame(1, -2, 481, 7, 24, 24, 3);
        sequence.AddFrame(1, -4, 431, 9, 21, 20, 3);
        sequence.AddFrame(1, -1, 403, 7, 24, 24, 3);
        sequence.AddFrame(-1, -4, 431, 36, 21, 20, 3);
        sequence.AddFrame(1, -2, 481, 34, 24, 24, 3);
        sequence.AddFrame(-3, -2, 456, 36, 21, 20, 3);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private int idleFrames;

    public DigLabourState State
    {
        get => GetState<DigLabourState>();
        set => SetState(value);
    }

    public DigLabour()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = true;
        AlwaysFaceToPlayer = true;

        PaletteName = "DigLabourPalette";
        SpriteSheetName = "DigLabour";

        SetAnimationNames("Idle", "Attacking", "Laughing");

        SetupStateArray<DigLabourState>();
        RegisterState(DigLabourState.IDLE, OnIdle, "Idle");
        RegisterState(DigLabourState.ATTACKING, OnAttacking, "Attacking");
        RegisterState(DigLabourState.LAUGHING, OnLaughing, "Laughing");
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter >= idleFrames)
            State = DigLabourState.ATTACKING;
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAME_TO_THROW_PICKAXE)
        {
            ThrowPickaxe();
        }
        else if (frameCounter >= ATTACKING_FRAMES)
        {
            idleFrames = IDLE_AFTER_ATTACKING_FRAMES;
            State = DigLabourState.IDLE;
        }
    }

    private void OnLaughing(EntityState state, long frameCounter)
    {
        if (frameCounter >= LAUGHING_FRAMES)
        {
            idleFrames = IDLE_AFTER_LAUGHING_FRAMES;
            State = DigLabourState.IDLE;
        }
    }

    private EntityReference<DigLabourPickaxe> ThrowPickaxe()
    {
        var player = Engine.Player;
        if (player == null)
            return null;

        var throwOrigin = Origin + (PICKAXE_SPAWN_OFFSET_X * Direction.GetHorizontalSignal(), PICKAXE_SPAWN_OFFSET_Y);
        MovementUtil.GetObliqueLaunchVelocity(throwOrigin, player.Origin, PICKAXE_INITIAL_SPEED, Gravity, out Vector velocity);

        DigLabourPickaxe pickaxe = Engine.Entities.Create<DigLabourPickaxe>(new
        {
            Origin = throwOrigin,
            Velocity = velocity,
            Direction
        });

        pickaxe.pitcher = this;
        pickaxe.Spawn();
        return pickaxe;
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

        Health = HEALTH;
        ContactDamage = CONTACT_DAMAGE;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        idleFrames = IDLE_AFTER_ATTACKING_FRAMES;
        State = DigLabourState.IDLE;
    }

    internal void NotifyPlayerDamagedByPickaxe()
    {
        State = DigLabourState.LAUGHING;
    }
}