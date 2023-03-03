using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Collision
{
    public class LanderCollisionChecker : PixelCollisionChecker
    {
        private PixelCollisionChecker leftChecker;
        private PixelCollisionChecker rightChecker;

        public LanderCollisionChecker()
        {
            leftChecker = new PixelCollisionChecker();
            rightChecker = new PixelCollisionChecker();
        }

        // TODO : In the future, refactor this to make it simpler and more effective at the same time.
        // TODO : Also, this isn't working fine with slopes for now, fix it!
        public CollisionFlags ComputeLandedState()
        {
            Box box = TestBox.RoundOriginToFloor();
            Box bottomMask = box.ClipTop(box.Height - STEP_SIZE);
            Box bottomMaskDisplaced = bottomMask + STEP_DOWN_VECTOR;

            Box bottomMaskDisplacedHalfLeft = bottomMaskDisplaced.HalfLeft();
            Box bottomMaskDisplacedHalfRight = bottomMaskDisplaced.HalfRight();

            leftChecker.Setup(bottomMaskDisplacedHalfLeft, IgnoreFlags, CheckWithWorld, CheckWithSolidSprites, ComputePlacements);
            rightChecker.Setup(bottomMaskDisplacedHalfRight, IgnoreFlags, CheckWithWorld, CheckWithSolidSprites, ComputePlacements);

            CollisionFlags bottomLeftDisplacedCollisionFlags = leftChecker.GetCollisionFlags();
            CollisionFlags bottomRightDisplacedCollisionFlags = rightChecker.GetCollisionFlags();

            if (!bottomLeftDisplacedCollisionFlags.CanBlockTheMove(Direction.DOWN) && !bottomRightDisplacedCollisionFlags.CanBlockTheMove(Direction.DOWN))
                return CollisionFlags.NONE;

            if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (!bottomLeftDisplacedCollisionFlags.CanBlockTheMove(Direction.DOWN) && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    if (ComputePlacements)
                        placements.AddRange(rightChecker.Placements);

                    return bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && !bottomRightDisplacedCollisionFlags.CanBlockTheMove(Direction.DOWN))
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

                leftChecker.Setup(bottomMaskHalfLeft, IgnoreFlags, CheckWithWorld, CheckWithSolidSprites, ComputePlacements);
                rightChecker.Setup(bottomMaskHalfRight, IgnoreFlags, CheckWithWorld, CheckWithSolidSprites, ComputePlacements);

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

        // Warning! The following methods can be terribly slow if you use small steps. Recommended step size is 1 (one pixel).

        public bool MoveContactFloor(FixedSingle maxDistance)
        {
            for (FixedSingle distance = FixedSingle.ZERO; distance <= maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
            {
                if (ComputeLandedState().CanBlockTheMove(Direction.DOWN))
                    return true;
            }

            return false;
        }

        public bool TryMoveContactFloor(FixedSingle maxDistance)
        {
            var lastBox = TestBox;
            for (FixedSingle distance = FixedSingle.ZERO; distance <= maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
            {
                if (ComputeLandedState().CanBlockTheMove(Direction.DOWN))
                    return true;
            }

            TestBox = lastBox;
            return false;
        }

        public bool TryMoveContactSlope(FixedSingle maxDistance)
        {
            var lastBox = TestBox;
            for (FixedSingle distance = FixedSingle.ZERO; distance <= maxDistance; distance += STEP_SIZE, TestBox += STEP_DOWN_VECTOR)
            {
                if (ComputeLandedState().HasFlag(CollisionFlags.SLOPE))
                    return true;
            }

            TestBox = lastBox;
            return false;
        }

        public bool AdjustOnTheFloor(FixedSingle maxDistance)
        {
            if (!ComputeLandedState().CanBlockTheMove(Direction.DOWN))
                return false;

            var lastBox = TestBox;

            for (FixedSingle distance = FixedSingle.ZERO; distance <= maxDistance; distance += STEP_SIZE)
            {
                TestBox += STEP_UP_VECTOR;
                if (!ComputeLandedState().CanBlockTheMove(Direction.DOWN))
                {
                    TestBox += STEP_DOWN_VECTOR;
                    return true;
                }
            }

            TestBox = lastBox;
            return false;
        }
    }
}