using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Triggers;
using XSharp.Engine.Graphics;
using XSharp.Engine.World;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

namespace XSharp.Engine.Entities.Objects;

public enum BossDoorState
{
    CLOSED = 0,
    OPENING = 1,
    PLAYER_CROSSING = 2,
    CLOSING = 3
}

public enum BossDoorOrientation
{
    VERTICAL,
    HORIZONTAL
}

public enum BossDoorDirection
{
    FORWARD,
    BACKWARD
}

public delegate void BossDoorEvent(BossDoor source);

public class BossDoor : BaseTrigger, IFSMEntity<BossDoorState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.PrecacheSound("Door Opening", @"X1\Door Opening.wav");
        Engine.PrecacheSound("Door Closing", @"X1\Door Closing.wav");
    }
    #endregion

    private static Box GetTriggerBoudingBox(Vector origin)
    {
        return (origin, (-13, -8), (8, 8));
    }

    public event BossDoorEvent OpeningEvent;
    public event BossDoorEvent PlayerCrossingEvent;
    public event BossDoorEvent ClosingEvent;
    public event BossDoorEvent ClosedEvent;

    private BossDoorOrientation orientation = BossDoorOrientation.VERTICAL;

    private EntityReference<BossDoorSprite> effect;

    private BossDoorSprite Effect => effect;

    public BossDoorOrientation Orientation
    {
        get => orientation;
        set
        {
            orientation = value;
            if (Effect != null)
                Effect.Rotation = Orientation == BossDoorOrientation.VERTICAL ? NinetyRotation.ANGLE_0 : NinetyRotation.ANGLE_90;
        }
    }

    public BossDoorDirection CrossDirection
    {
        get;
        set;
    } = BossDoorDirection.FORWARD;

    public bool Bidirectional
    {
        get;
        set;
    } = false;

    public BossDoorState State
    {
        get => Effect.State;
        set => Effect.State = value;
    }

    public bool StartBossBattle
    {
        get;
        set;
    }

    public bool AwaysVisible
    {
        get;
        set;
    } = true;

    public BossDoor()
    {
    }

    protected override void OnCreated()
    {
        base.OnCreated();

        effect = Engine.Entities.Create<BossDoorSprite>(new
        {
            Door = this,
            Visible = AwaysVisible,
            Origin = (Origin.X, Origin.Y - 1),
            Rotation = Orientation == BossDoorOrientation.VERTICAL ? NinetyRotation.ANGLE_0 : NinetyRotation.ANGLE_90
        });

        if (AwaysVisible)
            Effect.Spawn();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Hitbox = GetTriggerBoudingBox(Origin);

        Effect.Origin = (Origin.X, Origin.Y - 1);
        Effect.Rotation = Orientation == BossDoorOrientation.VERTICAL ? NinetyRotation.ANGLE_0 : NinetyRotation.ANGLE_90;

        if (!AwaysVisible)
            Effect.Spawn();

        State = BossDoorState.CLOSED;
    }

    protected override void OnDeath()
    {
        if (Effect != null)
        {
            Effect.Kill();
            effect = null;
        }

        base.OnDeath();
    }

    protected override void OnStartTrigger(Entity entity)
    {
        base.OnStartTrigger(entity);

        if (entity is Player player)
        {
            Engine.Camera.NoConstraints = true;
            Engine.Camera.FocusOn = null;
            player.StartBossDoorCrossing();
            Engine.KillAllAliveEnemiesAndWeapons();

            ChargingEffect chargingEffect = player.ChargingEffect;
            if (chargingEffect != null)
                Engine.FreezeSprites(player, effect, chargingEffect);
            else
                Engine.FreezeSprites(player, effect);

            Engine.Player.Animating = false;
            Engine.Player.InputLocked = true;

            if (!Bidirectional)
                Enabled = false;

            if (!AwaysVisible)
                Effect.Visible = true;

            State = BossDoorState.OPENING;
        }
    }

    internal void OnStartClosed()
    {
        if (!AwaysVisible)
            Effect.Visible = false;

        ClosedEvent?.Invoke(this);
    }

    internal void OnStartOpening()
    {
        OpeningEvent?.Invoke(this);
    }

    internal void OnOpening(long frameCounter)
    {
        if (frameCounter == 44)
            Engine.PlaySound(0, "Door Opening", true);
    }

    private Vector GetCameraMoveOffset()
    {
        switch (Orientation)
        {
            case BossDoorOrientation.VERTICAL:
                if (CrossDirection == BossDoorDirection.FORWARD)
                    return (SCREEN_WIDTH, 0);

                return (-SCREEN_WIDTH, 0);

            case BossDoorOrientation.HORIZONTAL:
                if (CrossDirection == BossDoorDirection.FORWARD)
                    return (0, SCREEN_HEIGHT);

                return (0, -SCREEN_HEIGHT);
        }

        return Vector.NULL_VECTOR;
    }

    internal void OnStartPlayerCrossing()
    {
        Engine.Player.Animating = true;

        Cell sceneCell = GetSceneCellFromPos(Engine.Player.Origin);
        Box sceneBox = GetSceneBoundingBox(sceneCell);
        Vector offset = GetCameraMoveOffset();
        Engine.Camera.MoveToLeftTop(sceneBox.LeftTop + offset, Orientation == BossDoorOrientation.VERTICAL ? (CAMERA_BOOS_DOOR_CROSSING_SMOOTH_SPEED, 0) : (0, CAMERA_BOOS_DOOR_CROSSING_SMOOTH_SPEED));

        if (StartBossBattle)
        {
            Engine.CameraConstraintsOrigin = Engine.CurrentCheckpoint.Origin + offset;
            Engine.CameraConstraintsBox = Engine.CurrentCheckpoint.Hitbox + offset;
        }

        PlayerCrossingEvent?.Invoke(this);
    }

    internal void OnPlayerCrossing(long frameCounter)
    {
        if (frameCounter == 120)
        {
            Engine.Player.StopBossDoorCrossing();
            Engine.UnfreezeSprites();
            Engine.Camera.NoConstraints = false;
            Engine.Camera.FocusOn = Engine.Player;
            State = BossDoorState.CLOSING;
        }
        else if (frameCounter < 120)
        {
            Engine.Player.Velocity =
                Orientation == BossDoorOrientation.VERTICAL
                ? CrossDirection == BossDoorDirection.FORWARD
                    ? (CROSSING_BOOS_DOOR_SPEED, 0)
                    : (-CROSSING_BOOS_DOOR_SPEED, 0)
                : CrossDirection == BossDoorDirection.FORWARD
                    ? (0, CROSSING_BOOS_DOOR_SPEED)
                    : (0, -CROSSING_BOOS_DOOR_SPEED);
        }
    }

    internal void OnStartClosing()
    {
        Engine.PlaySound(0, StartBossBattle ? "Door Closing" : "Door Opening", true);
        ClosingEvent?.Invoke(this);
    }

    internal void OnEndClosing()
    {
        if (StartBossBattle)
        {
            Engine.StartBossBattle();
        }
        else
        {
            var player = Engine.Player;
            player.Invincible = false;
            player.Blinking = false;
            player.InputLocked = false;
        }
    }
}