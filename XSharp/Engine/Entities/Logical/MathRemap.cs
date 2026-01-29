using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void MathRemapValueEvent(MathRemap source, float value);

[Entity("math_remap")]
public class MathRemap : LogicalEntity
{
    [Output]
    public event MathRemapValueEvent OutValue;

    public float MinInput
    {
        get;
        set;
    } = 0;

    public float MaxInput
    {
        get;
        set;
    } = 100;

    public float MinOutput
    {
        get;
        set;
    } = 0;

    public float MaxOutput
    {
        get;
        set;
    } = 100;

    public MathRemap()
    {
    }

    public float Interpolate(float input)
    {
        try
        {
            return MinOutput + (MaxOutput - MinOutput) * ((input - MinInput) / (MaxInput - MinInput));
        }
        catch (DivideByZeroException)
        {           
        }

        return MinOutput;
    }

    [Input]
    public float InValue(float input)
    {
        if (!Enabled)
            return MinOutput;

        var result = Interpolate(input);
        OutValue?.Invoke(this, result);
        return result;
    }
}