using System.Collections.Generic;
using XSharp.Engine.Entities;
using XSharp.Geometry;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    public class CollisionChecker
    {
        private static Vector GetStepVector(Vector dir)
        {
            if (dir.X == 0)
                return dir.Y > 0 ? STEP_DOWN_VECTOR : dir.Y < 0 ? STEP_UP_VECTOR : Vector.NULL_VECTOR;

            if (dir.Y == 0)
                return dir.X > 0 ? STEP_RIGHT_VECTOR : dir.X < 0 ? STEP_LEFT_VECTOR : Vector.NULL_VECTOR;

            FixedSingle x = dir.X;
            FixedSingle xm = x.Abs;
            FixedSingle y = dir.Y;

            return new Vector(x.Signal * STEP_SIZE, y / xm * STEP_SIZE);
        }

        public static bool IsSolidBlock(CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.MUD => true,
                CollisionData.TOP_MUD => true,
                CollisionData.LAVA => true,
                CollisionData.SOLID2 => true,
                CollisionData.SOLID3 => true,
                CollisionData.UNCLIMBABLE_SOLID => true,
                CollisionData.LEFT_CONVEYOR => true,
                CollisionData.RIGHT_CONVEYOR => true,
                CollisionData.UP_SLOPE_BASE => true,
                CollisionData.DOWN_SLOPE_BASE => true,
                CollisionData.SOLID => true,
                CollisionData.BREAKABLE => true,
                CollisionData.NON_LETHAL_SPIKE => true,
                CollisionData.LETHAL_SPIKE => true,
                CollisionData.SLIPPERY_SLOPE_BASE => true,
                CollisionData.SLIPPERY => true,
                CollisionData.DOOR => true,
                _ => false,
            };
        }

        public static bool IsSlope(CollisionData collisionData)
        {
            return collisionData is >= CollisionData.SLOPE_16_8 and <= CollisionData.SLOPE_0_4 or
                >= CollisionData.LEFT_CONVEYOR_SLOPE_16_12 and <= CollisionData.RIGHT_CONVEYOR_SLOPE_0_4 or
                >= CollisionData.SLIPPERY_SLOPE_16_8 and <= CollisionData.SLIPPERY_SLOPE_0_4;
        }

        public static RightTriangle MakeSlopeTriangle(int left, int right)
        {
            return left < right
                ? new RightTriangle(new Vector(0, right), MAP_SIZE, left - right)
                : new RightTriangle(new Vector(MAP_SIZE, left), -MAP_SIZE, right - left);
        }

        public static RightTriangle MakeSlopeTriangle(CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.SLOPE_16_8 => MakeSlopeTriangle(16, 8),
                CollisionData.SLOPE_8_0 => MakeSlopeTriangle(8, 0),
                CollisionData.SLOPE_8_16 => MakeSlopeTriangle(8, 16),
                CollisionData.SLOPE_0_8 => MakeSlopeTriangle(0, 8),
                CollisionData.SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.SLOPE_4_0 => MakeSlopeTriangle(4, 0),
                CollisionData.SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                CollisionData.LEFT_CONVEYOR_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.LEFT_CONVEYOR_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.LEFT_CONVEYOR_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.LEFT_CONVEYOR_SLOPE_4_0 => MakeSlopeTriangle(4, 0),

                CollisionData.RIGHT_CONVEYOR_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.RIGHT_CONVEYOR_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.RIGHT_CONVEYOR_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.RIGHT_CONVEYOR_SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                CollisionData.SLIPPERY_SLOPE_16_8 => MakeSlopeTriangle(16, 8),
                CollisionData.SLIPPERY_SLOPE_8_0 => MakeSlopeTriangle(8, 0),
                CollisionData.SLIPPERY_SLOPE_8_16 => MakeSlopeTriangle(8, 16),
                CollisionData.SLIPPERY_SLOPE_0_8 => MakeSlopeTriangle(0, 8),
                CollisionData.SLIPPERY_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.SLIPPERY_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.SLIPPERY_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.SLIPPERY_SLOPE_4_0 => MakeSlopeTriangle(4, 0),
                CollisionData.SLIPPERY_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.SLIPPERY_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.SLIPPERY_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.SLIPPERY_SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                _ => RightTriangle.EMPTY,
            };
        }

        public static bool CanBlockTheMove(CollisionFlags flags)
        {
            return flags is not CollisionFlags.NONE and not CollisionFlags.WATER and not CollisionFlags.WATER_SURFACE;
        }

        public static bool HasIntersection(Box box1, Box box2)
        {
            return (box1 & box2).IsValid(EPSLON);
        }

        public static bool HasIntersection(Box box, RightTriangle slope)
        {
            return slope.HasIntersectionWith(box, EPSLON, true);
        }

        public static CollisionFlags TestCollision(Box box, CollisionData collisionData, Box collisionBox, List<CollisionPlacement> placements, ref RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            if (collisionData == CollisionData.NONE || !HasIntersection(box, collisionBox))
                return CollisionFlags.NONE;

            CollisionFlags result = CollisionFlags.NONE;
            bool hasIntersection = HasIntersection(collisionBox, box);
            if (IsSolidBlock(collisionData) && hasIntersection && !ignore.HasFlag(CollisionFlags.BLOCK))
            {
                if (collisionData == CollisionData.UNCLIMBABLE_SOLID)
                {
                    if (!ignore.HasFlag(CollisionFlags.UNCLIMBABLE))
                    {
                        placements?.Add(new CollisionPlacement(CollisionFlags.BLOCK, box.LeftTop));
                        result = CollisionFlags.BLOCK | CollisionFlags.UNCLIMBABLE;
                    }
                }
                else
                {
                    placements?.Add(new CollisionPlacement(CollisionFlags.BLOCK, box.LeftTop));
                    result = CollisionFlags.BLOCK;
                }
            }
            else if (collisionData == CollisionData.LADDER && hasIntersection && !ignore.HasFlag(CollisionFlags.LADDER))
            {
                placements?.Add(new CollisionPlacement(CollisionFlags.LADDER, box.LeftTop));

                result = CollisionFlags.LADDER;
            }
            else if (collisionData == CollisionData.TOP_LADDER && hasIntersection && !ignore.HasFlag(CollisionFlags.TOP_LADDER))
            {
                placements?.Add(new CollisionPlacement(CollisionFlags.TOP_LADDER, box.LeftTop));

                result = CollisionFlags.TOP_LADDER;
            }
            else if (collisionData == CollisionData.WATER && hasIntersection && !ignore.HasFlag(CollisionFlags.WATER))
            {
                placements?.Add(new CollisionPlacement(CollisionFlags.WATER, box.LeftTop));

                result = CollisionFlags.WATER;
            }
            else if (collisionData == CollisionData.WATER_SURFACE && hasIntersection && !ignore.HasFlag(CollisionFlags.WATER_SURFACE))
            {
                placements?.Add(new CollisionPlacement(CollisionFlags.WATER_SURFACE, box.LeftTop));

                result = CollisionFlags.WATER_SURFACE;
            }
            else if (!ignore.HasFlag(CollisionFlags.SLOPE) && IsSlope(collisionData))
            {
                RightTriangle st = MakeSlopeTriangle(collisionData) + box.LeftTop;
                if (preciseCollisionCheck)
                {
                    if (HasIntersection(collisionBox, st))
                    {
                        placements?.Add(new CollisionPlacement(CollisionFlags.BLOCK, box.LeftTop));

                        slopeTriangle = st;
                        result = CollisionFlags.SLOPE;
                    }
                }
                else if (hasIntersection)
                {
                    placements?.Add(new CollisionPlacement(CollisionFlags.BLOCK, box.LeftTop));

                    slopeTriangle = st;
                    result = CollisionFlags.SLOPE;
                }
            }

            return result;
        }

        protected RightTriangle slopeTriangle;
        protected List<CollisionPlacement> placements;

        public Box TestBox
        {
            get;
            set;
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

        public Sprite IgnoreSprite
        {
            get;
            set;
        } = null;

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
        }

        public virtual void Setup(Box testBox, CollisionFlags ignoreFlags, Sprite ignoreSprite, FixedSingle maskSize, bool checkWithWorld, bool checkWithSolidSprites, bool computePlacements, bool preciseCollisionCheck)
        {
            if (computePlacements)
                placements.Clear();

            TestBox = testBox;
            IgnoreFlags = ignoreFlags;
            IgnoreSprite = ignoreSprite;
            MaskSize = maskSize;
            CheckWithWorld = checkWithWorld;
            CheckWithSolidSprites = checkWithSolidSprites;
            ComputePlacements = computePlacements;
            PreciseCollisionCheck = preciseCollisionCheck;
        }

        public void Setup(Box testBox)
        {
            Setup(testBox, CollisionFlags.NONE, null, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, Sprite ignoreSprite)
        {
            Setup(testBox, CollisionFlags.NONE, ignoreSprite, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags)
        {
            Setup(testBox, ignoreFlags, null, MASK_SIZE, true, true, false, true);
        }

        public void Setup(Box testBox, CollisionFlags ignoreFlags, Sprite ignoreSprite)
        {
            Setup(testBox, ignoreFlags, ignoreSprite, MASK_SIZE, true, true, false, true);
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
                        var mapPos = new Vector(col * MAP_SIZE, row * MAP_SIZE);
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
                List<Entity> entities = Engine.partition.Query(TestBox, BoxKind.COLLISIONBOX);
                foreach (var entity in entities)
                    if (entity is Entities.Sprite sprite && sprite != IgnoreSprite && sprite.CollisionData != CollisionData.NONE)
                    {
                        CollisionFlags collisionResult = TestCollision(sprite.CollisionBox, sprite.CollisionData, TestBox, ComputePlacements ? placements : null, ref slopeTriangle, IgnoreFlags, PreciseCollisionCheck);
                        if (collisionResult == CollisionFlags.NONE)
                            continue;

                        result |= collisionResult;
                    }
            }

            return result;
        }

        public Box MoveUntilIntersect(Vector dir, FixedSingle maxDistance)
        {
            Vector deltaDir = GetStepVector(dir);
            FixedSingle step = deltaDir.X == 0 ? deltaDir.Y.Abs : deltaDir.X.Abs;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += step, TestBox += deltaDir)
                if (CanBlockTheMove(GetCollisionFlags()))
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