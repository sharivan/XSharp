using XSharp.Engine.Collision;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Objects;

public class Probe8201U : Sprite
{
    private Vector moveOrigin;
    private FixedSingle speed;

    private Animation rocket;
    private Animation rocketJet;
    private Animation rocketPropellerJet;

    public FixedSingle MoveDistance
    {
        get;
        set;
    } = 80;

    public bool MovingVertically
    {
        get;
        set;
    } = true;

    public bool StartMovingBackward
    {
        get;
        set;
    } = true;

    public Probe8201U()
    {
        SpriteSheetName = "Platforms";
        Directional = false;
        MultiAnimation = true;

        SetAnimationNames("Probe8201U", "RocketPropellerJet", "RocketJet");
    }

    protected override Box GetHitbox()
    {
        return PROBE8201U_HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        CollisionData = CollisionData.SOLID;

        rocket = GetAnimationByName("Probe8201U");
        rocketJet = GetAnimationByName("RocketJet");
        rocketPropellerJet = GetAnimationByName("RocketPropellerJet");

        rocket.Visible = true;

        rocketJet.Offset = (0, 24);
        rocketJet.Visible = true;

        rocketPropellerJet.Offset = (0, 24);
        rocketPropellerJet.Visible = false;

        moveOrigin = Origin;
        speed = 0;
    }

    protected override void Think()
    {
        base.Think();

        var halfMoveDistance = MoveDistance * 0.5;
        var distance = MovingVertically ? (Origin.Y - moveOrigin.Y).Abs : (Origin.X - moveOrigin.X).Abs;
        if (distance <= halfMoveDistance)
            speed += StartMovingBackward ? -PROBE8201U_VERTICAL_ACCELERATION : PROBE8201U_VERTICAL_ACCELERATION;
        else
            speed += StartMovingBackward ? PROBE8201U_VERTICAL_ACCELERATION : -PROBE8201U_VERTICAL_ACCELERATION;

        speed = speed.Clamp(-PROBE8201U_TERMINAL_VERTICAL_SPEED, PROBE8201U_TERMINAL_VERTICAL_SPEED);

        Velocity = MovingVertically ? (0, speed) : (speed, 0);

        if (Velocity.Y < 0 && (
            StartMovingBackward && distance <= halfMoveDistance
            || !StartMovingBackward && distance > halfMoveDistance))
        {
            if (!rocketPropellerJet.Visible)
            {
                rocketJet.Visible = false;
                rocketPropellerJet.Visible = true;
                rocketPropellerJet.StartFromBegin();
            }
        }
        else if (!rocketJet.Visible)
        {
            rocketJet.Visible = true;
            rocketPropellerJet.Visible = false;
            rocketJet.StartFromBegin();
        }
    }
}