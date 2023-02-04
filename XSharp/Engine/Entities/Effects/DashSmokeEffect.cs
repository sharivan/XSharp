using XSharp.Geometry;

namespace XSharp.Engine.Entities.Effects
{
    public class DashSmokeEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.Hitbox.LeftTop + (8, 22),
                Direction.RIGHT => player.Hitbox.RightTop + (-16, 22),
                _ => Vector.NULL_VECTOR,
            };
        }

        public DashSmokeEffect(string name, Player player) : base(name, GetOrigin(player), 2, false, "DashSmokeEffect")
        {
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}