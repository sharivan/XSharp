using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin.ChillPenguin;

namespace XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin;

public enum PenguinLeverState
{
    SHOWING,
    IDLE,
    PULLED,
    HIDING
}

public class PenguinLever : Sprite, IFSMEntity<PenguinLeverState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(ChillPenguin));
    }
    #endregion

    private int showingFrameCounter;

    public PenguinLeverState State
    {
        get;
        set;
    } = PenguinLeverState.IDLE;

    public PenguinLever()
    {
        SpriteSheetName = "ChillPenguin";

        SetAnimationNames("Lever");
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return PENGUIN_LEVER_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;

        Show();
    }

    public void Show()
    {
        if (State == PenguinLeverState.IDLE)
        {
            showingFrameCounter = 0;
            State = PenguinLeverState.SHOWING;
        }
    }

    public void Hide()
    {
        if (State == PenguinLeverState.IDLE)
        {
            showingFrameCounter = 0;
            State = PenguinLeverState.HIDING;
        }
    }

    protected override void OnThink()
    {
        base.OnThink();

        switch (State)
        {
            case PenguinLeverState.SHOWING:
                showingFrameCounter++;
                Origin += Vector.DOWN_VECTOR;
                if (showingFrameCounter == PENGUIN_LEVER_MOVING_FRAMES)
                    State = PenguinLeverState.IDLE;

                break;

            case PenguinLeverState.HIDING:
                showingFrameCounter++;
                Origin += Vector.UP_VECTOR;
                if (showingFrameCounter == PENGUIN_LEVER_MOVING_FRAMES)
                    State = PenguinLeverState.IDLE;

                break;
        }
    }
}