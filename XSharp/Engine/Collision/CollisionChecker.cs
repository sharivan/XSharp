using System.Collections.Generic;
using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine.Collision
{
    public abstract class CollisionChecker
    {
        public static GameEngine Engine => GameEngine.Engine;

        public static MMXWorld World => GameEngine.Engine.World;

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

        public static bool HasIntersection(Vector v, Box box, BoxSide include = BoxSide.LEFT_TOP | BoxSide.INNER)
        {
            return box.Contains(v, include);
        }

        public static bool HasIntersection(Vector v, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.Contains(v, include);
        }

        public static bool HasIntersection(LineSegment line, Box box)
        {
            return box.HasIntersectionWith(line);
        }

        public static bool HasIntersection(LineSegment line, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return slope.HasIntersectionWith(line, include);
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
            return slope.HasIntersectionWith(box, RightTriangleSide.ALL);
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
        protected EntityList<Entity> resultSet;

        public Box TestBox
        {
            get;
            set;
        }

        public EntityList<Sprite> IgnoreSprites
        {
            get;
        }

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

        public CollisionChecker()
        {
            placements = new List<CollisionPlacement>();
            resultSet = new EntityList<Entity>();
            IgnoreSprites = new EntityList<Sprite>();
        }

        public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            if (computePlacements)
                placements.Clear();

            TestBox = testBox;
            IgnoreFlags = ignoreFlags;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            Setup(testBox, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
        {
            Setup(testBox, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

            IgnoreSprites.Clear();
            IgnoreSprites.AddRange(ignoreSprites);
        }

        public void Setup(Box testBox)
        {
            Setup(testBox, CollisionFlags.NONE, true, true, false);
        }

        public void Setup(Box testBox, params Sprite[] ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, true, true, false, ignoreSprites);
        }

        public void Setup(Box testBox, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, true, true, false);
        }

        public void Setup(Box testBox, BitSet ignoreSprites)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprites, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags)
        {
            Setup(testBox, ignoreFlags, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
        {
            Setup(testBox, ignoreFlags, true, true, false, ignoreSprites);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, EntityList<Sprite> ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, true, true, false);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites)
        {
            Setup(testBox, ignoreFlags, ignoreSprites, true, true, false);
        }

        public abstract CollisionFlags GetCollisionFlags();
    }
}