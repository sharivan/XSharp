using MMX.Geometry;

namespace MMX.Engine.Entities.Enemies
{
    public abstract class Enemy : Sprite
    {
        protected int contactDamage;

        public int ContactDamage => contactDamage;

        protected Enemy(GameEngine engine, string name, Vector origin, int spriteSheetIndex) : base(engine, name, origin, spriteSheetIndex, true)
        {
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        protected override void Think()
        {
            base.Think();

            if (Offscreen)
                KillOnNextFrame();
        }
    }
}
