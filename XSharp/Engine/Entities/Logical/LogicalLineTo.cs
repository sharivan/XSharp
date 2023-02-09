using XSharp.Math;

namespace XSharp.Engine.Entities.Logical
{
    public delegate void LogicalLineToEvent(LogicalLineTo source);

    public class LogicalLineTo : LogicalBranch
    {
        private FixedSingle lastDistance;

        public event LogicalLineToEvent LogicalLineToEvent;

        public Entity StartEntity
        {
            get;
            set;
        }

        public Entity EndEntity
        {
            get;
            set;
        }

        public LogicalLineTo()
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            lastDistance = StartEntity != null && EndEntity != null ? StartEntity.Origin.DistanceTo(EndEntity.Origin) : 0;
        }

        protected internal override void PostThink()
        {
            base.PostThink();

            if (!Enabled)
                return;

            if (StartEntity != null && EndEntity != null)
            {
                FixedSingle distance = StartEntity.Origin.DistanceTo(EndEntity.Origin);
                if (distance != lastDistance)
                {
                    LogicalLineToEvent?.Invoke(this);
                    lastDistance = distance;
                }
            }
        }
    }
}