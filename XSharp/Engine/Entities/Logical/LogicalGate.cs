using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalGateEvent(LogicalGate source);
public delegate void LogicalGateValueEvent(LogicalGate source, bool value);

[Entity("logic_gate")]
public class LogicalGate : LogicalEntity
{
    private LogicalGateMode mode = LogicalGateMode.AND;
    private bool inValueA = false;
    private bool inValueB = false;

    [Output]
    public event LogicalGateEvent OnResultTrue;

    [Output]
    public event LogicalGateEvent OnResultFalse;

    [Output]
    public event LogicalGateValueEvent OutValue;

    public LogicalGateMode Mode
    {
        get => mode;
        set => SetMode(value);
    }

    public bool InValueA
    {
        get => inValueA;
        set => SetValueA(value);
    }

    public bool InValueB
    {
        get => inValueB;
        set => SetValueB(value);
    }

    public bool Result
    {
        get;
        private set;
    } = false;

    public LogicalGate()
    {
    }

    private bool Eval()
    {
        switch (Mode)
        {
            case LogicalGateMode.AND:
                Result = inValueA && inValueB;
                break;

            case LogicalGateMode.OR:
                Result = inValueA || inValueB;
                break;

            case LogicalGateMode.NAND:
                Result = !(inValueA && inValueB);
                break;

            case LogicalGateMode.NOR:
                Result = !(inValueA || inValueB);
                break;

            case LogicalGateMode.XOR:
                Result = inValueA ^ inValueB;
                break;

            case LogicalGateMode.XNOR:
                Result = !(inValueA ^ inValueB);
                break;
        }

        return Result;
    }

    private void EvalTest()
    {
        Eval();

        if (Result)
            OnResultTrue?.Invoke(this);
        else
            OnResultFalse?.Invoke(this);
    }

    [Input]
    public void SetMode(LogicalGateMode type)
    {
        if (!Enabled)
            return;

        Mode = type;
        EvalTest();
    }

    [Input]
    public void SetValueA(bool value)
    {
        if (!Enabled)
            return;

        inValueA = value;
        EvalTest();
    }

    [Input]
    public void SetValueB(bool value)
    {
        if (!Enabled)
            return;

        inValueA = value;
        EvalTest();
    }

    [Input]
    public ThreeStateResult GetValue()
    {
        if (!Enabled)
            return ThreeStateResult.UNDEFINED;

        OutValue?.Invoke(this, Result);
        return Result ? ThreeStateResult.TRUE : ThreeStateResult.FALSE;
    }
}