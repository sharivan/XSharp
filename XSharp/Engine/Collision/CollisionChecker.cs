using System.Collections.Generic;
using System.Linq;

using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;
using XSharp.Util;

using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine.Collision;

[Serializable]
public abstract class CollisionChecker
{
    public static BaseEngine Engine => BaseEngine.Engine;

    public static MMXWorld World => BaseEngine.Engine.World;

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
        return box1.IsOverlaping(box2);
    }

    public static bool HasIntersection(Box box, RightTriangle slope, RightTriangleSide include = RightTriangleSide.ALL)
    {
        return slope.HasIntersectionWith(box, include);
    }

    private static CollisionFlags CheckCollisionData(Box box, CollisionData collisionData, List<CollisionPlacement> placements, CollisionFlags ignore)
    {
        CollisionFlags result = CollisionFlags.NONE;
        if (collisionData.IsSolidBlock())
        {
            switch (collisionData)
            {
                case CollisionData.UNCLIMBABLE_SOLID:
                    if (!ignore.IsUnclimbable())
                    {
                        placements?.Add(new CollisionPlacement(collisionData, box));
                        result = CollisionFlags.UNCLIMBABLE_BLOCK;
                    }

                    break;

                case CollisionData.LETHAL_SPIKE:
                    if (!ignore.IsLethalSpike() || ignore.IsUnclimbable())
                    {
                        placements?.Add(new CollisionPlacement(collisionData, box));
                        result = CollisionFlags.LETHAL_SPIKE;
                    }

                    break;

                case CollisionData.NON_LETHAL_SPIKE:
                case CollisionData.LAVA:
                    if (!ignore.IsNonLethalSpike() || ignore.IsUnclimbable())
                    {
                        placements?.Add(new CollisionPlacement(collisionData, box));
                        result = CollisionFlags.NON_LETHAL_SPIKE;
                    }

                    break;

                default:
                    if (!ignore.HasFlag(CollisionFlags.BLOCK) || ignore.IsUnclimbable())
                    {
                        placements?.Add(new CollisionPlacement(collisionData, box));
                        result = CollisionFlags.BLOCK;
                    }

                    break;
            }
        }
        else if (collisionData == CollisionData.LADDER && !ignore.IsBottomLadder())
        {
            placements?.Add(new CollisionPlacement(collisionData, box));

            result = CollisionFlags.LADDER;
        }
        else if (collisionData == CollisionData.TOP_LADDER && !ignore.IsTopLadder())
        {
            placements?.Add(new CollisionPlacement(collisionData, box));

            result = CollisionFlags.TOP_LADDER;
        }
        else if (collisionData == CollisionData.WATER && !ignore.IsUnderwater())
        {
            placements?.Add(new CollisionPlacement(collisionData, box));

            result = CollisionFlags.WATER;
        }
        else if (collisionData == CollisionData.WATER_SURFACE && !ignore.IsWaterSurface())
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
        {
            result = CheckCollisionData(box, collisionData, placements, ignore);
        }

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
        {
            result = CheckCollisionData(box, collisionData, placements, ignore);
        }

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
        {
            result = CheckCollisionData(box, collisionData, placements, ignore);
        }

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
        {
            result = CheckCollisionData(box, collisionData, placements, ignore);
        }

        return result;
    }

    protected List<CollisionPlacement> placements;
    protected EntitySet<Entity> resultSet;
    protected RightTriangle slopeTriangle;

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

    public EntitySet<Sprite> IgnoreSprites
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

    protected CollisionChecker()
    {
        placements = new List<CollisionPlacement>();
        resultSet = new EntitySet<Entity>();
        IgnoreSprites = new EntitySet<Sprite>();
    }

    protected abstract CollisionFlags GetCollisionVectorFlags();

    protected abstract CollisionFlags GetCollisionBoxFlags();

    public CollisionFlags GetCollisionFlags()
    {
        return TestKind switch
        {
            TouchingKind.VECTOR => GetCollisionVectorFlags(),
            TouchingKind.BOX => GetCollisionBoxFlags(),
            _ => CollisionFlags.NONE
        };
    }

    public virtual void Setup(Vector testVector, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        if (computePlacements)
            placements.Clear();

        TestKind = TouchingKind.VECTOR;
        TestVector = testVector;
        IgnoreFlags = ignoreFlags;
        CheckWithWorld = checkWithWorld;
        CheckWithSolidSprites = checkWithSolidSprites;
        ComputePlacements = computePlacements;
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, params Sprite[] ignoreSprites)
    {
        Setup(testVector, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

        IgnoreSprites.Clear();
        IgnoreSprites.AddRange(ignoreSprites);
    }
    public void Setup(Vector testVector, CollisionFlags ignoreFlags, EntitySet<Sprite> ignoreSprites, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        Setup(testVector, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

        IgnoreSprites.Clear();
        IgnoreSprites.AddRange(ignoreSprites);
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags, BitSet ignoreSprites, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        Setup(testVector, ignoreFlags, checkWithWorld, checkWithSolidSprites, computePlacements);

        IgnoreSprites.Clear();
        IgnoreSprites.AddRange(ignoreSprites);
    }

    public void Setup(Vector testVector)
    {
        Setup(testVector, CollisionFlags.NONE, true, true, false);
    }

    public void Setup(Vector testVector, params Sprite[] ignoreSprites)
    {
        Setup(testVector, CollisionFlags.NONE, true, true, false, ignoreSprites);
    }

    public void Setup(Vector testVector, EntitySet<Sprite> ignoreSprites)
    {
        Setup(testVector, CollisionFlags.NONE, ignoreSprites, true, true, false);
    }

    public void Setup(Vector testVector, BitSet ignoreSprites)
    {
        Setup(testVector, CollisionFlags.NONE, ignoreSprites, true, true, false);
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags)
    {
        Setup(testVector, ignoreFlags, true, true, false);
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags, params Sprite[] ignoreSprites)
    {
        Setup(testVector, ignoreFlags, true, true, false, ignoreSprites);
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags, EntitySet<Sprite> ignoreSprites)
    {
        Setup(testVector, ignoreFlags, ignoreSprites, true, true, false);
    }

    public void Setup(Vector testVector, CollisionFlags ignoreFlags, BitSet ignoreSprites)
    {
        Setup(testVector, ignoreFlags, ignoreSprites, true, true, false);
    }

    public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
    {
        if (computePlacements)
            placements.Clear();

        TestKind = TouchingKind.BOX;
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

    public void Setup(Box testBox, CollisionFlags ignoreFlags, EntitySet<Sprite> ignoreSprites, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements)
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

    public void Setup(Box testBox, EntitySet<Sprite> ignoreSprites)
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

    public void Setup(Box testBox, CollisionFlags ignoreFlags, EntitySet<Sprite> ignoreSprites)
    {
        Setup(testBox, ignoreFlags, ignoreSprites, true, true, false);
    }

    public void Setup(Box testBox, CollisionFlags ignoreFlags, BitSet ignoreSprites)
    {
        Setup(testBox, ignoreFlags, ignoreSprites, true, true, false);
    }

    public bool HasPlacement(CollisionData placementData)
    {
        foreach (var placement in placements)
        {
            if (placement.CollisionData == placementData)
                return true;
        }

        return false;
    }

    public bool HasPlacements(params CollisionData[] placementData)
    {
        foreach (var placement in placements)
        {
            if (placementData.Contains(placement.CollisionData))
                return true;
        }

        return false;
    }

    public bool HasPlacementAndNotOtherPlacements(CollisionData placementData, params CollisionData[] exclude)
    {
        bool contains = false;
        foreach (var placement in placements)
        {
            if (placement.CollisionData == placementData)
                contains = true;
            else if (exclude.Length > 0 && exclude.Contains(placement.CollisionData))
                return false;
        }

        return contains;
    }

    public bool HasPlacementAndOnlyAtLeastOtherPlacements(CollisionData placementData, params CollisionData[] additional)
    {
        bool contains = false;
        foreach (var placement in placements)
        {
            if (placement.CollisionData == placementData)
                contains = true;
            else if (additional.Length > 0 && !additional.Contains(placement.CollisionData))
                return false;
        }

        return contains;
    }

    public bool HasOnlyPlacement(CollisionData placementData)
    {
        if (placements.Count == 0)
            return false;

        foreach (var placement in placements)
        {
            if (placement.CollisionData != placementData)
                return false;
        }

        return true;
    }

    public bool HasOnlyPlacements(params CollisionData[] placementData)
    {
        if (placements.Count == 0)
            return false;

        foreach (var placement in placements)
        {
            if (!placementData.Contains(placement.CollisionData))
                return false;
        }

        return true;
    }
}