using System.Reflection;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities.Enemies.TurnCannon;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Objects;

public enum HoverPlatformState
{
    MOVING = 0,
    TURNING = 1
}

public class HoverPlatform : Sprite, IStateEntity<HoverPlatformState>, IControllable
{
    #region StaticFields
    public static readonly Box HITBOX = ((0, 0), (-16, -11), (16, 11));
    public static readonly Box COLLISION_BOX = ((0, 0), (-16, -4), (16, 4));
    public static readonly FixedSingle SPEED = 384 / 256.0;
    public const int FRAMES_TO_MOVE_AFTER_TURNING = 66;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction<TurnCannon>();

        var spriteSheet = Engine.CreateSpriteSheet("HoverPlatform", true, true);
        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Objects.Hover Platform.png");

        var sequence = spriteSheet.AddFrameSquence("Moving");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, 0, 4, 13, 32, 22, 2, true);
        sequence.AddFrame(0, 0, 54, 14, 32, 22, 1);
        sequence.AddFrame(0, 0, 104, 13, 32, 22, 2);
        sequence.AddFrame(0, 0, 154, 14, 32, 22, 1);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<TurnCannon> turnCannon;
    private HoverPlatformState lastState;

    public HoverPlatformState State
    {
        get => GetState<HoverPlatformState>();
        set => SetState(value);
    }

    public bool HasTurnCannon
    {
        get;
        set;
    } = false;

    public bool Paused
    {
        get;
        set;
    } = false;

    public TurnCannon TurnCannon => turnCannon;

    public HoverPlatform()
    {
    }

    private void CheckTurnCannon()
    {
        if (!HasTurnCannon && turnCannon is not null)
        {
            TurnCannon?.Kill();
            turnCannon = null;
        }
        else if (HasTurnCannon && turnCannon is null)
        {
            turnCannon = Engine.Entities.Create<TurnCannon>(new
            {
                Origin = Origin - (0, 17),
                Parent = this
            });
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "HoverPlatform";
        KillOnOffscreen = true;
        MirrorAnimationFromDirection = false;

        SetAnimationNames("Moving");

        SetupStateArray<HoverPlatformState>();
        RegisterState(HoverPlatformState.MOVING, OnMoving, "Moving");
        RegisterState(HoverPlatformState.TURNING, OnTurning, "Moving");

        CheckTurnCannon();
    }

    private void OnMoving(EntityState state, long frameCounter)
    {
        if (Direction == Direction.LEFT && BlockedLeft || Direction == Direction.RIGHT && BlockedRight)
        {
            Velocity = Vector.NULL_VECTOR;
            State = HoverPlatformState.TURNING;
        }
        else
        {
            Velocity = Paused ? Vector.NULL_VECTOR : (SPEED * Direction.GetHorizontalSignal(), 0);
        }
    }

    private void OnTurning(EntityState state, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;

        if (frameCounter >= FRAMES_TO_MOVE_AFTER_TURNING)
        {
            Direction = Direction.Oposite();
            State = HoverPlatformState.MOVING;
        }
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Resume()
    {
        Paused = false;
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

        CheckCollisionWithWorld = true;
        CollisionData = CollisionData.SOLID;

        CheckTurnCannon();
        TurnCannon?.Spawn();

        State = HoverPlatformState.MOVING;
    }

    protected override void OnDeath()
    {
        TurnCannon?.Kill();
        turnCannon = null;

        base.OnDeath();
    }
}