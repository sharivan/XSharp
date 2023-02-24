using XSharp.Engine.Entities;
using XSharp.Engine.Entities.Items;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;
using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine
{
    public class Camera : Entity
    {
        private Entity focusOn = null;

        private Vector vel = Vector.NULL_VECTOR;
        private FixedSingle moveDistance;
        private FixedSingle moveStep;
        private Vector moveToCenter;
        private bool moveToFocus = false;

        public bool NoConstraints
        {
            get;
            set;
        }

        public FixedSingle Width
        {
            get => Size.X;
            set => Size = (value, Height);
        }

        public FixedSingle Height
        {
            get => Size.Y;
            set => Size = (Width, value);
        }

        public Vector Size
        {
            get;
            set;
        }

        public static MMXWorld World => GameEngine.Engine.World;

        public bool SmoothOnNextMove
        {
            get;
            set;
        } = false;

        public FixedSingle SmoothSpeed
        {
            get;
            set;
        } = CAMERA_SMOOTH_SPEED;

        public FixedSingle MovingSpeed
        {
            get;
            private set;
        } = 0;

        public Vector LeftTop
        {
            get => Origin - Size * FixedSingle.HALF;
            set => Center = value + Size * FixedSingle.HALF;
        }

        public FixedSingle Left
        {
            get => LeftTop.X;
            set => LeftTop = (value, LeftTop.Y);
        }

        public FixedSingle Top
        {
            get => LeftTop.Y;
            set => LeftTop = (LeftTop.X, value);
        }

        public Vector LeftMiddle
        {
            get => (Center.X - Width * FixedSingle.HALF, Center.Y);
            set => Center = (value.X + Width * FixedSingle.HALF, value.Y);
        }

        public Vector LeftBottom
        {
            get => (Center.X - Width * FixedSingle.HALF, Center.Y + Height * FixedSingle.HALF);
            set => Center = (value.X + Width * FixedSingle.HALF, value.Y - Height * FixedSingle.HALF);
        }

        public Vector MiddleTop
        {
            get => (Center.X, Center.Y - Height * FixedSingle.HALF);
            set => Center = (value.X, value.Y + Height * FixedSingle.HALF);
        }

        public Vector Center
        {
            get => Origin;
            set
            {
                if (focusOn != null)
                    return;

                SetCenter(value);
            }
        }

        public Vector MiddleBottom
        {
            get => (Center.X, Center.Y + Height * FixedSingle.HALF);
            set => Center = (value.X, value.Y - Height * FixedSingle.HALF);
        }

        public Vector RightTop
        {
            get => (Center.X + Width * FixedSingle.HALF, Center.Y - Height * FixedSingle.HALF);
            set => Center = (value.X - Width * FixedSingle.HALF, value.Y + Height * FixedSingle.HALF);
        }

        public Vector RightMiddle
        {
            get => (Center.X + Width * FixedSingle.HALF, Center.Y);
            set => Center = (value.X - Width * FixedSingle.HALF, value.Y);
        }

        public Vector RightBottom
        {
            get => Center + Size * FixedSingle.HALF;
            set => Center = value - Size * FixedSingle.HALF;
        }

        public FixedSingle Right
        {
            get => RightBottom.X;
            set => RightBottom = (value, RightBottom.Y);
        }

        public FixedSingle Bottom
        {
            get => RightBottom.Y;
            set => RightBottom = (RightBottom.X, value);
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

        public Box BoundingBox
        {
            get
            {
                Vector sv2 = Size * FixedSingle.HALF;
                return new Box(Center, -sv2, sv2);
            }
        }

        public Box ExtendedBoundingBox
        {
            get
            {
                Vector sv2 = Size * FixedSingle.HALF + EXTENDED_BORDER_SCREEN_OFFSET;
                return new Box(Center, -sv2, sv2);
            }
        }

        public Vector Velocity
        {
            get => vel;
            set
            {
                vel = value.TruncFracPart();
                moveStep = vel.Length.TruncFracPart();
            }
        }

        public bool Moving => moveDistance >= STEP_SIZE;

        public Camera()
        {
            TouchingKind = TouchingKind.VECTOR;
        }

        protected override Box GetHitbox()
        {
            var box = ExtendedBoundingBox;
            return box - Origin;
        }

        private void SetCenter(Vector v)
        {
            SetCenter(v.X, v.Y);
        }

        public Vector ClampToBounds(Vector v)
        {
            return ClampToBounds(v.X, v.Y);
        }

        public Vector ClampToBounds(FixedSingle x, FixedSingle y)
        {
            Vector minCameraPos = Engine.MinCameraPos;
            Vector maxCameraPos = Engine.MaxCameraPos;

            FixedSingle w2 = Width * 0.5;
            FixedSingle h2 = Height * 0.5;

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

            return (x, y);
        }

        private void SetCenter(FixedSingle x, FixedSingle y)
        {
            Origin = ClampToBounds(x, y);
        }

        public void MoveToLeftTop(Vector dest)
        {
            MoveToCenter(dest + Size * FixedSingle.HALF, SmoothSpeed);
        }

        public void MoveToLeftTop(Vector dest, FixedSingle speed)
        {
            MoveToCenter(dest + Size * FixedSingle.HALF, speed);
        }

        public void MoveToCenter(Vector dest)
        {
            MoveToCenter(dest, SmoothSpeed);
        }

        public void MoveToCenter(Vector dest, FixedSingle speed)
        {
            if (speed < STEP_SIZE)
                return;

            dest = ClampToBounds(dest.TruncFracPart());
            Vector delta = dest - Center;
            FixedSingle moveDistance = delta.Length.TruncFracPart();
            if (moveDistance < STEP_SIZE)
            {
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
                return;
            }

            MovingSpeed = speed.TruncFracPart();
            this.moveDistance = moveDistance;
            vel = (delta * (speed / moveDistance)).TruncFracPart();
            moveStep = vel.Length.TruncFracPart();

            this.moveDistance -= moveStep;
            if (this.moveDistance < STEP_SIZE)
            {
                SetCenter(dest);
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
            }
            else
            {
                Vector oldCenter = Center;
                Vector newCenter = Center + vel;
                SetCenter(newCenter);

                if ((Center - oldCenter).Length.TruncFracPart() < STEP_SIZE)
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
            if (speed < STEP_SIZE || focusOn == null)
                return;

            Vector dest = focusOn.Origin;
            dest = ClampToBounds(dest);
            Vector delta = dest - Center;
            FixedSingle moveDistance = delta.Length.TruncFracPart();
            if (moveDistance < STEP_SIZE)
            {
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
                return;
            }

            MovingSpeed = speed.TruncFracPart();
            this.moveDistance = moveDistance;
            vel = (delta * (speed / moveDistance)).TruncFracPart();
            moveStep = vel.Length.TruncFracPart();

            this.moveDistance -= moveStep;
            if (this.moveDistance < STEP_SIZE)
            {
                SetCenter(dest);
                this.moveDistance = 0;
                MovingSpeed = 0;
                moveToCenter = Vector.NULL_VECTOR;
            }
            else
            {
                Vector oldCenter = Center;
                Vector newCenter = Center + vel;
                SetCenter(newCenter);

                if ((Center - oldCenter).Length.TruncFracPart() < STEP_SIZE)
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

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckTouchingEntities = true;
            CheckTouchingWithDeadEntities = true;
        }

        protected internal override void OnFrame()
        {
            base.OnFrame();

            if (Moving)
            {
                Vector oldCenter = Center;
                Vector newCenter = Center + vel;
                SetCenter(newCenter);

                if ((Center - oldCenter).Length.TruncFracPart() < STEP_SIZE)
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
                {
                    var focusOrigin = focusOn.Origin;
                    SetCenter(focusOrigin);
                }
            }
        }

        protected override void OnStartTouch(Entity entity)
        {
            base.OnStartTouch(entity);

            if (entity.Alive || entity.Spawning || !entity.Respawnable || !entity.RespawnOnNear)
                return;

            if ((!entity.Dead || Engine.FrameCounter - entity.DeathFrame >= entity.MinimumIntervalToRespawn) && !entity.IsOffscreen(VectorKind.ORIGIN))
            {
                // TODO : This needs a special check for Heart Tanks and Sub-Tanks
                if (entity is Item item && item.Collected)
                    return;

                if (entity is Sprite sprite)
                    sprite.Visible = true;

                entity.Spawn();
            }
        }
    }
}