using System.Reflection;

using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Objects;

internal class BossDoorEffect : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var bossDoorSpriteSheet = Engine.CreateSpriteSheet("Boos Door", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Objects.BossDoor.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            bossDoorSpriteSheet.CurrentTexture = texture;
        }

        var bossDoorHitbox = new Box(Vector.NULL_VECTOR, (-8, -23), (24, 25));

        var sequence = bossDoorSpriteSheet.AddFrameSquence("Closed");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 32, 0, 32, 48, 1, true);

        sequence = bossDoorSpriteSheet.AddFrameSquence("Opening");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 160, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 224, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 160, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 224, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 256, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 288, 0, 32, 48, 4);

        sequence = bossDoorSpriteSheet.AddFrameSquence("PlayerCrossing");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 32, 48, 1, true);

        sequence = bossDoorSpriteSheet.AddFrameSquence("Closing");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 288, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 256, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 224, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 160, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 224, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 160, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
        sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
        sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);

        bossDoorSpriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<BossDoor> door;

    public BossDoor Door
    {
        get => door;
        set => door = value;
    }

    public BossDoorState State
    {
        get => GetState<BossDoorState>();
        set => SetState(value);
    }

    public BossDoorEffect()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "Boos Door";
        Directional = false;
        Respawnable = true;

        SetAnimationNames("Closed", "Opening", "PlayerCrossing", "Closing");

        SetupStateArray(typeof(BossDoorState));
        RegisterState(BossDoorState.CLOSED, OnStartClosed, "Closed");
        RegisterState(BossDoorState.OPENING, OnStartOpening, OnOpening, null, "Opening");
        RegisterState(BossDoorState.PLAYER_CROSSING, OnStartPlayerCrossing, OnPlayerCrossing, null, "PlayerCrossing");
        RegisterState(BossDoorState.CLOSING, OnStartClosing, null, OnEndClosing, "Closing");
    }

    private void OnStartClosed(EntityState state, EntityState lastState)
    {
        Door?.OnStartClosed();
    }

    private void OnStartOpening(EntityState state, EntityState lastState)
    {
        Door?.OnStartOpening();
    }

    private void OnOpening(EntityState state, long frameCounter)
    {
        Door?.OnOpening(frameCounter);
    }

    private void OnStartPlayerCrossing(EntityState state, EntityState lastState)
    {
        Door?.OnStartPlayerCrossing();
    }

    private void OnPlayerCrossing(EntityState state, long frameCounter)
    {
        Door?.OnPlayerCrossing(frameCounter);
    }

    private void OnStartClosing(EntityState state, EntityState lastState)
    {
        Door?.OnStartClosing();
    }

    private void OnEndClosing(EntityState state)
    {
        Door?.OnEndClosing();
    }

    protected override void OnDeath()
    {
        door = null;
        base.OnDeath();
    }

    protected internal override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        switch (State)
        {
            case BossDoorState.OPENING when animation.Name == "Opening":
                State = BossDoorState.PLAYER_CROSSING;
                break;

            case BossDoorState.CLOSING when animation.Name == "Closing":
                State = BossDoorState.CLOSED;
                break;
        }
    }
}