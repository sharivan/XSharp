using System.Collections.Generic;

using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;
using MMXWorld = MMX.Engine.World.World;

namespace MMX.Engine
{
    public class BoxCollider
    {
        private MMXWorld world;
        private Box box;
        private FixedSingle maskSize;

        private CollisionFlags leftMaskFlags;
        private CollisionFlags upMaskFlags;
        private CollisionFlags rightMaskFlags;
        private readonly List<CollisionPlacement> leftCollisionPlacements;
        private readonly List<CollisionPlacement> upCollisionPlacements;
        private readonly List<CollisionPlacement> rightCollisionPlacements;
        private readonly List<CollisionPlacement> downCollisionPlacements;

        private RightTriangle landedSlope;

        private bool wasLandedOnSlope;
        private RightTriangle lastLandedSlope;

        private bool leftMaskComputed;
        private bool upMaskComputed;
        private bool rightMaskComputed;

        public MMXWorld World
        {
            get => world;

            set
            {
                world = value;
                UpdateColliders();
            }
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

        public FixedSingle MaskSize
        {
            get => maskSize;

            set
            {
                maskSize = value;
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
                    leftCollisionPlacements.Clear();
                    leftMaskFlags = world.GetCollisionFlags(LeftCollider, leftCollisionPlacements, CollisionFlags.NONE,true, CollisionSide.LEFT_WALL);
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
                    upCollisionPlacements.Clear();
                    upMaskFlags = world.GetCollisionFlags(UpCollider, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
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
                    rightCollisionPlacements.Clear();
                    rightMaskFlags = world.GetCollisionFlags(RightCollider, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);
                    rightMaskComputed = true;
                }

                return rightMaskFlags;
            }
        }

        public bool Landed => LandedOnBlock || LandedOnSlope || LandedOnTopLadder;

        public CollisionFlags DownMaskFlags { get;
            private set;
        }

        public bool LandedOnBlock => DownMaskFlags == CollisionFlags.BLOCK;

        public bool LandedOnSlope => DownMaskFlags == CollisionFlags.SLOPE;

        public bool LandedOnTopLadder => DownMaskFlags == CollisionFlags.TOP_LADDER;

        public RightTriangle LandedSlope => landedSlope;

        public BoxCollider(Box box) :
            this(null, box, MASK_SIZE)
        {
        }

        public BoxCollider(Box box, FixedSingle maskSize) :
            this(null, box, maskSize)
        {
        }

        public BoxCollider(MMXWorld world, Box box) :
            this(world, box, MASK_SIZE)
        {
        }

        public BoxCollider(MMXWorld world, Box box, FixedSingle maskSize)
        {
            this.world = world;
            this.box = box;
            this.maskSize = maskSize;

            leftCollisionPlacements = new List<CollisionPlacement>();
            upCollisionPlacements = new List<CollisionPlacement>();
            rightCollisionPlacements = new List<CollisionPlacement>();
            downCollisionPlacements = new List<CollisionPlacement>();

            UpdateColliders();
        }

        private void UpdateColliders()
        {
            LeftCollider = new Box(box.Left, box.Top + 1, -maskSize, box.Height - 4);
            UpCollider = new Box(box.LeftTop, box.Width, -maskSize);
            RightCollider = new Box(box.Right, box.Top + 1, maskSize, box.Height - 4);
            DownCollider = new Box(box.LeftBottom, box.Width, maskSize);

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
            leftCollisionPlacements.Clear();
            upCollisionPlacements.Clear();
            rightCollisionPlacements.Clear();
            downCollisionPlacements.Clear();

            if (world != null)
            {
                DownMaskFlags = world.ComputedLandedState(box, downCollisionPlacements, out landedSlope, maskSize, CollisionFlags.NONE);
                if (DownMaskFlags == CollisionFlags.SLOPE)
                {
                    ClipFromSlope(landedSlope);
                    wasLandedOnSlope = true;
                    lastLandedSlope = landedSlope;
                }
                /*else if (world.GetCollisionFlags(downCollider, out RightTriangle slope, CollisionFlags.NONE, true, CollisionSide.FLOOR).HasFlag(CollisionFlags.SLOPE))
                    ClipFromSlope(slope);
                else if (wasLandedOnSlope && downMaskFlags == CollisionFlags.NONE)
                {
                    ClipFromSlope(lastLandedSlope);
                }
                else
                    wasLandedOnSlope = false;*/

                upMaskFlags = world.GetCollisionFlags(UpCollider, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
                leftMaskFlags = world.GetCollisionFlags(LeftCollider, leftCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.LEFT_WALL);
                rightMaskFlags = world.GetCollisionFlags(RightCollider, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);

                leftMaskComputed = true;
                upMaskComputed = true;
                rightMaskComputed = true;
            }
            else
            {
                leftMaskComputed = true;
                upMaskComputed = true;
                rightMaskComputed = true;
                wasLandedOnSlope = false;

                leftMaskFlags = CollisionFlags.NONE;
                upMaskFlags = CollisionFlags.NONE;
                rightMaskFlags = CollisionFlags.NONE;
                DownMaskFlags = CollisionFlags.NONE;
            }
        }

        public void Translate(Vector delta)
        {
            box += delta;
            UpdateColliders();
        }

        public void MoveContactSolid(Vector dir, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE) => MoveContactSolid(dir, QUERY_MAX_DISTANCE, masks, ignore);

        public void MoveContactSolid(Vector dir, FixedSingle maxDistance, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            Vector delta1;
            Vector delta2;
            Box newBox;

            if (dir.X > 0)
            {
                newBox = masks.HasFlag(Direction.RIGHT)
                    ? world.MoveUntilIntersect(RightCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.RIGHT_WALL)
                    : RightCollider + dir;

                delta1 = newBox.Origin - RightCollider.Origin;
            }
            else if (dir.X < 0)
            {
                newBox = masks.HasFlag(Direction.LEFT)
                    ? world.MoveUntilIntersect(LeftCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.LEFT_WALL)
                    : LeftCollider + dir;

                delta1 = newBox.Origin - LeftCollider.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                newBox = masks.HasFlag(Direction.DOWN)
                    ? world.MoveUntilIntersect(DownCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER, CollisionSide.FLOOR)
                    : DownCollider + dir;

                delta2 = newBox.Origin - DownCollider.Origin;
            }
            else if (dir.Y < 0)
            {
                newBox = masks.HasFlag(Direction.UP)
                    ? world.MoveUntilIntersect(UpCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.CEIL)
                    : UpCollider + dir;

                delta2 = newBox.Origin - UpCollider.Origin;
            }
            else
                delta2 = delta1;

            Vector delta = delta1.Length < delta2.Length ? delta1 : delta2;

            box += delta;
            UpdateColliders();
        }

        public void MoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE) => MoveContactFloor(QUERY_MAX_DISTANCE, ignore);

        public void MoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            box = world.MoveContactFloor(box, maxDistance, maskSize, ignore);
            UpdateColliders();
        }

