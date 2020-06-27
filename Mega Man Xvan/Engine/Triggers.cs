using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public delegate void TriggerEvent(MMXObject obj);

    public abstract class AbstractTrigger : MMXObject
    {
        private MMXBox boundingBox;

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

        protected AbstractTrigger(GameEngine engine, MMXBox boundingBox) :
            base(engine, boundingBox.Origin)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;

            enabled = true;
            maxTriggers = uint.MaxValue;
        }

        protected override void OnStartTouch(MMXObject obj)
        {
            if (enabled && triggers < maxTriggers)
            {
                triggers++;
                DoTrigger(obj);
            }
        }

        protected virtual void DoTrigger(MMXObject obj)
        {
            TriggerEvent?.Invoke(obj);
        }

        protected override MMXBox GetBoundingBox()
        {
            return Origin + boundingBox;
        }

        protected override void SetBoundingBox(MMXBox boundingBox)
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

        public Trigger(GameEngine engine, MMXBox boudingBox) :
            base(engine, boudingBox)
        {
            uint i = MaxTriggers;
        }
    }

    public class CameraEventTrigger : AbstractTrigger
    {
        private List<MMXVector> extensions;

        public MMXVector ExtensionOrigin
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

        public CameraEventTrigger(GameEngine engine, MMXBox boudingBox) :
            base(engine, boudingBox)
        {
            extensions = new List<MMXVector>();
        }

        public CameraEventTrigger(GameEngine engine, MMXBox boudingBox, IEnumerable<MMXVector> extensions) :
            base(engine, boudingBox)
        {
            this.extensions = new List<MMXVector>(extensions);
        }

        protected override void DoTrigger(MMXObject obj)
        {
            base.DoTrigger(obj);

            Player player = obj as Player;
            if (player == null)
                return;

            engine.SetExtensions(ExtensionOrigin, extensions);
        }

        public void AddExtension(MMXVector extension)
        {
            extensions.Add(extension);
        }

        public MMXVector GetExtension(int index)
        {
            return extensions[index];
        }

        public bool ContainsExtension(MMXVector extension)
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
        private MMXVector characterPos;
        private MMXVector cameraPos;
        private MMXVector backgroundPos;
        private MMXVector forceBackground;
        private uint scroll;

        public MMXVector CharacterPos
        {
            get
            {
                return characterPos;
            }
        }

        public MMXVector CameraPos
        {
            get
            {
                return cameraPos;
            }
        }

        public MMXVector BackgroundPos
        {
            get
            {
                return backgroundPos;
            }
        }

        public MMXVector ForceBackground
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

        public Checkpoint(GameEngine engine, MMXBox boudingBox, MMXVector characterPos, MMXVector cameraPos, MMXVector backgroundPos, MMXVector forceBackground, uint scroll) :
            base(engine, boudingBox)
        {
            this.characterPos = characterPos;
            this.cameraPos = cameraPos;
            this.backgroundPos = backgroundPos;
            this.forceBackground = forceBackground;
            this.scroll = scroll;
        }

        internal void UpdateBoudingBox()
        {
            MMXBox boundingBox = engine.cameraConstraintsBox;
            MMXFloat minX = boundingBox.Left;
            MMXFloat minY = boundingBox.Top;
            MMXFloat maxX = boundingBox.Right;
            MMXFloat maxY = boundingBox.Bottom;

            foreach (MMXVector extension in engine.extensions)
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

            engine.cameraConstraintsBox = new MMXBox(minX, minY, maxX - minX, maxY - minY);
        }

        protected override void DoTrigger(MMXObject obj)
        {
            base.DoTrigger(obj);

            if (engine.currentCheckpoint == this)
                return;

            if (!(obj is Player))
                return;

            if (engine.currentCheckpoint == null)
                engine.cameraConstraintsBox = BoundingBox;
            else
                engine.cameraConstraintsBox |= BoundingBox;

            engine.currentCheckpoint = this;

            UpdateBoudingBox();

            //Enabled = false;
        }

        protected override void OnEndTouch(MMXObject obj)
        {
            if (!(obj is Player))
                return;

            //if (engine.currentCheckpoint == this)
            //    engine.currentCheckpoint = null;
        }
    }
}
