using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public class AxeMaxLumberjack : Enemy, IStateEntity<AxeMaxState, AxeMaxSubState>
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

    public AxeMax AxeMax
    {
        get => axeMax;
        internal set => axeMax = value;
    }

    public AxeMaxState State
    {
        get => GetState<AxeMaxState>();
        set => SetState(value);
    }

    public AxeMaxSubState SubState
    {
        get => GetSubState<AxeMaxSubState>();
        set => SetSubState(value);
    }

    public bool Throwing => State == AxeMaxState.THROWING;

    public AxeMaxLumberjack()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";
        Directional = true;
        DefaultDirection = Direction.LEFT;

        SetAnimationNames("Idle", "Laughing", "Throwing");

        SetupStateArray<AxeMaxState>();

        var state = (SpriteState) RegisterState<AxeMaxState, AxeMaxSubState>(AxeMaxState.IDLE, OnStartIdle, OnIdle, null);
        state.RegisterSubState(AxeMaxSubState.NOT_LAUGHING, "Idle");
        state.RegisterSubState(AxeMaxSubState.LAUGHING, "Laughing");

        state = (SpriteState) RegisterState<AxeMaxState, AxeMaxSubState>(AxeMaxState.THROWING, OnThrowing);
        state.RegisterSubState(AxeMaxSubState.NOT_LAUGHING, "Throwing");
        state.RegisterSubState(AxeMaxSubState.LAUGHING, "Throwing");
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        if (lastState != null && (AxeMaxState) lastState.ID == AxeMaxState.THROWING && !AxeMax.TrunkBase.Regenerating && AxeMax.TrunkBase.ThrownTrunkCount < AxeMax.TrunkCount)
            State = AxeMaxState.THROWING;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter >= 20 && AxeMax.TrunkBase.IsReady)
            State = AxeMaxState.THROWING;
    }

    private void OnThrowing(EntityState state, long frameCounter)
    {
        if (frameCounter == 18)
            AxeMax.TrunkBase.ThrowTrunk();
        else if (frameCounter >= 48)
            State = AxeMaxState.IDLE;
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

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = false;
        Direction = AxeMax.Direction;
        Health = HP;
        ContactDamage = CONTACT_DAMAGE;

        SetState(AxeMaxState.IDLE, AxeMaxSubState.NOT_LAUGHING);
    }
}