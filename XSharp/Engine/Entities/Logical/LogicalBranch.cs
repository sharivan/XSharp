namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalBranchEvent(LogicalBranch source);

[Entity("logic_branch")]
public class LogicalBranch : LogicalEntity
{
    private bool value = false;

    [Output]
    public event LogicalBranchEvent OnTrue;

    [Output]
    public event LogicalBranchEvent OnFalse;

    public bool Value
    {
        get => value;
        set => SetValue(value);
    }

    public LogicalBranch()
    {
    }

    [Input]
    public ThreeStateResult SetValue(bool value)
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        this.value = value;
        return value ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }

    [Input]
    public ThreeStateResult SetValueTest(bool value)
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        this.value = value;
        return Test();
    }

    [Input]
    public ThreeStateResult ToggleValue()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        value = !value;
        return value ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }

    [Input]
    public ThreeStateResult ToggleTest()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        Toggle();
        return Test();
    }

    [Input]
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