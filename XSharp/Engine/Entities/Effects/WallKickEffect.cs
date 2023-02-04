using XSharp.Geometry;

namespace XSharp.Engine.Entities.Effects
{
    public class WallKickEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.Hitbox.LeftTop + (-14, 27),
                Direction.RIGHT => player.Hitbox.RightTop + (14 - 11, 27),
                _ => Vector.NULL_VECTOR,
            };
        }

        public WallKickEffect(string name, Player player) : base(name, GetOrigin(player), 2, false, "WallKickEffect") { }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}