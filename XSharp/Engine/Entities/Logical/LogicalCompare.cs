using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalCompareEvent(LogicalCompare source);

[Entity("logic_compare")]
public class LogicalCompare : LogicalEntity
{
    private float value = 0;
    private float compareValue = 0;

    [Output]
    public event LogicalCompareEvent OnLessThan;

    [Output]
    public event LogicalCompareEvent OnEqualTo;

    [Output]
    public event LogicalCompareEvent OnNotEqualTo;

    [Output]
    public event LogicalCompareEvent OnGreaterThan;

    public float InitialValue
    {
        get;
        set;
    } = 0;

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float CompareValue
    {
        get => compareValue;
        set => SetCompareValue(value);
    }

    public LogicalCompare()
    {
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        value = InitialValue;
    }

    [Input]
    public void SetValue(float value)
    {
        if (!Enabled)
            return;

        this.value = value;
    }

    [Input]
    public int Compare()
    {
        if (!Enabled)
            return -2;

        int result;

        if (value < compareValue)
        {
            result = -1;
            OnNotEqualTo?.Invoke(this);
            OnLessThan?.Invoke(this);
        }
        else if (value > compareValue)
        {
            result = 1;
            OnNotEqualTo?.Invoke(this);
            OnGreaterThan?.Invoke(this);
        }
        else
        {
            result = 0;
            OnEqualTo?.Invoke(this);
        }

        return result;
    }

    [Input]
    public int SetValueCompare(float value)
    {
        if (!Enabled)
            return -2;

        this.value = value;
        return Compare();
    }

    [Input]
    public void SetCompareValue(float compareValue)
    {
        if (!Enabled)
            return;

        this.compareValue = compareValue;
    }
}