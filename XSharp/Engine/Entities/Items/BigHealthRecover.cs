using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public enum BigHealthRecoverState
{
    DROPPING = 0,
    IDLE = 1
}

public class BigHealthRecover : Item, IStateEntity<BigHealthRecoverState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }
    #endregion

    public BigHealthRecoverState State
    {
        get => GetState<BigHealthRecoverState>();
        set => SetState(value);
    }

    public BigHealthRecover()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X Weapons";

        SetAnimationNames("BigHealthRecoverDropping", "BigHealthRecoverIdle");

        SetupStateArray<BigHealthRecoverState>();
        RegisterState(BigHealthRecoverState.DROPPING, "BigHealthRecoverDropping");
        RegisterState(BigHealthRecoverState.IDLE, "BigHealthRecoverIdle");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        State = BigHealthRecoverState.DROPPING;
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        State = BigHealthRecoverState.IDLE;
    }

    protected override void OnCollecting(Player player)
    {
        player.Heal(BIG_HEALTH_RECOVER_AMOUNT);
    }
}