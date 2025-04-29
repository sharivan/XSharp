using System;

using XSharp.Engine.Entities;
using XSharp.Engine.World;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.Functions;

namespace XSharp.Engine.Collision;

[Flags]
public enum TracingMode
{
    NONE = 0,
    HORIZONTAL = 1,
    VERTICAL = 2,
    DIAGONAL = 4
}

// TODO : This class was not fully tested yet. Please do it later.
public class TracerCollisionChecker : CollisionChecker
{
    private bool tracing;
    private Parallelogram tracingParallelogram;

    protected TracingMode TracingBoxMode
    {
        get;
        private set;
    } = TracingMode.NONE;

    public bool TracingBackward
    {
        get;
        private set;
    } = false;

    public Vector TracingVector
    {
        get;
        private set;
    } = Vector.NULL_VECTOR;

    public Vector TracingDirection
    {
        get;
        private set;
    } = Vector.NULL_VECTOR;

    public FixedSingle TracingDistance
    {
        get;
        private set;
    }

    public Box TracingBox
    {
        get;
        private set;
    } = Box.EMPTY_BOX;

    public Box NearestObstacleBox
    {
        get;
        private set;
    } = Box.EMPTY_BOX;

    public RightTriangle NearestObstacleSlope
    {
        get;
        private set;
    } = RightTriangle.EMPTY;

    public CollisionData NearestObstacleCollisionData
    {
        get;
        private set;
    } = CollisionData.NONE;

    public FixedSingle NearestDistance
    {
        get;
        protected set;
    } = 0;

    public FixedSingle NearestBoxDistance
    {
        get;
        protected set;
    } = 0;

    public FixedSingle NearestSlopeDistance
    {
        get;
        protected set;
    } = 0;

    public TracerCollisionChecker()
    {
        tracingParallelogram = new Parallelogram();
    }

