using MMX.Engine.Entities.Effects;
using MMX.Geometry;

namespace MMX.Engine.Entities.Objects
{
    internal class BossDoorEffect : SpriteEffect
    {
        public BossDoor Door
        {
            get;
        }

        public BossDoorState State
        {
            get => GetState<BossDoorState>();
            set => SetState(value);
        }

        public BossDoorEffect(GameEngine engine, string name, BossDoor door, Vector origin) : base(engine, name, origin, 9, false, "Closed", "Opening", "PlayerCrossing", "Closing")
        {
            Door = door;

            SetupStateArray(typeof(BossDoorState));
            RegisterState(BossDoorState.CLOSED, OnStartClosed, null, null, "Closed");
            RegisterState(BossDoorState.OPENING, OnStartOpening, OnOpening, null, "Opening");
            RegisterState(BossDoorState.PLAYER_CROSSING, OnStartPlayerCrossing, OnPlayerCrossing, null, "PlayerCrossing");
            RegisterState(BossDoorState.CLOSING, OnStartClosing, OnClosing, null, "Closing");
        }

        public BossDoorEffect(GameEngine engine, BossDoor door, Vector origin) : this(engine, engine.GetExclusiveName(nameof(BossDoorEffect)), door, origin)
        {
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

        private void OnClosing(EntityState state, long frameCounter)
        {
            Door?.OnClosing(frameCounter);
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