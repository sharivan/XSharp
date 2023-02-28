using System;

namespace XSharp.Engine.Collision
{
    [Flags]
    public enum TracingMode
    {
        NONE = 0,
        HORIZONTAL = 1,
        VERTICAL = 2,
        DIAGONAL = 4
    }

    public class TracerCollisionChecker : CollisionChecker
    {
        public override CollisionFlags GetCollisionFlags()
        {
            throw new NotImplementedException();
        }
    }
}