using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Triggers;
using XSharp.Engine.Graphics;
using XSharp.Engine.World;
using XSharp.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.Functions;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public enum JediSigmaState
{
    PRE_INTRODUCING,
    INTRODUCING,
    IDLE,
    DEFENDING,
    ATTACKING,
    SLASHING,
    SHOOTING,
    SHOOTING_BEAM_SABER,
    POST_SHOOTING,
    WALL_JUMPING,
    DYING
}

public class JediSigma : Boss, IFSMEntity<JediSigmaState>
{
    #region StaticFields
    public static readonly Color[] PALETTE =
    [
        Color.Transparent,          // 0
        Color.FromBgra(0xFF202820), // 1
        Color.FromBgra(0xFF68A0F0), // 2
        Color.FromBgra(0xFF1858B0), // 3
        Color.FromBgra(0xFFF8F0D8), // 4
        Color.FromBgra(0xFFA0A0A0), // 5           
        Color.FromBgra(0xFF787878), // 6
        Color.FromBgra(0xFF48D048), // 7
        Color.FromBgra(0xFF109040), // 8
        Color.FromBgra(0xFF185020), // 9
        Color.FromBgra(0xFFF8B080), // A
        Color.FromBgra(0xFFB86048), // B
        Color.FromBgra(0xFFF03808), // C
        Color.FromBgra(0xFFB08038), // D
        Color.FromBgra(0xFF804820), // E
        Color.FromBgra(0xFF505050), // F
        Color.Transparent,          // 10
        Color.FromBgra(0xFF282828), // 11
        Color.FromBgra(0xFF504090), // 12
        Color.FromBgra(0xFF302060), // 13
        Color.FromBgra(0xFFF8F0D8), // 14
        Color.FromBgra(0xFFA0A0A0), // 15
        Color.FromBgra(0xFF787878), // 16
        Color.FromBgra(0xFFF03808), // 17
        Color.FromBgra(0xFFA83008), // 18
        Color.FromBgra(0xFF683010), // 19
        Color.FromBgra(0xFFF8B080), // 1A
        Color.FromBgra(0xFFB86048), // 1B
        Color.FromBgra(0xFF804020), // 1C
        Color.FromBgra(0xFF9870D8), // 1D
        Color.FromBgra(0xFFB88820), // 1E
        Color.FromBgra(0xFF282828)  // 1F
    ];

    public static readonly FixedSingle ATTACKING_SPEED = 1024 / 256.0;

    public static readonly FixedSingle CONTACT_DAMAGE = 10;
    public static readonly FixedSingle SLASH_DAMAGE = 14;
    public static readonly FixedSingle SHOT_DAMAGE = 8;

    public static readonly Box HITBOX = ((6, 4), (-13, -25), (13, 25));
    public static readonly Box ATTACKING_HITBOX = ((-14, 2), (-13, -24), (13, 24));
    public static readonly Box CAPE_HITBOX = ((0, 0), (-29, -24), (29, 24));
    public static readonly Box SLASH_HITBOX = ((0, 0), (-22, -25), (22, 25));
    public static readonly Box SHOT_HITBOX = ((0, 0), (-4, -4), (4, 4));
    public static readonly Box PRE_BEAM_SABER_HITBOX = ((0, 0), (-5, -14), (5, 14));
    public static readonly Box BEAM_SABER_HITBOX = ((0, 0), (-5, -27), (5, 27));
    public static readonly Box VERTICAL_PRE_BEAM_SABER_HITBOX = ((0, 0), (-6, -3), (6, 3));
    public static readonly Box VERTICAL_BEAM_SABER_HITBOX = ((0, 0), (-12, -3), (12, 3));
    public static readonly Box COLLISION_BOX = ((2, 4), (-24, -28), (24, 28));
    public static readonly FixedSingle COLLISION_BOX_LEGS_HEIGHT = 8;

    public static readonly FixedSingle CAPE_OFFSET_X = 32;
    public static readonly FixedSingle CAPE_OFFSET_Y = -1;
    public static readonly FixedSingle CAPE_SPEED_X = 1;
    public static readonly FixedSingle CAPE_SPEED_Y = -0.25;
    public static readonly FixedSingle SHOT_OFFSET_X = 0;
    public static readonly FixedSingle SHOT_OFFSET_Y = -20;
    public static readonly FixedSingle SLASH_OFFSET_X = 38;
    public static readonly FixedSingle SLASH_OFFSET_Y = -7;

