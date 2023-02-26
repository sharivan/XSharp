using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    public class SpriteCollider
    {
        private Box box;
        private bool checkCollisionWithWorld;
        private bool checkCollisionWithSolidSprites;
        private FixedSingle maskSize;
        private FixedSingle headHeight;
        private FixedSingle legsHeight;

        private ExtendedCollisionChecker leftCollisionChecker;
        private ExtendedCollisionChecker rightCollisionChecker;
        private ExtendedCollisionChecker upCollisionChecker;
        private ExtendedCollisionChecker downCollisionChecker;
        private ExtendedCollisionChecker innerCollisionChecker;

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

        public FixedSingle MaskSize
        {
            get => maskSize;

            set
            {
                maskSize = value;
                UpdateColliders();
            }
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
                    leftMaskFlags = leftCollisionChecker.GetTouchingFlags(Direction.LEFT, RoundMode.FLOOR);
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
                    upMaskFlags = upCollisionChecker.GetTouchingFlags(Direction.UP, RoundMode.FLOOR);
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
                    rightMaskFlags = rightCollisionChecker.GetTouchingFlags(Direction.RIGHT, RoundMode.FLOOR);
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
                    innerMaskFlags = innerCollisionChecker.GetCollisionFlags(RoundMode.FLOOR);
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
            : this(owner, box, MASK_SIZE, 0, 0, useCollisionPlacements)
        {
        }

        public SpriteCollider(Sprite owner, Box box, FixedSingle maskSize, FixedSingle headheight, FixedSingle legsHeight, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
        {
            Owner = owner;
            this.box = box;
            this.maskSize = maskSize;
            UseCollisionPlacements = useCollisionPlacements;
            this.headHeight = headheight;
            this.legsHeight = legsHeight;
            this.checkCollisionWithWorld = checkCollisionWithWorld;
            this.checkCollisionWithSolidSprites = checkCollisionWithSolidSprites;

            IgnoreSprites = new EntityList<Sprite>(owner);

            leftCollisionChecker = new ExtendedCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                MaskSize = maskSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            upCollisionChecker = new ExtendedCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                MaskSize = maskSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            rightCollisionChecker = new ExtendedCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                MaskSize = maskSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            downCollisionChecker = new ExtendedCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                MaskSize = maskSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            innerCollisionChecker = new ExtendedCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                MaskSize = maskSize,
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
            LeftCollider = ((box.Left, box.Origin.Y), (0, box.Mins.Y + headHeight), (1, box.Maxs.Y - legsHeight));
            UpCollider = ((box.Origin.X, box.Top), (box.Mins.X, 0), (box.Maxs.X, 1));
            RightCollider = ((box.Right, box.Origin.Y), (-1, box.Mins.Y + headHeight), (0, box.Maxs.Y - legsHeight));
            DownCollider = ((box.Origin.X, box.Bottom), (box.Mins.X, -1), (box.Maxs.X, 0));

            downCollisionChecker.Setup(DownCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            upCollisionChecker.Setup(UpCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            leftCollisionChecker.Setup(LeftCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            rightCollisionChecker.Setup(RightCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);
            innerCollisionChecker.Setup(box, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements);

            UpdateFlags();
        }

        private void ClipFromSlope(RightTriangle slope)
        {
            FixedSingle h = slope.HCathetusVector.X;
            var vclip = (FixedSingle) ((FixedDouble) slope.VCathetusVector.Y * (box.Width + maskSize) / h).Abs;

            if (h > 0)
                LeftCollider = LeftCollider.ClipBottom(vclip);
            else
                RightCollider = RightCollider.ClipBottom(vclip);
        }

        private void UpdateFlags()
        {
            DownMaskFlags = downCollisionChecker.ComputeLandedState(RoundMode.FLOOR);
            if (DownMaskFlags == CollisionFlags.SLOPE)
            {
                LandedSlope = downCollisionChecker.SlopeTriangle;
                ClipFromSlope(LandedSlope);
            }

            upMaskFlags = upCollisionChecker.GetTouchingFlags(Direction.UP, RoundMode.FLOOR);
            leftMaskFlags = leftCollisionChecker.GetTouchingFlags(Direction.LEFT, RoundMode.FLOOR);
            rightMaskFlags = rightCollisionChecker.GetTouchingFlags(Direction.RIGHT, RoundMode.FLOOR);
            innerMaskFlags = innerCollisionChecker.GetCollisionFlags(RoundMode.FLOOR);

            leftMaskComputed = true;
            upMaskComputed = true;
            rightMaskComputed = true;
            innerMaskComputed = true;
        }

        public void Translate(Vector delta)
        {
            box += delta;
            UpdateColliders();
        }

        public void MoveContactSolid(Vector dir, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            MoveContactSolid(dir, QUERY_MAX_DISTANCE, masks, ignore);
        }

        public void MoveContactSolid(Vector dir, FixedSingle maxDistance, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            Vector delta1;
            Vector delta2;
            Box newBox;

            if (dir.X > 0)
            {
                rightCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.RIGHT)
                    ? rightCollisionChecker.MoveUntilIntersect(dir, maxDistance, RoundMode.FLOOR)
                    : rightCollisionChecker.TestBox + dir;

                delta1 = newBox.Origin - rightCollisionChecker.TestBox.Origin;
            }
            else if (dir.X < 0)
            {
                leftCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.LEFT)
                    ? leftCollisionChecker.MoveUntilIntersect(dir, maxDistance, RoundMode.FLOOR)
                    : leftCollisionChecker.TestBox + dir;

                delta1 = newBox.Origin - leftCollisionChecker.TestBox.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                downCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.DOWN)
                    ? downCollisionChecker.MoveUntilIntersect(dir, maxDistance, RoundMode.FLOOR)
                    : downCollisionChecker.TestBox + dir;

                delta2 = newBox.Origin - downCollisionChecker.TestBox.Origin;
            }
            else if (dir.Y < 0)
            {
                upCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.UP)
                    ? upCollisionChecker.MoveUntilIntersect(dir, maxDistance, RoundMode.FLOOR)
                    : upCollisionChecker.TestBox + dir;

                delta2 = newBox.Origin - upCollisionChecker.TestBox.Origin;
            }
            else
                delta2 = delta1;

            Vector delta = delta1.Length < delta2.Length ? delta1 : delta2;

            box += delta;
            UpdateColliders();
        }

        public void MoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            MoveContactFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public void MoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            box = innerCollisionChecker.MoveContactFloor(maxDistance, RoundMode.FLOOR);
            UpdateColliders();
        }

        public void TryMoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            if (innerCollisionChecker.TryMoveContactFloor(maxDistance, RoundMode.FLOOR))
            {
                box = innerCollisionChecker.TestBox;
                UpdateColliders();
            }
        }

        public void TryMoveContactSlope(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            if (innerCollisionChecker.TryMoveContactSlope(maxDistance, RoundMode.FLOOR))
            {
                box = innerCollisionChecker.TestBox;
                UpdateColliders();
            }
        }

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public void AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            box = innerCollisionChecker.AdjustOnTheFloor(maxDistance, RoundMode.FLOOR);
            UpdateColliders();
        }

        public void AdjustOnTheLadder()
        {
            if (!UseCollisionPlacements)
                return;

            if (LandedOnTopLadder)
            {
                foreach (var placement in downCollisionChecker.Placements)
                {
                    if (placement.CollisionData == CollisionData.TOP_LADDER)
                    {
                        Box placementBox = placement.ObstableBox;
                        FixedSingle delta = placementBox.Left - box.Left + (MAP_SIZE - box.Width) * 0.5;
                        box += delta * Vector.RIGHT_VECTOR;
                        UpdateColliders();
                        return;
                    }
                }
            }
            else
            {
                foreach (var placement in upCollisionChecker.Placements)
                {
                    if (placement.CollisionData == CollisionData.LADDER)
                    {
                        Box placementBox = placement.ObstableBox;
                        FixedSingle delta = placementBox.Left - box.Left + (MAP_SIZE - box.Width) * 0.5;
                        box += delta * Vector.RIGHT_VECTOR;
                        UpdateColliders();
                        return;
                    }
                }
            }
        }
    }
}