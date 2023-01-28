using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class WallSlideEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.HitBox.LeftTop + (-5, 25),
                Direction.RIGHT => player.HitBox.RightTop + (5, 25),
                _ => Vector.NULL_VECTOR,
            };
        }

        public WallSlideEffect(GameEngine engine, string name, Player player) : base(engine, name, GetOrigin(player), 2, true, "WallSlideEffect")
        {
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}
