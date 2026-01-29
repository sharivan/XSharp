using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.BombBeen;

public enum BombBeenState
{
    FLYING = 0,
    DROPPING_BOMBS = 1,
    POST_DROPPING_BOMBS = 2
}

public class BombBeen : Enemy, IFSMEntity<BombBeenState>
{
    #region StaticFields
    public static readonly Color[] PALETTE =
    [
        Color.Transparent, // 0
        Color.FromBgra(0xFF28D0F8), // 1
        Color.FromBgra(0xFF1058D0), // 2
        Color.FromBgra(0xFFF8B840), // 3
        Color.FromBgra(0xFFD85820), // 4
        Color.FromBgra(0xFF883020), // 5           
        Color.FromBgra(0xFFF888A8), // 6
        Color.FromBgra(0xFFE82820), // 7
        Color.FromBgra(0xFF705858), // 8
        Color.FromBgra(0xFF282828), // 9
        Color.FromBgra(0xFFF0F0F0), // A
        Color.FromBgra(0xFFF0E080), // B
        Color.FromBgra(0xFFF8B830), // C
        Color.FromBgra(0xFFE87818), // D
        Color.FromBgra(0xFFD83000), // E
        Color.FromBgra(0xFF988888)  // F
    ];

    public static readonly FixedSingle SPEED = 320 / 256.0;
    public static readonly FixedSingle FIRST_BOMB_SPEED_X = 128 / 256.0;
    public static readonly FixedSingle SECOND_BOMB_SPEED_X = 384 / 256.0;
    public static readonly FixedSingle THIRD_BOMB_SPEED_X = 768 / 256.0;
    public static readonly FixedSingle BOMB_DROP_OFFSET = 8;
    public const int HEALTH = 4;
    public const int BOMB_HEALTH = 1;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly FixedSingle BOMB_EXPLOSION_DAMAGE = 1;
    public static readonly Box HITBOX = ((0, 0), (-7, -11), (7, 11));
    public static readonly Box BOMB_HITBOX = ((0, 2), (-5, -6), (5, 6));
    public static readonly Box BOMB_COLLISION_BOX = ((0, 2), (-4, -4), (4, 4));
    public const int ATTACK_INTERVAL = 90;
    public const int BOMB_FRAMES_TO_EXPLODE = 80;
    public const int BOMB_PLANT_FRAMES = 32;
    public const int BOMB_ABOUT_TO_EXPLODE_FRAMES = 48;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("bombBeenPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("BombBeen", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Bomb Been.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Flying");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 17, 110, 3, 25, 40, 1, true);
        sequence.AddFrame(8, 14, 55, 4, 36, 38, 1);
        sequence.AddFrame(7, 13, 5, 5, 35, 36, 1);

        sequence = spriteSheet.AddFrameSquence("DroppingBombs");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 17, 110, 3, 25, 40, 1);
        sequence.AddFrame(8, 14, 55, 4, 36, 38, 1);
        sequence.AddFrame(7, 13, 5, 5, 35, 36, 1);
        sequence.AddFrame(4, 17, 110, 3, 25, 40, 1);
        sequence.AddFrame(8, 14, 55, 4, 36, 38, 1);
        sequence.AddFrame(7, 13, 5, 5, 35, 36, 1);
        sequence.AddFrame(4, 17, 110, 3, 25, 40, 1);
        sequence.AddFrame(8, 14, 55, 4, 36, 38, 1);
        sequence.AddFrame(7, 13, 5, 5, 35, 36, 1);
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1);
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1);
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1); // total of 18 frames here
        sequence.AddFrame(4, 17, 400, 2, 25, 42, 1, true); // loop point here
        sequence.AddFrame(8, 14, 345, 3, 36, 40, 1);
        sequence.AddFrame(7, 13, 296, 4, 33, 38, 1); // bombs start to drop after 9 frames from the loop point above

        sequence = spriteSheet.AddFrameSquence("PostDroppingBombs");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1);
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1);
        sequence.AddFrame(4, 17, 255, 2, 25, 41, 1);
        sequence.AddFrame(8, 14, 200, 3, 36, 39, 1);
        sequence.AddFrame(7, 13, 150, 4, 35, 37, 1); // total of 9 frames

        sequence = spriteSheet.AddFrameSquence("BombDropped");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(0, 0, 433, 0, 10, 12, 1);

        sequence = spriteSheet.AddFrameSquence("BombPlanted");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(0, 0, 431, 18, 14, 10, 8);
        sequence.AddFrame(0, 0, 431, 35, 14, 10, 8);
        sequence.AddFrame(0, 0, 431, 18, 14, 10, 8);
        sequence.AddFrame(0, 0, 431, 35, 14, 10, 8); // total of 32 frames

        sequence = spriteSheet.AddFrameSquence("BombIdle");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 1, true);

        sequence = spriteSheet.AddFrameSquence("BombAboutToExplode");
        sequence.OriginOffset = -BOMB_HITBOX.Origin - BOMB_HITBOX.Mins;
        sequence.Hitbox = BOMB_HITBOX;
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 4);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 4);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 4);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 4);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 2);
        sequence.AddFrame(2, 0, 431, 18, 14, 10, 1, true);
        sequence.AddFrame(2, 0, 431, 35, 14, 10, 1);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public BombBeenState State
    {
        get => GetState<BombBeenState>();
        set => SetState(value);
    }

    public BombBeen()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;

        PaletteName = "bombBeenPalette";
        SpriteSheetName = "BombBeen";

        SetAnimationNames("Flying", "DroppingBombs", "PostDroppingBombs");

        SetupStateArray<BombBeenState>();
        RegisterState(BombBeenState.FLYING, OnFlying, "Flying");
        RegisterState(BombBeenState.DROPPING_BOMBS, OnDroppingBombs, "DroppingBombs");
        RegisterState(BombBeenState.POST_DROPPING_BOMBS, OnPostDroppingBombs, "PostDroppingBombs");
    }

    private void OnFlying(EntityState state, long frameCounter)
    {
        Velocity = SPEED * Direction.GetHorizontalUnitaryVector();

        if (frameCounter > 0 && frameCounter % ATTACK_INTERVAL == 0)
            State = BombBeenState.DROPPING_BOMBS;
    }

    private void OnDroppingBombs(EntityState stte, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;

        var player = Engine.Player;
        int signal = player != null ? (Origin.X - player.Origin.X > 0 ? -1 : 1) : Direction.GetHorizontalSignal();

        if (frameCounter == 27)
            DropBomb(FIRST_BOMB_SPEED_X * signal);
        else if (frameCounter == 51)
            DropBomb(SECOND_BOMB_SPEED_X * signal);
        else if (frameCounter == 109)
            DropBomb(THIRD_BOMB_SPEED_X * signal);
        else if (frameCounter >= 116)
            State = BombBeenState.POST_DROPPING_BOMBS;
    }

    private void OnPostDroppingBombs(EntityState stte, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;

        if (frameCounter >= 9)
            State = BombBeenState.FLYING;
    }

    private EntityReference<BombBeenBomb> DropBomb(FixedSingle speed)
    {
        BombBeenBomb bomb = Engine.Entities.Create<BombBeenBomb>(new
        {
            Origin = (Origin.X, Origin.Y + BOMB_DROP_OFFSET),
            Velocity = (speed, 0)
        });

        bomb.Spawn();
        return bomb;
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

        Health = HEALTH;
        ContactDamage = CONTACT_DAMAGE;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        State = BombBeenState.FLYING;
    }
}