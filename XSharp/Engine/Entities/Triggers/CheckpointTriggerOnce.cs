using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Triggers;

public class CheckpointTriggerOnce : Trigger
{
    private EntityReference<Checkpoint> checkpoint;
    private EntityReference<Checkpoint> lastCheckpoint = null;

    public Checkpoint Checkpoint
    {
        get => checkpoint;
        set => checkpoint = value;
    }

    public bool Triggered
    {
        get;
        private set;
    } = false;

    public Checkpoint LastCheckpoint => lastCheckpoint;

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
            lastCheckpoint = Engine.CurrentCheckpoint;
            LastCameraConstraintBox = Engine.CameraConstraintsBox;
            Engine.CurrentCheckpoint = Checkpoint;
        }
    }
}