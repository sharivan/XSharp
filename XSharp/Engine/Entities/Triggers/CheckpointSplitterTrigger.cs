using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Triggers;

public class CheckpointSplitterTrigger : SplitterTrigger
{
    private EntityReference<Checkpoint> checkpoint;
    private EntityReference<Checkpoint> lastCheckpoint = null;

    public Checkpoint Checkpoint
    {
        get => checkpoint;
        set => checkpoint = Engine.Entities.GetReferenceTo(value);
    }

    public SplitterTriggerDirection CheckpointDirection
    {
        get;
        set;
    } = SplitterTriggerDirection.FORWARD;

    public Checkpoint LastCheckpoint
    {
        get => lastCheckpoint;
        private set => lastCheckpoint = Engine.Entities.GetReferenceTo(value);
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

    public CheckpointSplitterTrigger()
    {
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