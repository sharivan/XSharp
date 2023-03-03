using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Collision
{
    public class SpriteCollider
    {
        private Box box;
        private bool checkCollisionWithWorld;
        private bool checkCollisionWithSolidSprites;
        private FixedSingle headHeight;
        private FixedSingle legsHeight;

        private LanderCollisionChecker leftCollisionChecker;
        private LanderCollisionChecker rightCollisionChecker;
        private LanderCollisionChecker upCollisionChecker;
        private LanderCollisionChecker downCollisionChecker;
        private LanderCollisionChecker innerCollisionChecker;

        private CollisionFlags leftMaskFlags;
        private CollisionFlags upMaskFlags;
        private CollisionFlags rightMaskFlags;
        private CollisionFlags innerMaskFlags;

        private bool leftMaskComputed;
        private bool upMaskComputed;
        private bool rightMaskComputed;
        private bool innerMaskComputed;

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

        public FixedSingle HeadHeight
        {
            get => headHeight;

            set
            {
                headHeight = value;
                UpdateColliders();
            }
        }

        public FixedSingle LegsHeight
        {
            get => legsHeight;

            set
            {
                legsHeight = value;
                UpdateColliders();
            }
        }

        public bool BlockedLeft => LeftMaskFlags.HasFlag(CollisionFlags.BLOCK);

        public Box LeftCollider
        {
            get;
            private set;
        }

        public Box UpCollider
        {
            get;
            private set;
        }

        public Box RightCollider
        {
            get;
            private set;
        }

        public Box DownCollider
        {
            get;
            private set;
        }

        public CollisionFlags LeftMaskFlags
        {
            get
            {
                if (!leftMaskComputed)
                {
                    leftMaskFlags = leftCollisionChecker.GetTouchingFlags(Direction.LEFT);
                    leftMaskComputed = true;
                }

                return leftMaskFlags;
            }
        }

        public bool BlockedUp => UpMaskFlags.HasFlag(CollisionFlags.BLOCK);

        public CollisionFlags UpMaskFlags
        {
            get
            {
                if (!upMaskComputed)
                {
                    upMaskFlags = upCollisionChecker.GetTouchingFlags(Direction.UP);
                    upMaskComputed = true;
                }

                return upMaskFlags;
            }
        }

        public bool BlockedRight => RightMaskFlags.HasFlag(CollisionFlags.BLOCK);

        public CollisionFlags RightMaskFlags
        {
            get
            {
                if (!rightMaskComputed)
                {
                    rightMaskFlags = rightCollisionChecker.GetTouchingFlags(Direction.RIGHT);
                    rightMaskComputed = true;
                }

                return rightMaskFlags;
            }
        }

        public bool Landed => LandedOnBlock || LandedOnSlope || LandedOnTopLadder;

        public CollisionFlags DownMaskFlags
        {
            get;
            private set;
        }

        public CollisionFlags InnerMaskFlags
        {
            get
            {
                if (!innerMaskComputed)
                {
                    innerMaskFlags = innerCollisionChecker.GetCollisionFlags();
                    innerMaskComputed = true;
                }

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

        public SpriteCollider(Sprite owner, Box box, bool useCollisionPlacements = false)
            : this(owner, box, 0, 0, useCollisionPlacements)
        {
        }

        public SpriteCollider(Sprite owner, Box box, FixedSingle headheight, FixedSingle legsHeight, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
        {
            Owner = owner;
            this.box = box;
            UseCollisionPlacements = useCollisionPlacements;
            headHeight = headheight;
            this.legsHeight = legsHeight;
            this.checkCollisionWithWorld = checkCollisionWithWorld;
            this.checkCollisionWithSolidSprites = checkCollisionWithSolidSprites;

            IgnoreSprites = new EntityList<Sprite>(owner);

            leftCollisionChecker = new LanderCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            upCollisionChecker = new LanderCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            rightCollisionChecker = new LanderCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            downCollisionChecker = new LanderCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            innerCollisionChecker = new LanderCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            UpdateColliders();
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

        private void UpdateColliders()
        {
            var box = this.box.TruncateOrigin();

            LeftCollider = ((box.Left, box.Origin.Y), (0, box.Mins.Y + headHeight), (1, box.Maxs.Y - legsHeight));
            UpCollider = ((box.Origin.X, box.Top - 1), (box.Mins.X, 0), (box.Maxs.X, 1));
            RightCollider = ((box.Right + 1, box.Origin.Y), (-1, box.Mins.Y + headHeight), (0, box.Maxs.Y - legsHeight));
            DownCollider = ((box.Origin.X, box.Bottom), (box.Mins.X, -1), (box.Maxs.X, 0));

            downCollisionChecker.Setup(DownCollider, CollisionFlags.NONE, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            upCollisionChecker.Setup(UpCollider, CollisionFlags.TOP_LADDER | CollisionFlags.SLOPE, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            leftCollisionChecker.Setup(LeftCollider, CollisionFlags.TOP_LADDER, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            rightCollisionChecker.Setup(RightCollider, CollisionFlags.TOP_LADDER, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            innerCollisionChecker.Setup(box, CollisionFlags.NONE, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);

            UpdateFlags();
        }

        private void UpdateFlags()
        {
            DownMaskFlags = downCollisionChecker.ComputeLandedState();
            if (DownMaskFlags == CollisionFlags.SLOPE)
                LandedSlope = downCollisionChecker.SlopeTriangle;

            upMaskFlags = upCollisionChecker.GetTouchingFlags(Direction.UP);
            leftMaskFlags = leftCollisionChecker.GetTouchingFlags(Direction.LEFT);
            rightMaskFlags = rightCollisionChecker.GetTouchingFlags(Direction.RIGHT);
            innerMaskFlags = innerCollisionChecker.GetCollisionFlags();

            leftMaskComputed = true;
            upMaskComputed = true;
            rightMaskComputed = true;
            innerMaskComputed = true;
        }

        public void Translate(Vector delta)
        {
            Box += delta;
        }

        public bool MoveContactSolid(Vector dir, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactSolid(dir, QUERY_MAX_DISTANCE, masks, ignore);
        }

        public bool MoveContactSolid(Vector dir, FixedSingle maxDistance, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            Vector delta1;
            Vector delta2;
            Box lastBox;
            bool contact = false;

            if (dir.X > 0)
            {
                lastBox = rightCollisionChecker.TestBox;
                rightCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                if (masks.HasFlag(Direction.RIGHT))
                    contact |= rightCollisionChecker.MoveContactSolid(dir, maxDistance);
                else
                    rightCollisionChecker.TestBox += dir;

                delta1 = rightCollisionChecker.TestBox.Origin - lastBox.Origin;
            }
            else if (dir.X < 0)
            {
                lastBox = leftCollisionChecker.TestBox;
                leftCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                if (masks.HasFlag(Direction.LEFT))
                    contact |= leftCollisionChecker.MoveContactSolid(dir, maxDistance);
                else
                    leftCollisionChecker.TestBox += dir;

                delta1 = leftCollisionChecker.TestBox.Origin - lastBox.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                lastBox = downCollisionChecker.TestBox;
                downCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                if (masks.HasFlag(Direction.DOWN))
                    contact |= downCollisionChecker.MoveContactSolid(dir, maxDistance);
                else
                    downCollisionChecker.TestBox += dir;

                delta2 = downCollisionChecker.TestBox.Origin - lastBox.Origin;
            }
            else if (dir.Y < 0)
            {
                lastBox = upCollisionChecker.TestBox;
                upCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                if (masks.HasFlag(Direction.UP))
                    contact |= upCollisionChecker.MoveContactSolid(dir, maxDistance);
                else
                    upCollisionChecker.TestBox += dir;

                delta2 = upCollisionChecker.TestBox.Origin - lastBox.Origin;
            }
            else
                delta2 = delta1;

            var delta = delta1.Length < delta2.Length ? delta1 : delta2;
            if (delta != Vector.NULL_VECTOR)
                Box += delta;

            return contact;
        }

        public bool MoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public bool MoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            var lastOrigin = downCollisionChecker.TestBox.Origin;

            downCollisionChecker.IgnoreFlags = ignore;
            bool contact = downCollisionChecker.MoveContactFloor(maxDistance);

            var delta = downCollisionChecker.TestBox.Origin - lastOrigin;
            if (delta != Vector.NULL_VECTOR)
                Box += delta;

            return contact;
        }

        public bool TryMoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            var lastOrigin = downCollisionChecker.TestBox.Origin;

            downCollisionChecker.IgnoreFlags = ignore;
            if (downCollisionChecker.TryMoveContactFloor(maxDistance))
            {
                var delta = downCollisionChecker.TestBox.Origin - lastOrigin;
                if (delta != Vector.NULL_VECTOR)
                    Box += delta;

                return true;
            }

            return false;
        }

        public bool TryMoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public bool TryMoveContactSlope(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            var lastOrigin = downCollisionChecker.TestBox.Origin;

            downCollisionChecker.IgnoreFlags = ignore;
            if (downCollisionChecker.TryMoveContactSlope(maxDistance))
            {
                var delta = downCollisionChecker.TestBox.Origin - lastOrigin;
                if (delta != Vector.NULL_VECTOR)
                    Box += delta;

                return true;
            }

            return false;
        }

        public bool TryMoveContactSlope(CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactSlope(QUERY_MAX_DISTANCE, ignore);
        }

        public bool AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            return AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public bool AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            var lastOrigin = downCollisionChecker.TestBox.Origin;

            downCollisionChecker.IgnoreFlags = ignore;
            bool contact = downCollisionChecker.AdjustOnTheFloor(maxDistance);

            var delta = downCollisionChecker.TestBox.Origin - lastOrigin;
            if (delta != Vector.NULL_VECTOR)
                Box += delta;

            return contact;
        }

        public bool AdjustOnTheLadder()
        {
            if (!UseCollisionPlacements)
                return false;

            if (LandedOnTopLadder)
            {
                foreach (var placement in downCollisionChecker.Placements)
                    if (placement.CollisionData == CollisionData.TOP_LADDER)
                    {
                        var placementBox = placement.ObstableBox;
                        var delta = placementBox.Left - box.Left + (MAP_SIZE - box.Width) * 0.5;

                        if (delta != 0)
                        {
                            Box += delta * Vector.RIGHT_VECTOR;
                            return true;
                        }

                        return false;
                    }
            }
            else
            {
                foreach (var placement in upCollisionChecker.Placements)
                    if (placement.CollisionData == CollisionData.LADDER)
                    {
                        var placementBox = placement.ObstableBox;
                        var delta = placementBox.Left - box.Left + (MAP_SIZE - box.Width) * 0.5;

                        if (delta != 0)
                        {
                            Box += delta * Vector.RIGHT_VECTOR;
                            return true;
                        }

                        return false;
                    }
            }

            return false;
        }
    }
}