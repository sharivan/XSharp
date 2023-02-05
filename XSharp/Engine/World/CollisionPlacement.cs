using XSharp.Engine.World;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;
using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine.World
{
    public readonly struct CollisionPlacement
    {
        public CollisionFlags Flags
        {
            get;
        }

        public Vector LeftTop
        {
            get;
        }

        public Vector RightBottom => LeftTop + (MAP_SIZE, MAP_SIZE);

        public Vector Center => (LeftTop + RightBottom) * FixedSingle.HALF;

        public Box BoudingBox => (LeftTop, RightBottom);

        public Cell FirstMapCell => MMXWorld.GetMapCellFromPos(LeftTop);

        public Cell LastMapCell => MMXWorld.GetMapCellFromPos(RightBottom);

        public int FirstMapRow => FirstMapCell.Row;

        public int FirstMapCol => FirstMapCell.Col;

        public int LastMapRow => LastMapCell.Row;

        public int LastMapCol => LastMapCell.Col;

        public CollisionData CollisionData
        {
            get
            {
                Map map = GameEngine.Engine.World.GetMapFrom(LeftTop);
                return map != null ? map.CollisionData : CollisionData.NONE;
            }
        }

        public RightTriangle SlopeTriangle => CollisionChecker.MakeSlopeTriangle(CollisionData) + LeftTop;

        public CollisionPlacement(CollisionFlags flags, Vector leftTop)
        {
            Flags = flags;
            this.LeftTop = leftTop;
        }

        public CollisionPlacement(CollisionFlags flags, Cell mapCell)
        {
            Flags = flags;
            LeftTop = MMXWorld.GetMapBoundingBox(mapCell).LeftTop;
        }

        public CollisionPlacement(CollisionFlags flags, int mapRow, int mapCol)
        {
            Flags = flags;
            LeftTop = MMXWorld.GetMapBoundingBox(mapRow, mapCol).LeftTop;
        }
    }
}