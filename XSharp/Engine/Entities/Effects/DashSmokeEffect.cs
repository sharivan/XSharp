using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class DashSmokeEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player) => player.Direction switch
        {
            Direction.LEFT => player.HitBox.LeftTop + (8, 22),
            Direction.RIGHT => player.HitBox.RightTop + (-16, 22),
            _ => Vector.NULL_VECTOR,
        };

        public DashSmokeEffect(GameEngine engine, string name, Player player) : base(engine, name, GetOrigin(player), 2, false, "DashSmokeEffect")
        {
        }

        internal override void OnAnimationEnd(Animation animation)
        {            
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}
