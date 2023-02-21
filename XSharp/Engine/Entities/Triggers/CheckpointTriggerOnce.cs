using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Triggers
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
        } = false;

        public Checkpoint LastCheckpoint
        {
            get;
            private set;
        } = null;

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

        public CheckpointTriggerOnce()
        {
            TouchingKind = TouchingKind.VECTOR;
        }

        protected override void OnStartTrigger(Entity obj)
        {
            base.OnStartTrigger(obj);

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