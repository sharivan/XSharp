using System.Reflection;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Objects;

public class Probe8201U : Sprite
{
    #region StaticFields
    public static readonly Box HITBOX = (Vector.NULL_VECTOR, (-11, -27), (11, 27));
    public static readonly FixedSingle HORIZONTAL_SPEED = 0.25;
    public static readonly FixedSingle TERMINAL_VERTICAL_SPEED = 1;
    public static readonly FixedSingle VERTICAL_ACCELERATION = 4 / 256.0;
    public static readonly FixedSingle BASE_MOVE_DISTANCE = 80;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        var spriteSheet = Engine.CreateSpriteSheet("Platforms", true, true);
        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Objects.Platforms.png");

        var sequence = spriteSheet.AddFrameSquence("Probe8201U");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(-2, -3, 124, 107, 18, 48, 7, true);
        sequence.AddFrame(-2, -3, 142, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 160, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 178, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 196, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 214, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 232, 107, 18, 48, 7);

        sequence = spriteSheet.AddFrameSquence("RocketPropellerJet");
        sequence.AddFrame(124, 155, 18, 23, 1, true, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(142, 155, 18, 23, 1, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(160, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(178, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(196, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);

        sequence = spriteSheet.AddFrameSquence("RocketJet");
        sequence.AddFrame(124, 155, 18, 23, 1, true, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(142, 155, 18, 23, 1, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(214, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(232, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(124, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private Vector moveOrigin;
    private FixedSingle speed;
    private bool movingBackward;

    private Animation rocket;
    private Animation rocketJet;
    private Animation rocketPropellerJet;

    public FixedSingle MoveDistance
    {
        get;
        set;
    } = BASE_MOVE_DISTANCE;

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
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "Platforms";
        MultiAnimation = true;
        KillOnOffscreen = false;

        SetAnimationNames("Probe8201U", "RocketPropellerJet", "RocketJet");
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override void OnSpawn()
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
        movingBackward = StartMovingBackward;
    }

    private void MoveHorizontally()
    {
        speed = !movingBackward
            ? HORIZONTAL_SPEED
            : -HORIZONTAL_SPEED;

        Velocity = (speed, 0);

        var distance = (Origin.X - moveOrigin.X).Abs;
        if (speed > 0 && distance == MoveDistance)
            movingBackward = !movingBackward;
        else if (speed < 0 && distance == 0)
            movingBackward = !movingBackward;
    }

    private void MoveVertically()
    {
        var halfMoveDistance = MoveDistance * FixedSingle.HALF;
        var distance = (Origin.Y - moveOrigin.Y).Abs;
        if (distance <= halfMoveDistance)
            speed += StartMovingBackward ? -VERTICAL_ACCELERATION : VERTICAL_ACCELERATION;
        else
            speed += StartMovingBackward ? VERTICAL_ACCELERATION : -VERTICAL_ACCELERATION;

        Velocity = (0, speed.Clamp(-TERMINAL_VERTICAL_SPEED, TERMINAL_VERTICAL_SPEED));

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

    protected override void OnThink()
    {
        base.OnThink();

        if (MovingVertically)
            MoveVertically();
        else
            MoveHorizontally();
    }
}