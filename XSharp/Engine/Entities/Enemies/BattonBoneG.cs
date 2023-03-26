using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies;

public enum BattonBoneGState
{
    IDLE = 0,
    ATTACKING = 1,
    ESCAPING = 2
}

public class BattonBoneG : Enemy, IStateEntity<BattonBoneGState>
{
    #region StaticFields
    public static readonly FixedSingle BATTON_BONE_G_ATTACK_SPEED = 256 / 256.0;
    public static readonly FixedSingle BATTON_BONE_G_ESCAPE_SPEED = 512 / 256.0;
    public const int BATTON_BONE_G_HEALTH = 2;
    public static readonly FixedSingle BATTON_BONE_G_CONTACT_DAMAGE = 1;

    public static readonly Color[] BATTON_BONE_G_PALETTE = new Color[]
    {
        Color.Transparent, // 0
        new Color(64, 136, 64, 255), // 1
        new Color(248, 192, 240, 255), // 2
        new Color(240, 48, 80, 255), // 3
        new Color(224, 216, 128, 255), // 4
        new Color(200, 160, 80, 255), // 5           
        new Color(152, 112, 48, 255), // 6
        new Color(120, 72, 48, 255), // 7
        new Color(104, 232, 168, 255), // 8
        new Color(96, 72, 128, 255), // 9
        new Color(136, 104, 184, 255), // 10
        new Color(168, 136, 224, 255), // 11
        new Color(240, 240, 240, 255), // 12
        new Color(160, 160, 160, 255), // 13
        new Color(104, 104, 104, 255), // 14
        new Color(40, 40, 40, 255), // 15
    };
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("battonBoneGPalette", BATTON_BONE_G_PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("BattonBoneG", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.X2.batton-bone-g.png");
        spriteSheet.CurrentPalette = palette;

        var battonBoneGIdleHitbox = new Box(Vector.NULL_VECTOR, new Vector(-6, -18), new Vector(6, 0));
        var battonBoneGAttackingHitbox = new Box(Vector.NULL_VECTOR, new Vector(-8, -14), new Vector(8, 0));

        // 0
        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -battonBoneGIdleHitbox.Mins;
        sequence.Hitbox = battonBoneGIdleHitbox;
        sequence.AddFrame(0, 4, 7, 1, 14, 23, 1, true);

        // 1
        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -battonBoneGAttackingHitbox.Mins;
        sequence.Hitbox = battonBoneGAttackingHitbox;
        sequence.AddFrame(4, 7, 22, 1, 30, 23, 1, true);
        sequence.AddFrame(10, 8, 53, 1, 39, 23, 3);
        sequence.AddFrame(5, 6, 93, 1, 29, 23, 3);
        sequence.AddFrame(3, 2, 123, 1, 23, 23, 3);
        sequence.AddFrame(3, 5, 147, 1, 23, 23, 4);
        sequence.AddFrame(4, 7, 22, 1, 30, 23, 3);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public BattonBoneGState State
    {
        get => GetState<BattonBoneGState>();
        set
        {
            CheckCollisionWithWorld = value == BattonBoneGState.ESCAPING;
            SetState(value);
        }
    }

    public BattonBoneG()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "BattonBoneG";

        SetAnimationNames("Idle", "Attacking");

        SetupStateArray<BattonBoneGState>();
        RegisterState(BattonBoneGState.IDLE, OnIdle, "Idle");
        RegisterState(BattonBoneGState.ATTACKING, OnAttacking, "Attacking");
        RegisterState(BattonBoneGState.ESCAPING, OnEscaping, "Attacking");
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;

        PaletteName = "battonBoneGPalette";
        Health = BATTON_BONE_G_HEALTH;
        ContactDamage = BATTON_BONE_G_CONTACT_DAMAGE;

        NothingDropOdd = 9000; // 90%
        SmallHealthDropOdd = 300; // 3%
        BigHealthDropOdd = 100; // 1%
        SmallAmmoDropOdd = 400; // 4%
        BigAmmoDropOdd = 175; // 1.75%
        LifeUpDropOdd = 25; // 0.25%

        State = BattonBoneGState.IDLE;
    }

    protected override void OnHurt(Sprite victim, FixedSingle damage)
    {
        Velocity = BATTON_BONE_G_ESCAPE_SPEED * Vector.UP_VECTOR;
        State = BattonBoneGState.ESCAPING;
    }

    protected override void OnStopMoving()
    {
        base.OnStopMoving();

        if (State == BattonBoneGState.ESCAPING)
        {
            Velocity = Vector.NULL_VECTOR;
            State = BattonBoneGState.IDLE;
        }
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (Engine.Player != null && frameCounter >= 60 && Origin.DistanceTo(Engine.Player.Origin, Metric.MAX) <= SCENE_SIZE * 0.5)
            State = BattonBoneGState.ATTACKING;
        else
            Velocity = Vector.NULL_VECTOR;
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        if (Engine.Player != null)
        {
            var delta = Engine.Player.Origin - Origin;
            Velocity = BATTON_BONE_G_ATTACK_SPEED * delta.Versor();
        }
        else
        {
            State = BattonBoneGState.ESCAPING;
        }
    }

    private void OnEscaping(EntityState state, long frameCounter)
    {
        if (BlockedUp)
        {
            Velocity = Vector.NULL_VECTOR;
            State = BattonBoneGState.IDLE;
        }
        else
        {
            Velocity = BATTON_BONE_G_ESCAPE_SPEED * Vector.UP_VECTOR;
        }
    }
}