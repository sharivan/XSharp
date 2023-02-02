using MMX.Geometry;

namespace MMX.Engine.Entities.Triggers
{
    public enum SplitterTriggerOrientation
    {
        HORIZONTAL,
        VERTICAL
    }

    public enum SplitterTriggerDirection
    {
        FORWARD,
        BACKWARD
    }

    public delegate void SplitterTriggerEventHandler(SplitterTrigger source, Entity target, SplitterTriggerDirection direction);

    public class SplitterTrigger : AbstractTrigger
    {
        public event SplitterTriggerEventHandler LineTriggerEvent;

        public SplitterTriggerOrientation Orientation
        {
            get; set;
        }

        public SplitterTrigger(GameEngine engine, Box box, SplitterTriggerOrientation orientation = SplitterTriggerOrientation.VERTICAL, VectorKind vectorKind = VectorKind.ORIGIN) :
            base(engine, box, TouchingKind.VECTOR, vectorKind)
        {
            Orientation = orientation;
        }

        protected virtual void OnSplitterTriggerEvent(Entity target, SplitterTriggerDirection side)
        {
            LineTriggerEvent?.Invoke(this, target, side);
        }

        protected override void OnTouching(Entity entity)
        {
            base.OnTouching(entity);

            if (!Enabled)
                return;

            Vector targetOrigin = entity.GetVector(VectorKind);
            Vector targetLastOrigin = entity.GetLastVector(VectorKind);

            switch (Orientation)
            {
                case SplitterTriggerOrientation.HORIZONTAL:
                    if (targetOrigin.Y < Origin.Y && targetLastOrigin.Y >= Origin.Y)
                        OnSplitterTriggerEvent(entity, SplitterTriggerDirection.BACKWARD);
                    else if (targetOrigin.Y >= Origin.Y && targetLastOrigin.Y < Origin.Y)
                        OnSplitterTriggerEvent(entity, SplitterTriggerDirection.FORWARD);

                    break;

                case SplitterTriggerOrientation.VERTICAL:
                    if (targetOrigin.X < Origin.X && targetLastOrigin.X >= Origin.X)
                        OnSplitterTriggerEvent(entity, SplitterTriggerDirection.BACKWARD);
                    else if (targetOrigin.X >= Origin.X && targetLastOrigin.X < Origin.X)
                        OnSplitterTriggerEvent(entity, SplitterTriggerDirection.FORWARD);

                    break;
            }
        }
    }
}
