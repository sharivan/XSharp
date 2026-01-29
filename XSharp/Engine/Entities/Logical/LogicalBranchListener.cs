using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Engine.Entities.Enemies.Bosses.Vile;

namespace XSharp.Engine.Entities.Logical;

public delegate void LogicalBranchListenerEvent(LogicalBranchListener source);

[Entity("logic_branch_listener")]
public class LogicalBranchListener : LogicalEntity
{
    [Output]
    public event LogicalBranchListenerEvent OnAllTrue;

    [Output]
    public event LogicalBranchListenerEvent OnAllFalse;

    [Output]
    public event LogicalBranchListenerEvent OnMixed;

    public LogicalBranch Branch01
    {
        get;
        set;
    }

    public LogicalBranch Branch02
    {
        get;
        set;
    }

    public LogicalBranch Branch03
    {
        get;
        set;
    }

    public LogicalBranch Branch04
    {
        get;
        set;
    }

    public LogicalBranch Branch05
    {
        get;
        set;
    }

    public LogicalBranch Branch06
    {
        get;
        set;
    }

    public LogicalBranch Branch07
    {
        get;
        set;
    }

    public LogicalBranch Branch08
    {
        get;
        set;
    }

    public LogicalBranch Branch09
    {
        get;
        set;
    }

    public LogicalBranch Branch10
    {
        get;
        set;
    }

    public LogicalBranch Branch11
    {
        get;
        set;
    }

    public LogicalBranch Branch12
    {
        get;
        set;
    }

    public LogicalBranch Branch13
    {
        get;
        set;
    }

    public LogicalBranch Branch14
    {
        get;
        set;
    }

    public LogicalBranch Branch15
    {
        get;
        set;
    }

    public LogicalBranch Branch16
    {
        get;
        set;
    }

    public LogicalBranchListener()
    {
    }

    private LogicalBranch GetBranch(int index)
    {
        return index switch
        {
            1 => Branch01,
            2 => Branch02,
            3 => Branch03,
            4 => Branch04,
            5 => Branch05,
            6 => Branch06,
            7 => Branch07,
            8 => Branch08,
            9 => Branch09,
            10 => Branch10,
            11 => Branch11,
            12 => Branch12,
            13 => Branch13,
            14 => Branch14,
            15 => Branch15,
            16 => Branch16,
            _ => null,
        };
    }

    [Input]
    public void SetValue(bool value)
    {
        if (!Enabled)
            return;

        for (int i = 1; i <= 16; i++)
        {
            var branch = GetBranch(i);
            branch?.SetValue(value);
        }
    }

    [Input]
    public void ToggleBranches()
    {
        if (!Enabled)
            return;

        for (int i = 1; i <= 16; i++)
        {
            var branch = GetBranch(i);
            branch?.ToggleValue();
        }
    }

    [Input]
    public void ToggleTest()
    {
        if (!Enabled)
            return;

        int trues = 0;
        int falses = 0;
        int totals = 0;

        for (int i = 1; i <= 16; i++)
        {
            var branch = GetBranch(i);
            if (branch != null)
            {
                totals++;
                var test = branch.ToggleTest();
                if (test == ThreeStateResult.TRUE)
                    trues++;
                else if (test == ThreeStateResult.FALSE)
                    falses++;
            }
        }

        if (totals > 0)
        {
            if (totals == trues)
                OnAllTrue?.Invoke(this);
            else if (totals == falses)
                OnAllFalse?.Invoke(this);
            else
                OnMixed?.Invoke(this);
        }
    }

    [Input]
    public void Test()
    {
        if (!Enabled)
            return;

        int trues = 0;
        int falses = 0;
        int totals = 0;

        for (int i = 1; i <= 16; i++)
        {
            var branch = GetBranch(i);
            if (branch != null)
            {
                totals++;
                var test = branch.Test();
                if (test == ThreeStateResult.TRUE)
                    trues++;
                else if (test == ThreeStateResult.FALSE)
                    falses++;
            }
        }

        if (totals > 0)
        {
            if (totals == trues)
                OnAllTrue?.Invoke(this);
            else if (totals == falses)
                OnAllFalse?.Invoke(this);
            else
                OnMixed?.Invoke(this);
        }
    }
}