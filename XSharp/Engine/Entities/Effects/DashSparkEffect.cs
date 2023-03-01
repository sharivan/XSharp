using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects
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
                Direction.LEFT => player.Hitbox.LeftTop + (23 - 9, 20),
                Direction.RIGHT => player.Hitbox.RightTop + (-23 + 9, 20),
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
                    SetCurrentAnimationByName("DashSparkEffect");
                    CurrentAnimation.StartFromBegin();
                }
            }
        }

        private Player player;

        public Player Player
        {
            get => player;
            set
            {
                player = value;
                if (value != null)
                {
                    Origin = GetOrigin(value);
                    Direction = value.Direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;
                }

                Parent = value;
            }
        }

        public DashSparkEffect()
        {
            SpriteSheetName = "X Effects";
            Directional = true;

            SetAnimationNames("PreDashSparkEffect", "DashSparkEffect");
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            if (animation.FrameSequenceName == "DashSparkEffect")
                KillOnNextFrame();
        }
    }
}