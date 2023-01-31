using MMX.Engine.Entities;
using MMX.Geometry;
using MMX.Math;
using static MMX.Engine.Consts;

namespace MMX.Engine.World
{
    public class Camera
    {
        private Vector lastCenter;
        private Vector center;
        private Entity focusOn;

        private FixedSingle moveDistance;
        private FixedSingle moveStep;
        private Vector moveToCenter;
        private bool moveToFocus;
        private Vector vel;

        internal Camera(World world, FixedSingle width, FixedSingle height)
        {
            World = world;
            Width = width;
            Height = height;

            lastCenter = Vector.NULL_VECTOR;
            center = new Vector(width / 2, height / 2);
            vel = Vector.NULL_VECTOR;
            focusOn = null;
            SmoothOnNextMove = false;
            SmoothSpeed = DASH_SPEED;
            moveToFocus = false;
            MovingSpeed = 0;
        }

        internal Camera(World world, Camera other)
        {
            World = world;
            Width = other.Width;
            Height = other.Height;

            lastCenter = other.lastCenter;
            center = other.center;
            vel = other.vel;
            focusOn = other.focusOn;
            SmoothOnNextMove = false;
            SmoothSpeed = DASH_SPEED;
            moveToFocus = false;
            MovingSpeed = 0;
        }

        public World World
        {
            get;
        }

        public FixedSingle Width
        {
            get;
        }

        public FixedSingle Height
        {
            get;
        }

        public bool SmoothOnNextMove
        {
            get;
            set;
        }

        public FixedSingle SmoothSpeed
        {
            get;
            set;
        }

        public FixedSingle MovingSpeed
        {
            get;
            private set;
        }

        private void SetCenter(Vector v)
        {
            SetCenter(v.X, v.Y);
        }

