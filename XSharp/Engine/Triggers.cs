using System.Collections.Generic;

using MMX.Geometry;

namespace MMX.Engine
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
        private List<Entity> triggereds;

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

        protected virtual void OnTrigger(Entity obj) => TriggerEvent?.Invoke(obj);

        protected override Box GetBoundingBox() => Origin + boundingBox;

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;
            SetOrigin(boundingBox.Origin);
        }
    }

    public class Trigger : AbstractTrigger
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

        public Trigger(GameEngine engine, Box boudingBox, TouchingKind touchingKind = TouchingKind.VECTOR, VectorKind vectorKind = VectorKind.ORIGIN) :
            base(engine, boudingBox, touchingKind, vectorKind)
        {
        }
    }

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
            base(engine, box, TouchingKind.VECTOR, vectorKind) => Orientation = orientation;

        protected virtual void OnSplitterTriggerEvent(Entity target, SplitterTriggerDirection side) => LineTriggerEvent?.Invoke(this, target, side);

        protected override void OnTouching(Entity obj)
        {
            base.OnTouching(obj);

            if (!Enabled)
                return;

            Vector targetOrigin = obj.Origin;
            Vector targetLastOrigin = obj.LastOrigin;

            switch (Orientation)
            {
                case SplitterTriggerOrientation.HORIZONTAL:
                    if (targetOrigin.Y < Origin.Y && targetLastOrigin.Y >= Origin.Y)
                        OnSplitterTriggerEvent(obj, SplitterTriggerDirection.BACKWARD);
                    else if (targetOrigin.Y >= Origin.Y && targetLastOrigin.Y < Origin.Y)
                        OnSplitterTriggerEvent(obj, SplitterTriggerDirection.FORWARD);

                    break;

                case SplitterTriggerOrientation.VERTICAL:
                    if (targetOrigin.X < Origin.X && targetLastOrigin.X >= Origin.X)
                        OnSplitterTriggerEvent(obj, SplitterTriggerDirection.BACKWARD);
                    else if (targetOrigin.X >= Origin.X && targetLastOrigin.X < Origin.X)
                        OnSplitterTriggerEvent(obj, SplitterTriggerDirection.FORWARD);

                    break;
            }
        }
    }

    public enum DynamicProperty
    {
        OBJECT_TILE,
        BACKGROUND_TILE,
        PALETTE
    }

    public class ChangeDynamicPropertyTrigger : SplitterTrigger
    {
        public DynamicProperty Property
        {
            get; set;
        }

        public int Forward
        {
            get; set;
        }

        public int Backward
        {
            get; set;
        }

        public ChangeDynamicPropertyTrigger(GameEngine engine, Box box, DynamicProperty prop, int forward, int backward, SplitterTriggerOrientation orientation = SplitterTriggerOrientation.VERTICAL) : base(engine, box, orientation, VectorKind.PLAYER_ORIGIN)
        {
            Property = prop;
            Forward = forward;
            Backward = backward;
        }

        private void ChangeProperty(int value)
        {
            switch (Property)
            {
                case DynamicProperty.OBJECT_TILE:
                    Engine.ObjectTile = value;
                    break;

                case DynamicProperty.BACKGROUND_TILE:
                    Engine.BackgroundTile = value;
                    break;

                case DynamicProperty.PALETTE:
                    Engine.Palette = value;
                    break;
            }
        }

        protected override void OnSplitterTriggerEvent(Entity obj, SplitterTriggerDirection direction)
        {
            base.OnSplitterTriggerEvent(obj, direction);

            if (obj is not Player)
                return;

            switch (direction)
            {
                case SplitterTriggerDirection.BACKWARD:
                    ChangeProperty(Backward);
                    break;

                case SplitterTriggerDirection.FORWARD:
                    ChangeProperty(Forward);
                    break;
            }
        }
    }

    public class CheckpointSplitterTrigger : SplitterTrigger
    {
        public Checkpoint Checkpoint
        {
            get;
            set;
        }

        public SplitterTriggerDirection CheckpointDirection
        {
            get;
            set;
        }

        public Checkpoint LastCheckpoint
        {
            get;
            private set;
        }

        public Box LastCameraConstraintBox
        {
            get;
            private set;
        }

        public int LastObjectTile
        {
            get;
            private set;
        }

        public int LastBackgroundTile
        {
            get;
            private set;
        }

        public int LastPalette
        {
            get;
            private set;
        }

        public CheckpointSplitterTrigger(GameEngine engine, Box box, Checkpoint checkpoint, SplitterTriggerOrientation orientation = SplitterTriggerOrientation.VERTICAL, SplitterTriggerDirection checkpointDirection = SplitterTriggerDirection.FORWARD) :
            base(engine, box, orientation, VectorKind.PLAYER_ORIGIN)
        {
            Checkpoint = checkpoint;
            CheckpointDirection = checkpointDirection;
            LastCheckpoint = null;
        }

        protected override void OnSplitterTriggerEvent(Entity obj, SplitterTriggerDirection direction)
        {
            base.OnSplitterTriggerEvent(obj, direction);

            if (obj is Player)
            {
                if (direction == CheckpointDirection)
                {
                    LastObjectTile = Engine.ObjectTile;
                    LastBackgroundTile = Engine.BackgroundTile;
                    LastPalette = Engine.Palette;
                    LastCheckpoint = Engine.CurrentCheckpoint;
                    LastCameraConstraintBox = Engine.cameraConstraintsBox;
                    Engine.CurrentCheckpoint = Checkpoint;
                }
                else
                {
                    Engine.SetCheckpoint(LastCheckpoint, LastObjectTile, LastBackgroundTile, LastPalette);
                    Engine.cameraConstraintsBox = LastCameraConstraintBox;
                }
            }
        }
    }

    public class CheckpointTriggerOnce : Trigger
    {
        public Checkpoint Checkpoint
        {
            get;
            set;
        }

        public bool Triggered
        {
            get;
            private set;
        }

        public Checkpoint LastCheckpoint
        {
            get;
            private set;
        }

        public Box LastCameraConstraintBox
        {
            get;
            private set;
        }

        public int LastObjectTile
        {
            get;
            private set;
        }

        public int LastBackgroundTile
        {
            get;
            private set;
        }

        public int LastPalette
        {
            get;
            private set;
        }

        public CheckpointTriggerOnce(GameEngine engine, Box box, Checkpoint checkpoint) :
            base(engine, box, TouchingKind.VECTOR, VectorKind.PLAYER_ORIGIN)
        {
            Checkpoint = checkpoint;
            Triggered = false;
            LastCheckpoint = null;
        }

        protected override void OnTrigger(Entity obj)
        {
            base.OnTrigger(obj);

            if (!Triggered && obj is Player)
            {
                Triggered = true;
                LastObjectTile = Engine.ObjectTile;
                LastBackgroundTile = Engine.BackgroundTile;
                LastPalette = Engine.Palette;
                LastCheckpoint = Engine.CurrentCheckpoint;
                LastCameraConstraintBox = Engine.cameraConstraintsBox;
                Engine.CurrentCheckpoint = Checkpoint;
            }
        }
    }

    public class CameraLockTrigger : AbstractTrigger
    {
        private readonly List<Vector> constraints;

        public IEnumerable<Vector> Constraints => constraints;

        public Vector ConstraintOrigin => BoundingBox.Center;

        public int ConstraintCount => constraints.Count;

        public CameraLockTrigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox, TouchingKind.VECTOR, VectorKind.PLAYER_ORIGIN) => constraints = new List<Vector>();

        public CameraLockTrigger(GameEngine engine, Box boudingBox, IEnumerable<Vector> constraints) :
            base(engine, boudingBox, TouchingKind.VECTOR, VectorKind.PLAYER_ORIGIN) => this.constraints = new List<Vector>(constraints);

        protected override void OnTrigger(Entity obj)
        {
            base.OnTrigger(obj);

            if (obj is not Player)
                return;

            Engine.SetCameraConstraints(ConstraintOrigin, constraints);
        }

        public void AddConstraint(Vector constraint) => constraints.Add(constraint);

        public Vector GetConstraint(int index) => constraints[index];

        public bool ContainsConstraint(Vector constraint) => constraints.Contains(constraint);

        public void ClearConstraints() => constraints.Clear();
    }
}
