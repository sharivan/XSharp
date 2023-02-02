using MMX.Geometry;

namespace MMX.Engine.Entities.Triggers
{
    public class CheckpointTriggerOnce : Trigger
    {
        public Checkpoint Checkpoint
        {
            get;
            set;
        }

        public bool Triggered
        {
            get;
            private set;
        }

        public Checkpoint LastCheckpoint
        {
            get;
            private set;
        }

        public Box LastCameraConstraintBox
        {
            get;
            private set;
        }

        public int LastObjectTile
        {
            get;
            private set;
        }

        public int LastBackgroundTile
        {
            get;
            private set;
        }

        public int LastPalette
        {
            get;
            private set;
        }

        public CheckpointTriggerOnce(GameEngine engine, Box box, Checkpoint checkpoint) :
            base(engine, box, TouchingKind.VECTOR)
        {
            Checkpoint = checkpoint;
            Triggered = false;
            LastCheckpoint = null;
        }

        protected override void OnTrigger(Entity obj)
        {
            base.OnTrigger(obj);

            if (!Triggered && obj is Player)
            {
                Triggered = true;
                LastObjectTile = Engine.ObjectTile;
                LastBackgroundTile = Engine.BackgroundTile;
                LastPalette = Engine.Palette;
                LastCheckpoint = Engine.CurrentCheckpoint;
                LastCameraConstraintBox = Engine.CameraConstraintsBox;
                Engine.CurrentCheckpoint = Checkpoint;
            }
        }
    }
}
