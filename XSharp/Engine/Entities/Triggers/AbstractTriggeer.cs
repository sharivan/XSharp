using MMX.Geometry;
using System.Collections.Generic;

namespace MMX.Engine.Entities.Triggers
{
    public delegate void TriggerEvent(Entity obj);

    public enum TouchingKind
    {
        VECTOR,
        BOX
    }

    public abstract class AbstractTrigger : Entity
    {
        private Box boundingBox;
        private readonly List<Entity> triggereds;

        public event TriggerEvent TriggerEvent;

        public bool Enabled
        {
            get;
            set;
        }

        public uint Triggers
        {
            get;
            protected set;
        }

        public TouchingKind TouchingKind
        {
            get;
            set;
        }

        public VectorKind VectorKind
        {
            get;
            set;
        }

        public bool Once => MaxTriggers == 1;

        public uint MaxTriggers
        {
            get;
            protected set;
        }

        protected AbstractTrigger(GameEngine engine, Box boundingBox, TouchingKind touchingKind = TouchingKind.VECTOR, VectorKind vectorKind = VectorKind.ORIGIN) :
            base(engine, "Trigger", boundingBox.Origin)
        {
            Enabled = true;
            MaxTriggers = uint.MaxValue;
            TouchingKind = touchingKind;
            VectorKind = vectorKind;

            triggereds = new List<Entity>();

            SetBoundingBox(boundingBox);
        }

        protected override void OnStartTouch(Entity entity)
        {
            base.OnStartTouch(entity);

            if (Enabled && Triggers < MaxTriggers)
            {
                switch (TouchingKind)
                {
                    case TouchingKind.VECTOR:
                    {
                        Vector v = entity.GetVector(VectorKind);
                        if (v <= HitBox)
                        {
                            triggereds.Add(entity);
                            Triggers++;
                            OnTrigger(entity);
                        }

                        break;
                    }

                    case TouchingKind.BOX:
                    {
                        Triggers++;
                        OnTrigger(entity);
                        break;
                    }
                }
            }
        }

        protected override void OnTouching(Entity entity)
        {
            base.OnTouching(entity);

            if (Enabled && Triggers < MaxTriggers && TouchingKind == TouchingKind.VECTOR && !triggereds.Contains(entity))
            {
                Vector v = entity.GetVector(VectorKind);
                if (v <= HitBox)
                {
                    triggereds.Add(entity);
                    Triggers++;
                    OnTrigger(entity);
                }
            }
        }

        protected override void OnEndTouch(Entity entity)
        {
            triggereds.Remove(entity);
        }

        protected virtual void OnTrigger(Entity entity)
        {
            TriggerEvent?.Invoke(entity);
        }

        protected override Box GetBoundingBox()
        {
            return boundingBox;
        }

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;
        }
    }
}
