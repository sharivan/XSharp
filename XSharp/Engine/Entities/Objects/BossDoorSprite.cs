using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Objects;

internal class BossDoorSprite : Sprite, IFSMEntity<BossDoorState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        var spriteSheet = Engine.CreateSpriteSheet("BoosDoor", true, true);
        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Objects.BossDoor.png");

        var bossDoorHitbox = new Box(Vector.NULL_VECTOR, (-8, -23), (24, 25));

        var sequence = spriteSheet.AddFrameSquence("Closed");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 32, 0, 32, 48, 1, true);

        sequence = spriteSheet.AddFrameSquence("Opening");
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

        sequence = spriteSheet.AddFrameSquence("PlayerCrossing");
        sequence.OriginOffset = -bossDoorHitbox.Mins;
        sequence.Hitbox = bossDoorHitbox;
        sequence.AddFrame(0, 0, 32, 48, 1, true);

        sequence = spriteSheet.AddFrameSquence("Closing");
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

        spriteSheet.ReleaseCurrentTexture();
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

    public BossDoorSprite()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "BoosDoor";
        Respawnable = true;
        CollisionData = CollisionData.SOLID;

        SetAnimationNames("Closed", "Opening", "PlayerCrossing", "Closing");

        SetupStateArray<BossDoorState>();
        RegisterState(BossDoorState.CLOSED, OnStartClosed, "Closed");
        RegisterState(BossDoorState.OPENING, OnStartOpening, OnOpening, null, "Opening");
        RegisterState(BossDoorState.PLAYER_CROSSING, OnStartPlayerCrossing, OnPlayerCrossing, null, "PlayerCrossing");
        RegisterState(BossDoorState.CLOSING, OnStartClosing, null, OnEndClosing, "Closing");
    }

    public override FixedSingle GetGravity()
    {
        return 0;
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

    protected override void OnSpawn()
    {
        base.OnSpawn();

        State = BossDoorState.CLOSED;
    }

    protected override void OnDeath()
    {
        door = null;
        base.OnDeath();
    }

    protected override void OnAnimationEnd(Animation animation)
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