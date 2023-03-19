using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public enum AxeMaxTrunkState
{
    IDLE = 0,
    RISING = 1,   
    THROWN = 2
}

public class AxeMaxTrunk : Sprite, IStateEntity<AxeMaxTrunkState>
{
    #region StaticFields
    public static readonly Box IDLE_HITBOX = ((0, 0), (-13, -11), (13, 5));

    public static readonly FixedSingle SPEED = 768 / 256.0;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(AxeMax));
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

    public AxeMaxTrunk()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";
        Directional = true;

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
        if (Landed && (TrunkBase.AxeMax.Lumberjack == null || !TrunkBase.AxeMax.Lumberjack.Throwing) && IntegerOrigin != TrunkBase.GetTrunkPositionFromIndex(TrunkIndex))
            State = AxeMaxTrunkState.RISING;
    }

    private void OnRising(EntityState state, long frameCounter)
    {
        var origin = IntegerOrigin;
        var targetOrigin = TrunkBase.GetTrunkPositionFromIndex(TrunkIndex);

        if (origin == targetOrigin)
            State = AxeMaxTrunkState.IDLE;
        else if (origin.Y > targetOrigin.Y)
            Origin += Vector.UP_VECTOR;
        else if (origin.Y < targetOrigin.Y)
            Origin += Vector.DOWN_VECTOR;
    }

    private void OnStartThrown(EntityState state, EntityState lastState)
    {
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

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        CheckCollisionWithSolidSprites = true;
        CanBeCarriedWhenLanded = false;
        CanBePushedHorizontally = false;
        CanBePushedVertically = false;
        AutoAdjustOnTheFloor = false;
        CollisionData = CollisionData.SOLID;

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

    protected override void OnDeath()
    {
        Hurtbox.Trunk = null;
        Hurtbox.Kill();

        TrunkBase?.NotifyTrunkDeath(this);

        base.OnDeath();
    }

    public void Throw(Direction direction)
    {
        if (State == AxeMaxTrunkState.IDLE)
        {
            Direction = direction;
            State = AxeMaxTrunkState.THROWN;
        }
    }
}