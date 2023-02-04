﻿using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public enum SmallHealthRecoverState
    {
        DROPPING = 0,
        IDLE = 1
    }

    public class SmallHealthRecover : Item
    {
        public SmallHealthRecoverState State
        {
            get => GetState<SmallHealthRecoverState>();
            set => SetState(value);
        }

        public SmallHealthRecover(string name, Vector origin, int durationFrames = 0) : base(name, origin, durationFrames, 1, false, "SmallHealthRecoverDropping", "SmallHealthRecoverIdle")
        {
            SetupStateArray(typeof(SmallHealthRecoverState));
            RegisterState(SmallHealthRecoverState.DROPPING, null, "SmallHealthRecoverDropping");
            RegisterState(SmallHealthRecoverState.IDLE, null, "SmallHealthRecoverIdle");
        }

        protected internal override void OnSpawn()
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
}