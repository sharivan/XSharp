using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.MetallC15;

public enum MetallC15State
{
    HIDDEN,
    CHASING,
    SHOOTING,
    HIDDING
}

public class MetallC15 : Enemy, IFSMEntity<MetallC15State>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFF004000), // 1
        Color.FromBgra(0xFF006020), // 2
        Color.FromBgra(0xFF208040), // 3
        Color.FromBgra(0xFF702008), // 4
        Color.FromBgra(0xFFB85828), // 5           
        Color.FromBgra(0xFFE8A840), // 6
        Color.FromBgra(0xFFA81030), // 7
        Color.FromBgra(0xFFD02040), // 8
        Color.FromBgra(0xFFF06898), // 9
        Color.FromBgra(0xFF685858), // A
        Color.FromBgra(0xFFA09090), // B
        Color.FromBgra(0xFFD0D0D0), // C
        Color.FromBgra(0xFF387038), // D
        Color.FromBgra(0xFF281010), // E
        Color.FromBgra(0xFF382020)  // F
    };

    public const int HEALTH = 1;
    public static readonly FixedSingle CONTACT_DAMAGE = 2;
    public static readonly Box HIDDEN_HITBOX = ((0, -1), (-7, -6), (7, 6));
    public static readonly Box SHOWING_HITBOX = ((1, -5), (-8, -9), (8, 9));
    public static readonly Box COLLISION_BOX = ((0, -5), (-10, -10), (10, 10));

    public static readonly FixedSingle ATTACK_DISTANCE = 80;

    public const int FRAMES_TO_SHOW = 28;
    public const int FRAMES_TO_SHOOT = 25;
    public const int FRAMES_TO_SHOW_BEFORE_SHOOTING = 120;
    public const int PRE_SHOOTING_FRAMES = 4;
    public const int SHOOTING_FRAMES = 32;
    public const int PRE_CHASING_FRAMES = 4;
    public const int FRAMES_TO_START_CHASING = 29;
    public const int PRE_HIDDING_FRAMES = 4;
    public const int HIDDING_FRAMES = 12;

    public static readonly FixedSingle CHASING_SPEED = 448 / 256.0;

    public static readonly FixedSingle SHOT_SPEED = 2;
    public static readonly FixedSingle SHOT_OFFSET_X = 3;
    public static readonly FixedSingle SHOT_OFFSET_Y = 1;
    public static readonly Box SHOT_HITBOX = ((0, 0), (-3, -3), (3, 3));
    public static readonly FixedSingle SHOT_DAMAGE = 1;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("MetallC15Palette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("MetallC15", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Metall C-15.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Hidden");
        sequence.OriginOffset = -HIDDEN_HITBOX.Origin - HIDDEN_HITBOX.Mins;
        sequence.Hitbox = HIDDEN_HITBOX;
        sequence.AddFrame(3, 0, 0, 0, 21, 12, 1, true);

        sequence = spriteSheet.AddFrameSquence("Chasing");
        sequence.OriginOffset = -HIDDEN_HITBOX.Origin - HIDDEN_HITBOX.Mins;
        sequence.Hitbox = HIDDEN_HITBOX;
        sequence.AddFrame(3, 0, 0, 0, 21, 12, 4);
        sequence.OriginOffset = -SHOWING_HITBOX.Origin - SHOWING_HITBOX.Mins;
        sequence.Hitbox = SHOWING_HITBOX;
        sequence.AddFrame(3, -7, 0, 0, 21, 12, 1);
        sequence.AddFrame(3, -5, 21, 0, 21, 14, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 3);
        sequence.AddFrame(3, 2, 63, 0, 21, 21, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 3);
        sequence.AddFrame(3, 2, 63, 0, 21, 21, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 8);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 4, true); // start chasing from here
        sequence.AddFrame(3, 2, 0, 21, 21, 21, 4);
        sequence.AddFrame(3, 0, 22, 21, 21, 20, 4);
        sequence.AddFrame(3, 2, 43, 21, 22, 21, 4);

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HIDDEN_HITBOX.Origin - HIDDEN_HITBOX.Mins;
        sequence.Hitbox = HIDDEN_HITBOX;
        sequence.AddFrame(3, 0, 0, 0, 21, 12, 4);
        sequence.OriginOffset = -SHOWING_HITBOX.Origin - SHOWING_HITBOX.Mins;
        sequence.Hitbox = SHOWING_HITBOX;
        sequence.AddFrame(3, -7, 0, 0, 21, 12, 1);
        sequence.AddFrame(3, -5, 21, 0, 21, 14, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 3);
        sequence.AddFrame(3, 2, 63, 0, 21, 21, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 3);
        sequence.AddFrame(3, 2, 63, 0, 21, 21, 3);
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 12); // shot spawns at frame 9 from here, total of 28 frames       

        sequence = spriteSheet.AddFrameSquence("Hidding");
        sequence.OriginOffset = -SHOWING_HITBOX.Origin - SHOWING_HITBOX.Mins;
        sequence.Hitbox = SHOWING_HITBOX;
        sequence.AddFrame(3, 1, 42, 0, 21, 20, 4);
        sequence.OriginOffset = -HIDDEN_HITBOX.Origin - HIDDEN_HITBOX.Mins;
        sequence.Hitbox = HIDDEN_HITBOX;
        sequence.AddFrame(3, 8, 42, 0, 21, 20, 1);
        sequence.AddFrame(3, 2, 21, 0, 21, 14, 3);
        sequence.AddFrame(3, 0, 0, 0, 21, 12, 2);
        sequence.AddFrame(3, 2, 21, 0, 21, 14, 2); // total of 12 frames

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(1, 1, 120, 0, 8, 8, 1, true);
        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private bool shotAlive;
    private bool preAction;

    public MetallC15State State
    {
        get => GetState<MetallC15State>();
        set => SetState(value);
    }

    public MetallC15()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.RIGHT;
        SpawnFacedToPlayer = true;
        AlwaysFaceToPlayer = true;

        PaletteName = "MetallC15Palette";
        SpriteSheetName = "MetallC15";

        SetAnimationNames("Hidden", "Chasing", "Shooting", "Hidding");

        SetupStateArray<MetallC15State>();
        RegisterState(MetallC15State.HIDDEN, OnStartHidden, OnHidden, null, "Hidden");
        RegisterState(MetallC15State.CHASING, OnStartChasing, OnChasing, null, "Chasing");
        RegisterState(MetallC15State.SHOOTING, OnStartShooting, OnShooting, null, "Shooting");
        RegisterState(MetallC15State.HIDDING, OnStartHidding, OnHidding, null, "Hidding");
    }

    private void OnStartHidden(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.REFLECT;
    }

    private void OnHidden(EntityState state, long frameCounter)
    {
        Velocity = Velocity.YVector;

        var player = Engine.Player;
        if (player == null)
            return;

        if (player.Direction == Direction)
        {
            if (frameCounter >= PRE_CHASING_FRAMES)
                State = MetallC15State.CHASING;
        }
        else if (!shotAlive && frameCounter >= FRAMES_TO_SHOW_BEFORE_SHOOTING)
        {
            var distance = Origin.DistanceTo(player.Origin, Metric.MAX);
            if (distance <= ATTACK_DISTANCE)
                State = MetallC15State.SHOOTING;
        }
    }

    private void OnStartChasing(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.ACCEPT;
        preAction = true;
    }

    private void OnChasing(EntityState state, long frameCounter)
    {
        preAction = frameCounter < PRE_CHASING_FRAMES;

        var player = Engine.Player;
        if (player == null)
        {
            State = MetallC15State.HIDDING;
            return;
        }

        if (player.Direction != Direction)
            State = MetallC15State.HIDDING;
        else if (frameCounter < FRAMES_TO_START_CHASING)
            Velocity = Velocity.YVector;
        else
            Velocity = (CHASING_SPEED * Direction.GetHorizontalSignal(), Velocity.Y);
    }

    private void OnStartShooting(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.ACCEPT;
        preAction = true;
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
        preAction = frameCounter < PRE_CHASING_FRAMES;

        Velocity = Velocity.YVector;

        if (frameCounter == FRAMES_TO_SHOOT)
            Shoot();
        else if (frameCounter >= SHOOTING_FRAMES)
            State = MetallC15State.HIDDING;
    }

    private void OnStartHidding(EntityState state, EntityState lastState)
    {
        HitResponse = HitResponse.ACCEPT;
        preAction = true;
    }

    private void OnHidding(EntityState state, long frameCounter)
    {
        preAction = frameCounter < PRE_CHASING_FRAMES;

        Velocity = Velocity.YVector;

        if (frameCounter >= HIDDING_FRAMES)
        {
            preAction = false;
            State = MetallC15State.HIDDEN;
        }
    }

    private EntityReference<MetallC15Shot> Shoot()
    {
        var player = Engine.Player;
        if (player == null)
            return null;

        var signal = Direction.GetHorizontalSignal() * DefaultDirection.GetHorizontalSignal();
        var shotOrigin = Origin + (SHOT_OFFSET_X * signal, SHOT_OFFSET_Y);
        var delta = player.Origin - shotOrigin;
        var velocity = (SHOT_SPEED * delta.Versor()).TruncFracPart();

        MetallC15Shot shot = Engine.Entities.Create<MetallC15Shot>(new
        {
            Origin = shotOrigin,
            Velocity = velocity,
            Direction
        });

        shotAlive = true;
        shot.shooter = this;
        shot.Spawn();
        return shot;
    }

    protected override Box GetHitbox()
    {
        return State switch
        {
            MetallC15State.HIDDEN => HIDDEN_HITBOX,
            MetallC15State.HIDDING => preAction ? SHOWING_HITBOX : HIDDEN_HITBOX,
            MetallC15State.CHASING or MetallC15State.SHOOTING => preAction ? HIDDEN_HITBOX : SHOWING_HITBOX,
            _ => Box.EMPTY_BOX,
        };
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

        shotAlive = false;

        State = MetallC15State.HIDDEN;
    }

    internal void NotifyShotDeath()
    {
        shotAlive = false;
    }
}