        public void TryMoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (world.TryMoveContactFloor(box, out Box newBox, maxDistance, maskSize, ignore))
            {
                box = newBox;
                UpdateColliders();
            }
        }

        public void TryMoveContactSlope(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (world.TryMoveContactSlope(box, out Box newBox, maxDistance, maskSize, ignore))
            {
                box = newBox;
                UpdateColliders();
            }
        }

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE) => AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);

        public void AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            box = world.AdjustOnTheFloor(box, maxDistance, maskSize, ignore);
            UpdateColliders();
        }

        public void AdjustOnTheLadder()
        {
            if (LandedOnTopLadder)
            {
                foreach (var placement in downCollisionPlacements)
                {
                    if (placement.Flag == CollisionFlags.TOP_LADDER)
                    {
                        Box placementBox = placement.Placement.BoudingBox;
                        FixedSingle delta = placementBox.Left + MAP_SIZE / 2 - box.Left - box.Width / 2;
                        box += delta * Vector.RIGHT_VECTOR;
                        UpdateColliders();
                        return;
                    }
                }
            }
            else
            {
                foreach (var placement in upCollisionPlacements)
                {
                    if (placement.Flag == CollisionFlags.LADDER)
                    {
                        Box placementBox = placement.Placement.BoudingBox;
                        FixedSingle delta = placementBox.Left + MAP_SIZE / 2 - box.Left - box.Width / 2;
                        box += delta * Vector.RIGHT_VECTOR;
                        UpdateColliders();
                        return;
                    }
                }
            }
        }
    }
}
