using XSharp.Geometry;

namespace XSharp.Engine.Entities.Logical
{
    public delegate void LogicalBranchEvent(LogicalBranch source);

    public class LogicalBranch : LogicalEntity
    {
        public event LogicalBranchEvent TrueEvent;
        public event LogicalBranchEvent FalseEvent;

        public bool Value
        {
            get;
            set;
        }

        public LogicalBranch(Vector origin) : this(null, origin)
        {
        }

        public LogicalBranch(string name, Vector origin) : base(name, origin)
        {
        }

        public void SetValueTest(bool value)
        {
            if (!Enabled)
                return;

            Value = value;
            Test();
        }

        public void Toggle()
        {
            if (!Enabled)
                return;

            Value = !Value;
        }

        public void ToggleTest()
        {
            if (!Enabled)
                return;

            Toggle();
            Test();
        }

        public void Test()
        {
            if (!Enabled)
                return;

            if (Value)
                TrueEvent?.Invoke(this);
            else
                FalseEvent?.Invoke(this);
        }
    }
}