    public override void Setup(Vector testVector, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        base.Setup(testVector, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

        tracing = false;
    }

    public override void Setup(Box testBox, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        base.Setup(testBox, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

        tracing = false;
    }

    private FixedSingle VectorDistanceTo(Box obstacleBox)
    {
        var traceLine = new LineSegment(TracingVector, TracingVector + TracingDirection);

        FixedSingle result = TracingDistance;
        for (int i = 0; i < 4; i++)
        {
            var boxSide = (BoxSide) (1 << i);
            var side = obstacleBox.GetSideSegment(boxSide);

            var type = side.Intersection(traceLine, out LineSegment intersection);
            if (type == GeometryType.VECTOR)
            {
                var distance = intersection.Start.DistanceTo(TracingVector);
                if (distance < result)
                    result = distance;
            }
        }

        return result;
    }

    private FixedSingle VectorDistanceTo(Box obstacleBox, RightTriangle obstacleSlope, CollisionData obstacleCollisionData)
    {
        var traceLine = new LineSegment(TracingVector, TracingVector + TracingDirection);

        if (obstacleCollisionData.IsSlope())
        {
            var hypothenuse = obstacleSlope.HypotenuseLine;
            var type = hypothenuse.Intersection(traceLine, out LineSegment intersection);

            return type == GeometryType.VECTOR ? intersection.Start.DistanceTo(TracingVector).TruncFracPart() : FixedSingle.MAX_VALUE;
        }

        return VectorDistanceTo(obstacleBox);
    }

    private FixedSingle BoxDistanceTo(Box obstacleBox)
    {
        if (TracingBoxMode == (TracingMode.HORIZONTAL | TracingMode.DIAGONAL))
        {
            var tracingLine = new LineSegment(TracingBox.LeftTop, TracingBox.LeftTop + TracingDirection);
            var x = TracingBackward ? obstacleBox.Left : obstacleBox.Right;
            var obstacleLine = new LineSegment((x, TestBox.Top), (x, TestBox.Bottom));
            var type = tracingLine.Intersection(obstacleLine, out tracingLine);

            return type == GeometryType.VECTOR ? tracingLine.Length.TruncFracPart() : TracingDirection.Length.TruncFracPart() + 1;
        }

        FixedSingle offset = TracingBoxMode == TracingMode.VERTICAL
            ? TracingBackward
                ? obstacleBox.Bottom - TracingBox.Top
                : obstacleBox.Top - TracingBox.Bottom
            : TracingBackward
                ? obstacleBox.Right - TracingBox.Left
                : obstacleBox.Left - TracingBox.Right;

        return offset.Abs;
    }

    private FixedSingle BoxDistanceTo(RightTriangle obstacleSlope)
    {
        var mb = TracingBox.MiddleBottom;
        var hypothenuse = obstacleSlope.HypotenuseLine;
        var bottomLine = new LineSegment(mb, mb + TracingDirection);
        var type = hypothenuse.Intersection(bottomLine, out LineSegment intersection);

        return TracingBoxMode == TracingMode.VERTICAL
            ? type == GeometryType.VECTOR ? (intersection.Start.Y - mb.Y).Abs.TruncFracPart() : TracingDirection.Length.TruncFracPart() + 1
            : type == GeometryType.VECTOR ? (intersection.Start.X - mb.X).Abs.TruncFracPart() : TracingDirection.Length.TruncFracPart() + 1;
    }

    private void CompareVectorAndUpdateWithNearestObstacle(Box obstacleBox, RightTriangle obstacleSlope, CollisionData obstacleCollisionData)
    {
        FixedSingle distance = VectorDistanceTo(obstacleBox, obstacleSlope, obstacleCollisionData);
        if (distance > TracingDistance)
            return;

        if (distance < NearestDistance)
        {
            NearestDistance = distance;

            if (obstacleCollisionData.IsSlope())
            {
                NearestSlopeDistance = distance;
                NearestObstacleSlope = obstacleSlope;
            }
            else
            {
                NearestBoxDistance = distance;
                NearestObstacleBox = obstacleBox;
            }

            NearestObstacleCollisionData = obstacleCollisionData;
        }
    }

    private void CompareBoxAndUpdateWithNearestObstacle(Box obstacleBox, RightTriangle obstacleSlope, CollisionData obstacleCollisionData)
    {
        FixedSingle distance = BoxDistanceTo(obstacleBox);
        if (distance > TracingDistance)
            return;

        if (distance < NearestBoxDistance)
        {
            if (obstacleCollisionData.IsSlope())
            {
                var slopeDistance = BoxDistanceTo(obstacleSlope);
                if (slopeDistance < NearestSlopeDistance)
                {
                    NearestDistance = slopeDistance;
                    NearestSlopeDistance = slopeDistance;
                    NearestBoxDistance = distance;
                    NearestObstacleBox = obstacleBox;
                    NearestObstacleSlope = obstacleSlope;
                    NearestObstacleCollisionData = obstacleCollisionData;
                }
            }
            else
            {
                NearestDistance = distance;
                NearestBoxDistance = distance;
                NearestObstacleBox = obstacleBox;
                NearestObstacleCollisionData = obstacleCollisionData;
            }
        }
        else if (distance == NearestBoxDistance && obstacleCollisionData.IsSlope())
        {
            distance = BoxDistanceTo(obstacleSlope);
            if (distance < NearestSlopeDistance)
            {
                NearestDistance = distance;
                NearestSlopeDistance = distance;
                NearestObstacleBox = obstacleBox;
                NearestObstacleSlope = obstacleSlope;
                NearestObstacleCollisionData = obstacleCollisionData;
            }
        }
    }

    protected override CollisionFlags GetCollisionVectorFlags()
    {
        CollisionFlags result = CollisionFlags.NONE;
        NearestObstacleBox = Box.EMPTY_BOX;
        var tracingLine = new LineSegment(TracingVector, TracingVector + TracingDirection);

        if (CheckWithWorld)
        {
            if (tracing)
            {
                Vector stepVector = GetStepVector(TracingDirection, MAP_SIZE);
                FixedSingle maxDistance = TracingDirection.X > TracingDirection.Y ? TracingDirection.X : TracingDirection.Y;

                for (FixedSingle distance = 0; distance <= maxDistance; distance += MAP_SIZE, TestVector += stepVector)
                {
                    Map map = World.ForegroundLayout.GetMapFrom(TestVector);
                    if (map != null)
                    {
                        Box mapBox = GetMapBoundingBox(GetMapCellFromPos(TestVector));
                        CollisionData collisionData = map.CollisionData;

                        CollisionFlags collisionResult = TestCollision(mapBox, collisionData, tracingLine, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                        CompareVectorAndUpdateWithNearestObstacle(mapBox, slopeTriangle, collisionData);
                    }
                }
            }
            else
            {
                Map map = World.ForegroundLayout.GetMapFrom(TestVector);
                if (map != null)
                {
                    Box mapBox = GetMapBoundingBox(GetMapCellFromPos(TestVector));
                    CollisionData collisionData = map.CollisionData;

                    CollisionFlags collisionResult = TestCollision(mapBox, collisionData, TestVector, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                    if (collisionResult != CollisionFlags.NONE)
                        result |= collisionResult;
                }
            }
        }

        if (CheckWithSolidSprites)
        {
            resultSet.Clear();
            if (tracing)
            {
                Engine.partition.Query(resultSet, tracingLine);
                foreach (var entity in resultSet)
                {
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        var hitbox = sprite.Hitbox;
                        var collisionData = sprite.CollisionData;
                        CollisionFlags collisionResult = TestCollision(hitbox, collisionData, tracingLine, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                        CompareVectorAndUpdateWithNearestObstacle(hitbox, slopeTriangle, collisionData);
                    }
                }
            }
            else
            {
                Engine.partition.Query(resultSet, TestVector);
                foreach (var entity in resultSet)
                {
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        var hitbox = sprite.Hitbox;
                        var collisionData = sprite.CollisionData;
                        CollisionFlags collisionResult = TestCollision(hitbox, collisionData, TestVector, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                    }
                }
            }
        }

        return result;
    }

    protected override CollisionFlags GetCollisionBoxFlags()
    {
        CollisionFlags result = CollisionFlags.NONE;
        NearestObstacleBox = Box.EMPTY_BOX;

        if (CheckWithWorld)
        {
            if (tracing && TracingBoxMode.HasFlag(TracingMode.DIAGONAL))
            {
                Vector stepVector = GetStepVectorHorizontal(TracingDirection, MAP_SIZE);
                FixedSingle maxDistance = TracingDirection.X > TracingDirection.Y ? TracingDirection.X : TracingDirection.Y;

                TestBox = TracingBox;
                for (FixedSingle distance = 0; distance <= maxDistance; distance += MAP_SIZE, TestBox += stepVector)
                {
                    Cell start = GetMapCellFromPos(TestBox.LeftTop);
                    Cell end = GetMapCellFromPos(TestBox.RightBottom);

                    int startRow = start.Row;
                    int startCol = start.Col;

                    if (startRow < 0)
                        startRow = 0;

                    if (startRow >= World.ForegroundLayout.MapRowCount)
                        startRow = World.ForegroundLayout.MapRowCount - 1;

                    if (startCol < 0)
                        startCol = 0;

                    if (startCol >= World.ForegroundLayout.MapColCount)
                        startCol = World.ForegroundLayout.MapColCount - 1;

                    int endRow = end.Row;
                    int endCol = end.Col;

                    if (endRow < 0)
                        endRow = 0;

                    if (endRow >= World.ForegroundLayout.MapRowCount)
                        endRow = World.ForegroundLayout.MapRowCount - 1;

                    if (endCol < 0)
                        endCol = 0;

                    if (endCol >= World.ForegroundLayout.MapColCount)
                        endCol = World.ForegroundLayout.MapColCount - 1;

                    for (int row = startRow; row <= endRow; row++)
                    {
                        for (int col = startCol; col <= endCol; col++)
                        {
                            var mapPos = GetMapLeftTop(row, col);
                            Map map = World.ForegroundLayout.GetMapFrom(mapPos);
                            if (map != null)
                            {
                                Box mapBox = GetMapBoundingBox(row, col);
                                CollisionData collisionData = map.CollisionData;

                                CollisionFlags collisionResult = TestCollision(mapBox, collisionData, tracingParallelogram, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                                if (collisionResult == CollisionFlags.NONE)
                                    continue;

                                result |= collisionResult;
                                CompareVectorAndUpdateWithNearestObstacle(mapBox, slopeTriangle, collisionData);
                            }
                        }
                    }
                }
            }
            else
            {
                Cell start = GetMapCellFromPos(TestBox.LeftTop);
                Cell end = GetMapCellFromPos(TestBox.RightBottom);

                int startRow = start.Row;
                int startCol = start.Col;

                if (startRow < 0)
                    startRow = 0;

                if (startRow >= World.ForegroundLayout.MapRowCount)
                    startRow = World.ForegroundLayout.MapRowCount - 1;

                if (startCol < 0)
                    startCol = 0;

                if (startCol >= World.ForegroundLayout.MapColCount)
                    startCol = World.ForegroundLayout.MapColCount - 1;

                int endRow = end.Row;
                int endCol = end.Col;

                if (endRow < 0)
                    endRow = 0;

                if (endRow >= World.ForegroundLayout.MapRowCount)
                    endRow = World.ForegroundLayout.MapRowCount - 1;

                if (endCol < 0)
                    endCol = 0;

                if (endCol >= World.ForegroundLayout.MapColCount)
                    endCol = World.ForegroundLayout.MapColCount - 1;

                for (int row = startRow; row <= endRow; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        var mapPos = GetMapLeftTop(row, col);
                        Map map = World.ForegroundLayout.GetMapFrom(mapPos);
                        if (map != null)
                        {
                            Box mapBox = GetMapBoundingBox(row, col);
                            CollisionData collisionData = map.CollisionData;

                            CollisionFlags collisionResult = TestCollision(mapBox, collisionData, TestBox, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                            if (collisionResult == CollisionFlags.NONE)
                                continue;

                            result |= collisionResult;

                            if (tracing)
                                CompareBoxAndUpdateWithNearestObstacle(mapBox, slopeTriangle, collisionData);
                        }
                    }
                }
            }
        }

        if (CheckWithSolidSprites)
        {
            resultSet.Clear();

            if (tracing && TracingBoxMode.HasFlag(TracingMode.DIAGONAL))
            {
                Engine.partition.Query(resultSet, tracingParallelogram);
                foreach (var entity in resultSet)
                {
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        var hitbox = sprite.Hitbox;
                        var collisionData = sprite.CollisionData;
                        CollisionFlags collisionResult = TestCollision(hitbox, collisionData, tracingParallelogram, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;

                        CompareBoxAndUpdateWithNearestObstacle(hitbox, slopeTriangle, collisionData);
                    }
                }
            }
            else
            {
                Engine.partition.Query(resultSet, TestBox);
                foreach (var entity in resultSet)
                {
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        var hitbox = sprite.Hitbox;
                        var collisionData = sprite.CollisionData;
                        CollisionFlags collisionResult = TestCollision(hitbox, collisionData, TestBox, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);

                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;

                        if (tracing)
                            CompareBoxAndUpdateWithNearestObstacle(hitbox, slopeTriangle, collisionData);
                    }
                }
            }
        }

        return result;
    }

    public CollisionFlags GetTouchingFlagsLeft()
    {
        var lastBox = TestBox;
        TestBox = new Box(TestBox.LeftTop - (STEP_SIZE, 0), STEP_SIZE, TestBox.Height);
        var flags = GetCollisionFlags();
        TestBox = lastBox;
        return flags;
    }

    public CollisionFlags GetTouchingFlagsUp()
    {
        var lastBox = TestBox;
        TestBox = new Box(TestBox.LeftTop - (0, STEP_SIZE), TestBox.Width, STEP_SIZE);
        var flags = GetCollisionFlags();
        TestBox = lastBox;
        return flags;
    }

    public CollisionFlags GetTouchingFlagsRight()
    {
        var lastBox = TestBox;
        TestBox = new Box(TestBox.RightTop, STEP_SIZE, TestBox.Height);
        var flags = GetCollisionFlags();
        TestBox = lastBox;
        return flags;
    }

    public CollisionFlags GetTouchingFlagsDown()
    {
        var lastBox = TestBox;
        TestBox = new Box(TestBox.LeftBottom, TestBox.Width, STEP_SIZE);
        var flags = GetCollisionFlags();
        TestBox = lastBox;
        return flags;
    }

    public CollisionFlags TraceRay(Vector direction, FixedSingle maxDistance)
    {
        if (direction == Vector.NULL_VECTOR || maxDistance <= 0)
            return CollisionFlags.NONE;

        if (TestKind != TouchingKind.VECTOR)
        {
            TestKind = TouchingKind.VECTOR;
            TestVector = TestBox.Origin;
        }

        var directionVersor = direction.Versor();

        tracing = true;
        TracingDistance = maxDistance;
        NearestDistance = TracingDistance + 1;
        NearestBoxDistance = NearestDistance;
        NearestSlopeDistance = NearestDistance;
        NearestObstacleCollisionData = CollisionData.NONE;
        TracingVector = TestVector;
        TracingDirection = maxDistance * directionVersor;

        var flags = GetCollisionFlags();
        TestVector = TracingVector + (NearestDistance * directionVersor).TruncFracPart();
        tracing = false;

        return flags;
    }

    public CollisionFlags MoveContactSolidHorizontal(FixedSingle dx)
    {
        if (dx == 0)
            return CollisionFlags.NONE;

        tracing = true;
        TestKind = TouchingKind.BOX;
        TracingDistance = dx.Abs;
        NearestDistance = TracingDistance + 1;
        NearestBoxDistance = NearestDistance;
        NearestSlopeDistance = NearestDistance;
        NearestObstacleCollisionData = CollisionData.NONE;
        TracingBox = TestBox;
        TestBox = new Box(dx < 0 ? TestBox.LeftTop - (TracingDistance, 0) : TestBox.RightTop, TracingDistance, TestBox.Height);
        TracingBoxMode = TracingMode.HORIZONTAL;
        TracingBackward = dx < 0;
        TracingDirection = dx * Vector.RIGHT_VECTOR;

        var flags = GetCollisionFlags();
        TestBox = TracingBox + (flags != CollisionFlags.NONE ? (dx.Signal * NearestDistance.TruncFracPart(), 0) : (dx, 0));

        tracing = false;

        return flags;
    }

    public CollisionFlags MoveContactSolidDiagonalHorizontal(Vector direction)
    {
        if (direction.X == 0)
            return CollisionFlags.NONE;

        FixedSingle dx = direction.X;

        tracing = true;
        TestKind = TouchingKind.BOX;
        TracingDistance = direction.Length;
        NearestDistance = TracingDistance + 1;
        NearestBoxDistance = NearestDistance;
        NearestSlopeDistance = NearestDistance;
        NearestObstacleCollisionData = CollisionData.NONE;
        TracingBox = TestBox;
        tracingParallelogram.SetupVertical(dx < 0 ? TestBox.LeftTop : TestBox.RightTop, direction, TestBox.Height);
        TestBox = tracingParallelogram.WrappingBox;
        TracingBoxMode = TracingMode.HORIZONTAL | TracingMode.DIAGONAL;
        TracingBackward = dx < 0;
        TracingDirection = direction;

        var flags = GetCollisionFlags();
        TestBox = TracingBox + (flags != CollisionFlags.NONE ? direction.VersorScale(NearestDistance).TruncFracPart() : direction);

        tracing = false;

        return flags;
    }

    public CollisionFlags MoveContactSolidVertical(FixedSingle dy)
    {
        if (dy == 0)
            return CollisionFlags.NONE;

        tracing = true;
        TestKind = TouchingKind.BOX;
        TracingDistance = dy.Abs;
        NearestDistance = TracingDistance + 1;
        NearestBoxDistance = NearestDistance;
        NearestSlopeDistance = NearestDistance;
        NearestObstacleCollisionData = CollisionData.NONE;
        TracingBox = TestBox;
        TestBox = new Box(dy < 0 ? TestBox.LeftTop - (0, TracingDistance) : TestBox.LeftBottom, TestBox.Width, TracingDistance);
        TracingBoxMode = TracingMode.VERTICAL;
        TracingBackward = dy < 0;
        TracingDirection = dy * Vector.UP_VECTOR;

        var flags = GetCollisionFlags();
        TestBox = TracingBox + (flags != CollisionFlags.NONE ? (0, dy.Signal * NearestDistance.TruncFracPart()) : (0, dy));

        tracing = false;

        return flags;
    }

    public CollisionFlags MoveContactSolidDiagonalVertical(Vector direction)
    {
        // TODO : Implement (if needed)
        throw new NotImplementedException();
    }

    public CollisionFlags ComputeLandedState()
    {
        return ComputeLandedState(out _);
    }

    private bool IsPerfectlyLandedOnSlope()
    {
        var lastTestKind = TestKind;
        TestVector = TestBox.MiddleBottom;
        TestKind = TouchingKind.VECTOR;

        var flags = TraceRay(Vector.DOWN_VECTOR, STEP_SIZE);
        TestKind = lastTestKind;

        return flags == CollisionFlags.SLOPE && (TestVector.Y - TestBox.Bottom).Abs < STEP_SIZE;
    }

    public CollisionFlags ComputeLandedState(out bool perfectlyLanded)
    {
        if (IsPerfectlyLandedOnSlope())
        {
            perfectlyLanded = true;
            return CollisionFlags.SLOPE;
        }

        if (GetCollisionFlags().HasFlag(CollisionFlags.SLOPE))
        {
            perfectlyLanded = false;
            return CollisionFlags.SLOPE;
        }

        IgnoreFlags &= ~CollisionFlags.SLOPE;

        var lastBox = TestBox;
        var lastIgnoreFlags = IgnoreFlags;
        var flags = MoveContactSolidVertical(STEP_SIZE);

        IgnoreFlags = lastIgnoreFlags;
        TestBox = lastBox;

        if (flags.CanBlockTheMove(Direction.DOWN))
        {
            flags = NearestObstacleCollisionData.ToCollisionFlags();
            perfectlyLanded = (lastBox.Bottom - NearestObstacleBox.Top).Abs < STEP_SIZE;
            return flags;
        }

        perfectlyLanded = false;
        return CollisionFlags.NONE;
    }

    private CollisionFlags AdjustOnTheSlope(FixedSingle maxDistance)
    {
        var line = new LineSegment(TestBox.MiddleBottom, TestBox.MiddleBottom - (0, maxDistance));
        var type = NearestObstacleSlope.HypotenuseLine.Intersection(line, out LineSegment intersection);
        if (type == GeometryType.VECTOR)
        {
            Vector delta = intersection.Start - TestBox.MiddleBottom;
            TestBox += delta;
            return CollisionFlags.SLOPE;
        }

        return CollisionFlags.NONE;
    }

    public CollisionFlags AdjustOnTheFloor(FixedSingle maxDistance)
    {
        var flags = GetCollisionFlags();
        if (!flags.CanBlockTheMove(Direction.DOWN))
            return CollisionFlags.NONE;

        flags = ComputeLandedState();
        if (flags == CollisionFlags.SLOPE)
            return AdjustOnTheSlope(maxDistance);

        var lastBox = TestBox;
        TestBox -= (0, maxDistance);
        if (MoveContactSolidVertical(maxDistance) == CollisionFlags.NONE)
            TestBox = lastBox;

        flags = ComputeLandedState(out bool perfectlyLanded);
        return perfectlyLanded ? flags : CollisionFlags.NONE;
    }
}