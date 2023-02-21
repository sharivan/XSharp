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
        private FixedSingle sideCollidersTopClip;
        private FixedSingle sideCollidersBottomClip;

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

        public FixedSingle SideCollidersTopClip
        {
            get => sideCollidersTopClip;

            set
            {
                sideCollidersTopClip = value;
                UpdateColliders();
            }
        }

        public FixedSingle SideCollidersBottomClip
        {
            get => sideCollidersBottomClip;

            set
            {
                sideCollidersBottomClip = value;
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
                    leftMaskFlags = leftCollisionChecker.GetCollisionFlags();
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
                    upMaskFlags = upCollisionChecker.GetCollisionFlags();
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
                    rightMaskFlags = rightCollisionChecker.GetCollisionFlags();
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
            : this(owner, box, MASK_SIZE, 0, 0, useCollisionPlacements)
        {
        }

        public SpriteCollider(Sprite owner, Box box, FixedSingle maskSize, FixedSingle sideCollidersTopClip, FixedSingle sideCollidersBottomClip, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
        {
            Owner = owner;
            this.box = box;
            this.maskSize = maskSize;
            UseCollisionPlacements = useCollisionPlacements;
            this.sideCollidersTopClip = sideCollidersTopClip;
            this.sideCollidersBottomClip = sideCollidersBottomClip;
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
            LeftCollider = new Box(box.Left - maskSize, box.Top + sideCollidersTopClip, maskSize, box.Height - sideCollidersTopClip - sideCollidersBottomClip);
            UpCollider = new Box(box.Left, box.Top - maskSize, box.Width, maskSize);
            RightCollider = new Box(box.Right, box.Top + sideCollidersTopClip, maskSize, box.Height - sideCollidersTopClip - sideCollidersBottomClip);
            DownCollider = new Box(box.LeftBottom, box.Width, maskSize);

            downCollisionChecker.Setup(DownCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements, true);
            upCollisionChecker.Setup(UpCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements, true);
            leftCollisionChecker.Setup(LeftCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements, true);
            rightCollisionChecker.Setup(RightCollider, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements, true);
            innerCollisionChecker.Setup(box, CollisionFlags.NONE, IgnoreSprites, maskSize, checkCollisionWithWorld, checkCollisionWithSolidSprites, UseCollisionPlacements, true);

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
            DownMaskFlags = downCollisionChecker.ComputeLandedState();
            if (DownMaskFlags == CollisionFlags.SLOPE)
            {
                LandedSlope = downCollisionChecker.SlopeTriangle;
                ClipFromSlope(LandedSlope);
            }

            upMaskFlags = upCollisionChecker.GetCollisionFlags();
            leftMaskFlags = leftCollisionChecker.GetCollisionFlags();
            rightMaskFlags = rightCollisionChecker.GetCollisionFlags();
            innerMaskFlags = innerCollisionChecker.GetCollisionFlags();

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
                    ? rightCollisionChecker.MoveUntilIntersect(dir, maxDistance)
                    : RightCollider + dir;

                delta1 = newBox.Origin - RightCollider.Origin;
            }
            else if (dir.X < 0)
            {
                leftCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.LEFT)
                    ? leftCollisionChecker.MoveUntilIntersect(dir, maxDistance)
                    : LeftCollider + dir;

                delta1 = newBox.Origin - LeftCollider.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                downCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.DOWN)
                    ? downCollisionChecker.MoveUntilIntersect(dir, maxDistance)
                    : DownCollider + dir;

                delta2 = newBox.Origin - DownCollider.Origin;
            }
            else if (dir.Y < 0)
            {
                upCollisionChecker.IgnoreFlags = ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER | CollisionFlags.WATER | CollisionFlags.WATER_SURFACE;
                newBox = masks.HasFlag(Direction.UP)
                    ? upCollisionChecker.MoveUntilIntersect(dir, maxDistance)
                    : UpCollider + dir;

                delta2 = newBox.Origin - UpCollider.Origin;
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
            box = innerCollisionChecker.MoveContactFloor(maxDistance);
            UpdateColliders();
        }

        public void TryMoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            if (innerCollisionChecker.TryMoveContactFloor(maxDistance))
            {
                box = innerCollisionChecker.TestBox;
                UpdateColliders();
            }
        }

        public void TryMoveContactSlope(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            innerCollisionChecker.IgnoreFlags = ignore;
            if (innerCollisionChecker.TryMoveContactSlope(maxDistance))
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
            box = innerCollisionChecker.AdjustOnTheFloor(maxDistance);
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
                        FixedSingle delta = placementBox.Left + MAP_SIZE * 0.5 - box.Left - box.Width * 0.5;
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
                        FixedSingle delta = placementBox.Left + MAP_SIZE * 0.5 - box.Left - box.Width * 0.5;
                        box += delta * Vector.RIGHT_VECTOR;
                        UpdateColliders();
                        return;
                    }
                }
            }
        }
    }
}