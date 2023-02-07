using XSharp.Geometry;

namespace XSharp.Engine.Entities.Logical
{
    public abstract class LogicalEntity : Entity, IEnableDisable
    {
        public bool Enabled
        {
            get;
            set;
        }

        protected LogicalEntity()
        {
        }

        protected override Box GetHitbox()
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