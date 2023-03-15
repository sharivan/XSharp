using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public enum PenguinLeverState
{
    SHOWING,
    IDLE,
    PULLED,
    HIDING
}

public class PenguinLever : Sprite, IStateEntity<PenguinLeverState>
{
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }

    private int showingFrameCounter;

    public PenguinLeverState State
    {
        get;
        set;
    } = PenguinLeverState.IDLE;

    public PenguinLever()
    {
        Directional = true;
        SpriteSheetName = "Penguin";

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

    protected internal override void OnSpawn()
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

    protected override void Think()
    {
        base.Think();

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