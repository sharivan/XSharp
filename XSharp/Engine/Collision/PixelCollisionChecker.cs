using XSharp.Engine.Entities;
using XSharp.Engine.World;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

namespace XSharp.Engine.Collision
{
    public class PixelCollisionChecker : CollisionChecker
    {
        public PixelCollisionChecker()
        {
        }

        protected override CollisionFlags GetCollisionVectorFlags()
        {
            CollisionFlags result = CollisionFlags.NONE;

            if (CheckWithWorld)                
            {
                Map map = World.GetMapFrom(TestVector);
                if (map != null)
                {
                    Box mapBox = GetMapBoundingBox(GetMapCellFromPos(TestVector));
                    CollisionData collisionData = map.CollisionData;

                    CollisionFlags collisionResult = TestCollision(mapBox, collisionData, TestVector, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                    if (collisionResult != CollisionFlags.NONE)
                        result |= collisionResult;
                }
            }

            if (CheckWithSolidSprites)
            {
                resultSet.Clear();
                Engine.partition.Query(resultSet, TestVector);
                foreach (var entity in resultSet)
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

            return result;
        }

        protected override CollisionFlags GetCollisionBoxFlags()
        {
            Box box = TestBox.RoundOriginToFloor();
            Cell start = GetMapCellFromPos(box.LeftTop);
            Cell end = GetMapCellFromPos(box.RightBottom);

            int startRow = start.Row;
            int startCol = start.Col;

            if (startRow < 0)
                startRow = 0;

            if (startRow >= World.MapRowCount)
                startRow = World.MapRowCount - 1;

            if (startCol < 0)
                startCol = 0;

            if (startCol >= World.MapColCount)
                startCol = World.MapColCount - 1;

            int endRow = end.Row;
            int endCol = end.Col;

            if (endRow < 0)
                endRow = 0;

            if (endRow >= World.MapRowCount)
                endRow = World.MapRowCount - 1;

            if (endCol < 0)
                endCol = 0;

            if (endCol >= World.MapColCount)
                endCol = World.MapColCount - 1;

            CollisionFlags result = CollisionFlags.NONE;

            if (CheckWithWorld)
                for (int row = startRow; row <= endRow; row++)
                    for (int col = startCol; col <= endCol; col++)
                    {
                        var mapPos = GetMapLeftTop(row, col);
                        Map map = World.GetMapFrom(mapPos);
                        if (map != null)
                        {
                            Box mapBox = GetMapBoundingBox(row, col);
                            CollisionData collisionData = map.CollisionData;

                            CollisionFlags collisionResult = TestCollision(mapBox, collisionData, box, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                            if (collisionResult == CollisionFlags.NONE)
                                continue;

                            result |= collisionResult;
                        }
                    }

            if (CheckWithSolidSprites)
            {
                resultSet.Clear();
                Engine.partition.Query(resultSet, box);
                foreach (var entity in resultSet)
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        CollisionFlags collisionResult = TestCollision(sprite.Hitbox, sprite.CollisionData, box, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                    }
            }

            return result;
        }

        // Warning! It can be terribly slow if you use small steps. Recommended step size is 1 (one pixel).
        public Box MoveContactSolid(Vector dir, FixedSingle maxDistance)
        {
            var direction = dir.GetDirection();
            var deltaDir = GetStepVector(dir, STEP_SIZE);
            var startBox = TestBox;

            var distance = STEP_SIZE;
            TestBox = startBox + deltaDir.TruncFracPart();
            int i = 1;
            for (; distance <= maxDistance; i++, distance += STEP_SIZE, TestBox = startBox + (deltaDir * i).TruncFracPart())
                if (GetCollisionFlags().CanBlockTheMove(direction))
                    break;

            TestBox = startBox + (deltaDir * (i - 1)).TruncFracPart();
            return TestBox;
        }

        public CollisionFlags GetTouchingFlags(Direction direction)
        {
            Vector dir = Vector.NULL_VECTOR;

            if (direction.HasFlag(Direction.LEFT))
                dir += STEP_LEFT_VECTOR;
            else if (direction.HasFlag(Direction.RIGHT))
                dir += STEP_RIGHT_VECTOR;

            if (direction.HasFlag(Direction.UP))
                dir += STEP_UP_VECTOR;
            else if (direction.HasFlag(Direction.DOWN))
                dir += STEP_DOWN_VECTOR;

            TestBox += dir;
            var flags = GetCollisionFlags();
            TestBox -= dir;

            return flags;
        }
    }
}