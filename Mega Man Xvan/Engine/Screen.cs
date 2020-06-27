using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Screen
    {
        private World world;
        private MMXFloat width;
        private MMXFloat height;

        private MMXVector lastCenter;
        private MMXVector center;
        private MMXObject focusOn;

        private MMXFloat moveDistance;
        private MMXFloat moveStep;
        private MMXVector moveTo;
        private MMXVector vel;

        internal Screen(World world, MMXFloat width, MMXFloat height)
        {
            this.world = world;
            this.width = width;
            this.height = height;

            lastCenter = MMXVector.NULL_VECTOR;
            center = new MMXVector(width / 2, height / 2);
            vel = MMXVector.NULL_VECTOR;
            focusOn = null;           
        }

        internal Screen(World world, Screen other)
        {
            this.world = world;
            width = other.width;
            height = other.height;

            lastCenter = other.lastCenter;
            center = other.center;
            vel = other.vel;
            focusOn = other.focusOn;
        }

        public World World
        {
            get
            {
                return world;
            }
        }

        public MMXFloat Width
        {
            get
            {
                return width;
            }
        }

        public MMXFloat Height
        {
            get
            {
                return height;
            }
        }

        private void SetLeftTop(MMXVector v)
        {
            SetCenter(v.X + width, v.Y + height);
        }

        private void SetCenter(MMXVector v)
        {
            SetCenter(v.X, v.Y);
        }

        private void SetCenter(MMXFloat x, MMXFloat y)
        {
            MMXVector minCameraPos = world.Engine.MinCameraPos;
            MMXVector maxCameraPos = world.Engine.MaxCameraPos;

            MMXFloat w2 = width / 2;
            MMXFloat h2 = height / 2;

            MMXFloat minX = minCameraPos.X + w2;
            MMXFloat minY = minCameraPos.Y + h2;
            MMXFloat maxX = MMXFloat.Min(maxCameraPos.X, world.Width) - w2;
            MMXFloat maxY = MMXFloat.Min(maxCameraPos.Y, world.Height) - h2;

            if (x < minX)
                x = minX;
            else if (x > maxX)
                x = maxX;

            if (y < minY)
                y = minY;
            else if (y > maxY)
                y = maxY;

            center = new MMXVector(x, y);
        }

        public MMXVector Center
        {
            get
            {
                return center;
            }
            set
            {
                if (focusOn != null)
                    return;

                SetCenter(value);
            }
        }

        public MMXVector LeftTop
        {
            get
            {
                return center - SizeVector / 2;
            }

            set
            {
                SetLeftTop(value);
            }
        }

        public MMXVector RightBottom
        {
            get
            {
                return center + SizeVector / 2;
            }
        }

        public MMXObject FocusOn
        {
            get
            {
                return focusOn;
            }
            set
            {
                focusOn = value;
                if (focusOn != null)
                    SetCenter(focusOn.Origin);
            }
        }

        public MMXVector SizeVector
        {
            get
            {
                return new MMXVector(width, height);
            }
        }

        public MMXBox BoudingBox
        {
            get
            {
                MMXVector sv2 = SizeVector / 2;
                return new MMXBox(center, -sv2, sv2);
            }
        }

        public MMXVector Velocity
        {
            get
            {
                return vel;
            }
            set
            {
                vel = value;
                moveStep = vel.Length;
            }
        }

        public bool Moving
        {
            get
            {
                return moveDistance > STEP_SIZE;
            }
        }

        public void MoveToLeftTop(MMXVector dest)
        {
            MoveToCenter(dest + SizeVector / 2, WALKING_SPEED);
        }

        public void MoveToLeftTop(MMXVector dest, MMXFloat speed)
        {
            MoveToCenter(dest + SizeVector / 2, speed);
        }

        public void MoveToCenter(MMXVector dest)
        {
            MoveToCenter(dest, WALKING_SPEED);
        }

        public void MoveToCenter(MMXVector dest, MMXFloat speed)
        {
            if (speed <= STEP_SIZE)
                return;

            MMXVector delta = dest - center;
            MMXFloat moveDistance = delta.Length;
            if (moveDistance <= STEP_SIZE)
                return;

            this.moveDistance = moveDistance;
            vel = delta * speed / moveDistance;
            moveStep = vel.Length;
            moveTo = dest;
        }

        public void StopMoving()
        {
            moveDistance = 0;
        }

        public MMXBox VisibleBox(MMXBox box)
        {
            return BoudingBox & box;
        }

        public bool IsVisible(MMXBox box)
        {
            return VisibleBox(box).Area() > 0;
        }

        public void OnFrame()
        {
            if (Moving)
            {
                MMXVector newCenter = center + vel;
                moveDistance -= moveStep;
                if (moveDistance <= STEP_SIZE)
                {
                    SetCenter(moveTo);
                    moveDistance = 0;
                }
                else
                    SetCenter(newCenter);
            }
            else if (focusOn != null)
                SetCenter(focusOn.Origin + HITBOX_HEIGHT * MMXVector.UP_VECTOR);

            if (center != lastCenter)
                lastCenter = center;
        }
    }
}
