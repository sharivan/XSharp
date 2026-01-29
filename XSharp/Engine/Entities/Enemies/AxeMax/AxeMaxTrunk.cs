using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public enum AxeMaxTrunkState
{
    IDLE = 0,
    RISING = 1,
    THROWN = 2
}

public class AxeMaxTrunk : Sprite, IFSMEntity<AxeMaxTrunkState>
{
    #region StaticFields
    public static readonly Box IDLE_HITBOX = ((0, 0), (-13, -11), (13, 5));

    public static readonly FixedSingle SPEED = 768 / 256.0;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction<AxeMax>();
    }
    #endregion

    private EntityReference<AxeMaxTrunkBase> trunkBase;
    private EntityReference<AxeMaxTrunkHurtbox> hurtbox;

    public AxeMaxTrunkBase TrunkBase
    {
        get => trunkBase;
        internal set => trunkBase = value;
    }

    public AxeMaxTrunkHurtbox Hurtbox => hurtbox;

    public AxeMaxTrunkState State
    {
        get => GetState<AxeMaxTrunkState>();
        set => SetState(value);
    }

    public int TrunkIndex
    {
        get;
        internal set;
    }

    public bool Rising => State == AxeMaxTrunkState.RISING;

    public bool Idle => State == AxeMaxTrunkState.IDLE;

    public bool Thrown => State == AxeMaxTrunkState.THROWN;

    public bool Ready => Idle && PixelOrigin.Y == TrunkBase.GetTrunkPositionFromIndex(TrunkIndex).Y && Landed;

    public AxeMaxTrunk()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";

        SetAnimationNames("TrunkIdle", "TrunkThrown");

        SetupStateArray<AxeMaxTrunkState>();
        RegisterState(AxeMaxTrunkState.IDLE, OnStartIdle, OnIdle, null, "TrunkIdle");
        RegisterState(AxeMaxTrunkState.RISING, OnRising, "TrunkIdle");
        RegisterState(AxeMaxTrunkState.THROWN, OnStartThrown, OnThrown, null, "TrunkThrown");

        hurtbox = Engine.Entities.Create<AxeMaxTrunkHurtbox>();
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        Hurtbox.Origin = Origin;
        Hurtbox.Trunk = this;
        Hurtbox.Spawn();

        TrunkBase?.NotifyTrunkReady(this);
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        var origin = PixelOrigin;
        var targetOrigin = TrunkBase.GetTrunkPositionFromIndex(TrunkIndex);

        if (TrunkBase.Landed && (TrunkBase.AxeMax.Lumberjack == null || !TrunkBase.AxeMax.Lumberjack.Throwing) && origin.Y > targetOrigin.Y)
            State = AxeMaxTrunkState.RISING;
    }

    private void OnRising(EntityState state, long frameCounter)
    {
        var origin = PixelOrigin;
        var targetOrigin = TrunkBase.GetTrunkPositionFromIndex(TrunkIndex);

        if (origin.Y <= targetOrigin.Y)
            State = AxeMaxTrunkState.IDLE;
        else
            Origin += Vector.UP_VECTOR;
    }

    private void OnStartThrown(EntityState state, EntityState lastState)
    {
        if (!Alive || MarkedToRemove)
            return;

        TrunkBase.ThrownTrunkCount++;
        Velocity = (Direction == Direction.LEFT ? -SPEED : SPEED, 0);
        Hurtbox.ContactDamage = AxeMaxTrunkHurtbox.CONTACT_DAMAGE;
    }

    private void OnThrown(EntityState state, long frameCounter)
    {
    }

    protected override Box GetHitbox()
    {
        return IDLE_HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return Idle ? base.GetGravity() : 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        CheckCollisionWithSolidSprites = true;
        CanBeCarriedWhenLanded = false;
        CanBePushedHorizontally = false;
        CanBePushedVertically = false;
        AutoAdjustOnTheFloor = false;
        CollisionData = CollisionData.SOLID;

        SendToBack();

        if (TrunkBase.FirstRegenerating)
        {
            State = AxeMaxTrunkState.IDLE;
        }
        else
        {
            Origin = (Origin.X, Origin.Y + 16);
            State = AxeMaxTrunkState.RISING;
        }
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (TrunkIndex >= 0)
        {
            if (Ready)
                TrunkBase.readyTrunks.Set(TrunkIndex);
            else
                TrunkBase.readyTrunks.Reset(TrunkIndex);
        }
    }

    protected override void OnDeath()
    {
        Hurtbox.Trunk = null;
        Hurtbox.Kill();

        TrunkIndex = -1;
        TrunkBase?.NotifyTrunkDeath(this);

        base.OnDeath();
    }

    public void Throw(Direction direction)
    {
        if (State == AxeMaxTrunkState.IDLE)
        {
            CheckCollisionWithSolidSprites = false;
            Direction = direction;
            State = AxeMaxTrunkState.THROWN;
        }
    }
}