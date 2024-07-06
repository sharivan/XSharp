using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public enum JediSigmaDoorState
{
    OPEN,
    CLOSING,
    CLOSED
}

public class JediSigmaDoor : Sprite, IFSMEntity<JediSigmaDoorState>
{
    #region StaticFields
    public static readonly Box HITBOX = ((0, 0), (-32, -11), (32, 11));
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        var spriteSheet = Engine.CreateSpriteSheet("JediSigmaDoor", true, true);
        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Objects.Jedi Sigma Door.png");

        var sequence = spriteSheet.AddFrameSquence("Open");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, -3, 0, 32, 64, 16, 1, true);

        sequence = spriteSheet.AddFrameSquence("Closing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, -3, 0, 32, 64, 16, 30);
        sequence.AddFrame(0, -3, 0, 0, 64, 16, 16);

        sequence = spriteSheet.AddFrameSquence("Closed");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, -3, 0, 16, 64, 16, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    internal EntityReference<JediSigma> sigma;

    public JediSigma Sigma => sigma;

    public JediSigmaDoorState State
    {
        get => GetState<JediSigmaDoorState>();
        set => SetState(value);
    }

    public JediSigmaDoor()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "JediSigmaDoor";
        KillOnOffscreen = false;

        SetAnimationNames("Open", "Closing", "Closed");

        SetupStateArray<JediSigmaDoorState>();
        RegisterState(JediSigmaDoorState.OPEN, OnStartOpen, "Open");
        RegisterState(JediSigmaDoorState.CLOSING, OnStartClosing, OnClosing, null, "Closing");
        RegisterState(JediSigmaDoorState.CLOSED, OnStartClosed, "Closed");
    }

    public void Close()
    {
        if (State is not JediSigmaDoorState.CLOSING and not JediSigmaDoorState.CLOSED)
            State = JediSigmaDoorState.CLOSING;
    }

    private void OnStartOpen(EntityState state, EntityState lastState)
    {
        CollisionData = CollisionData.NONE;
    }

    private void OnStartClosing(EntityState state, EntityState lastState)
    {
        CollisionData = CollisionData.NONE;
    }

    private void OnClosing(EntityState state, long frameCounter)
    {
        if (frameCounter >= 16)
            State = JediSigmaDoorState.CLOSED;
    }

    private void OnStartClosed(EntityState state, EntityState lastState)
    {
        CollisionData = CollisionData.SOLID;
        Sigma?.OnDoorClosed();
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        State = JediSigmaDoorState.OPEN;
    }
}