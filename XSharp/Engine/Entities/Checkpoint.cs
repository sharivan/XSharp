using MMX.Geometry;

namespace MMX.Engine.Entities
{
    public class Checkpoint : Entity
    {
        private Box boundingBox;

        public ushort Point
        {
            get;
        }

        public Vector CharacterPos
        {
            get;
        }

        public Vector CameraPos
        {
            get;
        }

        public Vector BackgroundPos
        {
            get;
        }

        public Vector ForceBackground
        {
            get;
        }

        public uint Scroll
        {
            get;
        }

        public Checkpoint(GameEngine engine, ushort point, Box boundingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll) :
            base(engine, "Checkpoint #" + point, boundingBox.Origin)
        {
            Point = point;
            CharacterPos = characterPos;
            CameraPos = cameraPos;
            BackgroundPos = backgroundPos;
            ForceBackground = forceBackground;
            Scroll = scroll;

            SetBoundingBox(boundingBox);
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