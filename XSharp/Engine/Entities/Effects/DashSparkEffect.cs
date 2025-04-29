using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Effects;

public enum DashingSparkEffectState
{
    PRE_DASHING,
    DASHING
}

public class DashSparkEffect : SpriteEffect
{
    public DashingSparkEffectState State
    {
        get;
        set
        {
            field = value;
            if (field == DashingSparkEffectState.DASHING)
            {
                Parent = null;
                SetCurrentAnimationByName("DashSparkEffect");
                CurrentAnimation.StartFromBegin();
            }
        }
    } = DashingSparkEffectState.PRE_DASHING;

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