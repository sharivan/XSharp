using System.Reflection;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies;

public enum ScriverState
{
    IDLE = 0,
    DRILLING = 1,
    JUMPING = 2,
    LANDING = 3
}

public class Scriver : Enemy, IStateEntity<ScriverState>
{
    [Precache]
    new internal static void Precache()
    {
        var scriverPalette = Engine.CreatePalette("scriverPalette", SCRIVER_PALETTE);
        var scriverSpriteSheet = Engine.CreateSpriteSheet("Scriver", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X2.scriver.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            scriverSpriteSheet.CurrentTexture = texture;
        }

        scriverSpriteSheet.CurrentPalette = scriverPalette;

        // 0
        var sequence = scriverSpriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -SCRIVER_HITBOX.Origin - SCRIVER_HITBOX.Mins;
        sequence.Hitbox = SCRIVER_HITBOX;
        sequence.AddFrame(-5, 6, 4, 4, 35, 30, 1, true);

        // 1
        sequence = scriverSpriteSheet.AddFrameSquence("Jumping");
        sequence.OriginOffset = -SCRIVER_HITBOX.Origin - SCRIVER_HITBOX.Mins;
        sequence.Hitbox = SCRIVER_HITBOX;
        sequence.AddFrame(-3, 6, 40, 4, 37, 30, 5);
        sequence.AddFrame(-7, 6, 78, 4, 35, 30, 5);
        sequence.AddFrame(4, -3, 115, 4, 43, 30, 1, true);

        // 2
        sequence = scriverSpriteSheet.AddFrameSquence("Landing");
        sequence.OriginOffset = -SCRIVER_HITBOX.Origin - SCRIVER_HITBOX.Mins;
        sequence.Hitbox = SCRIVER_HITBOX;
        sequence.AddFrame(-3, 6, 40, 4, 37, 30, 5);

        // 3
        sequence = scriverSpriteSheet.AddFrameSquence("Drilling");
        sequence.OriginOffset = -SCRIVER_DRILLING_HITBOX.Origin - SCRIVER_DRILLING_HITBOX.Mins;
        sequence.Hitbox = SCRIVER_DRILLING_HITBOX;
        sequence.AddFrame(-3, 0, 160, 10, 48, 24, 2, true);
        sequence.AddFrame(-4, 1, 209, 9, 46, 25, 2);
        sequence.AddFrame(-4, 0, 256, 10, 48, 24, 2);
        sequence.AddFrame(-4, 1, 305, 9, 46, 25, 2);
    }

    private bool flashing;
    private bool jumping;
    private int jumpCounter;

    public ScriverState State
    {
        get => GetState<ScriverState>();
        set => SetState(value);
    }

    public Scriver()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "Scriver";

        SetAnimationNames("Idle", "Drilling", "Jumping", "Landing");

        SetupStateArray(typeof(ScriverState));
        RegisterState(ScriverState.IDLE, OnIdle, "Idle");
        RegisterState(ScriverState.DRILLING, OnDrilling, "Drilling");
        RegisterState(ScriverState.JUMPING, OnJumping, "Jumping");
        RegisterState(ScriverState.LANDING, OnLanding, "Landing");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        flashing = false;
        jumping = false;
        jumpCounter = 0;

        PaletteName = "scriverPalette";
        Health = SCRIVER_HEALTH;
        ContactDamage = SCRIVER_CONTACT_DAMAGE;
        CollisionData = CollisionData.NONE;

        NothingDropOdd = 950; // 95%
        SmallHealthDropOdd = 30; // 3%
        BigHealthDropOdd = 15; // 1.5%
        SmallAmmoDropOdd = 0;
        BigAmmoDropOdd = 0;
        LifeUpDropOdd = 5; // 0.5%

        State = ScriverState.IDLE;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return SCRIVER_SIDE_COLLIDER_BOTTOM_CLIP;
    }

    protected override Box GetCollisionBox()
    {
        return SCRIVER_COLLISION_BOX;
    }

    protected override Box GetHitbox()
    {
        return State == ScriverState.DRILLING ? SCRIVER_DRILLING_HITBOX : SCRIVER_HITBOX;
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        flashing = true;
        PaletteName = "flashingPalette";

        return base.OnTakeDamage(attacker, ref damage);
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (Landed && Engine.Player != null)
        {
            if (frameCounter >= 12)
            {
                FaceToPlayer();

                if ((Engine.Player.Origin.X - Origin.X).Abs <= 50 && (Engine.Player.Origin.Y - Origin.Y).Abs <= 24)
                    State = ScriverState.DRILLING;
            }

            if (frameCounter >= 40 && State != ScriverState.DRILLING)
            {
                State = ScriverState.JUMPING;
                jumpCounter = 0;
            }
        }
    }

    private void OnJumping(EntityState state, long frameCounter)
    {
        if (frameCounter < 10)
        {
            Velocity = Vector.NULL_VECTOR;
        }
        else if (frameCounter == 10)
        {
            Velocity = (Direction == Direction.RIGHT ? SCRIVER_START_JUMP_OFFSET_X : -SCRIVER_START_JUMP_OFFSET_X, SCRIVER_START_JUMP_OFFSET_Y);
            jumping = true;
        }
        else if (frameCounter == 11)
        {
            Velocity = (Direction == Direction.RIGHT ? SCRIVER_JUMP_VELOCITY_X : -SCRIVER_JUMP_VELOCITY_X, SCRIVER_JUMP_VELOCITY_Y);
        }
        else
        {
            if (jumping && Landed)
            {
                jumpCounter++;
                jumping = false;
                Velocity = Vector.NULL_VECTOR;
                State = ScriverState.LANDING;
            }
            else if (!jumping || Velocity.Y < 0 && (BlockedLeft || BlockedRight || BlockedUp))
            {
                Velocity = Vector.NULL_VECTOR;
            }
        }
    }

    private void OnLanding(EntityState state, long frameCounter)
    {
        if (frameCounter >= 12)
            State = jumpCounter >= 2 ? ScriverState.IDLE : ScriverState.JUMPING;
    }

    private void OnDrilling(EntityState state, long frameCounter)
    {
        if (frameCounter >= 12 && (Engine.Player.Origin.X - Origin.X).Abs > 50)
            State = ScriverState.IDLE;
        else
            FaceToPlayer();
    }

    protected override bool PreThink()
    {
        if (flashing)
        {
            flashing = false;
            PaletteName = "scriverPalette";
        }

        return base.PreThink();
    }
}