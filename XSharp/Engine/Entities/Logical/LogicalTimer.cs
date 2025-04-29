namespace XSharp.Engine.Entities.Logical;

public delegate void TimerEvent(LogicalTimer source);

public class LogicalTimer : LogicalEntity
{
    public event TimerEvent OnTimer;

    public int Interval // in frames
    {
        get;
        set;
    }

    public long TickCounter
    {
        get;
        private set;
    } = 0;

    public LogicalTimer()
    {
    }

    public void Reset()
    {
        TickCounter = 0;
    }

    public void FireTimer()
    {
        if (!Enabled)
            return;

        OnTimer?.Invoke(this);
    }

    public void Increment(int amount)
    {
        TickCounter += amount;
    }

    public void Decrement(int amount)
    {
        TickCounter -= amount;
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (!Enabled)
            return;

        if (TickCounter >= Interval)
        {
            FireTimer();
            TickCounter = 0;
        }
        else
        {
            TickCounter++;
        }
    }
}