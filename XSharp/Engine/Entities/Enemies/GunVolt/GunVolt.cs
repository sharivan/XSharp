using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.GunVolt;

public enum GunVoltState
{
    IDLE = 0,
    PRE_SHOOTING = 1,
    SHOOTING_MISSILES = 2,
    BETWEEN_SHOOTING_MISSILES = 3,
    SHOOTING_SPARKS = 4,
    POST_SHOOTING = 5
}

public class GunVolt : Enemy, IFSMEntity<GunVoltState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF387038), // 1
        Color.FromBgra(0xFFA83050), // 2
        Color.FromBgra(0xFFE06080), // 3
        Color.FromBgra(0xFFF098B0), // 4
        Color.FromBgra(0xFFA86060), // 5           
        Color.FromBgra(0xFFE08878), // 6
        Color.FromBgra(0xFFF0C0A0), // 7
        Color.FromBgra(0xFF005078), // 8
        Color.FromBgra(0xFF0088C8), // 9
        Color.FromBgra(0xFF10C8F8), // A
        Color.FromBgra(0xFF707070), // B
        Color.FromBgra(0xFFA8A8A8), // C
        Color.FromBgra(0xFFE0E0E0), // D
        Color.FromBgra(0xFF70D870), // E
        Color.FromBgra(0xFF202028)  // F
    };

    public const int HEALTH = 32;
    public static readonly FixedSingle CONTACT_DAMAGE = 3;
    public static readonly Box HITBOX = ((0, 1), (-16, -29), (16, 29));
    public static readonly Box COLLISION_BOX = ((0, 0), (-16, -29), (16, 29));

    public const int SHORT_IDLE_FRAMES = 40;
    public const int LONG_IDLE_FRAMES = 100;
    public const int PRE_SHOOTING_FRAMES = 72;
    public const int SHOOTING_FRAMES = 17;
    public const int BETWEEN_SHOOTING_MISSILES_FRAMES = 26;
    public const int FRAME_TO_SHOOT = 2;
    public const int POST_SHOOTING_FRAMES = 23;

    public static readonly FixedSingle MISSILE_INITIAL_SPEED = 2;
    public static readonly FixedSingle MISSILE_ACCELERATION = 12 / 256.0;
    public static readonly Box MISSILE_HITBOX = ((0, 0), (-8, -8), (8, 8));
    public const int MISSILE_DAMAGE = 2;
    public const int MISSILE_SMOKE_SPAWN_INTERVAL = 4;
    public static readonly FixedSingle MISSILE1_SPAWN_OFFSET_X = 25;
    public static readonly FixedSingle MISSILE2_SPAWN_OFFSET_X = 8;
    public static readonly FixedSingle MISSILE_SPAWN_OFFSET_Y = 6;
    public static readonly FixedSingle MISSILE_SMOKE_SPAWN_OFFSET_X = 12;
    public static readonly FixedSingle MISSILE_SMOKE_SPAWN_OFFSET_Y = 1;

    public static readonly FixedSingle SPARK_SPEED = 1024 / 256.0;
    public static readonly Box SPARK_HITBOX = ((0, 0), (-4, -4), (4, 4));
    public static readonly Box SPARK_COLLISION_BOX = ((0, 0), (-6, -6), (6, 6));
    public static readonly FixedSingle SPARK1_SPAWN_OFFSET_X = 17;
    public static readonly FixedSingle SPARK2_SPAWN_OFFSET_X = 0;
    public static readonly FixedSingle SPARK_SPAWN_OFFSET_Y = 6;
    public const int SPARK_DAMAGE = 2;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.PrecacheSound("Torpedo", @"X1\25 - MMX - X Homing Torpedo.wav");
        Engine.PrecacheSound("Electric Bolt", @"X1\55 - MMX - Electric Bolt.wav");

        var palette = Engine.PrecachePalette("GunVoltPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("GunVolt", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Gun Volt.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 0, 2, 2, 42, 57, 4, true);
        sequence.AddFrame(6, 1, 52, 2, 42, 58, 4);

        sequence = spriteSheet.AddFrameSquence("PreShooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, -4, 101, 4, 44, 53, 3);
        sequence.AddFrame(6, -7, 151, 6, 44, 50, 3);
        sequence.AddFrame(6, -9, 201, 7, 44, 48, 3);
        sequence.AddFrame(6, -10, 251, 7, 44, 47, 3);
        sequence.AddFrame(6, -9, 201, 7, 44, 48, 24);
        sequence.AddFrame(14, -9, 297, 7, 52, 48, 3);
        sequence.AddFrame(6, 5, 1, 64, 44, 62, 3);
        sequence.AddFrame(6, 5, 51, 64, 44, 62, 5);
        sequence.AddFrame(6, 5, 101, 64, 44, 62, 4);
        sequence.AddFrame(6, 5, 151, 64, 44, 62, 21); // total of 72 frames

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 5, 201, 64, 44, 62, 2);
        sequence.AddFrame(6, 5, 260, 64, 44, 62, 3); // shot spawn here
        sequence.AddFrame(6, 5, 201, 64, 44, 62, 8);
        sequence.AddFrame(6, 5, 151, 64, 44, 62, 4);

        sequence = spriteSheet.AddFrameSquence("BetweenShootingMissiles");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(6, 5, 151, 64, 44, 62, 1, true);

        sequence = spriteSheet.AddFrameSquence("PostShooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(14, -9, 310, 78, 52, 48, 4);
        sequence.AddFrame(6, -9, 201, 7, 44, 48, 4);
        sequence.AddFrame(6, -10, 251, 7, 44, 47, 4);
        sequence.AddFrame(6, -9, 201, 7, 44, 48, 4);
        sequence.AddFrame(6, -7, 151, 6, 44, 50, 4);
        sequence.AddFrame(6, -4, 101, 4, 44, 53, 3); // total of 23 frames

        sequence = spriteSheet.AddFrameSquence("Missile");
        sequence.OriginOffset = -MISSILE_HITBOX.Origin - MISSILE_HITBOX.Mins;
        sequence.Hitbox = MISSILE_HITBOX;
        sequence.AddFrame(1, 1, 360, 9, 16, 11, 1, true);

        sequence = spriteSheet.AddFrameSquence("MissileSmoke");
        sequence.AddFrame(381, 11, 8, 8, 3);
        sequence.AddFrame(389, 11, 8, 8, 4);
        sequence.AddFrame(397, 11, 8, 8, 4);
        sequence.AddFrame(405, 11, 8, 8, 1); // total of 12 frames

        sequence = spriteSheet.AddFrameSquence("Spark");
        sequence.OriginOffset = -MISSILE_HITBOX.Origin - MISSILE_HITBOX.Mins;
        sequence.Hitbox = MISSILE_HITBOX;
        sequence.AddFrame(1, 1, 360, 38, 15, 14, 2, true);
        sequence.AddFrame(2, 2, 390, 37, 15, 16, 2);
        sequence.AddFrame(2, 2, 420, 37, 16, 16, 2);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private int idleFrames;
    private int missileShotCounter;

    public GunVoltState State
    {
        get => GetState<GunVoltState>();
        set => SetState(value);
    }

    public int MissileShotCount
    {
        get;
        set;
    } = 2;

    public GunVolt()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "GunVoltPalette";
        SpriteSheetName = "GunVolt";

        SetAnimationNames("Idle", "PreShooting", "Shooting", "BetweenShootingMissiles", "PostShooting");

        SetupStateArray<GunVoltState>();
        RegisterState(GunVoltState.IDLE, OnIdle, "Idle");
        RegisterState(GunVoltState.PRE_SHOOTING, OnPreShooting, "PreShooting");
        RegisterState(GunVoltState.SHOOTING_MISSILES, OnShootingMissiles, "Shooting");
        RegisterState(GunVoltState.BETWEEN_SHOOTING_MISSILES, OnBetweenShootingMissiles, "BetweenShootingMissiles");
        RegisterState(GunVoltState.SHOOTING_SPARKS, OnShootingSparks, "Shooting");
        RegisterState(GunVoltState.POST_SHOOTING, OnPostShooting, "PostShooting");
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter >= idleFrames)
            State = GunVoltState.PRE_SHOOTING;
    }

    private void OnPreShooting(EntityState state, long frameCounter)
    {
        if (frameCounter >= PRE_SHOOTING_FRAMES)
        {
            var choice = Engine.RNG.NextUInt(16);
            if (choice % 2 == 0)
            {
                missileShotCounter = 0;
                State = GunVoltState.SHOOTING_MISSILES;
            }
            else
            {
                State = GunVoltState.SHOOTING_SPARKS;
            }
        }
    }

    private void OnShootingMissiles(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAME_TO_SHOOT)
        {
            ShootMissile();
            missileShotCounter++;
        }
        else if (frameCounter >= SHOOTING_FRAMES)
        {
            State = missileShotCounter < MissileShotCount ? GunVoltState.BETWEEN_SHOOTING_MISSILES : GunVoltState.POST_SHOOTING;
        }
    }

    private void OnBetweenShootingMissiles(EntityState state, long frameCounter)
    {
        if (frameCounter >= BETWEEN_SHOOTING_MISSILES_FRAMES)
            State = GunVoltState.SHOOTING_MISSILES;
    }

    private void OnShootingSparks(EntityState state, long frameCounter)
    {
        if (frameCounter == SHOOTING_FRAMES)
            ShootSparks();
        else if (frameCounter >= SHOOTING_FRAMES)
            State = GunVoltState.POST_SHOOTING;
    }

    private void OnPostShooting(EntityState state, long frameCounter)
    {
        if (frameCounter >= POST_SHOOTING_FRAMES)
        {
            var choice = Engine.RNG.NextUInt(16);
            idleFrames = choice % 4 == 0 ? LONG_IDLE_FRAMES : SHORT_IDLE_FRAMES;
            State = GunVoltState.IDLE;
        }
    }

    private void ShootMissile()
    {
        ShootMissile(Origin + ((missileShotCounter % 2 == 0 ? MISSILE1_SPAWN_OFFSET_X : MISSILE2_SPAWN_OFFSET_X) * Direction.GetHorizontalSignal(), MISSILE_SPAWN_OFFSET_Y));

        Engine.PlaySound(4, "Torpedo");
    }

    private void ShootSparks()
    {
        ShootSpark(Origin + (SPARK1_SPAWN_OFFSET_X * Direction.GetHorizontalSignal(), SPARK_SPAWN_OFFSET_Y));
        ShootSpark(Origin + (SPARK2_SPAWN_OFFSET_X * Direction.GetHorizontalSignal(), SPARK_SPAWN_OFFSET_Y));

        Engine.PlaySound(4, "Electric Bolt");
    }

    private void ShootSpark(Vector origin)
    {
        GunVoltSpark spark = Engine.Entities.Create<GunVoltSpark>(new
        {
            Origin = origin,
            Direction
        });

        spark.Spawn();
    }

    private void ShootMissile(Vector origin)
    {
        GunVoltMissile spark = Engine.Entities.Create<GunVoltMissile>(new
        {
            Origin = origin,
            Direction
        });

        spark.Spawn();
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

        missileShotCounter = 0;

        idleFrames = LONG_IDLE_FRAMES;
        State = GunVoltState.IDLE;
    }
}