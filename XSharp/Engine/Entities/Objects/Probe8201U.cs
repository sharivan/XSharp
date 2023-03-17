using System.Reflection;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Objects;

public class Probe8201U : Sprite
{
    #region StaticFields
    public static readonly Box PROBE8201U_HITBOX = (Vector.NULL_VECTOR, (-11, -27), (11, 27));
    public static readonly FixedSingle PROBE8201U_HORIZONTAL_SPEED = 0.25;
    public static readonly FixedSingle PROBE8201U_TERMINAL_VERTICAL_SPEED = 1;
    public static readonly FixedSingle PROBE8201U_VERTICAL_ACCELERATION = 4 / 256.0;
    public static readonly FixedSingle PROBE8201U_BASE_MOVE_DISTANCE = 80;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        var platformsSpriteSheet = Engine.CreateSpriteSheet("Platforms", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Objects.Platforms.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            platformsSpriteSheet.CurrentTexture = texture;
        }

        var sequence = platformsSpriteSheet.AddFrameSquence("Probe8201U");
        sequence.OriginOffset = -PROBE8201U_HITBOX.Origin - PROBE8201U_HITBOX.Mins;
        sequence.Hitbox = PROBE8201U_HITBOX;
        sequence.AddFrame(-2, -3, 124, 107, 18, 48, 7, true);
        sequence.AddFrame(-2, -3, 142, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 160, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 178, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 196, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 214, 107, 18, 48, 7);
        sequence.AddFrame(-2, -3, 232, 107, 18, 48, 7);

        sequence = platformsSpriteSheet.AddFrameSquence("RocketPropellerJet");
        sequence.AddFrame(124, 155, 18, 23, 1, true, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(142, 155, 18, 23, 1, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(160, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(178, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(196, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);

        sequence = platformsSpriteSheet.AddFrameSquence("RocketJet");
        sequence.AddFrame(124, 155, 18, 23, 1, true, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(142, 155, 18, 23, 1, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(214, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(232, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);
        sequence.AddFrame(124, 155, 18, 23, 2, false, OriginPosition.MIDDLE_TOP);

        platformsSpriteSheet.ReleaseCurrentTexture();
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
    } = PROBE8201U_BASE_MOVE_DISTANCE;

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
        KillOnOffscreen = false;

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
        movingBackward = StartMovingBackward;
    }

    private void MoveHorizontally()
    {
        speed = !movingBackward
            ? PROBE8201U_HORIZONTAL_SPEED
            : -PROBE8201U_HORIZONTAL_SPEED;

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
            speed += StartMovingBackward ? -PROBE8201U_VERTICAL_ACCELERATION : PROBE8201U_VERTICAL_ACCELERATION;
        else
            speed += StartMovingBackward ? PROBE8201U_VERTICAL_ACCELERATION : -PROBE8201U_VERTICAL_ACCELERATION;

        Velocity = (0, speed.Clamp(-PROBE8201U_TERMINAL_VERTICAL_SPEED, PROBE8201U_TERMINAL_VERTICAL_SPEED));

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

    protected override void Think()
    {
        base.Think();

        if (MovingVertically)
            MoveVertically();
        else
            MoveHorizontally();
    }
}