    public static readonly FixedSingle PLAYER_WALK_OFFSET_X = 79;
    public static readonly FixedSingle SIGMA_PRE_INTRODUCING_OFFSET_X = 80;
    public static readonly FixedSingle PRE_INTRODUCING_STEP = 16;

    public const int FRAMES_TO_START_PRE_INTRODUCING = 14;
    public const int PRE_INTRODUCING_STEPS = 14;
    public const int FRAMES_TO_START_FILL_HP = 78 + 32;

    public const int PRE_ATTACK_FRAMES = 24;
    public const int DEFENDING_FRAMES = 7;
    public const int FRAME_TO_SHOT = 32;
    public const int POST_SHOT_FRAMES = 18;
    public const int FRAME_TO_SLASH = 8;
    public const int SLASH_FRAMES = 44;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.PrecacheSound("Sigma Stage", @"OST\X1\27 - Sigma Intro 2.mp3");
        Engine.PrecacheSound("Sigma Boss Intro", @"OST\X1\05 - Sigma Intro 1.mp3");
        Engine.PrecacheSound("Sigma Boss Battle", @"OST\X1\06 - Sigma Boss Battle.mp3");
        Engine.PrecacheSound("Jedi Sigma Battle", @"OST\X1\28 - Sigma Battle.mp3");
        Engine.PrecacheSound("Wolf Sigma Intro", @"OST\X1\29 - Stigma Defeated.mp3");
        Engine.PrecacheSound("Wolf Sigma Battle", @"OST\X1\30 - Sigma Battle 2.mp3");
        Engine.PrecacheSound("Jedi Sigma Slash", @"X1\108 - MMX - Sigma Teleport.wav");

