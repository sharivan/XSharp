using MMX.Geometry;

using MMX.Engine.Entities.Enemies;

namespace MMX.Engine.Entities.Enemies.Bosses
{
    public abstract class Boss : Enemy
    {
        protected Boss(GameEngine engine, string name, Vector origin, int spriteSheetIndex) : base(engine, name, origin, spriteSheetIndex)
        {
        }
    }
}
