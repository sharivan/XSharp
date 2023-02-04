using XSharp.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses
{
    public abstract class Boss : Enemy
    {
        protected Boss(string name, Vector origin, int spriteSheetIndex) : base(name, origin, spriteSheetIndex)
        {
        }
    }
}