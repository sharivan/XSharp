using System;
using System.Collections.Generic;
using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    [Flags]
    public enum TracingMode
    {
        NONE = 0,
        HORIZONTAL = 1,
        VERTICAL = 2,
        DIAGONAL = 4
    }

    public class HorizontalParallelogram : GeometrySet
    {
        private Box wrappingBox;
        private RightTriangle triangle1;
        private RightTriangle triangle2;

        public Box WrappingBox => wrappingBox;

        public Vector LeftTop => wrappingBox.LeftTop;

        public FixedSingle Width => wrappingBox.Width;

        public FixedSingle Height => wrappingBox.Height;

        public Vector Origin
        {
            get;
            private set;
        }

        public Vector Direction
        {
            get;
            private set;
        }

        public FixedSingle SmallerHeight
        {
            get;
            private set;
        }

        public HorizontalParallelogram() : base(SetOperation.INTERSECTION, (Box.EMPTY_BOX, false), (RightTriangle.EMPTY, true), (RightTriangle.EMPTY, true))
        {
        }

        public void Setup(Vector origin, Vector direction, FixedSingle smallerHeight)
        {
            GeometryOperations.HorizontalParallelogram(origin, direction, smallerHeight, out wrappingBox, out triangle1, out triangle2);

            parts[0] = (wrappingBox, false);
            parts[1] = (triangle1, true);
            parts[2] = (triangle2, true);

            Origin = origin;
            Direction = direction;
            SmallerHeight = smallerHeight;
        }
    }

    public class CollisionChecker
    {
        public static Vector GetStepVectorHorizontal(Vector dir, FixedSingle stepSize)
        {
            var dx = dir.X;
            var dy = dir.Y;

            if (dx == 0)
                return dy > 0 ? stepSize * Vector.DOWN_VECTOR : dy < 0 ? stepSize * Vector.UP_VECTOR : Vector.NULL_VECTOR;

            if (dy == 0)
                return dx > 0 ? stepSize * Vector.RIGHT_VECTOR : stepSize * Vector.LEFT_VECTOR;

            var xm = dx.Abs;

            return (dx.Signal * stepSize, (FixedSingle) ((FixedDouble) dy * stepSize / xm));
        }

        public static Vector GetStepVectorVertical(Vector dir, FixedSingle stepSize)
        {
            var dx = dir.X;
            var dy = dir.Y;

            if (dx == 0)
                return dy > 0 ? stepSize * Vector.DOWN_VECTOR : dy < 0 ? stepSize * Vector.UP_VECTOR : Vector.NULL_VECTOR;

            if (dy == 0)
                return dx > 0 ? stepSize * Vector.RIGHT_VECTOR : stepSize * Vector.LEFT_VECTOR;

            var ym = dy.Abs;

            return ((FixedSingle) ((FixedDouble) dx / ym * stepSize), dy.Signal * stepSize);
        }

        public static Vector GetStepVector(Vector dir, FixedSingle stepSize)
        {
            return dir.X.Abs > dir.Y.Abs ? GetStepVectorHorizontal(dir, stepSize) : GetStepVectorVertical(dir, stepSize);
        }

        public static bool HasIntersection(Vector v, Box box, BoxSide include = BoxSide.LEFT | BoxSide.TOP | BoxSide.INNER)
        {
            return box.Contains(v, EPSLON, include);
        }

        public static bool HasIntersection(Vector v, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.Contains(v, EPSLON, include);
        }

        public static bool HasIntersection(LineSegment line, Box box)
        {
            return box.HasIntersectionWith(line, EPSLON);
        }

        public static bool HasIntersection(LineSegment line, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.HasIntersectionWith(line, EPSLON, include);
        }

        public static bool HasIntersection(HorizontalParallelogram parallelogram, Box box)
        {
            return box.HasIntersectionWith(parallelogram);
        }

        public static bool HasIntersection(HorizontalParallelogram parallelogram, RightTriangle slope)
        {
            return slope.HasIntersectionWith(parallelogram);
        }

        public static bool HasIntersection(Box box1, Box box2)
        {
            return box1.IsOverlaping(box2);
        }

        public static bool HasIntersection(Box box, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.HasIntersectionWith(box, EPSLON, include);
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Box collisionBox, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(box, collisionBox))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            if (collisionData.IsSolidBlock() && !ignore.HasFlag(CollisionFlags.BLOCK))
            {
                if (collisionData == CollisionData.UNCLIMBABLE_SOLID)
                {
                    if (!ignore.HasFlag(CollisionFlags.UNCLIMBABLE))
                    {
                        placements?.Add(new CollisionPlacement(collisionData, box));
                        result = CollisionFlags.BLOCK | CollisionFlags.UNCLIMBABLE;
                    }
                }
                else
                {
                    placements?.Add(new CollisionPlacement(collisionData, box));
                    result = CollisionFlags.BLOCK;
                }
            }
            else if (collisionData == CollisionData.LADDER && !ignore.HasFlag(CollisionFlags.LADDER))
            {
                placements?.Add(new CollisionPlacement(collisionData, box));

                result = CollisionFlags.LADDER;
            }
            else if (collisionData == CollisionData.TOP_LADDER && !ignore.HasFlag(CollisionFlags.TOP_LADDER))
            {
                placements?.Add(new CollisionPlacement(collisionData, box));

                result = CollisionFlags.TOP_LADDER;
            }
            else if (collisionData == CollisionData.WATER && !ignore.HasFlag(CollisionFlags.WATER))
            {
                placements?.Add(new CollisionPlacement(collisionData, box));

                result = CollisionFlags.WATER;
            }
            else if (collisionData == CollisionData.WATER_SURFACE && !ignore.HasFlag(CollisionFlags.WATER_SURFACE))
            {
                placements?.Add(new CollisionPlacement(collisionData, box));

                result = CollisionFlags.WATER_SURFACE;
            }
            else if (!ignore.HasFlag(CollisionFlags.SLOPE) && collisionData.IsSlope())
            {
                RightTriangle st = collisionData.MakeSlopeTriangle() + box.LeftTop;
                if (HasIntersection(collisionBox, st))
                {
                    placements?.Add(new CollisionPlacement(collisionData, box));

                    slopeTriangle = st;
                    result = CollisionFlags.SLOPE;
                }
            }

            return result;
        }

        protected RightTriangle slopeTriangle;
        protected List<CollisionPlacement> placements;

        private HashSet<Entity> resultSet;

        public Box TestBox
        {
            get;
            set;
        }

        public EntityList<Sprite> IgnoreSprites
        {
            get;
        }

        public FixedSingle MaskSize
        {
            get;
            set;
        } = MASK_SIZE;

        public bool CheckWithWorld
        {
            get;
            set;
        } = true;

        public bool CheckWithSolidSprites
        {
            get;
            set;
        } = true;

        public CollisionFlags IgnoreFlags
        {
            get;
            set;
        } = CollisionFlags.NONE;

        public bool PreciseCollisionCheck
        {
            get;
            set;
        } = true;

        public bool ComputePlacements
        {
            get;
            set;
        } = false;

        public IEnumerable<CollisionPlacement> Placements => placements;

        public RightTriangle SlopeTriangle => slopeTriangle;

        public GameEngine Engine => GameEngine.Engine;

        public World World => GameEngine.Engine.World;

        public CollisionChecker()
        {
            placements = new List<CollisionPlacement>();
            resultSet = new HashSet<Entity>();
            IgnoreSprites = new EntityList<Sprite>();
        }

        public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            if (computePlacements)
                placements.Clear();

            TestBox = testBox;
            IgnoreFlags = ignoreFlags;
            MaskSize = maskSize;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;
            PreciseCollisionCheck = preciseCollisionCheck;
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox)
        {
            Setup(testBox, CollisionFlags.NONE, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, params Sprite[] ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, MASK_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Box testBox, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, BitSet ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags)
        {
            Setup(testBox, ignoreFlags, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, MASK_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, MASK_SIZE, true, true, false, true);
        }

        public CollisionFlags GetCollisionFlags()
        {
            Cell start = World.GetMapCellFromPos(TestBox.LeftTop);
            Cell end = World.GetMapCellFromPos(TestBox.RightBottom);

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
                        var mapPos = World.GetMapLeftTop(row, col);
                        Map map = World.GetMapFrom(mapPos);
                        if (map != null)
                        {
                            Box mapBox = World.GetMapBoundingBox(row, col);
                            CollisionData collisionData = map.CollisionData;

                            CollisionFlags collisionResult = TestCollision(mapBox, collisionData, TestBox, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags, PreciseCollisionCheck);
                            if (collisionResult == CollisionFlags.NONE)
                                continue;

                            result |= collisionResult;
                        }
                    }

            if (CheckWithSolidSprites)
            {
                resultSet.Clear();
                Engine.partition.Query(resultSet, TestBox, BoxKind.HITBOX);
                foreach (var entity in resultSet)
                    if (entity is Sprite sprite && sprite.CollisionData.IsSolidBlock() && !IgnoreSprites.Contains(sprite))
                    {
                        CollisionFlags collisionResult = TestCollision(sprite.Hitbox, sprite.CollisionData, TestBox, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags, PreciseCollisionCheck);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                    }
            }

            return result;
        }

        // TODO : Optmize it, can be terribly slow!
        public Box MoveUntilIntersect(Vector dir, FixedSingle maxDistance)
        {
            var deltaDir = GetStepVectorHorizontal(dir, STEP_SIZE);
            var startBox = TestBox;

            for (int i = 0; i * STEP_SIZE < maxDistance; i++, TestBox = startBox + (deltaDir * i).TruncFracPart())
                if (GetCollisionFlags().CanBlockTheMove())
                    break;

            return TestBox;
        }

        public CollisionFlags GetTouchingFlags(Vector dir)
        {
            TestBox += dir;
            return GetCollisionFlags();
        }
    }
}