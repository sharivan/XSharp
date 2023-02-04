using XSharp.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items
{
    public enum BigHealthRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class BigHealthRecover : Item
    {
        public BigHealthRecoverState State
        {
            get => GetState<BigHealthRecoverState>();
            set => SetState(value);
        }

        public BigHealthRecover(string name, Vector origin, int durationFrames = 0)
            : base(name, origin, durationFrames, 1, false, "BigHealthRecoverDropping", "BigHealthRecoverIdle")
        {
            SetupStateArray(typeof(BigHealthRecoverState));
            RegisterState(BigHealthRecoverState.DROPPING, null, "BigHealthRecoverDropping");
            RegisterState(BigHealthRecoverState.IDLE, null, "BigHealthRecoverIdle");
        }

        protected internal override void OnSpawn()
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
}