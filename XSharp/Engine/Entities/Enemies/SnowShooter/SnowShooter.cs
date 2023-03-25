using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.SnowShooter;

public enum SnowShooterState
{
    IDLE = 0,
    SHOOTING = 1
}

public class SnowShooter : Enemy, IStateEntity<SnowShooterState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF8088C0), // 1
        Color.FromBgra(0xFF98A8D8), // 2
        Color.FromBgra(0xFFE0E0F8), // 3
        Color.FromBgra(0xFF188040), // 4
        Color.FromBgra(0xFF18B068), // 5           
        Color.FromBgra(0xFF70F080), // 6
        Color.FromBgra(0xFF185060), // 7
        Color.FromBgra(0xFFB86810), // 8
        Color.FromBgra(0xFFE89010), // 9
        Color.FromBgra(0xFFF8C810), // A
        Color.FromBgra(0xFFF8F8F8), // B
        Color.FromBgra(0xFFF870A0), // C
        Color.FromBgra(0xFFE01018), // D
        Color.FromBgra(0xFF801020), // E
        Color.FromBgra(0xFF183028)  // F
    };

    public const int HEALTH = 8;
    public static readonly FixedSingle CONTACT_DAMAGE = 3;
    public static readonly Box HITBOX = ((0, 32), (-7, -17), (7, 17));
    
    public static readonly FixedSingle SHOT_SPEED = 1024 / 256.0;
    public static readonly FixedSingle SHOT_MAX_SPEED_Y = 614 / 256.0;
    public const int FRAMES_BETWEEN_SHOOTS = 128;
    public const int FRAMES_TO_SHOT = 40;
    public static readonly FixedSingle SHOT_DAMAGE = 2;
    public static readonly Box SHOT_HITBOX = ((0, 0), (-5, -5), (5, 5));

    public static readonly FixedSingle ATTACK_DISTANCE_X = 150;
    public static readonly RightTriangle LEFT_SHOT_SENSOR = new((-ATTACK_DISTANCE_X, 0), ATTACK_DISTANCE_X, ATTACK_DISTANCE_X);
    public static readonly RightTriangle RIGHT_SHOT_SENSOR = new((ATTACK_DISTANCE_X, 0), -ATTACK_DISTANCE_X, ATTACK_DISTANCE_X);
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.PrecacheSound("Beam Pulse", @"resources\sounds\mmx\42 - MMX - Beam Pulse.wav");

        var palette = Engine.PrecachePalette("snowShooterPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("SnowShooter", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X.Snow Shooter.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            spriteSheet.CurrentTexture = texture;
        }

        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(3, 8, 309, 5, 21, 43, 1, true);

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(1, 5, 10, 5, 19, 43, 9);
        sequence.AddFrame(1, 9, 61, 3, 18, 47, 8);
        sequence.AddFrame(1, 13, 104, 2, 32, 50, 8);
        sequence.AddFrame(1, 15, 152, 0, 36, 50, 5);
        sequence.AddFrame(1, 10, 202, 4, 35, 45, 6);
        sequence.AddFrame(1, 18, 256, 0, 27, 53, 4); // total of 40 frames

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(-1, -1, 365, 17, 8, 8, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public SnowShooterState State
    {
        get => GetState<SnowShooterState>();
        set => SetState(value);
    }

    public SnowShooter()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;

        PaletteName = "snowShooterPalette";
        SpriteSheetName = "SnowShooter";

        SetAnimationNames("Idle", "Shooting");

        SetupStateArray<SnowShooterState>();
        RegisterState(SnowShooterState.IDLE, OnIdle, "Idle");
        RegisterState(SnowShooterState.SHOOTING, OnShooting, "Shooting");
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player != null)
        {
            if (GetHorizontalDirection(player) == Direction)
                Direction = Direction.Oposite();

            if (frameCounter >= FRAMES_BETWEEN_SHOOTS)
            {
                var sensor = Origin + (Direction == Direction.LEFT ? LEFT_SHOT_SENSOR : RIGHT_SHOT_SENSOR);
                if (sensor.HasIntersectionWith(player.CollisionBox))
                    State = SnowShooterState.SHOOTING;
            }
        }
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
        if (frameCounter >= FRAMES_TO_SHOT)
        {
            Shoot();
            State = SnowShooterState.IDLE;
        }
    }

    private EntityReference<SnowShooterShot> Shoot()
    {
        FixedSingle vx;
        FixedSingle vy;

        var player = Engine.Player;
        if (player == null)
        {
            vx = SHOT_SPEED * Direction.GetHorizontalSignal();
            vy = 0;
        }
        else
        {
            var delta = player.Origin - Origin;
            var velocity = SHOT_SPEED * delta.Versor();

            var velocityLengthSquare = velocity.GetLengthSquare();
            if (velocity.Y.Abs > SHOT_MAX_SPEED_Y)
            {
                vy = SHOT_MAX_SPEED_Y * velocity.Y.Signal;
                vx = System.Math.Sqrt(velocityLengthSquare - (FixedDouble) vy * vy) * delta.X.Signal;
            }
            else
            {
                vy = velocity.Y;
                vx = velocity.X;
            }
        }

        SnowShooterShot shot = Engine.Entities.Create<SnowShooterShot>(new
        {
            Origin,
            Velocity = (vx, vy)
        });

        Engine.PlaySound(5, "Beam Pulse");

        shot.Spawn();
        return shot;
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

        State = SnowShooterState.IDLE;
    }
}