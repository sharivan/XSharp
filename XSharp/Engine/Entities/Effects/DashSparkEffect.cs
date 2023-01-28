using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public enum DashingSparkEffectState
    {
        PRE_DASHING,
        DASHING
    }

    public class DashSparkEffect : SpriteEffect
    {
        private static Vector GetOrigin(Player player)
        {
            return player.Direction switch
            {
                Direction.LEFT => player.HitBox.LeftTop + (23 - 9, 20),
                Direction.RIGHT => player.HitBox.RightTop + (-23 + 9, 20),
                _ => Vector.NULL_VECTOR,
            };
        }

        private DashingSparkEffectState state = DashingSparkEffectState.PRE_DASHING;

        public DashingSparkEffectState State
        {
            get => state;
            set
            {
                state = value;
                if (state == DashingSparkEffectState.DASHING)
                {
                    Parent = null;
                    var index = GetAnimationIndex("DashSparkEffect");
                    CurrentAnimationIndex = index;
                    CurrentAnimation.StartFromBegin();
                }
            }
        }

        public DashSparkEffect(GameEngine engine, string name, Player player) : base(engine, name, GetOrigin(player), 2, true, "PreDashSparkEffect", "DashSparkEffect")
        {
            Direction = player.Direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;
            Parent = player;
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            if (animation.FrameSequenceName == "DashSparkEffect")
                KillOnNextFrame();
        }
    }
}
