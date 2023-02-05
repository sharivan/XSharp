using XSharp.Geometry;

namespace XSharp.Engine.Entities
{
    public class Checkpoint : Entity
    {
        private Box boundingBox;

        public ushort Point
        {
            get;
            set;
        }

        public Vector CharacterPos
        {
            get;
            set;
        }

        public Vector CameraPos
        {
            get;
            set;
        }

        public Vector BackgroundPos
        {
            get;
            set;
        }

        public Vector ForceBackground
        {
            get;
            set;
        }

        public uint Scroll
        {
            get;
            set;
        }

        public Checkpoint()
        {
        }

        protected override Box GetBoundingBox()
        {
            return boundingBox;
        }

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox;
        }
    }
}