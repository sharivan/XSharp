using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class WallSlideEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.Hitbox.LeftTop + (-5, 25),
                Direction.RIGHT => player.Hitbox.RightTop + (5, 25),
                _ => Vector.NULL_VECTOR,
            };
        }

        public WallSlideEffect(string name, Player player) : base(name, GetOrigin(player), 2, true, "WallSlideEffect")
        {
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}