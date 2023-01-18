﻿using MMX.Geometry;

namespace MMX.Engine
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
            base(engine, boundingBox.Origin)
        {
            Point = point;
            CharacterPos = characterPos;
            CameraPos = cameraPos;
            BackgroundPos = backgroundPos;
            ForceBackground = forceBackground;
            Scroll = scroll;

            this.boundingBox = boundingBox - boundingBox.Origin;
        }

        protected override Box GetBoundingBox() => Origin + boundingBox;

        protected override void SetBoundingBox(Box boundingBox)
        {
            this.boundingBox = boundingBox - boundingBox.Origin;
            SetOrigin(boundingBox.Origin);
        }
    }
}