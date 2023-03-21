using System.Reflection;

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

public class Jamminger : Enemy, IStateEntity<JammingerState>
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

    public static readonly FixedSingle SPEED = 384 / 256.0;
    public static readonly FixedSingle TURNING_SPEED_Y = 1344 / 256.0;
    public static readonly FixedSingle TURNING_GRAVITY = 192 / 256.0;
    public static readonly FixedSingle SKIDDING_DECELERATION = 5 / 256.0;
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

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X.Jamminger.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            spriteSheet.CurrentTexture = texture;
        }

        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Chasing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 11, 7, 5, 31, 32, 1, true);
        sequence.AddFrame(4, 11, 108, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 58, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 158, 5, 29, 32, 1);
        sequence.AddFrame(4, 11, 208, 5, 29, 32, 1);

        sequence = spriteSheet.AddFrameSquence("Laughing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(5, 6, 444, 0, 33, 37, 3);
        sequence.AddFrame(6, 5, 4, 14, 37, 37, 3);
        sequence.AddFrame(3, 5, 479, 27, 33, 37, 4);
        sequence.AddFrame(6, 5, 4, 14, 37, 37, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public JammingerState State
    {
        get => GetState<JammingerState>();
        set => SetState(value);
    }

    public Jamminger()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Directional = true;

        PaletteName = "jammingerPalette";
        SpriteSheetName = "Jamminger";

        SetAnimationNames("Chasing", "Laughing");

        SetupStateArray<JammingerState>();
        RegisterState(JammingerState.CHASING, OnChasing, "Chasing");
        RegisterState(JammingerState.LAUGHING, OnLaughing, "Laughing");
        RegisterState(JammingerState.SMOOTH_CHASING, OnSmoothChasing, "Chasing");
        RegisterState(JammingerState.LEAVING, OnLeaving, "Chasing");
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
    }

    private void OnLaughing(EntityState state, long frameCounter)
    {
    }

    private void OnSmoothChasing(EntityState state, long frameCounter)
    {
    }

    private void OnLeaving(EntityState state, long frameCounter)
    {
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected internal override void OnSpawn()
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

        State = JammingerState.CHASING;
    }
}