using MMX.Geometry;
using MMX.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public delegate void TriggerEvent(Entity obj);

    public abstract class AbstractTrigger : Entity
    {
        private Box boundingBox;
        protected uint maxTriggers;

        public event TriggerEvent TriggerEvent;

        public bool Enabled { get;
            set; }

        public uint Triggers { get;
            private set;
        }

        public bool Once => maxTriggers == 1;

        public uint MaxTriggers => maxTriggers;

        protected AbstractTrigger(GameEngine engine, Box boundingBox) :
            base(engine, boundingBox.Origin)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;

            Enabled = true;
            maxTriggers = uint.MaxValue;
        }

        protected override void OnStartTouch(Entity obj)
        {
            if (Enabled && Triggers < maxTriggers)
            {
                Triggers++;
                DoTrigger(obj);
            }
        }

        protected virtual void DoTrigger(Entity obj) => TriggerEvent?.Invoke(obj);

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

            set => maxTriggers = value ? 1 : uint.MaxValue;
        }

        public new uint MaxTriggers
        {
            get => base.MaxTriggers;

            set => maxTriggers = value;
        }

        public Trigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox)
        {
            uint i = MaxTriggers;
        }
    }

    public class CameraLockTrigger : AbstractTrigger
    {
        private readonly List<Vector> extensions;

        public Vector ExtensionOrigin => BoundingBox.Center;

        public int ExtensionCount => extensions.Count;

        public CameraLockTrigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox) => extensions = new List<Vector>();

        public CameraLockTrigger(GameEngine engine, Box boudingBox, IEnumerable<Vector> extensions) :
            base(engine, boudingBox) => this.extensions = new List<Vector>(extensions);

        protected override void DoTrigger(Entity obj)
        {
            base.DoTrigger(obj);

            if (obj is not Player)
                return;

            engine.SetExtensions(ExtensionOrigin, extensions);
        }

        public void AddExtension(Vector extension) => extensions.Add(extension);

        public Vector GetExtension(int index) => extensions[index];

        public bool ContainsExtension(Vector extension) => extensions.Contains(extension);

        public void ClearExtensions() => extensions.Clear();
    }

    public class Checkpoint : AbstractTrigger
    {
        public int Point { get; }

        public Vector CharacterPos { get; }

        public Vector CameraPos { get; }

        public Vector BackgroundPos { get; }

        public Vector ForceBackground
        {
            get;
        }

        public uint Scroll { get; }

        public Checkpoint(GameEngine engine, int point, Box boudingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll) :
            base(engine, boudingBox)
        {
            this.Point = point;
            this.CharacterPos = characterPos;
            this.CameraPos = cameraPos;
            this.BackgroundPos = backgroundPos;
            this.ForceBackground = forceBackground;
            this.Scroll = scroll;
        }

        internal void UpdateBoudingBox()
        {
            Box boundingBox = engine.cameraConstraintsBox;
            FixedSingle minX = boundingBox.Left;
            FixedSingle minY = boundingBox.Top;
            FixedSingle maxX = boundingBox.Right;
            FixedSingle maxY = boundingBox.Bottom;

            foreach (Vector extension in engine.extensions)
            {
                if (extension.Y == 0)
                {
                    if (extension.X < 0)
                        minX = engine.extensionOrigin.X + extension.X;
                    else
                        maxX = engine.extensionOrigin.X + extension.X;
                }
                else if (extension.X == 0)
                {
                    if (extension.Y < 0)
                        minY = engine.extensionOrigin.Y + extension.Y;
                    else
                        maxY = engine.extensionOrigin.Y + extension.Y;
                }
            }

            engine.cameraConstraintsBox = new Box(minX, minY, maxX - minX, maxY - minY);
        }

        protected override void DoTrigger(Entity obj)
        {
            base.DoTrigger(obj);

            if (engine.CurrentCheckpoint == this)
                return;

            if (obj is not Player)
                return;

            if (engine.CurrentCheckpoint == null)
                engine.cameraConstraintsBox = BoundingBox;
            else
                engine.cameraConstraintsBox |= BoundingBox;

            engine.CurrentCheckpoint = this;

            UpdateBoudingBox();

            //Enabled = false;
        }

        protected override void OnEndTouch(Entity obj)
        {
            if (obj is not Player)
                return;

            //if (engine.currentCheckpoint == this)
            //    engine.currentCheckpoint = null;
        }
    }
}
