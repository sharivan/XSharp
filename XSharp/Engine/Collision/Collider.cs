using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Collision
{
    public class Collider
    {
        private Box box;
        private bool checkCollisionWithWorld;
        private bool checkCollisionWithSolidSprites;

        protected TracerCollisionChecker collisionChecker;

        protected CollisionFlags leftMaskFlags = CollisionFlags.NONE;
        protected CollisionFlags upMaskFlags = CollisionFlags.NONE;
        protected CollisionFlags rightMaskFlags = CollisionFlags.NONE;
        protected CollisionFlags downMaskFlags = CollisionFlags.NONE;
        protected CollisionFlags innerMaskFlags = CollisionFlags.NONE;

        protected bool leftMaskComputed = false;
        protected bool upMaskComputed = false;
        protected bool rightMaskComputed = false;
        protected bool downMaskComputed = false;
        protected bool innerMaskComputed = false;

        public Sprite Owner
        {
            get;
        }

        public Box Box
        {
            get => box;

            set
            {
                box = value;
                UpdateColliders();
            }
        }

        public EntityList<Sprite> IgnoreSprites
        {
            get;
        }

        public bool CheckCollisionWithWorld
        {
            get => checkCollisionWithWorld;
            set
            {
                checkCollisionWithWorld = value;
                UpdateColliders();
            }
        }

        public bool CheckCollisionWithSolidSprites
        {
            get => checkCollisionWithSolidSprites;
            set
            {
                checkCollisionWithSolidSprites = value;
                UpdateColliders();
            }
        }

        public bool BlockedLeft => LeftMaskFlags.CanBlockTheMove(Direction.LEFT);

        public CollisionFlags LeftMaskFlags
        {
            get
            {
                if (!leftMaskComputed)
                    UpdateFlags();

                return leftMaskFlags;
            }
        }

        public bool BlockedUp => UpMaskFlags.CanBlockTheMove(Direction.UP);

        public CollisionFlags UpMaskFlags
        {
            get
            {
                if (!upMaskComputed)
                    UpdateFlags();

                return upMaskFlags;
            }
        }

        public bool BlockedRight => RightMaskFlags.CanBlockTheMove(Direction.RIGHT);

        public CollisionFlags RightMaskFlags
        {
            get
            {
                if (!rightMaskComputed)
                    UpdateFlags();

                return rightMaskFlags;
            }
        }

        public bool Landed => LandedOnBlock || LandedOnSlope || LandedOnTopLadder;

        public CollisionFlags DownMaskFlags
        {
            get
            {
                if (!downMaskComputed)
                    UpdateFlags();

                return downMaskFlags;
            }
        }

        public CollisionFlags InnerMaskFlags
        {
            get
            {
                if (!innerMaskComputed)
                    UpdateFlags();

                return innerMaskFlags;
            }
        }

        public bool UseCollisionPlacements
        {
            get;
            private set;
        }

        public bool LandedOnBlock => DownMaskFlags == CollisionFlags.BLOCK;

        public bool LandedOnSlope => DownMaskFlags == CollisionFlags.SLOPE;

        public bool LandedOnTopLadder => DownMaskFlags == CollisionFlags.TOP_LADDER;

        public RightTriangle LandedSlope
        {
            get;
            private set;
        }

        public bool Underwater => (innerMaskFlags & CollisionFlags.WATER) != 0;

        public bool TouchingWaterSurface => (innerMaskFlags & CollisionFlags.WATER_SURFACE) != 0;

        public Collider(Sprite owner, Box box, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
        {
            Owner = owner;
            this.box = box;
            UseCollisionPlacements = useCollisionPlacements;
            this.checkCollisionWithWorld = checkCollisionWithWorld;
            this.checkCollisionWithSolidSprites = checkCollisionWithSolidSprites;

            IgnoreSprites = new EntityList<Sprite>(owner);

            collisionChecker = new TracerCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };
        }

        public void ClearIgnoredSprites()
        {
            IgnoreSprites.Clear();
            IgnoreSprites.Add(Owner);
        }

        public void AddIgnoredSprite(Sprite sprite)
        {
            IgnoreSprites.Add(sprite);
        }

        protected virtual void UpdateColliders()
        {
            collisionChecker.Setup(box, CollisionFlags.NONE, IgnoreSprites, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);

            UpdateFlags();
        }

        private void UpdateFlags()
        {
            TracerCollisionChecker downCollisionChecker = GetMoveDownCollisionChecker();
            downMaskFlags = downCollisionChecker.ComputeLandedState();
            if (downMaskFlags == CollisionFlags.SLOPE)
                LandedSlope = downCollisionChecker.SlopeTriangle;

            upMaskFlags = GetMoveUpCollisionChecker().GetTouchingFlagsUp();
            leftMaskFlags = GetMoveLeftCollisionChecker().GetTouchingFlagsLeft();
            rightMaskFlags = GetMoveRightCollisionChecker().GetTouchingFlagsRight();
            innerMaskFlags = collisionChecker.GetCollisionFlags();

            leftMaskComputed = true;
            upMaskComputed = true;
            rightMaskComputed = true;
            downMaskComputed = true;
            innerMaskComputed = true;
        }

        protected virtual Box GetMoveLeftBox()
        {
            return box;
        }

        protected virtual Box GetMoveUpBox()
        {
            return box;
        }

        protected virtual Box GetMoveRightBox()
        {
            return box;
        }

        protected virtual Box GetMoveDownBox()
        {
            return box;
        }

        protected virtual TracerCollisionChecker GetMoveLeftCollisionChecker()
        {
            return collisionChecker;
        }

        protected virtual TracerCollisionChecker GetMoveUpCollisionChecker()
        {
            return collisionChecker;
        }

        protected virtual TracerCollisionChecker GetMoveRightCollisionChecker()
        {
            return collisionChecker;
        }

        protected virtual TracerCollisionChecker GetMoveDownCollisionChecker()
        {
            return collisionChecker;
        }

        public void Translate(Vector delta)
        {
            box += delta;
            UpdateColliders();
        }

        public CollisionFlags MoveContactSolidHorizontal(FixedSingle dx, Direction masks = Direction.LEFTRIGHT, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (dx == 0)
                return CollisionFlags.NONE;

            Box newBox;
            Vector delta;
            CollisionFlags flags = CollisionFlags.NONE;
            TracerCollisionChecker collisionChecker = null;

            if (dx > 0)
            {
                Box moveBox = GetMoveRightBox();

                if (masks.HasFlag(Direction.RIGHT))
                {
                    collisionChecker = GetMoveRightCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidHorizontal(dx);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + (dx, 0);

                delta = newBox.Origin - moveBox.Origin;
            }
            else
            {
                Box moveBox = GetMoveLeftBox();

                if (masks.HasFlag(Direction.RIGHT))
                {
                    collisionChecker = GetMoveLeftCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidHorizontal(dx);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + (dx, 0);

                delta = newBox.Origin - moveBox.Origin;
            }

            //Box union = Box | newBox;
            Box += delta.TruncFracPart();

            return flags;
        }

        public CollisionFlags MoveContactSolidDiagonalHorizontal(Vector delta, Direction masks = Direction.LEFTRIGHT, CollisionFlags ignore = CollisionFlags.NONE)
        {
            // TODO : Implement checking collising top and bottom here

            if (delta.X == 0)
                return CollisionFlags.NONE;

            Box newBox;
            CollisionFlags flags = CollisionFlags.NONE;

            if (delta.X > 0)
            {
                Box moveBox = GetMoveRightBox();

                if (masks.HasFlag(Direction.RIGHT))
                {
                    TracerCollisionChecker collisionChecker = GetMoveRightCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidDiagonalHorizontal(delta);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + delta;

                delta = newBox.Origin - moveBox.Origin;
            }
            else
            {
                Box moveBox = GetMoveLeftBox();

                if (masks.HasFlag(Direction.RIGHT))
                {
                    TracerCollisionChecker collisionChecker = GetMoveLeftCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidDiagonalHorizontal(delta);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + delta;

                delta = newBox.Origin - moveBox.Origin;
            }

            Box += delta.TruncFracPart();

            return flags;
        }

        public CollisionFlags MoveContactSolidVertical(FixedSingle dy, Direction masks = Direction.UPDOWN, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (dy == 0)
                return CollisionFlags.NONE;

            Box newBox;
            Vector delta;
            CollisionFlags flags = CollisionFlags.NONE;

            if (dy > 0)
            {
                Box moveBox = GetMoveDownBox();

                if (masks.HasFlag(Direction.DOWN))
                {
                    TracerCollisionChecker collisionChecker = GetMoveDownCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidVertical(dy);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + (0, dy);

                delta = newBox.Origin - moveBox.Origin;
            }
            else
            {
                Box moveBox = GetMoveUpBox();

                if (masks.HasFlag(Direction.UP))
                {
                    TracerCollisionChecker collisionChecker = GetMoveUpCollisionChecker();
                    collisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                    flags = collisionChecker.MoveContactSolidVertical(dy);
                    newBox = collisionChecker.TestBox;
                }
                else
                    newBox = moveBox + (0, dy);

                delta = newBox.Origin - moveBox.Origin;
            }

            Box union = Box | newBox;
            Box += delta.TruncFracPart();

            return flags;
        }

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public void AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            TracerCollisionChecker collisionChecker = GetMoveDownCollisionChecker();

            Vector lastOrigin = collisionChecker.TestBox.Origin;
            collisionChecker.IgnoreFlags = ignore;
            collisionChecker.AdjustOnTheFloor(maxDistance);

            Vector delta = collisionChecker.TestBox.Origin - lastOrigin;
            Box += delta.TruncFracPart();
        }

        public void AdjustOnTheLadder()
        {
            if (!UseCollisionPlacements)
                return;

            if (LandedOnTopLadder)
            {
                TracerCollisionChecker collisionChecker = GetMoveDownCollisionChecker();

                foreach (var placement in collisionChecker.Placements)
                    if (placement.CollisionData == CollisionData.TOP_LADDER)
                    {
                        Box placementBox = placement.ObstableBox;
                        FixedSingle delta = placementBox.Left + MAP_SIZE * 0.5 - Box.Left - Box.Width * 0.5;
                        Box += delta.TruncFracPart() * Vector.RIGHT_VECTOR;
                        return;
                    }
            }
            else
            {
                TracerCollisionChecker collisionChecker = GetMoveUpCollisionChecker();

                foreach (var placement in collisionChecker.Placements)
                    if (placement.CollisionData == CollisionData.LADDER)
                    {
                        Box placementBox = placement.ObstableBox;
                        FixedSingle delta = placementBox.Left + MAP_SIZE * 0.5 - Box.Left - Box.Width * 0.5;
                        Box += delta.TruncFracPart() * Vector.RIGHT_VECTOR;
                        return;
                    }
            }
        }

        public bool IsTouchingLeft(Box other)
        {
            Box box = GetMoveLeftBox();
            return box.ClipLeft(-STEP_SIZE).IsOverlaping(other);
        }

        public bool IsTouchingRight(Box other)
        {
            Box box = GetMoveRightBox();
            return box.ClipRight(-STEP_SIZE).IsOverlaping(other);
        }

        public bool IsTouchingUp(Box other)
        {
            Box box = GetMoveUpBox();
            return box.ClipTop(-STEP_SIZE).IsOverlaping(other);
        }

        public bool IsTouchingDown(Box other)
        {
            Box box = GetMoveDownBox();
            return box.ClipBottom(-STEP_SIZE).IsOverlaping(other);
        }
    }
}