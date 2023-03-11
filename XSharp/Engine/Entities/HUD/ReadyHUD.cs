using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD;

public class ReadyHUD : HUD
{
    public ReadyHUD()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Offset = READY_OFFSET;
        SpriteSheetName = "Ready";
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CurrentAnimationIndex = 0;
        CurrentAnimation.StartFromBegin();
    }

    protected internal override void OnAnimationEnd(Animation animation)
    {
        Engine.SpawnPlayer();
        KillOnNextFrame();
    }
}