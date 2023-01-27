using System.Collections.Generic;

using MMX.Geometry;

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
            base(engine, boundingBox.Origin)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;

            Enabled = true;
            MaxTriggers = uint.MaxValue;
            TouchingKind = touchingKind;
            VectorKind = vectorKind;

            triggereds = new List<Entity>();
        }

        protected override void OnStartTouch(Entity obj)
        {
            base.OnStartTouch(obj);

            if (Enabled && Triggers < MaxTriggers)
            {
                switch (TouchingKind)
                {
                    case TouchingKind.VECTOR:
                    {
                        Vector v = obj.GetVector(VectorKind);
                        if (v <= HitBox)
                        {
                            triggereds.Add(obj);
                            Triggers++;
                            OnTrigger(obj);
                        }

                        break;
                    }

                    case TouchingKind.BOX:
                    {
                        Triggers++;
                        OnTrigger(obj);
                        break;
                    }
                }
            }
        }

        protected override void OnTouching(Entity obj)
        {
            base.OnTouching(obj);

            if (Enabled && Triggers < MaxTriggers && TouchingKind == TouchingKind.VECTOR && !triggereds.Contains(obj))
            {
                Vector v = obj.GetVector(VectorKind);
                if (v <= HitBox)
                {
                    triggereds.Add(obj);
                    Triggers++;
                    OnTrigger(obj);
                }
            }
        }

        protected override void OnEndTouch(Entity obj)
        {
            triggereds.Remove(obj);
        }

        protected virtual void OnTrigger(Entity obj)
        {
            TriggerEvent?.Invoke(obj);
        }

        protected override Box GetBoundingBox()
        {
            return Origin + boundingBox;
        }

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;
            SetOrigin(boundingBox.Origin);
        }
    }
}
