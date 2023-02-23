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
            return box1.IsOverlaping(box2);
        }

        public static bool HasIntersection(Box box, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.HasIntersectionWith(box, EPSLON, include);
        }

        private static CollisionFlags CheckCollisionData(Box box, CollisionData collisionData, List<CollisionPlacement> placements, CollisionFlags ignore)
        {
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

            return result;
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Vector v, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(v, box))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            if (collisionData.IsSlope())
            {
                if (!ignore.HasFlag(CollisionFlags.SLOPE))
                {
                    RightTriangle st = collisionData.MakeSlopeTriangle() + box.LeftTop;

                    if (HasIntersection(v, st))
                    {
                        placements?.Add(new CollisionPlacement(collisionData, st));

                        slopeTriangle = st;
                        result = CollisionFlags.SLOPE;
                    }
                }
            }
            else
                result = CheckCollisionData(box, collisionData, placements, ignore);

            return result;
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, LineSegment line, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(line, box))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            if (collisionData.IsSlope())
            {
                if (!ignore.HasFlag(CollisionFlags.SLOPE))
                {
                    RightTriangle st = collisionData.MakeSlopeTriangle() + box.LeftTop;

                    if (HasIntersection(line, st))
                    {
                        placements?.Add(new CollisionPlacement(collisionData, st));

                        slopeTriangle = st;
                        result = CollisionFlags.SLOPE;
                    }
                }
            }
            else
                result = CheckCollisionData(box, collisionData, placements, ignore);

            return result;
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Parallelogram parallelogram, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(parallelogram, box))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            if (collisionData.IsSlope())
            {
                if (!ignore.HasFlag(CollisionFlags.SLOPE))
                {
                    RightTriangle st = collisionData.MakeSlopeTriangle() + box.LeftTop;

                    if (HasIntersection(parallelogram, st))
                    {
                        placements?.Add(new CollisionPlacement(collisionData, st));

                        slopeTriangle = st;
                        result = CollisionFlags.SLOPE;
                    }
                }
            }
            else
                result = CheckCollisionData(box, collisionData, placements, ignore);

            return result;
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Box collisionBox, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(collisionBox, box))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            if (collisionData.IsSlope())
            {
                if (!ignore.HasFlag(CollisionFlags.SLOPE))
                {
                    RightTriangle st = collisionData.MakeSlopeTriangle() + box.LeftTop;

                    if (HasIntersection(collisionBox, st))
                    {
                        placements?.Add(new CollisionPlacement(collisionData, st));

                        slopeTriangle = st;
                        result = CollisionFlags.SLOPE;
                    }
                }
            }
            else
                result = CheckCollisionData(box, collisionData, placements, ignore);

            return result;
        }

        protected RightTriangle slopeTriangle;
        protected List<CollisionPlacement> placements;

        private HashSet<Entity> resultSet;
        private bool tracing;

        public Parallelogram tracingParallelogram;

        public TouchingKind TestKind
        {
            get;
            set;
        } = TouchingKind.BOX;

        public Vector TestVector
        {
            get;
            set;
        } = Vector.NULL_VECTOR;

        public Box TestBox
        {
            get;
            set;
        } = Box.EMPTY_BOX;

        public EntityList<Sprite> IgnoreSprites
        {
            get;
        }

        public FixedSingle StepSize
        {
            get;
            set;
        } = STEP_SIZE;

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

        public IEnumerable<CollisionPlacement> Placements => placements;

        public RightTriangle SlopeTriangle => slopeTriangle;

        public GameEngine Engine => GameEngine.Engine;

        public World World => GameEngine.Engine.World;

        public CollisionChecker()
        {
            placements = new List<CollisionPlacement>();
            resultSet = new HashSet<Entity>();
            IgnoreSprites = new EntityList<Sprite>();
            tracingParallelogram = new Parallelogram();
        }

        public virtual void Setup(Vector testVector, CollisionFlags ignoreFlags, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            if (computePlacements)
                placements.Clear();

            TestKind = TouchingKind.VECTOR;
            TestVector = testVector;
            IgnoreFlags = ignoreFlags;
            StepSize = stepSize;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;

            tracing = false;
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck, params Sprite[] ignoreSprites)
        {
            Setup(testVector, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }
        public void Setup(Vector testVector, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testVector, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags, BitSet ignoreSprites, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testVector, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Vector testVector)
        {
            Setup(testVector, CollisionFlags.NONE, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Vector testVector, params Sprite[] ignoreSprites)
        {
            Setup(testVector, CollisionFlags.NONE, STEP_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Vector testVector, EntityList<Sprite> ignoreSprites)
        {
            Setup(testVector, CollisionFlags.NONE, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Vector testVector, BitSet ignoreSprites)
        {
            Setup(testVector, CollisionFlags.NONE, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags)
        {
            Setup(testVector, ignoreFlags, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
        {
            Setup(testVector, ignoreFlags, STEP_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites)
        {
            Setup(testVector, ignoreFlags, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Vector testVector, CollisionFlags ignoreFlags, BitSet ignoreSprites)
        {
            Setup(testVector, ignoreFlags, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            if (computePlacements)
                placements.Clear();

            TestKind = TouchingKind.BOX;
            TestBox = testBox;
            IgnoreFlags = ignoreFlags;
            StepSize = stepSize;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;

            tracing = false;
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testBox, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites, FixedSingle stepSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            Setup(testBox, ignoreFlags, stepSize, checkWithWorld, checkWithSolidSprites, computePlacements, preciseCollisionCheck);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox)
        {
            Setup(testBox, CollisionFlags.NONE, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, params Sprite[] ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, STEP_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Box testBox, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, BitSet ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags)
        {
            Setup(testBox, ignoreFlags, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, STEP_SIZE, true, true, false, true, ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, STEP_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, STEP_SIZE, true, true, false, true);
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

        private CollisionFlags GetCollisionVectorFlags()
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
                        Map map = World.GetMapFrom(TestVector);
                        if (map != null)
                        {
                            Box mapBox = World.GetMapBoundingBox(World.GetMapCellFromPos(TestVector));
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
                    Map map = World.GetMapFrom(TestVector);
                    if (map != null)
                    {
                        Box mapBox = World.GetMapBoundingBox(World.GetMapCellFromPos(TestVector));
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
                    Engine.partition.Query(resultSet, tracingLine, BoxKind.HITBOX);
                    foreach (var entity in resultSet)
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
                else
                {
                    Engine.partition.Query(resultSet, TestVector, BoxKind.HITBOX);
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
            }

            return result;
        }

        private CollisionFlags GetCollisionBoxFlags()
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

                        for (int row = startRow; row <= endRow; row++)
                            for (int col = startCol; col <= endCol; col++)
                            {
                                var mapPos = World.GetMapLeftTop(row, col);
                                Map map = World.GetMapFrom(mapPos);
                                if (map != null)
                                {
                                    Box mapBox = World.GetMapBoundingBox(row, col);
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
                else
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

                    for (int row = startRow; row <= endRow; row++)
                        for (int col = startCol; col <= endCol; col++)
                        {
                            var mapPos = World.GetMapLeftTop(row, col);
                            Map map = World.GetMapFrom(mapPos);
                            if (map != null)
                            {
                                Box mapBox = World.GetMapBoundingBox(row, col);
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

            if (CheckWithSolidSprites)
            {
                resultSet.Clear();

                if (tracing && TracingBoxMode.HasFlag(TracingMode.DIAGONAL))
                {
                    Engine.partition.Query(resultSet, tracingParallelogram, BoxKind.HITBOX);
                    foreach (var entity in resultSet)
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
                else
                {
                    Engine.partition.Query(resultSet, TestBox, BoxKind.HITBOX);
                    foreach (var entity in resultSet)
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

            return result;
        }

        public CollisionFlags GetCollisionFlags()
        {
            return TestKind switch
            {
                TouchingKind.VECTOR => GetCollisionVectorFlags(),
                TouchingKind.BOX => GetCollisionBoxFlags(),
                _ => CollisionFlags.NONE
            };
        }

        public CollisionFlags GetTouchingFlagsLeft()
        {
            var lastBox = TestBox;
            TestBox = new Box(TestBox.LeftTop - (StepSize, 0), StepSize, TestBox.Height);
            var flags = GetCollisionFlags();
            TestBox = lastBox;
            return flags;
        }

        public CollisionFlags GetTouchingFlagsUp()
        {
            var lastBox = TestBox;
            TestBox = new Box(TestBox.LeftTop - (0, StepSize), TestBox.Width, StepSize);
            var flags = GetCollisionFlags();
            TestBox = lastBox;
            return flags;
        }

        public CollisionFlags GetTouchingFlagsRight()
        {
            var lastBox = TestBox;
            TestBox = new Box(TestBox.RightTop, StepSize, TestBox.Height);
            var flags = GetCollisionFlags();
            TestBox = lastBox;
            return flags;
        }

        public CollisionFlags GetTouchingFlagsDown()
        {
            var lastBox = TestBox;
            TestBox = new Box(TestBox.LeftBottom, TestBox.Width, StepSize);
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

            var flags = TraceRay(Vector.DOWN_VECTOR, StepSize);
            TestKind = lastTestKind;

            return flags == CollisionFlags.SLOPE && (TestVector.Y - TestBox.Bottom).Abs < StepSize;
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
            var flags = MoveContactSolidVertical(StepSize);

            IgnoreFlags = lastIgnoreFlags;
            TestBox = lastBox;

            if (flags.CanBlockTheMove(Direction.DOWN))
            {
                flags = NearestObstacleCollisionData.ToCollisionFlags();
                perfectlyLanded = (lastBox.Bottom - NearestObstacleBox.Top).Abs < StepSize;
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
}