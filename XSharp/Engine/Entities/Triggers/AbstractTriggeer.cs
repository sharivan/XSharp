using MMX.Geometry;
using System.Collections.Generic;

namespace MMX.Engine.Entities.Triggers
{
    public delegate void TriggerEvent(AbstractTrigger source, Entity activator);

    public enum TouchingKind
    {
        VECTOR,
        BOX
    }

    public abstract class AbstractTrigger : Entity
    {
        private Box boundingBox;
        private readonly List<Entity> triggerings;

        public event TriggerEvent StartTriggerEvent;
        public event TriggerEvent TriggerEvent;
        public event TriggerEvent StopTriggerEvent;

        public bool Enabled
        {
            get => CheckCollisionWithEntities;
            set
            {
                if (Alive && !value)
                {
                    foreach (var triggered in triggerings)
                        OnStopTrigger(triggered);

                    triggerings.Clear();
                }

                CheckCollisionWithEntities = value;
            }
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
        } = 0;

        protected AbstractTrigger(string name, Box boundingBox, TouchingKind touchingKind = TouchingKind.VECTOR, VectorKind vectorKind = VectorKind.ORIGIN)
            : base(name, boundingBox.Origin)
        {
            TouchingKind = touchingKind;
            VectorKind = vectorKind;

            triggerings = new List<Entity>();

            SetBoundingBox(boundingBox);
        }

        protected AbstractTrigger(Box boundingBox, TouchingKind touchingKind = TouchingKind.VECTOR, VectorKind vectorKind = VectorKind.ORIGIN)
            : this(null, boundingBox, touchingKind, vectorKind)
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Enabled = true;
        }

        protected override void OnStartTouch(Entity entity)
        {
            base.OnStartTouch(entity);

            if (Enabled && (MaxTriggers == 0 || Triggers < MaxTriggers))
            {
                switch (TouchingKind)
                {
                    case TouchingKind.VECTOR:
                    {
                        Vector v = entity.GetVector(VectorKind);
                        if (v <= Hitbox)
                        {
                            triggerings.Add(entity);
                            Triggers++;

                            OnStartTrigger(entity);
                            OnTrigger(entity);
                        }

                        break;
                    }

                    case TouchingKind.BOX:
                    {
                        triggerings.Add(entity);
                        Triggers++;

                        OnStartTrigger(entity);
                        OnTrigger(entity);
                        break;
                    }
                }
            }
        }

        protected override void OnTouching(Entity entity)
        {
            base.OnTouching(entity);

            if (Enabled && (MaxTriggers == 0 || Triggers < MaxTriggers))
            {
                if (TouchingKind == TouchingKind.VECTOR)
                {
                    Vector v = entity.GetVector(VectorKind);
                    if (v <= Hitbox)
                    {
                        if (!triggerings.Contains(entity))
                        {
                            triggerings.Add(entity);
                            OnStartTrigger(entity);
                        }

                        Triggers++;
                        OnTrigger(entity);
                    }
                    else if (!(v <= Hitbox) && triggerings.Contains(entity))
                    {
                        triggerings.Remove(entity);
                        OnStopTrigger(entity);
                    }
                }
                else
                {
                    Triggers++;
                    OnTrigger(entity);
                }
            }
        }

        protected override void OnEndTouch(Entity entity)
        {
            base.OnEndTouch(entity);

            if (triggerings.Contains(entity))
            {
                triggerings.Remove(entity);
                OnStopTrigger(entity);
            }
        }

        protected virtual void OnStartTrigger(Entity entity)
        {
            StartTriggerEvent?.Invoke(this, entity);
        }

        protected virtual void OnTrigger(Entity entity)
        {
            TriggerEvent?.Invoke(this, entity);
        }

        protected virtual void OnStopTrigger(Entity entity)
        {
            StopTriggerEvent?.Invoke(this, entity);
        }

        protected override Box GetBoundingBox()
        {
            return boundingBox;
        }

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;
            UpdatePartition(true);
        }

        public bool IsTriggering(Entity entity)
        {
            return triggerings.Contains(entity);
        }
    }
}