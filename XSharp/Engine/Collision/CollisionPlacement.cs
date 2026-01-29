using XSharp.Math.Fixed.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine.Collision;

[Serializable]
public readonly struct CollisionPlacement
{
    public CollisionData CollisionData
    {
        get;
    }

    public Box ObstableBox
    {
        get;
    }

    public RightTriangle ObstableSlope
    {
        get;
    }

    public CollisionPlacement(CollisionData collisionData, Box obstableBox)
    {
        CollisionData = collisionData;
        ObstableBox = obstableBox;
        ObstableSlope = RightTriangle.EMPTY;
    }

    public CollisionPlacement(CollisionData collisionData, RightTriangle obstacleSlope)
    {
        CollisionData = collisionData;
        ObstableBox = Box.EMPTY_BOX;
        ObstableSlope = obstacleSlope;
    }

    public void Deconstruct(out CollisionData collisionData, out Box obstacleBox)
    {
        collisionData = CollisionData;
        obstacleBox = ObstableBox;
    }

    public void Deconstruct(out CollisionData collisionData, out RightTriangle obstacleSlope)
    {
        collisionData = CollisionData;
        obstacleSlope = ObstableSlope;
    }

    public void Deconstruct(out CollisionData collisionData, out Box obstacleBox, out RightTriangle obstacleSlope)
    {
        collisionData = CollisionData;
        obstacleBox = ObstableBox;
        obstacleSlope = ObstableSlope;
    }
}