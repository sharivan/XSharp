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

    public enum ParallelogramVertex
    {
        LEFT_TOP = 0,
        LEFT_BOTTOM = 1,
        RIGHT_BOTTOM = 2,
        RIGHT_TOP = 3
    }

    public enum ParallelogramSide
    {
        LEFT = 0,
        TOP = 1,
        RIGHT = 2,
        BOTTOM = 3
    }

    public class Parallelogram : GeometrySet, IShape
    {
        private static void HorizontalParallelogram(Vector origin, Vector direction, FixedSingle smallWidth, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
        {
            if (direction.X > 0)
            {
                if (direction.Y > 0)
                {
                    box = new Box(origin, smallWidth + direction.X, direction.Y);
                    triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                    triangle2 = new RightTriangle(origin + (direction.X + smallWidth, 0), -direction.X, direction.Y);
                }
                else
                {
                    box = new Box(origin + (0, direction.Y), smallWidth + direction.X, -direction.Y);
                    triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                    triangle2 = new RightTriangle(origin + (direction.X + smallWidth, 0), -direction.X, direction.Y);
                }
            }
            else if (direction.Y > 0)
            {
                box = new Box(origin + (direction.X, 0), smallWidth - direction.X, direction.Y);
                triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                triangle2 = new RightTriangle(origin + (-smallWidth, direction.Y), direction.X, -direction.Y);
            }
            else
            {
                box = new Box(origin + direction, smallWidth - direction.X, -direction.Y);
                triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                triangle2 = new RightTriangle(origin + (-smallWidth, direction.Y), direction.X, -direction.Y);
            }
        }

        private static void VerticalParallelogram(Vector origin, Vector direction, FixedSingle smallHeight, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
        {
            if (direction.X > 0)
            {
                if (direction.Y > 0)
                {
                    box = new Box(origin, direction.X, smallHeight + direction.Y);
                    triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                    triangle2 = new RightTriangle(origin + (0, direction.Y + smallHeight), direction.X, -direction.Y);
                }
                else
                {
                    box = new Box(origin + (0, direction.Y), direction.X, smallHeight - direction.Y);
                    triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                    triangle2 = new RightTriangle(origin + (direction.X, smallHeight), -direction.X, direction.Y);
                }
            }
            else if (direction.Y > 0)
            {
                box = new Box(origin + (direction.X, 0), -direction.X, smallHeight + direction.Y);
                triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                triangle2 = new RightTriangle(origin + (0, direction.Y + smallHeight), direction.X, -direction.Y);
            }
            else
            {
                box = new Box(origin + direction, -direction.X, smallHeight - direction.Y);
                triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                triangle2 = new RightTriangle(origin + (direction.X, smallHeight), -direction.X, direction.Y);
            }
        }

        private Vector[] vertices;
        private LineSegment[] sides;
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

        public override FixedSingle Length => 2 * (Direction.Length + SmallerHeight);

        public FixedDouble Area => WrappingBox.Area - triangle1.Area - triangle2.Area;

        public Parallelogram() : base(SetOperation.INTERSECTION, (Box.EMPTY_BOX, false), (RightTriangle.EMPTY, true), (RightTriangle.EMPTY, true))
        {
            vertices = new Vector[4];
            sides = new LineSegment[4];
        }

        public LineSegment GetSegment(ParallelogramSide side)
        {
            return sides[(int) side];
        }

        public Vector GetVertex(ParallelogramVertex vertex)
        {
            return vertices[(int) vertex];
        }

        public void SetupHorizontal(Vector origin, Vector direction, FixedSingle smallerHeight)
        {
            HorizontalParallelogram(origin, direction, smallerHeight, out wrappingBox, out triangle1, out triangle2);

            parts[0] = (wrappingBox, false);
            parts[1] = (triangle1, true);
            parts[2] = (triangle2, true);

            if (direction.X.Signal * direction.Y.Signal > 0)
            {
                vertices[0] = triangle1.HCathetusOpositeVertex;
                vertices[1] = triangle1.VCathetusOpositeVertex;
                vertices[2] = triangle2.HCathetusOpositeVertex;
                vertices[3] = triangle2.VCathetusOpositeVertex;
            }
            else
            {
                vertices[0] = triangle1.VCathetusOpositeVertex;
                vertices[1] = triangle1.HCathetusOpositeVertex;
                vertices[2] = triangle2.VCathetusOpositeVertex;
                vertices[3] = triangle2.HCathetusOpositeVertex;
            }

            sides[0] = new LineSegment(vertices[0], vertices[1]);
            sides[1] = new LineSegment(vertices[1], vertices[2]);
            sides[2] = new LineSegment(vertices[2], vertices[3]);
            sides[3] = new LineSegment(vertices[3], vertices[0]);

            Origin = origin;
            Direction = direction;
            SmallerHeight = smallerHeight;
        }

        public void SetupVertical(Vector origin, Vector direction, FixedSingle smallerHeight)
        {
            VerticalParallelogram(origin, direction, smallerHeight, out wrappingBox, out triangle1, out triangle2);

            parts[0] = (wrappingBox, false);
            parts[1] = (triangle1, true);
            parts[2] = (triangle2, true);

            if (direction.X.Signal * direction.Y.Signal > 0)
            {
                vertices[0] = triangle1.VCathetusOpositeVertex;
                vertices[1] = triangle2.HCathetusOpositeVertex;
                vertices[2] = triangle2.VCathetusOpositeVertex;
                vertices[3] = triangle1.HCathetusOpositeVertex;
            }
            else
            {
                vertices[0] = triangle1.HCathetusOpositeVertex;
                vertices[1] = triangle2.VCathetusOpositeVertex;
                vertices[2] = triangle2.HCathetusOpositeVertex;
                vertices[3] = triangle1.VCathetusOpositeVertex;
            }

            sides[0] = new LineSegment(vertices[0], vertices[1]);
            sides[1] = new LineSegment(vertices[1], vertices[2]);
            sides[2] = new LineSegment(vertices[2], vertices[3]);
            sides[3] = new LineSegment(vertices[3], vertices[0]);

            Origin = origin;
            Direction = direction;
            SmallerHeight = smallerHeight;
        }

        public bool HasIntersection(LineSegment line)
        {
            foreach (var side in sides)
                if (side.HasIntersectionWith(line))
                    return true;

            return Contains(line.Start) || Contains(line.End);
        }

        public bool HasIntersection(Box box)
        {
            Box intersection = box & wrappingBox;
            if (!intersection.IsValid())
                return false;

            if (intersection == wrappingBox
                || Contains(intersection.LeftTop)
                || Contains(intersection.RightTop)
                || Contains(intersection.LeftBottom)
                || Contains(intersection.RightBottom))
                return true;

            foreach (var side in sides)
                if (intersection.HasIntersectionWith(side))
                    return true;

            return false;
        }

        public bool HasIntersection(RightTriangle triangle)
        {
            Box intersection = triangle.WrappingBox & wrappingBox;
            if (!intersection.IsValid())
                return false;

            if (Contains(triangle.HypothenuseOpositeVertex)
                || Contains(triangle.HCathetusOpositeVertex)
                || Contains(triangle.VCathetusOpositeVertex))
                return true;

            foreach (var vertex in vertices)
                if (triangle.Contains(vertex))
                    return true;

            foreach (var side in sides)
                if (triangle.HasIntersectionWith(side))
                    return true;

            return false;
        }

        public override bool HasIntersectionWith(IGeometry geometry)
        {
            return (IGeometry) this == geometry
                || geometry switch
                {
                    Vector v => Contains(v),
                    Box box => HasIntersection(box),
                    LineSegment line => HasIntersection(line),
                    RightTriangle triangle => HasIntersection(triangle),
                    _ => throw new NotImplementedException()
                };
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

        public static bool HasIntersection(Parallelogram parallelogram, Box box)
        {
            return parallelogram.HasIntersection(box);
        }

        public static bool HasIntersection(Parallelogram parallelogram, RightTriangle slope)
        {
            return parallelogram.HasIntersection(slope);
        }

        public static bool HasIntersection(Box box1, Box box2)
        {
            return box1.IsOverlaping(box2, BoxSide.LEFT_TOP | BoxSide.INNER, BoxSide.LEFT_TOP | BoxSide.INNER);
        }

        public static bool HasIntersection(Box box, RightTriangle slope)
        {
            return slope.HasIntersectionWith(box, EPSLON, RightTriangleSide.ALL);
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Box collisionBox, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE)
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

        private EntityList<Entity> resultSet;

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
            resultSet = new EntityList<Entity>();
            IgnoreSprites = new EntityList<Sprite>();
        }

        public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            if (computePlacements)
                placements.Clear();

            TestBox = testBox;
            IgnoreFlags = ignoreFlags;
            MaskSize = maskSize;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            Setup(testBox, ignoreFlags, maskSize, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox)
        {
            Setup(testBox, CollisionFlags.NONE, MASK_SIZE, true, true, false);
        }

        public void Setup(Box testBox, params Sprite[] ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, MASK_SIZE, true, true, false, ignoreSprites);
        }

        public void Setup(Box testBox, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, MASK_SIZE, true, true, false);
        }

        public void Setup(Box testBox, BitSet ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, MASK_SIZE, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags)
        {
            Setup(testBox, ignoreFlags, MASK_SIZE, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, MASK_SIZE, true, true, false, ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, MASK_SIZE, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, MASK_SIZE, true, true, false);
        }

        public CollisionFlags GetCollisionFlags(RoundMode mode = RoundMode.NONE)
        {
            Box box = TestBox.RoundOrigin(mode);
            Cell start = World.GetMapCellFromPos(box.LeftTop);
            Cell end = World.GetMapCellFromPos(box.RightBottom);

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
        public Box MoveUntilIntersect(Vector dir, FixedSingle maxDistance, RoundMode mode = RoundMode.NONE)
        {
            var deltaDir = GetStepVectorHorizontal(dir, STEP_SIZE);
            var startBox = TestBox;

            for (int i = 0; i * STEP_SIZE <= maxDistance; i++, TestBox = startBox + (deltaDir * i).TruncFracPart())
                if (GetCollisionFlags(mode).CanBlockTheMove())
                    break;

            return TestBox;
        }

        public CollisionFlags GetTouchingFlags(Direction direction, RoundMode mode = RoundMode.NONE)
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
            var flags = GetCollisionFlags(mode);
            TestBox -= dir;

            return flags;
        }
    }
}