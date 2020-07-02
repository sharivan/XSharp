using MMX.Geometry;
using MMX.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class BoxCollider
    {
        private World world;
        private Box box;
        private FixedSingle maskSize;

        private CollisionFlags leftMaskFlags;
        private CollisionFlags upMaskFlags;
        private CollisionFlags rightMaskFlags;
        private CollisionFlags downMaskFlags;

        private Box leftCollider;
        private Box upCollider;
        private Box rightCollider;
        private Box downCollider;

        private List<CollisionPlacement> leftCollisionPlacements;
        private List<CollisionPlacement> upCollisionPlacements;
        private List<CollisionPlacement> rightCollisionPlacements;
        private List<CollisionPlacement> downCollisionPlacements;

        private RightTriangle landedSlope;

        private bool wasLandedOnSlope;
        private RightTriangle lastLandedSlope;

        private bool leftMaskComputed;
        private bool upMaskComputed;
        private bool rightMaskComputed;

        public World World
        {
            get
            {
                return world;
            }

            set
            {
                world = value;
                UpdateColliders();
            }
        }

        public Box Box
        {
            get
            {
                return box;
            }

            set
            {
                box = value;
                UpdateColliders();
            }
        }

        public FixedSingle MaskSize
        {
            get
            {
                return maskSize;
            }

            set
            {
                maskSize = value;
                UpdateColliders();
            }
        }

        public bool BlockedLeft
        {
            get
            {
                return LeftMaskFlags.HasFlag(CollisionFlags.BLOCK);
            }
        }

        public Box LeftCollider
        {
            get
            {
                return leftCollider;
            }
        }

        public Box UpCollider
        {
            get
            {
                return upCollider;
            }
        }

        public Box RightCollider
        {
            get
            {
                return rightCollider;
            }
        }

        public Box DownCollider
        {
            get
            {
                return downCollider;
            }
        }

        public CollisionFlags LeftMaskFlags
        {
            get
            {
                if (!leftMaskComputed)
                {
                    leftCollisionPlacements.Clear();
                    leftMaskFlags = world.GetCollisionFlags(leftCollider, leftCollisionPlacements, CollisionFlags.NONE,true, CollisionSide.LEFT_WALL);
                    leftMaskComputed = true;
                }

                return leftMaskFlags;
            }
        }

        public bool BlockedUp
        {
            get
            {
                return UpMaskFlags.HasFlag(CollisionFlags.BLOCK);
            }
        }

        public CollisionFlags UpMaskFlags
        {
            get
            {
                if (!upMaskComputed)
                {
                    upCollisionPlacements.Clear();
                    upMaskFlags = world.GetCollisionFlags(upCollider, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
                    upMaskComputed = true;
                }

                return upMaskFlags;
            }
        }

        public bool BlockedRight
        {
            get
            {
                return RightMaskFlags.HasFlag(CollisionFlags.BLOCK);
            }
        }

        public CollisionFlags RightMaskFlags
        {
            get
            {
                if (!rightMaskComputed)
                {
                    rightCollisionPlacements.Clear();
                    rightMaskFlags = world.GetCollisionFlags(rightCollider, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);
                    rightMaskComputed = true;
                }

                return rightMaskFlags;
            }
        }

        public bool Landed
        {
            get
            {
                return LandedOnBlock || LandedOnSlope || LandedOnTopLadder;
            }
        }

        public CollisionFlags DownMaskFlags
        {
            get
            {
                return downMaskFlags;
            }
        }

        public bool LandedOnBlock
        {
            get
            {
                return downMaskFlags == CollisionFlags.BLOCK;
            }
        }

        public bool LandedOnSlope
        {
            get
            {
                return downMaskFlags == CollisionFlags.SLOPE;
            }
        }

        public bool LandedOnTopLadder
        {
            get
            {
                return downMaskFlags == CollisionFlags.TOP_LADDER;
            }
        }

        public RightTriangle LandedSlope
        {
            get
            {
                return landedSlope;
            }
        }

        public BoxCollider(Box box) :
            this(null, box, MASK_SIZE)
        {
        }

        public BoxCollider(Box box, FixedSingle maskSize) :
            this(null, box, maskSize)
        {
        }

        public BoxCollider(World world, Box box) :
            this(world, box, MASK_SIZE)
        {
        }

        public BoxCollider(World world, Box box, FixedSingle maskSize)
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
            leftCollider = new Box(box.LeftTop, -maskSize, box.Height);
            upCollider = new Box(box.LeftTop, box.Width, -maskSize);
            rightCollider = new Box(box.RightTop, maskSize, box.Height);
            downCollider = new Box(box.LeftBottom, box.Width, maskSize);

            UpdateFlags();
        }

        private void ClipFromSlope(RightTriangle slope)
        {
            FixedSingle h = slope.HCathetusVector.X;
            FixedSingle vclip = (FixedSingle) ((FixedDouble) slope.VCathetusVector.Y * (box.Width + maskSize) / h).Abs;

            if (h > 0)
                leftCollider = leftCollider.ClipBottom(vclip);
            else
                rightCollider = rightCollider.ClipBottom(vclip);
        }

        private void UpdateFlags()
        {
            leftCollisionPlacements.Clear();
            upCollisionPlacements.Clear();
            rightCollisionPlacements.Clear();
            downCollisionPlacements.Clear();

            if (world != null)
            {
                leftMaskComputed = false;
                upMaskComputed = false;
                rightMaskComputed = false;

                downMaskFlags = world.ComputedLandedState(box, downCollisionPlacements, out landedSlope, maskSize, CollisionFlags.NONE);
                if (downMaskFlags == CollisionFlags.SLOPE)
                {
                    ClipFromSlope(landedSlope);
                    wasLandedOnSlope = true;
                    lastLandedSlope = landedSlope;
                }
                else if (world.GetCollisionFlags(downCollider, out RightTriangle slope, CollisionFlags.NONE, true, CollisionSide.FLOOR).HasFlag(CollisionFlags.SLOPE))
                    ClipFromSlope(slope);
                else if (wasLandedOnSlope && downMaskFlags == CollisionFlags.NONE)
                    ClipFromSlope(lastLandedSlope);
                else
                    wasLandedOnSlope = false;

                //upMaskFlags = world.GetCollisionFlags(upCollider, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
                //leftMaskFlags = world.GetCollisionFlags(leftCollider, leftCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.LEFT_WALL);
                //rightMaskFlags = world.GetCollisionFlags(rightCollider, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);
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
                downMaskFlags = CollisionFlags.NONE;
            }
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
                if (masks.HasFlag(Direction.RIGHT))
                    newBox = world.MoveUntilIntersect(rightCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.RIGHT_WALL);
                else
                    newBox = rightCollider + dir;

                delta1 = newBox.Origin - rightCollider.Origin;
            }
            else if (dir.X < 0)
            {
                if (masks.HasFlag(Direction.LEFT))
                    newBox = world.MoveUntilIntersect(leftCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.LEFT_WALL);
                else
                    newBox = leftCollider + dir;

                delta1 = newBox.Origin - leftCollider.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                if (masks.HasFlag(Direction.DOWN))
                    newBox = world.MoveUntilIntersect(downCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER, CollisionSide.FLOOR);
                else
                    newBox = downCollider + dir;

                delta2 = newBox.Origin - downCollider.Origin;
            }
            else if (dir.Y < 0)
            {
                if (masks.HasFlag(Direction.UP))
                    newBox = world.MoveUntilIntersect(upCollider, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER, CollisionSide.CEIL);
                else
                    newBox = upCollider + dir;

                delta2 = newBox.Origin - upCollider.Origin;
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

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

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
