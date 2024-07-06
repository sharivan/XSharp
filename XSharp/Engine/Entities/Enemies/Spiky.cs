using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies;

public enum SpikyState
{
    WHEELING = 0,
    TURNING = 1,
    SKIDDING = 2
}

public class Spiky : Enemy, IFSMEntity<SpikyState>
{
    #region StaticFields
    public static readonly Color[] PALETTE =
    [
        Color.Transparent, // 0
        Color.FromBgra(0xFF304080), // 1
        Color.FromBgra(0xFF2870D0), // 2
        Color.FromBgra(0xFF00C0F8), // 3
        Color.FromBgra(0xFFA0E0F8), // 4
        Color.FromBgra(0xFFF8F8F8), // 5           
        Color.FromBgra(0xFFF0E870), // 6
        Color.FromBgra(0xFFF07800), // 7
        Color.FromBgra(0xFF786000), // 8
        Color.FromBgra(0xFF902000), // 9
        Color.FromBgra(0xFFE01030), // A
        Color.FromBgra(0xFFF8A0E0), // B
        Color.FromBgra(0xFFA0A0A0), // C
        Color.FromBgra(0xFF606060), // D
        Color.FromBgra(0xFFB89000), // E
        Color.FromBgra(0xFF282828)  // F
    ];

    public static readonly FixedSingle SPEED = 384 / 256.0;
    public static readonly FixedSingle TURNING_SPEED_Y = 1344 / 256.0;
    public static readonly FixedSingle TURNING_GRAVITY = 192 / 256.0;
    public static readonly FixedSingle SKIDDING_DECELERATION = 5 / 256.0;
    public const int HEALTH = 5;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HITBOX = ((0, 0), (-12, -13), (12, 13));
    public static readonly Box SKIDDING_HITBOX = ((0, 9), (-13, -11), (13, 11));
    public static readonly Box COLLISION_BOX = ((0, 3), (-13, -16), (13, 16));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 13;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("spikyPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Spiky", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Spiky.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Wheeling");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 5, 4, 14, 37, 37, 3, true);
        sequence.AddFrame(5, 4, 105, 15, 35, 35, 3);
        sequence.AddFrame(5, 5, 4, 14, 37, 37, 3);

        sequence = spriteSheet.AddFrameSquence("Turning");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 6, 444, 0, 33, 37, 3);
        sequence.AddFrame(6, 5, 4, 14, 37, 37, 3);
        sequence.AddFrame(3, 5, 479, 27, 33, 37, 4);
        sequence.AddFrame(6, 5, 4, 14, 37, 37, 1, true);

        sequence = spriteSheet.AddFrameSquence("Skidding");
        sequence.OriginOffset = -SKIDDING_HITBOX.Origin - SKIDDING_HITBOX.Mins;
        sequence.Hitbox = SKIDDING_HITBOX;
        sequence.AddFrame(5, 11, 154, 17, 37, 32, 3);
        sequence.AddFrame(5, -2, 204, 24, 37, 18, 3);
        sequence.AddFrame(-1, -3, 306, 24, 34, 18, 3);
        sequence.AddFrame(6, -2, 254, 24, 38, 18, 3);
        sequence.AddFrame(9, -3, 356, 24, 34, 18, 3);
        sequence.AddFrame(5, -2, 204, 24, 37, 18, 3);
        sequence.AddFrame(-1, -3, 306, 24, 34, 18, 3);
        sequence.AddFrame(6, -4, 404, 24, 38, 17, 23); // total of 44 frames     

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public SpikyState State
    {
        get => GetState<SpikyState>();
        set => SetState(value);
    }

    public Spiky()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "spikyPalette";
        SpriteSheetName = "Spiky";

        SetAnimationNames("Wheeling", "Turning", "Skidding");

        SetupStateArray<SpikyState>();
        RegisterState(SpikyState.WHEELING, OnWheeling, "Wheeling");
        RegisterState(SpikyState.TURNING, OnTurning, "Turning");
        RegisterState(SpikyState.SKIDDING, OnSkidding, "Skidding");
    }

    private void OnWheeling(EntityState state, long frameCounter)
    {
        Velocity = (Direction.GetHorizontalSignal() * SPEED, Velocity.Y);
        if (Velocity.X > 0 && BlockedRight || Velocity.X < 0 && BlockedLeft)
        {
            Velocity = (0, -TURNING_SPEED_Y);
            State = SpikyState.TURNING;
        }
    }

    private void OnTurning(EntityState state, long frameCounter)
    {
        if (Velocity.Y > 0 && Landed)
        {
            Direction = Direction.Oposite();
            State = SpikyState.WHEELING;
        }
    }

    private void OnSkidding(EntityState state, long frameCounter)
    {
        if (frameCounter == 44)
            Break();
        else
            Velocity = (Direction.GetHorizontalSignal() * (Velocity.X.Abs - SKIDDING_DECELERATION), Velocity.Y);
    }

    public override FixedSingle GetGravity()
    {
        return State == SpikyState.TURNING ? TURNING_GRAVITY : base.GetGravity();
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return COLLISION_BOX_LEGS_HEIGHT;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override Box GetHitbox()
    {
        return State == SpikyState.WHEELING ? HITBOX : SKIDDING_HITBOX;
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

        State = SpikyState.WHEELING;
    }

    protected override void OnDamaged(Sprite attacker, FixedSingle damage)
    {
        base.OnDamaged(attacker, damage);

        if (Health == 1)
            State = SpikyState.SKIDDING;
    }
}