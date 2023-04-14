using SharpDX;

using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Objects;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.TurnCannon;

public enum TurnCannonState
{
    TURNING_TO_SHOT_HORIZONTALLY = 0,
    SHOOTING_DIAGONALLY = 1,
    TURNING_TO_SHOT_DIAGONALLY = 2,
    SHOOTING_HORIZONTALLY = 3
}

internal enum TurnCannonType
{
    FIXED = 0,
    MOBILE = 1
}

public class TurnCannon : Enemy, IFSMEntity<TurnCannonState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF585868), // 1
        Color.FromBgra(0xFF788098), // 2
        Color.FromBgra(0xFFB0B8D0), // 3
        Color.FromBgra(0xFFD0D0E8), // 4
        Color.FromBgra(0xFFE8B040), // 5           
        Color.FromBgra(0xFFD08838), // 6
        Color.FromBgra(0xFFB87028), // 7
        Color.FromBgra(0xFF784030), // 8
        Color.FromBgra(0xFF4098C0), // 9
        Color.FromBgra(0xFF50F0E0), // A
        Color.FromBgra(0xFFF8F8F8), // B
        Color.FromBgra(0xFF60D060), // C
        Color.FromBgra(0xFF38A050), // D
        Color.FromBgra(0xFF187848), // E
        Color.FromBgra(0xFF302030), // F
    };

    public const int HEALTH = 10;
    public static readonly FixedSingle CONTACT_DAMAGE = 3;
    public static readonly Box HITBOX = ((0, 0), (-14, -11), (14, 11));

    public const int TURNING_FRAMES = 52;
    public const int FRAMES_TO_SHOT = 30;
    public const int SHOOTING_FRAMES = 66;
    public static readonly FixedSingle SHOT_SPEED = 2;
    public static readonly FixedSingle SHOT_DAMAGE = 2;
    public static readonly Box SHOT_HITBOX = ((0, 0), (-5, -5), (5, 5));
    public static readonly FixedSingle DIAGONAL_SHOT_ORIGIN_OFFSET_X = 9;
    public static readonly FixedSingle DIAGONAL_SHOT_ORIGIN_OFFSET_Y = 8;
    public static readonly FixedSingle DIAGONAL_SMOKE_ORIGIN_OFFSET_X = 12;
    public static readonly FixedSingle DIAGONAL_SMOKE_ORIGIN_OFFSET_Y = 11;
    public static readonly FixedSingle HORIZONTAL_SHOT_ORIGIN_OFFSET_X = 16;
    public static readonly FixedSingle HORIZONTAL_SHOT_ORIGIN_OFFSET_Y = 0;
    public static readonly FixedSingle HORIZONTAL_SMOKE_ORIGIN_OFFSET_X = 20;
    public static readonly FixedSingle HORIZONTAL_SMOKE_ORIGIN_OFFSET_Y = 0;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Smoke>();

        var palette = Engine.PrecachePalette("TurnCannonPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("TurnCannon", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Turn Cannon.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("TurningToShotHorizontallyFixed");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 1, true); // total of 52 frames to start to shoot

        sequence = spriteSheet.AddFrameSquence("TurningToShotDiagonallyMobile");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 1, true); // total of 52 frames to start to shoot

        sequence = spriteSheet.AddFrameSquence("ShootingHorizontallyFixed");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 30);
        sequence.AddFrame(2, 0, 32, 44, 32, 22, 6); // shots are spawned here
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 30); // total of 66 frames

        sequence = spriteSheet.AddFrameSquence("ShootingDiagonallyMobile");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 30);
        sequence.AddFrame(2, 0, 0, 66, 32, 22, 6); // shots are spawned here
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 30); // total of 66 frames

        sequence = spriteSheet.AddFrameSquence("TurningToShotDiagonallyFixed");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 0, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 1, true); // total of 52 frames to start to shoot

        sequence = spriteSheet.AddFrameSquence("TurningToShotHorizontallyMobile");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 32, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 64, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 96, 22, 32, 22, 8);
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 1, true); // total of 52 frames to start to shoot

        sequence = spriteSheet.AddFrameSquence("ShootingDiagonallyFixed");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 30);
        sequence.AddFrame(2, 0, 0, 44, 32, 22, 6); // shots are spawned here
        sequence.AddFrame(2, 0, 0, 0, 32, 22, 30); // total of 66 frames

        sequence = spriteSheet.AddFrameSquence("ShootingHorizontallyMobile");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 30);
        sequence.AddFrame(2, 0, 32, 66, 32, 22, 6); // shots are spawned here
        sequence.AddFrame(2, 0, 0, 22, 32, 22, 30); // total of 66 frames

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(-1, -1, 92, 62, 8, 8, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public TurnCannonState State
    {
        get => GetState<TurnCannonState>();
        set => SetState(value);
    }

    public IControllable Base => Parent as IControllable;

    public TurnCannon()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        UpdateOriginFromParentDirection = false;
        UpdateDirectionFromParent = false;
        SpawnFacedToPlayer = false;

        PaletteName = "TurnCannonPalette";
        SpriteSheetName = "TurnCannon";

        SetAnimationNames
        (
            "TurningToShotDiagonallyFixed",
            "TurningToShotDiagonallyMobile",
            "ShootingDiagonallyFixed",
            "ShootingDiagonallyMobile",
            "TurningToShotHorizontallyFixed",
            "TurningToShotHorizontallyMobile",
            "ShootingHorizontallyFixed",
            "ShootingHorizontallyMobile"
        );

        SetupStateArray<TurnCannonState>();
        var state = (SpriteState) RegisterState<TurnCannonState, TurnCannonType>(TurnCannonState.TURNING_TO_SHOT_DIAGONALLY, OnTurningToShotDiagonally);
        state.RegisterSubState(TurnCannonType.FIXED, "TurningToShotDiagonallyFixed");
        state.RegisterSubState(TurnCannonType.MOBILE, "TurningToShotDiagonallyMobile");

        state = (SpriteState) RegisterState<TurnCannonState, TurnCannonType>(TurnCannonState.SHOOTING_DIAGONALLY, OnStartShootingDiagonally, OnShootingDiagonally, OnEndShootingDiagonally);
        state.RegisterSubState(TurnCannonType.FIXED, "ShootingDiagonallyFixed");
        state.RegisterSubState(TurnCannonType.MOBILE, "ShootingDiagonallyMobile");

        state = (SpriteState) RegisterState<TurnCannonState, TurnCannonType>(TurnCannonState.TURNING_TO_SHOT_HORIZONTALLY, OnTurningToShotHorizontally);
        state.RegisterSubState(TurnCannonType.FIXED, "TurningToShotHorizontallyFixed");
        state.RegisterSubState(TurnCannonType.MOBILE, "TurningToShotHorizontallyMobile");

        state = (SpriteState) RegisterState<TurnCannonState, TurnCannonType>(TurnCannonState.SHOOTING_HORIZONTALLY, OnStartShootingHorizontally, OnShootingHorizontally, OnEndShootingHorizontally);
        state.RegisterSubState(TurnCannonType.FIXED, "ShootingHorizontallyFixed");
        state.RegisterSubState(TurnCannonType.MOBILE, "ShootingHorizontallyMobile");
    }

    private void OnTurningToShotDiagonally(EntityState state, long frameCounter)
    {
        if (frameCounter >= TURNING_FRAMES)
            State = TurnCannonState.SHOOTING_DIAGONALLY;
    }

    private void OnStartShootingDiagonally(EntityState state, EntityState lastState)
    {
        Base?.Pause();
    }

    private void OnShootingDiagonally(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAMES_TO_SHOT)
            ShootDiagonally();
        else if (frameCounter >= SHOOTING_FRAMES)
            State = TurnCannonState.TURNING_TO_SHOT_HORIZONTALLY;
    }

    private void OnEndShootingDiagonally(EntityState state)
    {
        Base?.Resume();
    }

    private void OnTurningToShotHorizontally(EntityState state, long frameCounter)
    {
        if (frameCounter >= TURNING_FRAMES)
            State = TurnCannonState.SHOOTING_HORIZONTALLY;
    }

    private void OnStartShootingHorizontally(EntityState state, EntityState lastState)
    {
        Base?.Pause();
    }

    private void OnShootingHorizontally(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAMES_TO_SHOT)
            ShootHorizontally();
        else if (frameCounter >= SHOOTING_FRAMES)
            State = TurnCannonState.TURNING_TO_SHOT_DIAGONALLY;
    }

    private void OnEndShootingHorizontally(EntityState state)
    {
        Base?.Resume();
    }

    private EntityReference<TurnCannonShot> Shoot(Vector origin, Vector velocity)
    {
        TurnCannonShot shot = Engine.Entities.Create<TurnCannonShot>(new
        {
            Origin = origin,
            Velocity = velocity
        });

        shot.Spawn();
        Engine.SpawnSmoke(origin);

        return shot;
    }

    private void ShootDiagonally()
    {
        var speed = SHOT_SPEED * FixedSingle.SQRT_2_INVERSE;

        Shoot(Origin + (DIAGONAL_SHOT_ORIGIN_OFFSET_X, UpsideDown ? DIAGONAL_SHOT_ORIGIN_OFFSET_Y : -DIAGONAL_SHOT_ORIGIN_OFFSET_Y), (speed, UpsideDown ? speed : -speed));
        Shoot(Origin + (-DIAGONAL_SHOT_ORIGIN_OFFSET_X, UpsideDown ? DIAGONAL_SHOT_ORIGIN_OFFSET_Y : -DIAGONAL_SHOT_ORIGIN_OFFSET_Y), (-speed, UpsideDown ? speed : -speed));
    }

    private void ShootHorizontally()
    {
        Shoot(Origin + (HORIZONTAL_SHOT_ORIGIN_OFFSET_X, 0), (SHOT_SPEED, 0));
        Shoot(Origin + (-HORIZONTAL_SHOT_ORIGIN_OFFSET_X, 0), (-SHOT_SPEED, 0));
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Health = HEALTH;
        ContactDamage = CONTACT_DAMAGE;
        CheckCollisionWithSolidSprites = false;
        CheckCollisionWithWorld = false;
        AutoAdjustOnTheFloor = false;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        if (Base != null)
            SetState(TurnCannonState.TURNING_TO_SHOT_HORIZONTALLY, TurnCannonType.MOBILE);
        else
            SetState(TurnCannonState.TURNING_TO_SHOT_DIAGONALLY, TurnCannonType.FIXED);
    }

    protected override void OnDeath()
    {
        if (State is TurnCannonState.SHOOTING_DIAGONALLY or TurnCannonState.SHOOTING_HORIZONTALLY)
            Base?.Resume();

        base.OnDeath();
    }
}