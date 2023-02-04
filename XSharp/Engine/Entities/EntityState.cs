﻿namespace XSharp.Engine.Entities
{
    public delegate void EntityStateEvent(EntityState state);
    public delegate void EntityStateFrameEvent(EntityState state, long frameCounter);

    public class EntityState
    {
        public event EntityStateEvent StartEvent;
        public event EntityStateFrameEvent FrameEvent;
        public event EntityStateEvent EndEvent;

        public Entity Entity
        {
            get;
            internal set;
        }

        public int ID
        {
            get;
            internal set;
        } = -1;

        public long FrameCounter
        {
            get;
            private set;
        } = 0;

        protected internal virtual void OnStart()
        {
            FrameCounter = 0;
            StartEvent?.Invoke(this);
        }

        protected internal virtual void OnFrame()
        {
            FrameEvent?.Invoke(this, FrameCounter);
            FrameCounter++;
        }

        protected internal virtual void OnEnd()
        {
            EndEvent?.Invoke(this);
        }
    }
}
