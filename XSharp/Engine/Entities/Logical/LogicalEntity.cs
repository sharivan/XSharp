using XSharp.Geometry;

namespace XSharp.Engine.Entities.Logical
{
    public abstract class LogicalEntity : Entity
    {
        public bool Enabled
        {
            get;
            set;
        }

        public LogicalEntity(Vector origin) : this(null, origin)
        {
        }

        public LogicalEntity(string name, Vector origin) : base(name, origin)
        {
        }

        protected override Box GetBoundingBox()
        {
            return Box.EMPTY_BOX;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Enabled = true;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }
    }
}