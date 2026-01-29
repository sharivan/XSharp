using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalEntityEvent(LogicalEntity source);

public abstract class LogicalEntity : Entity, IEnableDisable
{
    private bool enabled = true;

    public event LogicalEntityEvent OnEnabled;
    public event LogicalEntityEvent OnDisabled;

    public bool Enabled
    {
        get => enabled;
        set => SetEnabled(value);
    }

    protected LogicalEntity()
    {
    }

    protected override Box GetHitbox()
    {
        return Box.EMPTY_BOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Enabled = true;
    }

    public void SetEnabled(bool value)
    {
        if (enabled == value)
            return;

        enabled = value;

        if (enabled)
            OnEnabled?.Invoke(this);
        else
            OnDisabled?.Invoke(this);
    }

    public void Enable()
    {
        SetEnabled(true);
    }

    public void Disable()
    {
        SetEnabled(false);
    }

    public void Toggle()
    {
        SetEnabled(!enabled);
    }
}