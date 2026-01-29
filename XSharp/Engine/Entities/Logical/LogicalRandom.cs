using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalRandomEvent(LogicalRandom source);

[Entity("logic_random")]
public class LogicalRandom : LogicalEntity
{
    private bool triggered = false;

    [Output]
    public event LogicalRandomEvent OnTrigger01;

    [Output]
    public event LogicalRandomEvent OnTrigger02;

    [Output]
    public event LogicalRandomEvent OnTrigger03;

    [Output]
    public event LogicalRandomEvent OnTrigger04;

    [Output]
    public event LogicalRandomEvent OnTrigger05;

    [Output]
    public event LogicalRandomEvent OnTrigger06;

    [Output]
    public event LogicalRandomEvent OnTrigger07;

    [Output]
    public event LogicalRandomEvent OnTrigger08;

    [Output]
    public event LogicalRandomEvent OnTrigger09;

    [Output]
    public event LogicalRandomEvent OnTrigger10;

    [Output]
    public event LogicalRandomEvent OnTrigger11;

    [Output]
    public event LogicalRandomEvent OnTrigger12;

    [Output]
    public event LogicalRandomEvent OnTrigger13;

    [Output]
    public event LogicalRandomEvent OnTrigger14;

    [Output]
    public event LogicalRandomEvent OnTrigger15;

    [Output]
    public event LogicalRandomEvent OnTrigger16;

    public bool Once
    {
        get;
        set;
    } = false;

    public LogicalRandom()
    {
    }

    [Input]
    public uint Trigger()
    {
        if (!Enabled)
            return 0;

        if (Once && triggered)
            return 0;

        triggered = true;

        uint rnd = Engine.RNG.NextValue(1, 16);
        switch (rnd)
        {
            case 1:
                OnTrigger01?.Invoke(this);
                break;

            case 2:
                OnTrigger02?.Invoke(this);
                break;

            case 3:
                OnTrigger03?.Invoke(this);
                break;

            case 4:
                OnTrigger04?.Invoke(this);
                break;

            case 5:
                OnTrigger05?.Invoke(this);
                break;

            case 6:
                OnTrigger06?.Invoke(this);
                break;

            case 7:
                OnTrigger07?.Invoke(this);
                break;

            case 8:
                OnTrigger08?.Invoke(this);
                break;

            case 9:
                OnTrigger09?.Invoke(this);
                break;

            case 10:
                OnTrigger10?.Invoke(this);
                break;

            case 11:
                OnTrigger11?.Invoke(this);
                break;

            case 12:
                OnTrigger12?.Invoke(this);
                break;

            case 13:
                OnTrigger13?.Invoke(this);
                break;

            case 14:
                OnTrigger14?.Invoke(this);
                break;

            case 15:
                OnTrigger15?.Invoke(this);
                break;

            case 16:
                OnTrigger16?.Invoke(this);
                break;
        }

        return rnd;
    }
}