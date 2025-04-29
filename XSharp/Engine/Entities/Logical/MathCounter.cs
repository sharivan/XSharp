using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void MathCounterEvent(MathCounter source);
public delegate void MathCounterValueEvent(MathCounter source, float value);

public class MathCounter : LogicalEntity
{
    public event MathCounterValueEvent OutValue;
    public event MathCounterEvent OnHitMin;
    public event MathCounterEvent OnHitMax;
    public event MathCounterValueEvent OutGetValue;
    public event MathCounterEvent OnChangedFromMin;
    public event MathCounterEvent OnChangedFromMax;

    private float value = 0;

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float InitialValue
    {
        get;
        set;
    } = 0;

    public float MinValue
    {
        get;
        set;
    } = 0;

    public float MaxValue
    {
        get;
        set;
    } = 100;

    public MathCounter()
    {
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        value = InitialValue;
        Clamp();
    }

    private void Clamp()
    {
        if (value < MinValue)
            value = MinValue;
        else if (value > MaxValue)
            value = MaxValue;
    }

    public float GetValue()
    {
        if (!Enabled)
            return 0;

        OutGetValue?.Invoke(this, value);
        return value;
    }

    public float SetValue(float value)
    {
        if (!Enabled)
            return Value;

        float lastValue = this.value;

        this.value = value;
        Clamp();

        if (Value != lastValue)
        {
            OutValue?.Invoke(this, Value);

            if (Value == MinValue)
                OnHitMin?.Invoke(this);

            if (Value == MaxValue)
                OnHitMax?.Invoke(this);

            if (lastValue == MinValue)
                OnChangedFromMin?.Invoke(this);

            if (lastValue == MaxValue)
                OnChangedFromMax?.Invoke(this);
        }

        return Value;
    }

    public float SetValueNoFire(float value)
    {
        if (!Enabled)
            return Value;

        this.value = value;
        Clamp();
        return Value;
    }

    public float Reset()
    {
        if (!Enabled)
            return Value;

        return SetValue(InitialValue);
    }

    public float Increment()
    {
        if (!Enabled)
            return Value;

        return SetValue(value + 1);
    }

    public float Decrement()
    {
        if (!Enabled)
            return Value;

        return SetValue(value - 1);
    }

    public float Add(float value)
    {
        if (!Enabled)
            return Value;

        return SetValue(this.value + value);
    }

    public float Subtract(float value)
    {
        if (!Enabled)
            return Value;

        return SetValue(this.value - value);
    }

    public float Multiply(float value)
    {
        if (!Enabled)
            return Value;

        return SetValue(this.value * value);
    }

    public float Divide(float value)
    {
        if (!Enabled)
            return Value;

        try
        {
            return SetValue(this.value / value);
        }
        catch (DivideByZeroException)
        {
        }

        return Value;
    }
}