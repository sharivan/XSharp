using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.RayBit;

public enum RayBitState
{
    IDLE = 0,
    JUMPING = 1,
    FALLING = 2,
    SHOOTING = 3
}

public class RayBit : Enemy, IStateEntity<RayBitState>
{
    #region StaticFields
    public static readonly Color[] RAYBIT_PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF208050), // 1
        Color.FromBgra(0xFF38A060), // 2
        Color.FromBgra(0xFF70E070), // 3
        Color.FromBgra(0xFF70E070), // 4
        Color.FromBgra(0xFFB83060), // 5           
        Color.FromBgra(0xFFE83858), // 6
        Color.FromBgra(0xFF402010), // 7
        Color.FromBgra(0xFFA86848), // 8
        Color.FromBgra(0xFFE0A058), // 9
        Color.FromBgra(0xFFF0D070), // A
        Color.FromBgra(0xFFF8F8F8), // B
        Color.FromBgra(0xFFB0C0E8), // C
        Color.FromBgra(0xFF8890E8), // D
        Color.FromBgra(0xFF7058C0), // E
        Color.FromBgra(0xFF402028) // F
    };

    public static readonly FixedSingle HP = 4;
    public static readonly FixedSingle CONTACT_DAMAGE = 4;

    public static readonly Box HITBOX = ((0, -1), (-12, -11), (12, 11));
    public static readonly Box COLLISION_BOX = ((0, -1), (-12, -11), (12, 11));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 13;

    public static readonly FixedSingle JUMPING_SPEED_X = 384 / 256.0;
    public static readonly FixedSingle JUMPING_SPEED_Y = -960 / 256.0;

    public static readonly FixedSingle SHOT_OFFSET_ORIGIN_X = 11;
    public static readonly FixedSingle SHOT_OFFSET_ORIGIN_Y = -21;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("rayBitPalette", RAYBIT_PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("RayBit", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X.RayBit.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            spriteSheet.CurrentTexture = texture;
        }

        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 10, 6, 4, 27, 31, 1, true);

        sequence = spriteSheet.AddFrameSquence("PreJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, -1, 56, 10, 28, 20, 8);
        sequence.AddFrame(3, 2, 104, 6, 31, 27, 1, true);

        sequence = spriteSheet.AddFrameSquence("Jumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 4, 156, 0, 28, 39, 1, true);

        sequence = spriteSheet.AddFrameSquence("Falling");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(3, 2, 104, 6, 31, 27, 1, true);

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, -1, 56, 10, 28, 20, 8);
        sequence.AddFrame(3, 2, 104, 6, 31, 27, 1);
        sequence.AddFrame(-2, 3, 207, 4, 26, 32, 14);
        sequence.AddFrame(-2, 3, 257, 4, 26, 32, 1, true);

        spriteSheet.CurrentPalette = null;

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = Vector.NULL_VECTOR;
        sequence.Hitbox = Box.EMPTY_BOX;
        sequence.AddFrame(317, 36, 6, 6, 2);
        sequence.AddFrame(315, 16, 10, 8, 2);
        sequence.OriginOffset = -RayBitShot.HITBOX.Origin - RayBitShot.HITBOX.Mins;
        sequence.Hitbox = RayBitShot.HITBOX;
        sequence.AddFrame(0, -2, 363, 34, 14, 10, 3);
        sequence.AddFrame(1, -1, 362, 14, 16, 12, 4);
        sequence.AddFrame(0, -2, 363, 34, 14, 10, 2, true);
        sequence.AddFrame(1, -1, 362, 14, 16, 12, 2);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public RayBitState State
    {
        get => GetState<RayBitState>();
        set => SetState(value);
    }

    private int jumpCounter = 0;

    public RayBit()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "rayBitPalette";
        SpriteSheetName = "RayBit";
        Directional = true;
        DefaultDirection = Direction.LEFT;

        SetAnimationNames("Idle", "PreJumping", "Jumping", "Falling", "Shooting");

        SetupStateArray<RayBitState>();
        RegisterState(RayBitState.IDLE, OnStartIdle, OnIdle, null, "Idle");
        RegisterState(RayBitState.JUMPING, OnStartJumping, OnJumping, null, "PreJumping");
        RegisterState(RayBitState.FALLING, "Falling");
        RegisterState(RayBitState.SHOOTING, OnShooting, "Shooting");
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

        jumpCounter = 0;
        Direction = DefaultDirection;
        Health = HP;
        ContactDamage = CONTACT_DAMAGE;

        State = RayBitState.IDLE;
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        if (State is RayBitState.JUMPING or RayBitState.FALLING)
            State = RayBitState.IDLE;
    }

    private RayBitState RandomNonIdleState()
    {
        var random = Engine.RNG.NextInt(16);
        return random % 2 == 0 ? RayBitState.JUMPING : RayBitState.SHOOTING;
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        Velocity = Vector.NULL_VECTOR;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter == 14)
        {
            if (jumpCounter >= 3)
                Direction = Direction.Oposite();
        }
        else if (frameCounter == 38)
            State = jumpCounter < 3 ? RayBitState.JUMPING : RandomNonIdleState();
    }

    private void OnStartJumping(EntityState state, EntityState lastState)
    {
        Velocity = Vector.NULL_VECTOR;
        jumpCounter++;
    }

    private void OnJumping(EntityState state, long frameCounter)
    {
        if (frameCounter < 16)
        {
            Velocity = Vector.NULL_VECTOR;
        }
        else if (frameCounter == 16)
        {
            SetCurrentAnimationByName("Jumping");
            Velocity = (Direction == Direction.LEFT ? -JUMPING_SPEED_X : JUMPING_SPEED_X, JUMPING_SPEED_Y);
        }
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;

        if (frameCounter == 12)
            Shoot();
        else if (frameCounter == 32)
            State = RayBitState.IDLE;
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (State != RayBitState.FALLING && Velocity.Y >= 0.5)
            State = RayBitState.FALLING;
    }

    private EntityReference<RayBitShot> Shoot()
    {
        RayBitShot shoot = Engine.Entities.Create<RayBitShot>(new
        {
            Shooter = this,
            Origin = Origin + (Direction == Direction.LEFT ? -SHOT_OFFSET_ORIGIN_X : SHOT_OFFSET_ORIGIN_X, SHOT_OFFSET_ORIGIN_Y),
            Direction
        });

        shoot.Spawn();
        return shoot;
    }
}