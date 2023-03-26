using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Snowball;

public enum SnowballState
{
    SMALL = 0,
    MEDIUM = 1,
    BIG = 2
}

public class Snowball : Enemy, IStateEntity<SnowballState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF7070A0), // 1
        Color.FromBgra(0xFF7888B8), // 2
        Color.FromBgra(0xFF88B0D8), // 3
        Color.FromBgra(0xFFC0C8F8), // 4
        Color.FromBgra(0xFFF8F8F8), // 5           
        Color.FromBgra(0xFF505050), // 6
        Color.FromBgra(0xFF707070), // 7
        Color.FromBgra(0xFF888888), // 8
        Color.FromBgra(0xFFA8A8A8), // 9
        Color.FromBgra(0xFFC8C8C8), // A
        Color.FromBgra(0xFFE8E8E8), // B
        Color.FromBgra(0xFFE06088), // C
        Color.FromBgra(0xFFB01018), // D
        Color.FromBgra(0xFF601020), // E
        Color.FromBgra(0xFF182018)  // F
    };

    public const int HEALTH = 8;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;

    public static readonly FixedSingle SPEED = 332 / 256.0;
    public static readonly FixedSingle TERMINAL_SPEED = 824 / 256.0;
    public static readonly FixedSingle ACCELERATION = 12 / 256.0;

    public const int FRAMES_TO_GROW = 36;

    public static readonly Box SMALL_HITBOX = ((0, 0), (-8, -8), (8, 8));
    public static readonly Box SMALL_COLLISION_BOX = ((0, 8), (-8, -8), (8, 8));
    public static readonly Box MEDIUM_HITBOX = ((0, 0), (-11, -11), (11, 11));
    public static readonly Box MEDIUM_COLLISION_BOX = ((0, 4), (-11, -11), (11, 11));
    public static readonly Box BIG_HITBOX = ((0, 0), (-14, -14), (14, 14));
    public static readonly Box BIG_COLLISION_BOX = ((0, 0), (-14, -14), (14, 14));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 8;

    public static readonly FixedSingle DEBRIS_FAST_SPEED = 3;
    public static readonly FixedSingle DEBRIS_SLOW_SPEED = 1.5;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("snowballPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Snowball", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Snowball.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Small");
        sequence.OriginOffset = -SMALL_HITBOX.Origin - SMALL_HITBOX.Mins;
        sequence.Hitbox = SMALL_HITBOX;
        sequence.AddFrame(4, 4, 11, 20, 24, 24, 6, true);
        sequence.AddFrame(4, 4, 61, 20, 24, 24, 4);

        sequence = spriteSheet.AddFrameSquence("Medium");
        sequence.OriginOffset = -SMALL_HITBOX.Origin - SMALL_HITBOX.Mins;
        sequence.Hitbox = SMALL_HITBOX;
        sequence.AddFrame(5, 5, 107, 16, 32, 32, 6, true);
        sequence.AddFrame(5, 5, 157, 16, 32, 32, 4);

        sequence = spriteSheet.AddFrameSquence("Big");
        sequence.OriginOffset = -SMALL_HITBOX.Origin - SMALL_HITBOX.Mins;
        sequence.Hitbox = SMALL_HITBOX;
        sequence.AddFrame(6, 6, 203, 12, 40, 40, 6, true);
        sequence.AddFrame(6, 6, 259, 12, 40, 40, 4);

        sequence = spriteSheet.AddFrameSquence("SmallestDebris");
        sequence.AddFrame(319, 5, 8, 8, 1, true);

        sequence = spriteSheet.AddFrameSquence("SmallDebris");
        sequence.AddFrame(335, 26, 12, 8, 1, true);

        sequence = spriteSheet.AddFrameSquence("MediumDebris");
        sequence.AddFrame(319, 20, 10, 14, 1, true);

        sequence = spriteSheet.AddFrameSquence("BigDebris");
        sequence.AddFrame(335, 5, 16, 16, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public SnowballState State
    {
        get => GetState<SnowballState>();
        set => SetState(value);
    }

    public Snowball()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;

        PaletteName = "snowballPalette";
        SpriteSheetName = "Snowball";

        SetAnimationNames("Small", "Medium", "Big");

        SetupStateArray<SnowballState>();
        RegisterState(SnowballState.SMALL, OnRolling, "Small");
        RegisterState(SnowballState.MEDIUM, OnRolling, "Medium");
        RegisterState(SnowballState.BIG, OnRolling, "Big");
    }

    private void OnRolling(EntityState state, long frameCounter)
    {
        if (BlockedLeft || BlockedRight)
        {
            Break();
            return;
        }

        if (!Landed)
            return;

        var vx = Velocity.X;
        vx += ACCELERATION * Direction.GetHorizontalSignal();

        if (vx.Abs > TERMINAL_SPEED)
            vx = TERMINAL_SPEED * vx.Signal;

        Velocity = (vx, Velocity.Y);

        if (frameCounter == FRAMES_TO_GROW)
        {
            switch (State)
            {
                case SnowballState.SMALL:
                    State = SnowballState.MEDIUM;
                    break;

                case SnowballState.MEDIUM:
                    State = SnowballState.BIG;
                    break;
            }
        }
    }

    protected override Box GetHitbox()
    {
        return State switch
        {
            SnowballState.SMALL => SMALL_HITBOX,
            SnowballState.MEDIUM => MEDIUM_HITBOX,
            SnowballState.BIG => BIG_HITBOX,
            _ => Box.EMPTY_BOX
        };
    }

    protected override Box GetCollisionBox()
    {
        return State switch
        {
            SnowballState.SMALL => SMALL_COLLISION_BOX,
            SnowballState.MEDIUM => MEDIUM_COLLISION_BOX,
            SnowballState.BIG => BIG_COLLISION_BOX,
            _ => Box.EMPTY_BOX
        };
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

        Velocity = Vector.NULL_VECTOR;

        NothingDropOdd = 100; // 100%
        SmallHealthDropOdd = 0; // 0%
        BigHealthDropOdd = 0; // 0%
        SmallAmmoDropOdd = 0; // 0%
        BigAmmoDropOdd = 0; // 0%
        LifeUpDropOdd = 0; // 0%

        State = SnowballState.SMALL;
    }

    private EntityReference<SnowballDebrisEffect> CreateDebris(SnowballDebrisType type, int initialBlinkFrame, Direction direction, bool fast)
    {
        SnowballDebrisEffect debris = Engine.Entities.Create<SnowballDebrisEffect>(new
        {
            DebrisType = type,
            Origin,
            Velocity = (fast ? DEBRIS_FAST_SPEED : DEBRIS_SLOW_SPEED) * direction.GetUnitaryVector()
        });

        debris.initialBlinkFrame = initialBlinkFrame;
        debris.Spawn();
        return debris;
    }

    protected override void OnExplode()
    {
        CreateDebris(SnowballDebrisType.SMALLEST, 3, Direction.UP, false);
        CreateDebris(SnowballDebrisType.SMALL, 3, Direction.LEFTUP, false);
        CreateDebris(SnowballDebrisType.MEDIUM, 3, Direction.RIGHTUP, false);
        CreateDebris(SnowballDebrisType.BIG, 3, Direction.LEFT, false);
        CreateDebris(SnowballDebrisType.BIG, 3, Direction.RIGHT, false);

        CreateDebris(SnowballDebrisType.SMALLEST, 4, Direction.UP, true);
        CreateDebris(SnowballDebrisType.SMALLEST, 4, Direction.LEFTUP, true);
        CreateDebris(SnowballDebrisType.SMALL, 4, Direction.RIGHTUP, true);
        CreateDebris(SnowballDebrisType.MEDIUM, 4, Direction.LEFT, true);
        CreateDebris(SnowballDebrisType.BIG, 4, Direction.RIGHT, true);
    }
}