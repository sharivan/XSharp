namespace XSharp.Engine.Entities.Triggers
{
    public class Trigger : BaseTrigger
    {
        public new bool Once
        {
            get => base.Once;
            set => MaxTriggers = value ? 1 : uint.MaxValue;
        }

        public new uint MaxTriggers
        {
            get => base.MaxTriggers;
            set => base.MaxTriggers = value;
        }

        public Trigger()
        {
        }
    }
}
