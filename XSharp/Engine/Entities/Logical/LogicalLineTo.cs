using XSharp.Interop;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalLineToDistanceEvent(LogicalLineTo source, float distance);
public delegate void LogicalLineToVectorEvent(LogicalLineTo source, Vector2 vector);

public class LogicalLineTo : LogicalBranch
{
    private FixedSingle lastDistance;

    public event LogicalLineToDistanceEvent OnChangeDistance;
    public event LogicalLineToVectorEvent OnChangeVector;

    public Entity StartEntity
    {
        get;
        set;
    }

    public Entity EndEntity
    {
        get;
        set;
    }

    public Metric Metric
    {
        get;
        set;
    } = Metric.EUCLIDIAN;

    public LogicalLineTo()
    {
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