using XSharp.Interop;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalLineToDistanceEvent(LogicalLineTo source, float distance);
public delegate void LogicalLineToVectorEvent(LogicalLineTo source, Vector2 vector);

[Entity("logic_line_to")]
public class LogicalLineTo : LogicalEntity
{
    private FixedSingle lastDistance;

    [Output]
    public event LogicalLineToDistanceEvent OnChangeDistance;

    [Output]
    public event LogicalLineToVectorEvent OnChangeVector;

    private Entity startEntity;
    private Entity endEntity;

    public Entity StartEntity
    {
        get => startEntity;
        set => SetStartEntity(value);
    }

    public Entity EndEntity
    {
        get => endEntity;
        set => SetEndEntity(value);
    }

    public Metric Metric
    {
        get;
        set;
    } = Metric.EUCLIDIAN;

    public LogicalLineTo()
    {
    }

    [Input]
    public void SetStartEntity(Entity entity)
    {
        startEntity = entity;
    }

    [Input]
    public void SetEndEntity(Entity entity)
    {
        endEntity = entity;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        lastDistance = StartEntity != null && EndEntity != null ? StartEntity.Origin.DistanceTo(EndEntity.Origin, Metric) : 0;
    }

    protected override void OnPostThink()
    {
        base.OnPostThink();

        if (!Enabled)
            return;

        if (StartEntity != null && EndEntity != null)
        {
            FixedSingle distance = StartEntity.Origin.DistanceTo(EndEntity.Origin, Metric);
            if (distance != lastDistance)
            {
                OnChangeDistance?.Invoke(this, (float) distance);
                OnChangeVector?.Invoke(this, (StartEntity.Origin - EndEntity.Origin).ToVector2());
                lastDistance = distance;
            }
        }
    }
}