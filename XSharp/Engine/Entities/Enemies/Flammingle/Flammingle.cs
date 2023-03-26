using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Flammingle;

public enum FlammingleState
{
    IDLE = 0,
    ATTACKING = 1
}

public class Flammingle : Enemy, IStateEntity<FlammingleState>
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFFF0F0F0), // 1
        Color.FromBgra(0xFF989898), // 2
        Color.FromBgra(0xFF505050), // 3
        Color.FromBgra(0xFF9890F0), // 4
        Color.FromBgra(0xFF8078D8), // 5           
        Color.FromBgra(0xFF6058B0), // 6
        Color.FromBgra(0xFF4038A0), // 7
        Color.FromBgra(0xFFF8C0F0), // 8
        Color.FromBgra(0xFFD880B8), // 9
        Color.FromBgra(0xFFB85088), // A
        Color.FromBgra(0xFFF0D888), // B
        Color.FromBgra(0xFFC09860), // C
        Color.FromBgra(0xFF986830), // D
        Color.FromBgra(0xFF68E8A8), // E
        Color.FromBgra(0xFF282828)  // F
    };

    public const int HEALTH = 16;
    public static readonly FixedSingle CONTACT_DAMAGE = 3;
    public static readonly Box HITBOX = ((1, 14), (-5, -22), (5, 22));
    public static readonly Box IDLE_HITBOX = ((1, 0), (-5, -36), (5, 36));

    public static readonly FixedSingle ATTACK_DISTANCE_X = 104;
    public const int ATTACKING_FRAMES = 80;
    public const int FRAME_TO_SHOOT = 38;

    public static readonly FixedSingle SHOT_OFFSET_X = 16;
    public static readonly FixedSingle SHOT_OFFSET_Y = 16;
    public static readonly FixedSingle SHOT_SPEED = 2;
    public static readonly Box SHOT_HITBOX = ((0, 0), (-8, -8), (8, 8));
    public const int SHOT_DAMAGE = 2;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("flamminglePalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Flammingle", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X1.Flammingle.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 32, true);
        sequence.AddFrame(8, 30, 56, 1, 27, 74, 2);
        sequence.AddFrame(8, 30, 106, 1, 27, 74, 4);
        sequence.AddFrame(8, 30, 56, 1, 27, 74, 2);
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 4);
        sequence.AddFrame(8, 30, 56, 1, 27, 74, 2);
        sequence.AddFrame(8, 30, 106, 1, 27, 74, 4);
        sequence.AddFrame(8, 30, 56, 1, 27, 74, 2);
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 16);
        sequence.AddFrame(8, 25, 156, 3, 27, 69, 4);
        sequence.AddFrame(8, 21, 206, 5, 27, 65, 32);
        sequence.AddFrame(8, 25, 156, 3, 27, 69, 4);
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 34);
        sequence.AddFrame(8, 30, 56, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 6, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 306, 1, 27, 74, 32);
        sequence.AddFrame(8, 30, 6, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 56, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 32);
        sequence.AddFrame(8, 30, 56, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 6, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 306, 1, 27, 74, 32);
        sequence.AddFrame(8, 30, 6, 163, 27, 74, 1);
        sequence.AddFrame(8, 30, 56, 163, 27, 74, 1);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 31, 256, 0, 27, 75, 32);
        sequence.AddFrame(8, 30, 6, 1, 27, 74, 4);
        sequence.AddFrame(19, 14, 2, 89, 37, 58, 4); // at second frame (38 frames in total) the shot is spawned
        sequence.AddFrame(24, 7, 50, 92, 39, 51, 4);
        sequence.AddFrame(17, 6, 104, 93, 32, 50, 4);
        sequence.AddFrame(11, -1, 157, 96, 26, 43, 4);
        sequence.AddFrame(10, -1, 201, 96, 37, 43, 4);
        sequence.AddFrame(8, 2, 250, 95, 39, 46, 4);
        sequence.AddFrame(8, 16, 303, 88, 34, 60, 4);
        sequence.AddFrame(8, 22, 356, 85, 27, 66, 4);
        sequence.AddFrame(8, 22, 113, 171, 27, 66, 1);
        sequence.AddFrame(8, 22, 356, 85, 27, 66, 1);
        sequence.AddFrame(8, 22, 160, 171, 27, 66, 1);
        sequence.AddFrame(8, 22, 356, 85, 27, 66, 1);
        sequence.AddFrame(8, 22, 113, 171, 27, 66, 1);
        sequence.AddFrame(8, 22, 356, 85, 27, 66, 1);
        sequence.AddFrame(8, 22, 160, 171, 27, 66, 1);
        sequence.AddFrame(8, 22, 356, 85, 27, 66, 1);
        sequence.AddFrame(8, 27, 406, 82, 27, 71, 4); // total of 80 frames

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(-1, -1, 362, 30, 15, 15, 1, true);
        sequence.AddFrame(-1, -1, 412, 30, 15, 15, 1);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private bool shooting;

    public FlammingleState State
    {
        get => GetState<FlammingleState>();
        set => SetState(value);
    }

    public Flammingle()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "flamminglePalette";
        SpriteSheetName = "Flammingle";

        SetAnimationNames("Idle", "Attacking");

        SetupStateArray<FlammingleState>();
        RegisterState(FlammingleState.IDLE, OnIdle, "Idle");
        RegisterState(FlammingleState.ATTACKING, OnStartAttacking, OnAttacking, null, "Attacking");
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (frameCounter >= 4
            && player != null
            && GetHorizontalDirection(player) == Direction
            && (player.Origin.X - Origin.X).Abs <= ATTACK_DISTANCE_X
            && Hitbox.VerticallInterval.IsOverlaping(player.CollisionBox.VerticallInterval))
            State = FlammingleState.ATTACKING;
    }

    private void OnStartAttacking(EntityState state, EntityState lastState)
    {
        shooting = false;
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        if (frameCounter >= ATTACKING_FRAMES)
            State = FlammingleState.IDLE;
        else if (frameCounter == FRAME_TO_SHOOT)
            Shoot();
    }

    private EntityReference<FlammingleShot> Shoot()
    {
        shooting = true;

        var player = Engine.Player;
        if (player != null)
        {
            var delta = Engine.Player.Origin - Origin;
            FlammingleShot shot = Engine.Entities.Create<FlammingleShot>(new
            {
                Origin = Origin + (SHOT_OFFSET_X * Direction.GetHorizontalSignal(), -SHOT_OFFSET_Y),
                Velocity = SHOT_SPEED * delta.Versor()
            });

            shot.Spawn();
            return shot;
        }

        return null;
    }

    protected override Box GetHitbox()
    {
        return State == FlammingleState.IDLE || State == FlammingleState.ATTACKING && !shooting ? IDLE_HITBOX : HITBOX;
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

        shooting = false;

        State = FlammingleState.IDLE;
    }
}