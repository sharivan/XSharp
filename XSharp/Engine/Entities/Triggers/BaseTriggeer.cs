using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Triggers;

public delegate void TriggerEvent(BaseTrigger source, Entity activator);

public abstract class BaseTrigger : Entity
{
    private Box hitbox = Box.EMPTY_BOX;

    public event TriggerEvent StartTriggerEvent;
    public event TriggerEvent TriggerEvent;
    public event TriggerEvent StopTriggerEvent;

    new public Box Hitbox
    {
        get => base.Hitbox;
        set => base.Hitbox = value;
    }

    public bool Enabled
    {
        get => CheckTouchingEntities;
        set => CheckTouchingEntities = value;
    }

    public uint Triggers
    {
        get;
        protected set;
    }

    public bool Once => MaxTriggers == 1;

    public uint MaxTriggers
    {
        get;
        protected set;
    } = 0;

    protected BaseTrigger()
    {
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (Enabled)
            OnStartTrigger(entity);
    }

    protected override void OnTouching(Entity entity)
    {
        base.OnTouching(entity);

        if (Enabled && (MaxTriggers == 0 || Triggers < MaxTriggers))
        {
            Triggers++;
            OnTrigger(entity);
        }
    }

    protected override void OnEndTouch(Entity entity)
    {
        base.OnEndTouch(entity);

        if (Enabled)
            OnStopTrigger(entity);
    }

    protected virtual void OnStartTrigger(Entity entity)
    {
        StartTriggerEvent?.Invoke(this, entity);
    }

    protected virtual void OnTrigger(Entity entity)
    {
        TriggerEvent?.Invoke(this, entity);
    }

    protected virtual void OnStopTrigger(Entity entity)
    {
        StopTriggerEvent?.Invoke(this, entity);
    }

    protected override Box GetHitbox()
    {
        return hitbox;
    }

    protected override void SetHitbox(Box hitbox)
    {
        this.hitbox = hitbox;
    }
}