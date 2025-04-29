namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalBranchEvent(LogicalBranch source);

public class LogicalBranch : LogicalEntity
{
    private bool value = false;

    public event LogicalBranchEvent OnTrue;
    public event LogicalBranchEvent OnFalse;

    public bool Value
    {
        get => value;
        set => SetValue(value);
    }

    public LogicalBranch()
    {
    }

    public ThreeStateResult SetValue(bool value)
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        this.value = value;
        return value ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }

    public ThreeStateResult SetValueTest(bool value)
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        this.value = value;
        return Test();
    }

    public ThreeStateResult ToggleValue()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        value = !value;
        return value ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }

    public ThreeStateResult ToggleTest()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        Toggle();
        return Test();
    }

    public ThreeStateResult Test()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        if (value)
            OnTrue?.Invoke(this);
        else
            OnFalse?.Invoke(this);

        return value ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }
}