        private void SetCenter(FixedSingle x, FixedSingle y)
        {
            Vector minCameraPos = World.Engine.MinCameraPos;
            Vector maxCameraPos = World.Engine.MaxCameraPos;

            FixedSingle w2 = Width / 2;
            FixedSingle h2 = Height / 2;

            FixedSingle minX = minCameraPos.X + w2;
            FixedSingle minY = minCameraPos.Y + h2;
            FixedSingle maxX = FixedSingle.Min(maxCameraPos.X, World.Width) - w2;
            FixedSingle maxY = FixedSingle.Min(maxCameraPos.Y, World.Height) - h2;

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

        public Vector LeftTop
        {
            get => center - SizeVector / 2;
            set => Center = value + SizeVector / 2;
        }

        public Vector LeftMiddle
        {
            get => (center.X - Width / 2, center.Y);
            set => Center = (value.X + Width / 2, value.Y);
        }

        public Vector LeftBottom
        {
            get => (center.X - Width / 2, center.Y + Height / 2);
            set => Center = (value.X + Width / 2, value.Y - Height / 2);
        }

        public Vector MiddleTop
        {
            get => (center.X, center.Y - Height / 2);
            set => Center = (value.X, value.Y + Height / 2);
        }

        public Vector Center
        {
            get => center;
            set
            {
                if (focusOn != null)
                    return;

                SetCenter(value);
            }
        }

        public Vector MiddleBottom
        {
            get => (center.X, center.Y + Height / 2);
            set => Center = (value.X, value.Y - Height / 2);
        }

        public Vector RightTop
        {
            get => (center.X + Width / 2, center.Y - Height / 2);
            set => Center = (value.X - Width / 2, value.Y + Height / 2);
        }

        public Vector RightMiddle
        {
            get => (center.X + Width / 2, center.Y);
            set => Center = (value.X - Width / 2, value.Y);
        }

        public Vector RightBottom
        {
            get => center + SizeVector / 2;
            set => Center = value - SizeVector / 2;
        }

        public Entity FocusOn
        {
            get => focusOn;
            set
            {
                focusOn = value;
                if (focusOn != null)
                    SetCenter(focusOn.Origin);
            }
        }

        public Vector SizeVector => new(Width, Height);

        public Box BoundingBox
        {
            get
            {
                Vector sv2 = SizeVector / 2;
                return new Box(center, -sv2, sv2);
            }
        }

        public Box ExtendedBoundingBox
        {
            get
            {
                Vector sv2 = SizeVector / 2 + EXTENDED_BORDER_SCREEN_OFFSET;
                return new Box(center, -sv2, sv2);
            }
        }

        public Vector Velocity
        {
            get => vel;
            set
            {
                vel = value;
                moveStep = vel.Length;
            }
        }

        public bool Moving => moveDistance > STEP_SIZE;

        public void MoveToLeftTop(Vector dest)
        {
            MoveToCenter(dest + SizeVector / 2, SmoothSpeed);
        }

        public void MoveToLeftTop(Vector dest, FixedSingle speed)
        {
            MoveToCenter(dest + SizeVector / 2, speed);
        }

        public void MoveToCenter(Vector dest)
        {
            MoveToCenter(dest, SmoothSpeed);
        }

        public void MoveToCenter(Vector dest, FixedSingle speed)
        {
            if (speed <= STEP_SIZE)
                return;

            Vector delta = dest - center;
            FixedSingle moveDistance = delta.Length;
            if (moveDistance <= STEP_SIZE)
            {
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
                return;
            }

            MovingSpeed = speed;
            this.moveDistance = moveDistance;
            vel = delta * (speed / moveDistance);
            moveStep = vel.Length;

            this.moveDistance -= moveStep;
            if (this.moveDistance <= STEP_SIZE)
            {
                SetCenter(dest);
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
            }
            else
            {
                Vector oldCenter = center;
                Vector newCenter = center + vel;
                SetCenter(newCenter);

                if (center == oldCenter)
                {
                    this.moveDistance = 0;
                    MovingSpeed = 0;
                    moveToCenter = Vector.NULL_VECTOR;
                }
                else
                    moveToCenter = dest;
            }

            moveToFocus = false;
        }

        public void MoveToFocus(FixedSingle speed)
        {
            if (speed <= STEP_SIZE || focusOn == null)
                return;

            Vector dest = focusOn.Origin + HITBOX_HEIGHT * Vector.UP_VECTOR;
            Vector delta = dest - center;
            FixedSingle moveDistance = delta.Length;
            if (moveDistance <= STEP_SIZE)
            {
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
                return;
            }

            MovingSpeed = speed;
            this.moveDistance = moveDistance;
            vel = delta * (speed / moveDistance);
            moveStep = vel.Length;

            this.moveDistance -= moveStep;
            if (this.moveDistance <= STEP_SIZE)
            {
                SetCenter(dest);
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
            }
            else
            {
                Vector oldCenter = center;
                Vector newCenter = center + vel;
                SetCenter(newCenter);

                if (center == oldCenter)
                {
                    this.moveDistance = 0;
                    MovingSpeed = 0;
                    moveToCenter = Vector.NULL_VECTOR;
                }
                else
                    moveToCenter = dest;
            }

            moveToFocus = true;
        }

        public void StopMoving()
        {
            moveDistance = 0;
        }

        public Box VisibleBox(Box box)
        {
            return BoundingBox & box;
        }

        public bool IsVisible(Box box)
        {
            return VisibleBox(box).IsValid(EPSLON);
        }

        public void OnFrame()
        {
            if (Moving)
            {
                Vector oldCenter = center;
                Vector newCenter = center + vel;
                SetCenter(newCenter);

                if (center == oldCenter)
                {
                    moveDistance = 0;
                    MovingSpeed = 0;
                    moveToFocus = false;
                }
                else if (moveToFocus)
                    MoveToFocus(MovingSpeed);
                else
                    MoveToCenter(moveToCenter, MovingSpeed);
            }
            else if (focusOn != null)
            {
                if (SmoothOnNextMove)
                {
                    SmoothOnNextMove = false;
                    MoveToFocus(SmoothSpeed);
                }
                else
                    SetCenter(focusOn.Origin + HITBOX_HEIGHT * Vector.UP_VECTOR);
            }

            if (center != lastCenter)
                lastCenter = center;
        }
    }
}
