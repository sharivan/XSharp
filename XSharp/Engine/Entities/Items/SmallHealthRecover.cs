using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public enum SmallHealthRecoverState
{
    DROPPING = 0,
    IDLE = 1
}

public class SmallHealthRecover : Item, IFSMEntity<SmallHealthRecoverState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }
    #endregion

    public SmallHealthRecoverState State
    {
        get => GetState<SmallHealthRecoverState>();
        set => SetState(value);
    }

    public SmallHealthRecover()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X Weapons";

        SetAnimationNames("SmallHealthRecoverDropping", "SmallHealthRecoverIdle");

        SetupStateArray<SmallHealthRecoverState>();
        RegisterState(SmallHealthRecoverState.DROPPING, "SmallHealthRecoverDropping");
        RegisterState(SmallHealthRecoverState.IDLE, "SmallHealthRecoverIdle");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        State = SmallHealthRecoverState.DROPPING;
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        State = SmallHealthRecoverState.IDLE;
    }

    protected override void OnCollecting(Player player)
    {
        player.Heal(SMALL_HEALTH_RECOVER_AMOUNT);
    }
}