        var palette = Engine.PrecachePalette("JediSigmaPalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("JediSigma", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Enemies.Bosses.X1.Sigma.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("PreIntroducing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(13, 7, 6, 15, 58, 60, 1, true);

        sequence = spriteSheet.AddFrameSquence("Introducing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(8, 7, 72, 15, 45, 60, 4);
        sequence.AddFrame(7, 7, 123, 15, 45, 60, 4);
        sequence.AddFrame(8, 7, 72, 15, 45, 60, 20);
        sequence.AddFrame(7, 7, 303, 15, 38, 60, 22);
        sequence.AddFrame(15, 2, 357, 20, 50, 55, 20);
        sequence.AddFrame(19, 2, 413, 20, 54, 55, 4);
        sequence.AddFrame(23, 2, 473, 20, 58, 55, 4); // total of 78 frames
        sequence.AddFrame(40, 7, 537, 15, 75, 61, 1, true); // hp start to fill after 32 frames from here

        sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(40, 7, 537, 15, 75, 61, 1, true);

        sequence = spriteSheet.AddFrameSquence("Defending");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(16, 2, 105, 16, 41, 32, 7);

        sequence = spriteSheet.AddFrameSquence("Attacking");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(13, 2, 13, 100, 71, 55, 4);
        sequence.OriginOffset = -ATTACKING_HITBOX.Origin - ATTACKING_HITBOX.Mins;
        sequence.Hitbox = ATTACKING_HITBOX;
        sequence.AddFrame(3, 1, 92, 100, 59, 52, 1, true);

        sequence = spriteSheet.AddFrameSquence("WallJumping");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(12, 13, 159, 97, 41, 57, 5);
        sequence.AddFrame(10, 13, 206, 97, 41, 57, 5); // total of 10 frames

        sequence = spriteSheet.AddFrameSquence("Shooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(40, 7, 537, 15, 75, 61, 32);
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 4, true); // shot spawn here
        sequence.AddFrame(40, 8, 699, 14, 75, 61, 8); // total of 44 frames, loop of 12 frames

        sequence = spriteSheet.AddFrameSquence("PostShooting");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 18);

        sequence = spriteSheet.AddFrameSquence("PreAttack");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(39, 10, 618, 12, 75, 63, 4);
        sequence.AddFrame(40, 7, 780, 15, 75, 60, 8);
        sequence.AddFrame(40, 7, 537, 15, 75, 61, 12); // total of 24 frames

        sequence = spriteSheet.AddFrameSquence("Slashing");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(15, 10, 13, 100, 71, 55, 1);
        sequence.AddFrame(8, 10, 273, 100, 64, 52, 5);
        sequence.AddFrame(19, 27, 345, 83, 64, 72, 5); // slash effect spawn at second frame from here with the slash hitbox
        sequence.AddFrame(18, 27, 415, 83, 63, 72, 33); // slash hitbox unspawn after 14 frames from here, total of 44 frames

        sequence = spriteSheet.AddFrameSquence("Dying");
        sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
        sequence.Hitbox = HITBOX;
        sequence.AddFrame(0, 0, 811, 97, 47, 61, 1, true); // TODO : Get hitbox offset

        sequence = spriteSheet.AddFrameSquence("Cape");
        sequence.OriginOffset = -CAPE_HITBOX.Origin - CAPE_HITBOX.Mins;
        sequence.Hitbox = CAPE_HITBOX;
        sequence.AddFrame(0, 0, 176, 25, 58, 48, 1, true);

        sequence = spriteSheet.AddFrameSquence("Slash");
        sequence.OriginOffset = -SLASH_HITBOX.Origin - SLASH_HITBOX.Mins;
        sequence.Hitbox = SLASH_HITBOX;
        sequence.AddFrame(0, 0, 486, 91, 79, 50, 1, true);

        sequence = spriteSheet.AddFrameSquence("Shot");
        sequence.OriginOffset = -SHOT_HITBOX.Origin - SHOT_HITBOX.Mins;
        sequence.Hitbox = SHOT_HITBOX;
        sequence.AddFrame(4, 4, 745, 127, 16, 16, 1);
        sequence.AddFrame(2, 2, 765, 129, 12, 12, 1);
        sequence.AddFrame(0, 0, 781, 131, 8, 8, 1, true);

        sequence = spriteSheet.AddFrameSquence("BeamSaber");
        sequence.OriginOffset = -PRE_BEAM_SABER_HITBOX.Origin - PRE_BEAM_SABER_HITBOX.Mins;
        sequence.Hitbox = PRE_BEAM_SABER_HITBOX;
        sequence.AddFrame(0, 2, 479, 190, 28, 32, 4);
        sequence.OriginOffset = -BEAM_SABER_HITBOX.Origin - BEAM_SABER_HITBOX.Mins;
        sequence.Hitbox = BEAM_SABER_HITBOX;
        sequence.AddFrame(0, 4, 446, 173, 17, 62, 1, true);

        sequence = spriteSheet.AddFrameSquence("VerticalBeamSaber");
        sequence.OriginOffset = -VERTICAL_PRE_BEAM_SABER_HITBOX.Origin - VERTICAL_PRE_BEAM_SABER_HITBOX.Mins;
        sequence.Hitbox = VERTICAL_PRE_BEAM_SABER_HITBOX;
        sequence.AddFrame(2, 9, 411, 219, 16, 15, 4);
        sequence.OriginOffset = -VERTICAL_BEAM_SABER_HITBOX.Origin - VERTICAL_BEAM_SABER_HITBOX.Mins;
        sequence.Hitbox = VERTICAL_BEAM_SABER_HITBOX;
        sequence.AddFrame(4, 5, 404, 199, 30, 11, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<Trigger> trigger;
    private EntityReference<JediSigmaDoor> door;
    private EntityReference<JediSigmaCape> cape;

    private long triggerFrameCounter;
    private int introducingCounter;
    private Vector finalOrigin;
    private JediSigmaState lastState;
    private bool firstAttack;

    public Trigger Trigger => trigger;

    public JediSigmaDoor Door => door;

    public JediSigmaCape Cape => cape;

    public JediSigmaState State
    {
        get => GetState<JediSigmaState>();
        set => SetState(value);
    }

    public JediSigma()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = true;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames(
            "PreIntroducing",
            "Introducing",
            "Idle",
            "Defending",
            "PreAttack",
            "Attacking",
            "WallJumping",
            "Shooting",
            "PostShooting",
            "Slashing",
            "Dying"
        );

        SetupStateArray<JediSigmaState>();
        RegisterState(JediSigmaState.PRE_INTRODUCING, OnStartPreIntroducing, OnPreIntroducing, null, "PreIntroducing");
        RegisterState(JediSigmaState.INTRODUCING, OnStartIntroducing, OnIntroducing, null, "Introducing");
        RegisterState(JediSigmaState.IDLE, OnStartIdle, OnIdle, null, "Idle");
        RegisterState(JediSigmaState.DEFENDING, OnDefending, "Defending");
        RegisterState(JediSigmaState.ATTACKING, OnStartAttacking, OnAttacking, null, "Attacking");
        RegisterState(JediSigmaState.SLASHING, OnStartSlashing, OnSlashing, null, "Slashing");
        RegisterState(JediSigmaState.SHOOTING, OnShooting, "Shooting");
        RegisterState(JediSigmaState.SHOOTING_BEAM_SABER, OnShootingBeamSaber, "Slashing");
        RegisterState(JediSigmaState.POST_SHOOTING, OnPostShooting, "PostShooting");
        RegisterState(JediSigmaState.WALL_JUMPING, OnWallJumping, "WallJumping");
        RegisterState(JediSigmaState.DYING, "Dying");
    }

    protected override void OnCreated()
    {
        base.OnCreated();

        triggerFrameCounter = 0;

        trigger = Engine.Entities.Create<Trigger>(new
        {
            Origin,
            Hitbox = (Origin, (-32, -10), (32, 4)),
            KillOnOffscreen = false
        });

        Trigger.StartTriggerEvent += OnStartTrigger;
        Trigger.TriggerEvent += OnTrigger;

        door = Engine.Entities.Create<JediSigmaDoor>(new
        {
            Origin = (Origin.X, Origin.Y + 31 + 8),
        });

        Door.sigma = this;

        cape = Engine.Entities.Create<JediSigmaCape>();
        Cape.sigma = this;

        Trigger.Spawn();
        Door.Spawn();
    }

    public override FixedSingle GetGravity()
    {
        return State switch
        {
            JediSigmaState.PRE_INTRODUCING or JediSigmaState.ATTACKING => (FixedSingle) 0,
            _ => base.GetGravity(),
        };
    }

    private void OnStartTrigger(BaseTrigger source, Entity activator)
    {
        if (activator is not Player player)
            return;

        Engine.StartFadingOST(0, 90, Engine.StopOST);

        Engine.Camera.NoConstraints = true;
        Engine.Camera.FocusOn = null;
        player.StartBossDoorCrossing();
        Engine.KillAllAliveEnemiesAndWeapons();

        ChargingEffect chargingEffect = player.ChargingEffect;
        if (chargingEffect != null)
            Engine.FreezeSprites(player, Door, chargingEffect);
        else
            Engine.FreezeSprites(player, Door);

        player.Animating = false;
        player.InputLocked = true;

        Cell sceneCell = GetSceneCellFromPos(player.Origin);
        Box sceneBox = GetSceneBoundingBox(sceneCell);
        Vector offset = (0, -SCREEN_HEIGHT);
        Engine.Camera.MoveToLeftTop(sceneBox.LeftTop + offset, (0, CAMERA_BOOS_DOOR_CROSSING_SMOOTH_SPEED));

        Engine.CameraConstraintsOrigin = (0, 0);
        Engine.CameraConstraintsBox = (0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    }

    private void OnTrigger(BaseTrigger source, Entity activator)
    {
        if (activator is not Player player)
            return;

        if (triggerFrameCounter == 48)
        {
            Trigger.Enabled = false;

            player.Animating = true;
            player.StopBossDoorCrossing();

            Engine.UnfreezeSprites();
            Engine.Camera.NoConstraints = false;
            Engine.Camera.FocusOn = player;

            Door.Close();
        }
        else if (triggerFrameCounter < 48)
        {
            player.Velocity = (0, -CROSSING_BOOS_DOOR_SPEED);
        }

        triggerFrameCounter++;
    }

    internal void OnDoorClosed()
    {
        var player = Engine.Player;
        //player.Invincible = false;
        player.Blinking = false;
        player.WalkTo(Origin - (PLAYER_WALK_OFFSET_X, 0), OnPlayerWakingEnd);
        //player.InputLocked = false;
    }

    private void OnPlayerWakingEnd()
    {
        var player = Engine.Player;
        player.FaceToPosition(Origin);

        Engine.StartSigmaBossBattle();
    }

    private void OnStartPreIntroducing(EntityState state, EntityState lastState)
    {
        introducingCounter = 0;
    }

    private void OnPreIntroducing(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAMES_TO_START_PRE_INTRODUCING)
        {
            Origin += (SIGMA_PRE_INTRODUCING_OFFSET_X, -1);
            finalOrigin = Origin;
            Origin -= (0, PRE_INTRODUCING_STEPS * PRE_INTRODUCING_STEP);
            Visible = true;
        }
        else if (frameCounter > FRAMES_TO_START_PRE_INTRODUCING)
        {
            if (Origin.Y == finalOrigin.Y)
            {
                introducingCounter++;

                if (introducingCounter >= PRE_INTRODUCING_STEPS)
                    State = JediSigmaState.INTRODUCING;
                else
                    Origin -= (0, (PRE_INTRODUCING_STEPS - introducingCounter) * PRE_INTRODUCING_STEP);
            }
            else
                Origin += (0, PRE_INTRODUCING_STEP);
        }
    }

    private void OnStartIntroducing(EntityState state, EntityState lastState)
    {
        CheckCollisionWithWorld = true;
        CheckCollisionWithSolidSprites = true;

        Cape.Spawn();
    }

    private void OnIntroducing(EntityState state, long frameCounter)
    {
        if (frameCounter == FRAMES_TO_START_FILL_HP)
        {
            Cape.Kill();
            cape = null;

            StartHealthFilling();
        }
    }

    private void OnStartIdle(EntityState state, EntityState lastState)
    {
        this.lastState = lastState != null ? (JediSigmaState) lastState.ID : JediSigmaState.IDLE;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        FaceToPlayer();

        if (firstAttack)
        {
            firstAttack = false;
            State = JediSigmaState.ATTACKING;
        }
        else
        {
            var value = Engine.RNG.NextUInt(16);

            switch (lastState)
            {
                case JediSigmaState.IDLE:
                    switch (value)
                    {
                        case >= 0 and < 4:
                            State = JediSigmaState.ATTACKING;
                            break;

                        case >= 4 and < 8:
                            State = JediSigmaState.DEFENDING;
                            break;

                        case >= 8 and < 12:
                            State = JediSigmaState.SHOOTING;
                            break;

                        case >= 12 and < 16:
                            State = JediSigmaState.SHOOTING_BEAM_SABER;
                            break;
                    }

                    break;

                case JediSigmaState.ATTACKING:
                    break;

                case JediSigmaState.DEFENDING:
                    break;

                case JediSigmaState.SHOOTING:
                    break;

                case JediSigmaState.SHOOTING_BEAM_SABER:
                    break;

                default:
                    break;
            }
        }
    }

    private void OnDefending(EntityState state, long frameCounter)
    {
        if (frameCounter >= DEFENDING_FRAMES)
        {
            var value = Engine.RNG.NextUInt(16);
            switch (value)
            {
                case >= 0 and < 8:
                    State = JediSigmaState.ATTACKING;
                    break;

                case >= 8 and < 12:
                    State = JediSigmaState.SHOOTING;
                    break;

                case >= 12 and < 16:
                    State = JediSigmaState.SHOOTING_BEAM_SABER;
                    break;
            }
        }
    }

    private void OnStartAttacking(EntityState state, EntityState lastState)
    {
        Velocity = (ATTACKING_SPEED * Direction.GetHorizontalSignal(), 0);
    }

    private void OnAttacking(EntityState state, long frameCounter)
    {
        var player = Engine.Player;
        if (player == null)
        {
            State = JediSigmaState.IDLE;
            return;
        }

        if (GetHorizontalDirection(player) == Direction)
        {
            var delta = Engine.Player.Origin - Origin;
            if (delta.X.Abs < SLASH_HITBOX.Width && delta.Y.Abs < SLASH_HITBOX.Height)
            {
                State = JediSigmaState.SLASHING;
                return;
            }
        }

        if (BlockedLeft || BlockedRight)
            State = JediSigmaState.IDLE;
    }

    private void OnStartSlashing(EntityState state, EntityState lastState)
    {
        Velocity = Vector.NULL_VECTOR;
    }

    private void OnSlashing(EntityState state, long frameCounter)
    {
        if (frameCounter >= SLASH_FRAMES)
            State = JediSigmaState.IDLE;
    }

    private void OnShooting(EntityState state, long frameCounter)
    {
    }

    private void OnShootingBeamSaber(EntityState state, long frameCounter)
    {
    }

    private void OnPostShooting(EntityState state, long frameCounter)
    {
    }

    private void OnWallJumping(EntityState state, long frameCounter)
    {
    }

    protected override void OnPlayBossBattleOST()
    {
        Engine.PlayJediSigmaBatleOST();
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return COLLISION_BOX;
    }

    protected override FixedSingle GetCollisionBoxLegsHeight()
    {
        return COLLISION_BOX_LEGS_HEIGHT;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CanGoOutOfMapBounds = true;
        CheckCollisionWithWorld = false;
        CheckCollisionWithSolidSprites = false;
        ContactDamage = CONTACT_DAMAGE;
        Visible = false;

        Velocity = Vector.NULL_VECTOR;
        State = JediSigmaState.PRE_INTRODUCING;

        firstAttack = true;
    }

    protected override void OnDeath()
    {
        Door?.Kill();

        base.OnDeath();
    }

    protected override void OnStartBattle()
    {
        base.OnStartBattle();

        State = JediSigmaState.IDLE;
    }

    protected override void OnDying()
    {
        State = JediSigmaState.DYING;
    }
}