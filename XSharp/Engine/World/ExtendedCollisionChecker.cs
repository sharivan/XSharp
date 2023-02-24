﻿using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    public class ExtendedCollisionChecker : CollisionChecker
    {
        private CollisionChecker leftChecker;
        private CollisionChecker rightChecker;

        public ExtendedCollisionChecker()
        {
            leftChecker = new CollisionChecker();
            rightChecker = new CollisionChecker();
        }

        // TODO : In the future, refactor this to make it simpler and more effective at the same time.
        public CollisionFlags ComputeLandedState()
        {
            Box bottomMask = TestBox.ClipTop(TestBox.Height - MaskSize);
            Box bottomMaskDisplaced = bottomMask + MaskSize * Vector.DOWN_VECTOR;

            Box bottomMaskDisplacedHalfLeft = bottomMaskDisplaced.HalfLeft();
            Box bottomMaskDisplacedHalfRight = bottomMaskDisplaced.HalfRight();

            leftChecker.Setup(bottomMaskDisplacedHalfLeft, IgnoreFlags, IgnoreSprites, MaskSize, CheckWithWorld, CheckWithSolidSprites, ComputePlacements, PreciseCollisionCheck);
            rightChecker.Setup(bottomMaskDisplacedHalfRight, IgnoreFlags, IgnoreSprites, MaskSize, CheckWithWorld, CheckWithSolidSprites, ComputePlacements, PreciseCollisionCheck);

            CollisionFlags bottomLeftDisplacedCollisionFlags = leftChecker.GetCollisionFlags();
            CollisionFlags bottomRightDisplacedCollisionFlags = rightChecker.GetCollisionFlags();

            if (!bottomLeftDisplacedCollisionFlags.CanBlockTheMove() && !bottomRightDisplacedCollisionFlags.CanBlockTheMove())
                return CollisionFlags.NONE;

            if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (!bottomLeftDisplacedCollisionFlags.CanBlockTheMove() && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    if (ComputePlacements)
                        placements.AddRange(rightChecker.Placements);

                    return bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && !bottomRightDisplacedCollisionFlags.CanBlockTheMove())
                {
                    if (ComputePlacements)
                        placements.AddRange(leftChecker.Placements);

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    if (ComputePlacements)
                    {
                        placements.AddRange(leftChecker.Placements);
                        placements.AddRange(rightChecker.Placements);
                    }

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }
            }
            else
            {
                Box bottomMaskHalfLeft = bottomMask.HalfLeft();
                Box bottomMaskHalfRight = bottomMask.HalfRight();

                leftChecker.Setup(bottomMaskHalfLeft, IgnoreFlags, IgnoreSprites, MaskSize, CheckWithWorld, CheckWithSolidSprites, ComputePlacements, PreciseCollisionCheck);
                rightChecker.Setup(bottomMaskHalfRight, IgnoreFlags, IgnoreSprites, MaskSize, CheckWithWorld, CheckWithSolidSprites, ComputePlacements, PreciseCollisionCheck);

                CollisionFlags bottomLeftCollisionFlags = leftChecker.GetCollisionFlags();
                CollisionFlags bottomRightCollisionFlags = rightChecker.GetCollisionFlags();

                if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK))
                    {
                        if (ComputePlacements)
                            placements.AddRange(leftChecker.Placements);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER))
                    {
                        if (ComputePlacements)
                            placements.AddRange(leftChecker.Placements);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (rightChecker.SlopeTriangle.HCathetusSign > 0)
                    {
                        if (ComputePlacements)
                            placements.AddRange(rightChecker.Placements);

                        slopeTriangle = rightChecker.SlopeTriangle;
                        return CollisionFlags.SLOPE;
                    }

                    return CollisionFlags.NONE;
                }

                if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.BLOCK)
                    {
                        if (ComputePlacements)
                            placements.AddRange(rightChecker.Placements);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.TOP_LADDER)
                    {
                        if (ComputePlacements)
                            placements.AddRange(rightChecker.Placements);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (leftChecker.SlopeTriangle.HCathetusSign < 0)
                    {
                        if (ComputePlacements)
                            placements.AddRange(leftChecker.Placements);

                        slopeTriangle = leftChecker.SlopeTriangle;
                        return CollisionFlags.SLOPE;
                    }

                    return CollisionFlags.NONE;
                }

                if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (ComputePlacements)
                    {
                        placements.AddRange(leftChecker.Placements);
                        placements.AddRange(rightChecker.Placements);
                    }

                    slopeTriangle = leftChecker.SlopeTriangle;
                    return CollisionFlags.SLOPE;
                }
            }

            return CollisionFlags.NONE;
        }

        // TODO : Optmize it, can be terribly slow!
        public Box MoveContactFloor(FixedSingle maxDistance)
        {
            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
            {
                if (ComputeLandedState().CanBlockTheMove())
                    break;
            }

            return TestBox;
        }

        // TODO : Optmize it, can be terribly slow!
        public bool TryMoveContactFloor(FixedSingle maxDistance)
        {
            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
                if (ComputeLandedState().CanBlockTheMove())
                    return true;

            return false;
        }

        // TODO : Optmize it, can be terribly slow!
        public bool TryMoveContactSlope(FixedSingle maxDistance)
        {
            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
                if (ComputeLandedState().HasFlag(CollisionFlags.SLOPE))
                    return true;

            return false;
        }

        // TODO : Optmize it, can be terribly slow!
        public Box AdjustOnTheFloor(FixedSingle maxDistance)
        {
            if (!ComputeLandedState().CanBlockTheMove())
                return TestBox;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE)
            {
                TestBox += STEP_UP_VECTOR;
                if (!ComputeLandedState().CanBlockTheMove())
                {
                    TestBox -= STEP_UP_VECTOR;
                    break;
                }
            }

            return TestBox;
        }
    }
}