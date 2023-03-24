using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public enum DashingSparkEffectState
{
    PRE_DASHING,
    DASHING
}

public class DashSparkEffect : SpriteEffect
{
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

    public DashSparkEffect()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X Effects";
        DefaultDirection = Direction.LEFT;

        SetAnimationNames("PreDashSparkEffect", "DashSparkEffect");
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        if (animation.Name == "DashSparkEffect")
            KillOnNextFrame();
    }
}