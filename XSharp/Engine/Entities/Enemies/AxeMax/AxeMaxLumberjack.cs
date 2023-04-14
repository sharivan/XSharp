using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public enum AxeMaxLumberjackState
{
    IDLE = 0,
    LAUGHING = 1,
    THROWING = 2
}

public class AxeMaxLumberjack : Enemy, IFSMEntity<AxeMaxLumberjackState>
{
    #region StaticFields
    public static readonly FixedSingle HP = 16;
    public static readonly FixedSingle CONTACT_DAMAGE = 4;

    public static readonly Box HITBOX = ((-6, 0), (-12, -16), (12, 16));
    public static readonly Box COLLISION_BOX = ((-6, 0), (-12, -16), (12, 16));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 5;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(AxeMax));
    }
    #endregion

    private EntityReference<AxeMax> axeMax;
    private int throwCounter;
    private bool laughOnIdle;

    public AxeMax AxeMax
    {
        get => axeMax;
        internal set => axeMax = value;
    }

    public AxeMaxLumberjackState State
    {
        get => GetState<AxeMaxLumberjackState>();
        set => SetState(value);
    }

    public bool Throwing => State == AxeMaxLumberjackState.THROWING;

    public AxeMaxLumberjack()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";
        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        SetAnimationNames("Idle", "Laughing", "Throwing");

        SetupStateArray<AxeMaxLumberjackState>();
        RegisterState(AxeMaxLumberjackState.IDLE, OnStartIdle, OnIdle, null, "Idle");
        RegisterState(AxeMaxLumberjackState.LAUGHING, OnLaughing, "Laughing");
        RegisterState(AxeMaxLumberjackState.THROWING, OnThrowing, "Throwing");
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        if (lastState != null && (AxeMaxLumberjackState) lastState.ID == AxeMaxLumberjackState.THROWING && throwCounter < AxeMax.TrunkCount)
            State = AxeMaxLumberjackState.THROWING;
        else if (laughOnIdle)
        {
            laughOnIdle = false;
            State = AxeMaxLumberjackState.LAUGHING;
        }
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter >= 50 && AxeMax.TrunkBase.IsReady)
        {
            throwCounter = 0;
            State = AxeMaxLumberjackState.THROWING;
        }
    }

    private void OnLaughing(EntityState state, long frameCounter)
    {
        if (frameCounter >= 122)
            State = AxeMaxLumberjackState.IDLE;
    }

    private void OnThrowing(EntityState state, long frameCounter)
    {
        if (frameCounter == 18)
        {
            throwCounter++;
            AxeMax.TrunkBase.ThrowTrunk();
        }
        else if (frameCounter >= 48)
            State = AxeMaxLumberjackState.IDLE;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return COLLISION_BOX_LEGS_HEIGHT;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = false;
        Direction = AxeMax.Direction;
        Health = HP;
        ContactDamage = CONTACT_DAMAGE;

        throwCounter = 0;
        laughOnIdle = false;

        State = AxeMaxLumberjackState.IDLE;
    }

    internal void MakeLaughing()
    {
        laughOnIdle = true;
    }
}