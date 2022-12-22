using MMX.Geometry;

namespace MMX.Engine.Enemies
{
    public abstract class Enemy : Sprite
    {
        protected int contactDamage;

        public int ContactDamage => contactDamage;

        protected Enemy(GameEngine engine, string name, Vector origin, SpriteSheet sheet) : base(engine, name, origin, sheet, true)
        {
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }
    }
}
