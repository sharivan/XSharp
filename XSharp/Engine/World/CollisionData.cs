using XSharp.Engine.World;
using XSharp.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine
{
    public enum CollisionData
    {
        NONE = 0x00,
        SLOPE_16_8 = 0x01,
        SLOPE_8_0 = 0x02,
        SLOPE_8_16 = 0x03,
        SLOPE_0_8 = 0x04,
        SLOPE_16_12 = 0x05,
        SLOPE_12_8 = 0x06,
        SLOPE_8_4 = 0x07,
        SLOPE_4_0 = 0x08,
        SLOPE_12_16 = 0x09,
        SLOPE_8_12 = 0x0A,
        SLOPE_4_8 = 0x0B,
        SLOPE_0_4 = 0x0C,
        WATER = 0x0D,
        WATER_SURFACE = 0x0E,
        UNKNOW0F = 0x0F,
        UNKNOW10 = 0x10,
        MUD = 0x11,
        LADDER = 0x12,
        TOP_LADDER = 0x13,
        UNKNOW14 = 0x14,
        UNKNOW15 = 0x15,
        UNKNOW16 = 0x16,
        UNKNOW17 = 0x17,
        UNKNOW18 = 0x18,
        UNKNOW19 = 0x19,
        UNKNOW1A = 0x1A,
        UNKNOW1B = 0x1B,
        TOP_MUD = 0x1C,
        UNKNOW1D = 0x1D,
        UNKNOW1E = 0x1E,
        UNKNOW1F = 0x1F,
        UNKNOW20 = 0x20,
        UNKNOW21 = 0x21,
        UNKNOW22 = 0x22,
        UNKNOW23 = 0x23,
        UNKNOW24 = 0x24,
        UNKNOW25 = 0x25,
        UNKNOW26 = 0x26,
        UNKNOW27 = 0x27,
        UNKNOW28 = 0x28,
        UNKNOW29 = 0x29,
        UNKNOW2A = 0x2A,
        UNKNOW2B = 0x2B,
        UNKNOW2C = 0x2C,
        UNKNOW2D = 0x2D,
        UNKNOW2E = 0x2E,
        UNKNOW2F = 0x2F,
        UNKNOW30 = 0x30,
        UNKNOW31 = 0x31,
        UNKNOW32 = 0x32,
        LAVA = 0x33,
        SOLID2 = 0x34,
        SOLID3 = 0x35,
        UNCLIMBABLE_SOLID = 0x36,
        LEFT_CONVEYOR = 0x37,
        RIGHT_CONVEYOR = 0x38,
        UP_SLOPE_BASE = 0x39,
        DOWN_SLOPE_BASE = 0x3A,
        SOLID = 0x3B,
        BREAKABLE = 0x3C,
        DOOR = 0x3D,
        NON_LETHAL_SPIKE = 0x3E,
        LETHAL_SPIKE = 0x3F,
        UNKNOW40 = 0x40,
        UNKNOW41 = 0x41,
        UNKNOW42 = 0x42,
        UNKNOW43 = 0x43,
        UNKNOW44 = 0x44,
        LEFT_CONVEYOR_SLOPE_16_12 = 0x45,
        LEFT_CONVEYOR_SLOPE_12_8 = 0x46,
        LEFT_CONVEYOR_SLOPE_8_4 = 0x47,
        LEFT_CONVEYOR_SLOPE_4_0 = 0x48,
        RIGHT_CONVEYOR_SLOPE_12_16 = 0x49,
        RIGHT_CONVEYOR_SLOPE_8_12 = 0x4A,
        RIGHT_CONVEYOR_SLOPE_4_8 = 0x4B,
        RIGHT_CONVEYOR_SLOPE_0_4 = 0x4C,
        UNKNOW4D = 0x4D,
        UNKNOW4E = 0x4E,
        UNKNOW4F = 0x4F,
        SEMI_SOLID = 0x53,
        SLIPPERY_SLOPE_16_8 = 0x81,
        SLIPPERY_SLOPE_8_0 = 0x82,
        SLIPPERY_SLOPE_8_16 = 0x83,
        SLIPPERY_SLOPE_0_8 = 0x84,
        SLIPPERY_SLOPE_16_12 = 0x85,
        SLIPPERY_SLOPE_12_8 = 0x86,
        SLIPPERY_SLOPE_8_4 = 0x87,
        SLIPPERY_SLOPE_4_0 = 0x88,
        SLIPPERY_SLOPE_12_16 = 0x89,
        SLIPPERY_SLOPE_8_12 = 0x8A,
        SLIPPERY_SLOPE_4_8 = 0x8B,
        SLIPPERY_SLOPE_0_4 = 0x8C,
        SLIPPERY_SLOPE_BASE = 0xBA,
        SLIPPERY_BORDER_FLOOR = 0xBB,
        SLIPPERY_FLOOR = 0xBE
    }

    public static class CollisionDataExtensions
    {
        public static CollisionFlags ToCollisionFlags(this CollisionData collisionData)
        {
            return collisionData.IsSolidBlock()
                ? CollisionFlags.BLOCK
                : collisionData.IsSlope()
                ? CollisionFlags.SLOPE
                : collisionData switch
                {
                    CollisionData.WATER => CollisionFlags.WATER,
                    CollisionData.WATER_SURFACE => CollisionFlags.WATER_SURFACE,
                    CollisionData.LADDER => CollisionFlags.LADDER,
                    CollisionData.TOP_LADDER => CollisionFlags.TOP_LADDER,
                    _ => CollisionFlags.NONE,
                };
        }

        public static bool IsSolidBlock(this CollisionData collisionData)
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
                CollisionData.SLIPPERY_BORDER_FLOOR => true,
                CollisionData.SLIPPERY_FLOOR => true,
                CollisionData.DOOR => true,
                _ => false,
            };
        }

        public static bool IsMud(this CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.MUD => true,
                CollisionData.TOP_MUD => true,
                _ => false,
            };
        }

        public static bool IsSlipperyFloor(this CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.SLIPPERY_SLOPE_BASE => true,
                CollisionData.SLIPPERY_BORDER_FLOOR => true,
                CollisionData.SLIPPERY_FLOOR => true,
                _ => false,
            };
        }

        public static bool IsSlope(this CollisionData collisionData)
        {
            return collisionData is >= CollisionData.SLOPE_16_8 and <= CollisionData.SLOPE_0_4 or
                >= CollisionData.LEFT_CONVEYOR_SLOPE_16_12 and <= CollisionData.RIGHT_CONVEYOR_SLOPE_0_4 or
                >= CollisionData.SLIPPERY_SLOPE_16_8 and <= CollisionData.SLIPPERY_SLOPE_0_4;
        }

        public static bool IsSlipperySlope(this CollisionData collisionData)
        {
            return collisionData is >= CollisionData.SLIPPERY_SLOPE_16_8 and <= CollisionData.SLIPPERY_SLOPE_0_4;
        }

        public static bool IsConveyorSlope(this CollisionData collisionData)
        {
            return collisionData is >= CollisionData.LEFT_CONVEYOR_SLOPE_16_12 and <= CollisionData.RIGHT_CONVEYOR_SLOPE_0_4;
        }

        public static bool IsWater(this CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.WATER => true,
                CollisionData.WATER_SURFACE => true,
                _ => false,
            };
        }

        public static RightTriangle MakeSlopeTriangle(int left, int right)
        {
            return left < right
                ? new RightTriangle(new Vector(0, right), MAP_SIZE, left - right)
                : new RightTriangle(new Vector(MAP_SIZE, left), -MAP_SIZE, right - left);
        }

        public static RightTriangle MakeSlopeTriangle(this CollisionData collisionData)
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
    }
}