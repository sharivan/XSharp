namespace XSharp.Engine.Entities.Logical
{
    public delegate void TimerEvent(LogicalTimer source);

    public class LogicalTimer : LogicalEntity
    {
        public event TimerEvent TimerEvent;

        public int Interval // in frames
        {
            get;
            set;
        }

        public long FrameCounter
        {
            get;
            private set;
        } = 0;

        public LogicalTimer()
        {
        }

        public void Reset()
        {
            FrameCounter = 0;
        }

        public void FireTimer()
        {
            if (!Enabled)
                return;

            TimerEvent?.Invoke(this);
        }

        public void Increment(int amount)
        {
            FrameCounter += amount;
        }
        public void Decrement(int amount)
        {
            FrameCounter -= amount;
        }

        public void Toggle()
        {
            Enabled = !Enabled;
        }

        protected override void Think()
        {
            base.Think();

            if (!Enabled)
                return;

            if (FrameCounter >= Interval)
            {
                FireTimer();
                FrameCounter = 0;
            }
            else
            {
                FrameCounter++;
            }
        }
    }
}