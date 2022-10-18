using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.World
{
    public class Screen
    {
        private World world;
        private FixedSingle width;
        private FixedSingle height;

        private Vector lastCenter;
        private Vector center;
        private Entity focusOn;

        private FixedSingle moveDistance;
        private FixedSingle moveStep;
        private Vector moveTo;
        private Vector vel;

        internal Screen(World world, FixedSingle width, FixedSingle height)
        {
            this.world = world;
            this.width = width;
            this.height = height;

            lastCenter = Vector.NULL_VECTOR;
            center = new Vector(width / 2, height / 2);
            vel = Vector.NULL_VECTOR;
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

        public FixedSingle Width
        {
            get
            {
                return width;
            }
        }

        public FixedSingle Height
        {
            get
            {
                return height;
            }
        }

        private void SetLeftTop(Vector v)
        {
            SetCenter(v.X + width, v.Y + height);
        }

        private void SetCenter(Vector v)
        {
            SetCenter(v.X, v.Y);
        }

        private void SetCenter(FixedSingle x, FixedSingle y)
        {
            Vector minCameraPos = world.Engine.MinCameraPos;
            Vector maxCameraPos = world.Engine.MaxCameraPos;

            FixedSingle w2 = width / 2;
            FixedSingle h2 = height / 2;

            FixedSingle minX = minCameraPos.X + w2;
            FixedSingle minY = minCameraPos.Y + h2;
            FixedSingle maxX = FixedSingle.Min(maxCameraPos.X, world.Width) - w2;
            FixedSingle maxY = FixedSingle.Min(maxCameraPos.Y, world.Height) - h2;

            if (x < minX)
                x = minX;
            else if (x > maxX)
                x = maxX;

            if (y < minY)
                y = minY;
            else if (y > maxY)
                y = maxY;

            center = new Vector(x, y);
        }

        public Vector Center
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

        public Vector LeftTop
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

        public Vector RightBottom
        {
            get
            {
                return center + SizeVector / 2;
            }
        }

        public Entity FocusOn
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

        public Vector SizeVector
        {
            get
            {
                return new Vector(width, height);
            }
        }

        public Box BoudingBox
        {
            get
            {
                Vector sv2 = SizeVector / 2;
                return new Box(center, -sv2, sv2);
            }
        }

        public Vector Velocity
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

        public void MoveToLeftTop(Vector dest)
        {
            MoveToCenter(dest + SizeVector / 2, WALKING_SPEED);
        }

        public void MoveToLeftTop(Vector dest, FixedSingle speed)
        {
            MoveToCenter(dest + SizeVector / 2, speed);
        }

        public void MoveToCenter(Vector dest)
        {
            MoveToCenter(dest, WALKING_SPEED);
        }

        public void MoveToCenter(Vector dest, FixedSingle speed)
        {
            if (speed <= STEP_SIZE)
                return;

            Vector delta = dest - center;
            FixedSingle moveDistance = delta.Length;
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

        public Box VisibleBox(Box box)
        {
            return BoudingBox & box;
        }

        public bool IsVisible(Box box)
        {
            return VisibleBox(box).Area > 0;
        }

        public void OnFrame()
        {
            if (Moving)
            {
                Vector newCenter = center + vel;
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
                SetCenter(focusOn.Origin + HITBOX_HEIGHT * Vector.UP_VECTOR);

            if (center != lastCenter)
                lastCenter = center;
        }
    }
}
