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
        private MMXBox box;
        private MMXFloat maskSize;

        private MMXFloat vClipLeft;
        private MMXFloat vClipRight;

        private CollisionFlags leftMaskFlags;
        private CollisionFlags upMaskFlags;
        private CollisionFlags rightMaskFlags;
        private CollisionFlags downMaskFlags;

        private List<CollisionPlacement> leftCollisionPlacements;
        private List<CollisionPlacement> upCollisionPlacements;
        private List<CollisionPlacement> rightCollisionPlacements;
        private List<CollisionPlacement> downCollisionPlacements;

        private MMXRightTriangle landedSlope;

        private bool wasLandedOnSlope;
        private MMXRightTriangle lastLandedSlope;

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
                UpdateFlags();
            }
        }

        public MMXBox Box
        {
            get
            {
                return box;
            }

            set
            {
                box = value;
                UpdateFlags();
            }
        }

        public MMXFloat MaskSize
        {
            get
            {
                return maskSize;
            }

            set
            {
                maskSize = value;
                UpdateFlags();
            }
        }

        public bool BlockedLeft
        {
            get
            {
                return LeftMaskFlags.HasFlag(CollisionFlags.BLOCK);
            }
        }

        public CollisionFlags LeftMaskFlags
        {
            get
            {
                if (!leftMaskComputed)
                {
                    leftCollisionPlacements.Clear();
                    leftMaskFlags = world.GetCollisionFlags(box.ClipBottom(vClipLeft) + maskSize * MMXVector.LEFT_VECTOR, leftCollisionPlacements, CollisionFlags.NONE,true, CollisionSide.LEFT_WALL);
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
                    upMaskFlags = world.GetCollisionFlags(box + maskSize * MMXVector.UP_VECTOR, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
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
                    rightMaskFlags = world.GetCollisionFlags(box.ClipBottom(vClipRight) + maskSize * MMXVector.RIGHT_VECTOR, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);
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

        public MMXRightTriangle LandedSlope
        {
            get
            {
                return landedSlope;
            }
        }

        public BoxCollider(MMXBox box) :
            this(null, box, MASK_SIZE)
        {
        }

        public BoxCollider(MMXBox box, MMXFloat maskSize) :
            this(null, box, maskSize)
        {
        }

        public BoxCollider(World world, MMXBox box) :
            this(world, box, MASK_SIZE)
        {
        }

        public BoxCollider(World world, MMXBox box, MMXFloat maskSize)
        {
            this.world = world;
            this.box = box;
            this.maskSize = maskSize;

            leftCollisionPlacements = new List<CollisionPlacement>();
            upCollisionPlacements = new List<CollisionPlacement>();
            rightCollisionPlacements = new List<CollisionPlacement>();
            downCollisionPlacements = new List<CollisionPlacement>();

            UpdateFlags();
        }

        private void ClipFromSlope(MMXRightTriangle slope)
        {
            MMXFloat h = slope.HCathetusVector.X;
            MMXFloat vclip = Math.Abs(slope.VCathetusVector.Y * (box.Width + maskSize) / h);

            if (h > 0)
            {
                vClipLeft = vclip;
                vClipRight = 0;
            }
            else
            {
                vClipLeft = 0;
                vClipRight = vclip;
            }
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
                else if (world.GetCollisionFlags(box, out MMXRightTriangle slope, CollisionFlags.NONE, true, CollisionSide.FLOOR).HasFlag(CollisionFlags.SLOPE))
                    ClipFromSlope(slope);
                else if (wasLandedOnSlope)
                    ClipFromSlope(lastLandedSlope);
                else
                {
                    vClipLeft = 0;
                    vClipRight = 0;
                    wasLandedOnSlope = false;
                }

                upMaskFlags = world.GetCollisionFlags(box + maskSize * MMXVector.UP_VECTOR, upCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.CEIL);
                leftMaskFlags = world.GetCollisionFlags(box.ClipBottom(vClipLeft) + maskSize * MMXVector.LEFT_VECTOR, leftCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.LEFT_WALL);
                rightMaskFlags = world.GetCollisionFlags(box.ClipBottom(vClipRight) + maskSize * MMXVector.RIGHT_VECTOR, rightCollisionPlacements, CollisionFlags.NONE, true, CollisionSide.RIGHT_WALL);
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

        public void Translate(MMXVector delta)
        {
            box += delta;
            UpdateFlags();
        }

        public void MoveContactSolid(MMXVector dir, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            MoveContactSolid(dir, QUERY_MAX_DISTANCE, masks, ignore);
        }

        public void MoveContactSolid(MMXVector dir, MMXFloat maxDistance, Direction masks = Direction.ALL, CollisionFlags ignore = CollisionFlags.NONE)
        {
            MMXVector delta1;
            MMXVector delta2;
            MMXBox newBox;

            if (dir.X > 0)
            {
                if (masks.HasFlag(Direction.RIGHT))
                {
                    newBox = world.MoveUntilIntersect(box.ClipBottom(vClipRight) + maskSize * MMXVector.RIGHT_VECTOR, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER);
                    newBox -= maskSize * MMXVector.RIGHT_VECTOR;
                }
                else
                    newBox = box + dir;

                delta1 = newBox.Origin - box.Origin;
            }
            else if (dir.X < 0)
            {
                if (masks.HasFlag(Direction.LEFT))
                {
                    newBox = world.MoveUntilIntersect(box.ClipBottom(vClipLeft) + maskSize * MMXVector.LEFT_VECTOR, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER);
                    newBox -= maskSize * MMXVector.LEFT_VECTOR;
                }
                else
                    newBox = box + dir;

                delta1 = newBox.Origin - box.Origin;
            }
            else
                delta1 = dir;

            if (dir.Y > 0)
            {
                if (masks.HasFlag(Direction.DOWN))
                {
                    newBox = world.MoveUntilIntersect(box + maskSize * MMXVector.DOWN_VECTOR, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER);
                    newBox -= maskSize * MMXVector.DOWN_VECTOR;
                }
                else
                    newBox = box + dir;

                delta2 = newBox.Origin - box.Origin;
            }
            else if (dir.Y < 0)
            {
                if (masks.HasFlag(Direction.UP))
                {
                    newBox = world.MoveUntilIntersect(box + maskSize * MMXVector.UP_VECTOR, dir, maxDistance, maskSize, ignore | CollisionFlags.LADDER | CollisionFlags.TOP_LADDER);
                    newBox -= maskSize * MMXVector.UP_VECTOR;
                }
                else
                    newBox = box + dir;

                delta2 = newBox.Origin - box.Origin;
            }
            else
                delta2 = delta1;

            MMXVector delta = delta1.Length < delta2.Length ? delta1 : delta2;

            box += delta;
            UpdateFlags();
        }

        public void MoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            MoveContactFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public void MoveContactFloor(MMXFloat maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            box = world.MoveContactFloor(box, maxDistance, maskSize, ignore);
            UpdateFlags();
        }

        public void TryMoveContactFloor(MMXFloat maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (world.TryMoveContactFloor(box, out MMXBox newBox, maxDistance, maskSize, ignore))
            {
                box = newBox;
                UpdateFlags();
            }
        }

        public void TryMoveContactSlope(MMXFloat maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (world.TryMoveContactSlope(box, out MMXBox newBox, maxDistance, maskSize, ignore))
            {
                box = newBox;
                UpdateFlags();
            }
        }

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

        public void AdjustOnTheFloor(MMXFloat maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            box = world.AdjustOnTheFloor(box, maxDistance, maskSize, ignore);
            UpdateFlags();
        }

        public void AdjustOnTheLadder()
        {
            if (LandedOnTopLadder)
            {
                foreach (var placement in downCollisionPlacements)
                {
                    if (placement.Flag == CollisionFlags.TOP_LADDER)
                    {
                        MMXBox placementBox = placement.Placement.BoudingBox;
                        MMXFloat delta = placementBox.Left + MAP_SIZE / 2 - box.Left - box.Width / 2;
                        box += delta * MMXVector.RIGHT_VECTOR;
                        UpdateFlags();
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
                        MMXBox placementBox = placement.Placement.BoudingBox;
                        MMXFloat delta = placementBox.Left + MAP_SIZE / 2 - box.Left - box.Width / 2;
                        box += delta * MMXVector.RIGHT_VECTOR;
                        UpdateFlags();
                        return;
                    }
                }
            }
        }
    }
}
