using XSharp.Engine.Entities.Effects;
using XSharp.Geometry;

namespace XSharp.Engine.Entities.Objects
{
    internal class BossDoorEffect : SpriteEffect
    {
        public BossDoor Door
        {
            get;
            internal set;
        }

        public BossDoorState State
        {
            get => GetState<BossDoorState>();
            set => SetState(value);
        }

        public BossDoorEffect()
        {
            SpriteSheetIndex = 9;
            Directional = false;

            SetAnimationNames("Closed", "Opening", "PlayerCrossing", "Closing");

            SetupStateArray(typeof(BossDoorState));
            RegisterState(BossDoorState.CLOSED, OnStartClosed, null, null, "Closed");
            RegisterState(BossDoorState.OPENING, OnStartOpening, OnOpening, null, "Opening");
            RegisterState(BossDoorState.PLAYER_CROSSING, OnStartPlayerCrossing, OnPlayerCrossing, null, "PlayerCrossing");
            RegisterState(BossDoorState.CLOSING, OnStartClosing, null, OnEndClosing, "Closing");
        }

        private void OnStartClosed(EntityState state)
        {
            Door?.OnStartClosed();
        }

        private void OnStartOpening(EntityState state)
        {
            Door.OnStartOpening();
        }

        private void OnOpening(EntityState state, long frameCounter)
        {
            Door?.OnOpening(frameCounter);
        }

        private void OnStartPlayerCrossing(EntityState state)
        {
            Door?.OnStartPlayerCrossing();
        }

        private void OnPlayerCrossing(EntityState state, long frameCounter)
        {
            Door?.OnPlayerCrossing(frameCounter);
        }

        private void OnStartClosing(EntityState state)
        {
            Door?.OnStartClosing();
        }

        private void OnEndClosing(EntityState state)
        {
            Door.OnEndClosing();
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            switch (State)
            {
                case BossDoorState.OPENING when animation.FrameSequenceName == "Opening":
                    State = BossDoorState.PLAYER_CROSSING;
                    break;

                case BossDoorState.CLOSING when animation.FrameSequenceName == "Closing":
                    State = BossDoorState.CLOSED;
                    break;
            }
        }
    }
}