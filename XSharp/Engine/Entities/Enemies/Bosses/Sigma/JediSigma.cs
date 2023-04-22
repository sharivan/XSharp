using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public enum JediSigmaState
{
    PRE_INTRODUCING,
    INTRODUCING,
    IDLE,
    DEFENDING,
    ATTACKING
}

public class JediSigma : Boss, IFSMEntity<JediSigmaState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF202820), // 1
        Color.FromBgra(0xFF68A0F0), // 2
        Color.FromBgra(0xFF1858B0), // 3
        Color.FromBgra(0xFFF8F0D8), // 4
        Color.FromBgra(0xFFA0A0A0), // 5           
        Color.FromBgra(0xFF787878), // 6
        Color.FromBgra(0xFF48D048), // 7
        Color.FromBgra(0xFF109040), // 8
        Color.FromBgra(0xFF185020), // 9
        Color.FromBgra(0xFFF8B080), // A
        Color.FromBgra(0xFFB86048), // B
        Color.FromBgra(0xFFF03808), // C
        Color.FromBgra(0xFFB08038), // D
        Color.FromBgra(0xFF804820), // E
        Color.FromBgra(0xFF505050), // F
        Color.Transparent,          // 10
        Color.FromBgra(0xFF282828), // 11
        Color.FromBgra(0xFF504090), // 12
        Color.FromBgra(0xFF302060), // 13
        Color.FromBgra(0xFFF8F0D8), // 14
        Color.FromBgra(0xFFA0A0A0), // 15
        Color.FromBgra(0xFF787878), // 16
        Color.FromBgra(0xFFF03808), // 17
        Color.FromBgra(0xFFA83008), // 18
        Color.FromBgra(0xFF683010), // 19
        Color.FromBgra(0xFFF8B080), // 1A
        Color.FromBgra(0xFFB86048), // 1B
        Color.FromBgra(0xFF804020), // 1C
        Color.FromBgra(0xFF9870D8), // 1D
        Color.FromBgra(0xFFB88820), // 1E
        Color.FromBgra(0xFF282828)  // 1F
    };

    public static readonly FixedSingle ATTACKING_SPEED = 1024 / 256.0;

    public static readonly FixedSingle CONTACT_DAMAGE = 10;
    public static readonly FixedSingle SLASH_DAMAGE = 14;
    public static readonly FixedSingle SHOT_DAMAGE = 8;

    public static readonly Box HITBOX = ((6, 4), (-13, -25), (13, 25));
    public static readonly Box ATTACKING_HITBOX = ((-14, 2), (-13, -24), (13, 24));
    public static readonly Box CAPE_HITBOX = ((0, 0), (-29, -24), (29, 24));
    public static readonly Box SLASH_HITBOX = ((0, 0), (-22, -25), (22, 25));
    public static readonly Box SHOT_HITBOX = ((0, 0), (-4, -4), (4, 4));
    public static readonly Box COLLISION_BOX = ((2, 4), (-24, -27), (24, 27));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 8;

    public static readonly FixedSingle SHOT_OFFSET_X = 0;
    public static readonly FixedSingle SHOT_OFFSET_Y = -20;
    public static readonly FixedSingle SLASH_OFFSET_X = 38;
    public static readonly FixedSingle SLASH_OFFSET_Y = -7;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("JediSigmaPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("JediSigma", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.Bosses.X1.Sigma.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("PreIntroducing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(13, 7, 6, 15, 58, 60, 1, true);

        sequence = spriteSheet.AddFrameSquence("Introducing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 7, 72, 15, 45, 60, 4);
        sequence.AddFrame(7, 7, 123, 15, 45, 60, 4);
        sequence.AddFrame(8, 7, 72, 15, 45, 60, 20);
        sequence.AddFrame(7, 7, 303, 15, 38, 60, 22);
        sequence.AddFrame(15, 2, 357, 20, 50, 55, 20);
        sequence.AddFrame(19, 2, 413, 20, 54, 55, 4);
        sequence.AddFrame(30, 2, 473, 20, 58, 55, 4);
        sequence.AddFrame(40, 8, 537, 15, 75, 61, 1, true); // hp start to fill after 32 frames from here

        sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(40, 8, 537, 15, 75, 61, 1, true);

        sequence = spriteSheet.AddFrameSquence("Defending");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(16, 2, 105, 16, 41, 32, 7);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(13, 2, 13, 100, 71, 55, 4);
        sequence.OriginOffset = -ATTACKING_HITBOX.Origin - ATTACKING_HITBOX.Mins;
        sequence.Hitbox = ATTACKING_HITBOX;
        sequence.AddFrame(3, 1, 13, 100, 71, 55, 1, true);

        sequence = spriteSheet.AddFrameSquence("WallJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(12, 13, 159, 97, 41, 57, 5);
        sequence.AddFrame(10, 13, 206, 97, 41, 57, 5); // total of 10 frames

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(40, 8, 537, 15, 75, 61, 32);
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 4, true); // shot spawn here
        sequence.AddFrame(40, 8, 699, 14, 75, 61, 8); // total of 44 frames, loop of 12 frames

        sequence = spriteSheet.AddFrameSquence("PostShooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 18);

        sequence = spriteSheet.AddFrameSquence("PreAttack");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 4);
        sequence.AddFrame(40, 7, 780, 15, 75, 60, 8);
        sequence.AddFrame(40, 8, 537, 15, 75, 61, 12); // total of 24 frames

        sequence = spriteSheet.AddFrameSquence("Slashing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(15, 10, 13, 100, 71, 55, 1);
        sequence.AddFrame(8, 10, 273, 100, 64, 52, 5);
        sequence.AddFrame(19, 27, 345, 83, 64, 72, 5); // slash effect spawn at second frame from here with the slash hitbox
        sequence.AddFrame(18, 27, 415, 83, 63, 72, 33); // slash hitbox unspawn after 14 frames from here, total of 44 frames

        sequence = spriteSheet.AddFrameSquence("Dying");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, 0, 811, 97, 47, 61, 1, true); // TODO : Get hitbox offset

        sequence = spriteSheet.AddFrameSquence("Cape");
        sequence.OriginOffset = -CAPE_HITBOX.Origin - CAPE_HITBOX.Mins;
        sequence.Hitbox = CAPE_HITBOX;
        sequence.AddFrame(0, 0, 176, 25, 58, 48, 1, true);

        sequence = spriteSheet.AddFrameSquence("Slash");
        sequence.OriginOffset = -SLASH_HITBOX.Origin - SLASH_HITBOX.Mins;
        sequence.Hitbox = SLASH_HITBOX;
        sequence.AddFrame(0, 0, 486, 91, 79, 50, 1, true);

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(4, 4, 745, 127, 16, 16, 1);
        sequence.AddFrame(2, 2, 765, 129, 12, 12, 1);
        sequence.AddFrame(0, 0, 781, 131, 8, 8, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public JediSigmaState State
    {
        get => GetState<JediSigmaState>();
        set => SetState(value);
    }

    public JediSigma()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = true;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames(
            "PreIntroducing", 
            "Introducing", 
            "Idle", 
            "Defending", 
            "PreAttack", 
            "Attacking", 
            "WallJumping", 
            "Shooting", 
            "PostShooting", 
            "Slashing", 
            "Dying"
        );

        SetupStateArray<JediSigmaState>();
        RegisterState(JediSigmaState.PRE_INTRODUCING, OnIntroducing, "PreIntroducing");
        RegisterState(JediSigmaState.INTRODUCING, OnIntroducing, "Introducing");
        RegisterState(JediSigmaState.IDLE, OnIdle, "Idle");
        RegisterState(JediSigmaState.DEFENDING, OnAttacking, "Defending");
        RegisterState(JediSigmaState.ATTACKING, OnAttacking, "Attacking");
    }

    private void OnIntroducing(EntityState state, long frameCounter)
    {
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
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

        ContactDamage = CONTACT_DAMAGE;

        Velocity = Vector.NULL_VECTOR;
        State = JediSigmaState.PRE_INTRODUCING;
    }
}