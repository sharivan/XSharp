using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class WallKickEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.HitBox.LeftTop + (-14, 27),
                Direction.RIGHT => player.HitBox.RightTop + (14 - 11, 27),
                _ => Vector.NULL_VECTOR,
            };
        }

        public WallKickEffect(GameEngine engine, string name, Player player) : base(engine, name, GetOrigin(player), 2, false, "WallKickEffect") { }

        internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}
