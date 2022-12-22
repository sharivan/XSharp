using MMX.Geometry;

namespace MMX.Engine.Enemies.Bosses
{
    public abstract class Boss : Enemy
    {
        protected Boss(GameEngine engine, string name, Vector origin, SpriteSheet sheet) : base(engine, name, origin, sheet)
        {
        }
    }
}
