﻿using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Triggers
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

    public delegate void SplitterTriggerEvent(SplitterTrigger source, Entity activator, SplitterTriggerDirection direction);

    public class SplitterTrigger : BaseTrigger
    {
        public event SplitterTriggerEvent SplitterTriggerEvent;

        public SplitterTriggerOrientation Orientation
        {
            get; set;
        } = SplitterTriggerOrientation.VERTICAL;

        public SplitterTrigger()
        {
            TouchingKind = TouchingKind.VECTOR;
        }

        protected virtual void OnSplitterTriggerEvent(Entity target, SplitterTriggerDirection side)
        {
            SplitterTriggerEvent?.Invoke(this, target, side);
        }

        protected override void OnTrigger(Entity entity)
        {
            base.OnTrigger(entity);

            Vector targetOrigin = entity.GetVector(TouchingVectorKind);
            Vector targetLastOrigin = entity.GetLastVector(TouchingVectorKind);

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