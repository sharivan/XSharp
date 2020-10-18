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

        private bool enabled;
        private uint triggers;
        protected uint maxTriggers;

        public event TriggerEvent TriggerEvent;

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }

        public uint Triggers
        {
            get
            {
                return triggers;
            }
        }

        public bool Once
        {
            get
            {
                return maxTriggers == 1;
            }
        }

        public uint MaxTriggers
        {
            get
            {
                return maxTriggers;
            }
        }

        protected AbstractTrigger(GameEngine engine, Box boundingBox) :
            base(engine, boundingBox.Origin)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;

            enabled = true;
            maxTriggers = uint.MaxValue;
        }

        protected override void OnStartTouch(Entity obj)
        {
            if (enabled && triggers < maxTriggers)
            {
                triggers++;
                DoTrigger(obj);
            }
        }

        protected virtual void DoTrigger(Entity obj)
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

    public class Trigger : AbstractTrigger
    {
        public new bool Once
        {
            get
            {
                return base.Once;
            }

            set
            {
                maxTriggers = value ? 1 : uint.MaxValue;
            }
        }

        public new uint MaxTriggers
        {
            get
            {
                return base.MaxTriggers;
            }

            set
            {
                maxTriggers = value;
            }
        }

        public Trigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox)
        {
            uint i = MaxTriggers;
        }
    }

    public class CameraLockTrigger : AbstractTrigger
    {
        private List<Vector> extensions;

        public Vector ExtensionOrigin
        {
            get
            {
                return BoundingBox.Center;
            }
        }

        public int ExtensionCount
        {
            get
            {
                return extensions.Count;
            }
        }

        public CameraLockTrigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox)
        {
            extensions = new List<Vector>();
        }

        public CameraLockTrigger(GameEngine engine, Box boudingBox, IEnumerable<Vector> extensions) :
            base(engine, boudingBox)
        {
            this.extensions = new List<Vector>(extensions);
        }

        protected override void DoTrigger(Entity obj)
        {
            base.DoTrigger(obj);

            Player player = obj as Player;
            if (player == null)
                return;

            engine.SetExtensions(ExtensionOrigin, extensions);
        }

        public void AddExtension(Vector extension)
        {
            extensions.Add(extension);
        }

        public Vector GetExtension(int index)
        {
            return extensions[index];
        }

        public bool ContainsExtension(Vector extension)
        {
            return extensions.Contains(extension);
        }

        public void ClearExtensions()
        {
            extensions.Clear();
        }
    }

    public class Checkpoint : AbstractTrigger
    {
        private int point;
        private Vector characterPos;
        private Vector cameraPos;
        private Vector backgroundPos;
        private Vector forceBackground;
        private uint scroll;

        public int Point
        {
            get
            {
                return point;
            }
        }

        public Vector CharacterPos
        {
            get
            {
                return characterPos;
            }
        }

        public Vector CameraPos
        {
            get
            {
                return cameraPos;
            }
        }

        public Vector BackgroundPos
        {
            get
            {
                return backgroundPos;
            }
        }

        public Vector ForceBackground
        {
            get
            {
                return forceBackground;
            }
        }

        public uint Scroll
        {
            get
            {
                return scroll;
            }
        }

        public Checkpoint(GameEngine engine, int point, Box boudingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll) :
            base(engine, boudingBox)
        {
            this.point = point;
            this.characterPos = characterPos;
            this.cameraPos = cameraPos;
            this.backgroundPos = backgroundPos;
            this.forceBackground = forceBackground;
            this.scroll = scroll;
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

            if (!(obj is Player))
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
            if (!(obj is Player))
                return;

            //if (engine.currentCheckpoint == this)
            //    engine.currentCheckpoint = null;
        }
    }
}
