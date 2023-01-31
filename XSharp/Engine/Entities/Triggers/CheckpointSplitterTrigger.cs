using MMX.Geometry;

namespace MMX.Engine.Entities.Triggers
{
    public class CheckpointSplitterTrigger : SplitterTrigger
    {
        public Checkpoint Checkpoint
        {
            get;
            set;
        }

        public SplitterTriggerDirection CheckpointDirection
        {
            get;
            set;
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

        public CheckpointSplitterTrigger(GameEngine engine, Box box, Checkpoint checkpoint, SplitterTriggerOrientation orientation = SplitterTriggerOrientation.VERTICAL, SplitterTriggerDirection checkpointDirection = SplitterTriggerDirection.FORWARD) :
            base(engine, box, orientation, VectorKind.PLAYER_ORIGIN)
        {
            Checkpoint = checkpoint;
            CheckpointDirection = checkpointDirection;
            LastCheckpoint = null;
        }

        protected override void OnSplitterTriggerEvent(Entity obj, SplitterTriggerDirection direction)
        {
            base.OnSplitterTriggerEvent(obj, direction);

            if (obj is Player)
            {
                if (direction == CheckpointDirection)
                {
                    LastObjectTile = Engine.ObjectTile;
                    LastBackgroundTile = Engine.BackgroundTile;
                    LastPalette = Engine.Palette;
                    LastCheckpoint = Engine.CurrentCheckpoint;
                    LastCameraConstraintBox = Engine.CameraConstraintsBox;
                    Engine.CurrentCheckpoint = Checkpoint;
                }
                else
                {
                    Engine.SetCheckpoint(LastCheckpoint, LastObjectTile, LastBackgroundTile, LastPalette);
                    Engine.CameraConstraintsBox = LastCameraConstraintBox;
                }
            }
        }
    }
}
