using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalSwitchEvent(LogicalSwitch source);

public class LogicalSwitch : LogicalEntity
{
    private int value = 0;

    public event LogicalSwitchEvent OnCase01;
    public event LogicalSwitchEvent OnCase02;
    public event LogicalSwitchEvent OnCase03;
    public event LogicalSwitchEvent OnCase04;
    public event LogicalSwitchEvent OnCase05;
    public event LogicalSwitchEvent OnCase06;
    public event LogicalSwitchEvent OnCase07;
    public event LogicalSwitchEvent OnCase08;
    public event LogicalSwitchEvent OnCase09;
    public event LogicalSwitchEvent OnCase10;
    public event LogicalSwitchEvent OnCase11;
    public event LogicalSwitchEvent OnCase12;
    public event LogicalSwitchEvent OnCase13;
    public event LogicalSwitchEvent OnCase14;
    public event LogicalSwitchEvent OnCase15;
    public event LogicalSwitchEvent OnCase16;
    public event LogicalSwitchEvent OnDefault;

    public int Value
    {
        get => value;
        set => SetValue(value);
    }

    public LogicalSwitch()
    {
    }

    public int SetValue(int value)
    {
        if (!Enabled)
            return 0;

        this.value = value;
        return value;
    }

    public int SetValueTest(int value)
    {
        if (!Enabled)
            return 0;

        this.value = value;
        return Test();
    }

    public int Test()
    {
        if (!Enabled)
            return 0;

        switch (value)
        {
            case 1:
                OnCase01?.Invoke(this);
                break;

            case 2:
                OnCase02?.Invoke(this);
                break;

            case 3:
                OnCase03?.Invoke(this);
                break;

            case 4:
                OnCase04?.Invoke(this);
                break;

            case 5:
                OnCase05?.Invoke(this);
                break;

            case 6:
                OnCase06?.Invoke(this);
                break;

            case 7:
                OnCase07?.Invoke(this);
                break;

            case 8:
                OnCase08?.Invoke(this);
                break;

            case 9:
                OnCase09?.Invoke(this);
                break;

            case 10:
                OnCase10?.Invoke(this);
                break;

            case 11:
                OnCase11?.Invoke(this);
                break;

            case 12:
                OnCase12?.Invoke(this);
                break;

            case 13:
                OnCase13?.Invoke(this);
                break;

            case 14:
                OnCase14?.Invoke(this);
                break;

            case 15:
                OnCase15?.Invoke(this);
                break;

            case 16:
                OnCase16?.Invoke(this);
                break;

            default:
                OnDefault?.Invoke(this);
                break;
        }

        return value;
    }
}