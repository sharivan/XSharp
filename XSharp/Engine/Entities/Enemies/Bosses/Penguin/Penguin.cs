using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public enum PenguinState
{
    IDLE = 0,
    INTRODUCING = 1,
    SHOOTING_ICE = 2,
    BLOWING = 3,
    SLIDING = 4,
    JUMPING = 5,
    HANGING = 6,
    TAKING_DAMAGE = 7,
    IN_FLAMES = 8,
    DYING = 9
}

public class Penguin : Boss, IStateEntity<PenguinState>
{
    public static readonly bool DONT_ATTACK = false;

    public static readonly Color[] PENGUIN_PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF303040), // 1
        Color.FromBgra(0xFF3870F0), // 2
        Color.FromBgra(0xFFD08050), // 3
        Color.FromBgra(0xFFF8B050), // 4
        Color.FromBgra(0xFFF0F0F0), // 5           
        Color.FromBgra(0xFFB0B0C8), // 6
        Color.FromBgra(0xFF686880), // 7
        Color.FromBgra(0xFF185068), // 8
        Color.FromBgra(0xFF205898), // 9
        Color.FromBgra(0xFFF03808), // A
        Color.FromBgra(0xFFA83008), // B
        Color.FromBgra(0xFF683010), // C
        Color.FromBgra(0xFF9870D8), // D
        Color.FromBgra(0xFF6848B8), // E
        Color.FromBgra(0xFF282828), // F

        Color.Transparent, // 10
        Color.FromBgra(0xFF404800), // 11
        Color.FromBgra(0xFFF8F8F8), // 12
        Color.FromBgra(0xFFC8D8E0), // 13
        Color.FromBgra(0xFF98C0D0), // 14
        Color.FromBgra(0xFF70A8B8), // 15
        Color.FromBgra(0xFF4090A8), // 16
        Color.FromBgra(0xFF187898), // 17
        Color.FromBgra(0xFF185058), // 18
        Color.FromBgra(0xFF803090), // 19
        Color.FromBgra(0xFFB0F8F8), // 1A
        Color.FromBgra(0xFFA8F8F8), // 1B
        Color.FromBgra(0xFFA0F8F8), // 1C
        Color.FromBgra(0xFF98F8F8), // 1D
        Color.FromBgra(0xFF90F8F8), // 1E
        Color.FromBgra(0xFF88F8F8) // 1F
    };

    [Precache]
    new internal static void Precache()
    {
        Engine.PrecacheSound("Chill Penguin Breath", @"resources\sounds\mmx\52 - MMX - Chill Penguin Breath.wav");
        Engine.PrecacheSound("Misc. dash, jump, move (3)", @"resources\sounds\mmx\91 - MMX - Misc. dash, jump, move (3).wav");
        Engine.PrecacheSound("Ice", @"resources\sounds\mmx\34 - MMX - Ice.wav");
        Engine.PrecacheSound("Ice Freeze", @"resources\sounds\mmx\35 - MMX - Ice Freeze.wav");
        Engine.PrecacheSound("Ice Break", @"resources\sounds\mmx\36 - MMX - Ice Break.wav");

        var penguinPalette = Engine.PrecachePalette("penguinPalette", PENGUIN_PALETTE);
        var penguinSpriteSheet = Engine.CreateSpriteSheet("Penguin", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.Bosses.X1.Penguin.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            penguinSpriteSheet.CurrentTexture = texture;
        }

        penguinSpriteSheet.CurrentPalette = penguinPalette;

        // 0
        var sequence = penguinSpriteSheet.AddFrameSquence("FallingIntroducing");
        sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
        sequence.Hitbox = PENGUIN_COLLISION_BOX;
        sequence.AddFrame(2, 15, 170, 20, 35, 44, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("LandingIntroducing");
        sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
        sequence.Hitbox = PENGUIN_COLLISION_BOX;
        sequence.AddFrame(7, 1, 136, 172, 39, 35, 6);
        sequence.AddFrame(6, -2, 96, 175, 38, 32, 6);

        sequence = penguinSpriteSheet.AddFrameSquence("Introducing");
        sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
        sequence.Hitbox = PENGUIN_COLLISION_BOX;
        sequence.AddFrame(6, 2, 6, 177, 38, 36, 6);
        sequence.AddFrame(7, 0, 136, 172, 39, 35, 6);
        sequence.AddFrame(6, -2, 96, 175, 38, 32, 5);
        sequence.AddFrame(3, 3, 48, 76, 42, 37, 5);
        sequence.AddFrame(3, 3, 94, 76, 43, 37, 5);
        sequence.AddFrame(7, 2, 141, 77, 39, 36, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("IntroducingEnd");
        sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
        sequence.Hitbox = PENGUIN_COLLISION_BOX;
        sequence.AddFrame(6, 2, 184, 77, 38, 36, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(10, 4, 6, 77, 38, 36, 15);
        sequence.AddFrame(10, 4, 184, 77, 38, 36, 15);
        sequence.AddFrame(10, 4, 6, 77, 38, 36, 15);
        sequence.AddFrame(10, 4, 184, 77, 38, 36, 15);
        sequence.AddFrame(10, 4, 6, 77, 38, 36, 7);
        sequence.AddFrame(11, 3, 136, 172, 39, 35, 7, true);
        sequence.AddFrame(10, 4, 6, 77, 38, 36, 7);

        sequence = penguinSpriteSheet.AddFrameSquence("ShootingIce");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(11, 3, 136, 172, 39, 35, 6);
        sequence.AddFrame(7, 5, 48, 76, 42, 37, 5);
        sequence.AddFrame(7, 5, 94, 76, 43, 37, 5);
        sequence.AddFrame(16, 3, 132, 129, 42, 35, 5);
        sequence.AddFrame(14, 3, 174, 129, 43, 35, 5);

        sequence = penguinSpriteSheet.AddFrameSquence("PreSliding");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(7, 5, 48, 76, 42, 37, 11);
        sequence.AddFrame(7, 5, 94, 76, 43, 37, 11);
        sequence.AddFrame(8, 3, 90, 130, 37, 34, 7);

        sequence = penguinSpriteSheet.AddFrameSquence("Sliding");
        sequence.OriginOffset = -PENGUIN_SLIDE_HITBOX.Origin - PENGUIN_SLIDE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_SLIDE_HITBOX;
        sequence.AddFrame(0, 9, 221, 133, 40, 31, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Blowing");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(7, 5, 48, 76, 42, 37, 11);
        sequence.AddFrame(7, 5, 94, 76, 43, 37, 11);
        sequence.AddFrame(16, 3, 132, 129, 42, 35, 4, true);
        sequence.AddFrame(14, 3, 174, 129, 43, 35, 4);

        sequence = penguinSpriteSheet.AddFrameSquence("PreJumping");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(5, 0, 96, 175, 38, 32, 5);
        sequence.AddFrame(8, 6, 8, 127, 36, 38, 5);

        sequence = penguinSpriteSheet.AddFrameSquence("Jumping");
        sequence.OriginOffset = -PENGUIN_JUMP_HITBOX.Origin - PENGUIN_JUMP_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_JUMP_HITBOX;
        sequence.AddFrame(10, 4, 47, 127, 37, 38, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Falling");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(5, 14, 170, 20, 35, 44, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Landing");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(11, 3, 136, 172, 39, 35, 6);
        sequence.AddFrame(10, 0, 96, 175, 38, 32, 6);

        sequence = penguinSpriteSheet.AddFrameSquence("Hanging");
        sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_HITBOX;
        sequence.AddFrame(5, 19, 12, 4, 36, 60, 8);
        sequence.AddFrame(5, 19, 54, 4, 34, 58, 8);
        sequence.AddFrame(5, 19, 95, 4, 32, 60, 8);
        sequence.AddFrame(6, 20, 131, 4, 32, 60, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("TakingDamage");
        sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_TAKING_DAMAGE_HITBOX;
        sequence.AddFrame(14, 4, 9, 169, 35, 41, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Dying");
        sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_TAKING_DAMAGE_HITBOX;
        sequence.AddFrame(15, 3, 9, 169, 35, 41, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("InFlames");
        sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_TAKING_DAMAGE_HITBOX;
        sequence.AddFrame(16, 7, 52, 165, 38, 47, 21);

        penguinSpriteSheet.CurrentPalette = null;

        sequence = penguinSpriteSheet.AddFrameSquence("Ice");
        sequence.OriginOffset = -PENGUIN_ICE_HITBOX.Origin - PENGUIN_ICE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_ICE_HITBOX;
        sequence.AddFrame(0, 2, 57, 232, 14, 14, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("IceFragment");
        sequence.OriginOffset = -PENGUIN_ICE_FRAGMENT_HITBOX.Origin - PENGUIN_ICE_FRAGMENT_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_ICE_FRAGMENT_HITBOX;
        sequence.AddFrame(0, 0, 58, 216, 8, 8, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Sculpture");
        sequence.OriginOffset = -PENGUIN_SCULPTURE_HITBOX.Origin - PENGUIN_SCULPTURE_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_SCULPTURE_HITBOX;
        sequence.AddFrame(-2, -7, 82, 233, 13, 16, 19);
        sequence.AddFrame(2, -3, 104, 224, 19, 24, 19);
        sequence.AddFrame(4, -1, 183, 226, 23, 28, 19);
        sequence.AddFrame(5, 0, 133, 217, 28, 32, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Lever");
        sequence.OriginOffset = -PENGUIN_LEVER_HITBOX.Origin - PENGUIN_LEVER_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_LEVER_HITBOX;
        sequence.AddFrame(-8, -4, 169, 225, 10, 16, 1, true);

        sequence = penguinSpriteSheet.AddFrameSquence("Snow");
        sequence.OriginOffset = -PENGUIN_SNOW_HITBOX.Origin - PENGUIN_SNOW_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_SNOW_HITBOX;
        sequence.AddFrame(1, 1, 186, 172, 18, 18, 1, true);
        sequence.AddFrame(1, 1, 204, 172, 18, 18, 1);
        sequence.AddFrame(2, 2, 222, 172, 18, 18, 1);
        sequence.AddFrame(2, 2, 240, 172, 18, 18, 1);
        sequence.AddFrame(2, 2, 258, 172, 18, 18, 1);
        sequence.AddFrame(3, 3, 276, 172, 18, 18, 1);
        sequence.AddFrame(3, 2, 186, 190, 18, 18, 1);
        sequence.AddFrame(3, 2, 204, 190, 18, 18, 1);
        sequence.AddFrame(4, 3, 222, 190, 18, 18, 1);
        sequence.AddFrame(3, 3, 240, 190, 18, 18, 1);
        sequence.AddFrame(3, 3, 258, 190, 18, 18, 1);
        sequence.AddFrame(4, 4, 276, 190, 18, 18, 1);
        sequence.AddFrame(2, 3, 186, 208, 18, 18, 1);
        sequence.AddFrame(2, 3, 204, 208, 18, 18, 1);
        sequence.AddFrame(3, 4, 222, 208, 18, 18, 1);

        sequence = penguinSpriteSheet.AddFrameSquence("FrozenBlock");
        sequence.OriginOffset = -PENGUIN_FROZEN_BLOCK_HITBOX.Origin - PENGUIN_FROZEN_BLOCK_HITBOX.Mins;
        sequence.Hitbox = PENGUIN_FROZEN_BLOCK_HITBOX;
        sequence.AddFrame(14, 3, 6, 216, 37, 38, 1, true);

        penguinSpriteSheet.ReleaseCurrentTexture();
    }

    private bool firstAttack;
    private bool hanging;
    private bool snowing;
    private int snowingFrameCounter;
    private bool wasShootingIce;
    private int iceCount;

    private EntityReference<PenguinLever> lever;
    private EntityReference<Mist> mist;
    private EntityReference<PenguinSculpture> sculpture1;
    private EntityReference<PenguinSculpture> sculpture2;
    private EntityReference<PenguinFrozenBlock> frozenBlock;

    private PenguinLever Lever => lever;

    private Mist Mist => mist;

    private PenguinSculpture Sculpture1 => sculpture1;

    private PenguinSculpture Sculpture2 => sculpture2;

    private PenguinFrozenBlock FrozenBlock => frozenBlock;

    public PenguinState State
    {
        get => GetState<PenguinState>();
        set
        {
            if (DONT_ATTACK)
            {
                if (value is PenguinState.IDLE or PenguinState.INTRODUCING or PenguinState.TAKING_DAMAGE or PenguinState.DYING)
                    SetState(value);
            }
            else
            {
                SetState(value);
            }
        }
    }

    public Penguin()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        DefaultDirection = Direction.LEFT;
        SpriteSheetName = "Penguin";
        PaletteName = "penguinPalette";

        ContactDamage = 6;

        SetAnimationNames(
            "FallingIntroducing", "LandingIntroducing", "Introducing", "IntroducingEnd", "Idle", "ShootingIce",
            "PreSliding", "Sliding", "Blowing", "PreJumping", "Jumping", "Falling", "Landing", "Hanging",
            "TakingDamage", "InFlames", "Dying"
            );

        SetupStateArray(typeof(PenguinState));
        RegisterState(PenguinState.IDLE, OnIdle, "Idle");
        RegisterState(PenguinState.INTRODUCING, "FallingIntroducing");
        RegisterState(PenguinState.SHOOTING_ICE, OnShootingIce, "ShootingIce");
        RegisterState(PenguinState.BLOWING, OnStartBlowing, OnBlowing, null, "Blowing");
        RegisterState(PenguinState.SLIDING, OnStartSliding, OnSliding, OnEndSliding, "PreSliding");
        RegisterState(PenguinState.JUMPING, OnStartJumping, OnJumping, null, "PreJumping");
        RegisterState(PenguinState.HANGING, OnStartHanging, OnHanging, null, "Idle");
        RegisterState(PenguinState.TAKING_DAMAGE, OnTakingDamage, "TakingDamage");
        RegisterState(PenguinState.IN_FLAMES, OnInFlames, "InFlames");
        RegisterState(PenguinState.DYING, OnStartDying, "Dying");

        lever = Engine.Entities.Create<PenguinLever>();
        mist = Engine.Entities.Create<Mist>();

        sculpture1 = Engine.Entities.Create<PenguinSculpture>(new
        {
            Shooter = this,
            Respawnable = true
        });

        sculpture2 = Engine.Entities.Create<PenguinSculpture>(new
        {
            Shooter = this,
            Respawnable = true
        });

        frozenBlock = Engine.Entities.Create<PenguinFrozenBlock>(new
        {
            Attacker = this,
            Respawnable = true
        });
    }

    public override FixedSingle GetGravity()
    {
        return hanging || State == PenguinState.DYING ? 0 : base.GetGravity();
    }

    protected override Box GetCollisionBox()
    {
        return PENGUIN_COLLISION_BOX;
    }

    protected override Box GetHitbox()
    {
        return State switch
        {
            PenguinState.INTRODUCING => PENGUIN_COLLISION_BOX,
            PenguinState.SLIDING => PENGUIN_SLIDE_HITBOX,
            PenguinState.JUMPING => CurrentAnimationName == "Jumping" ? PENGUIN_JUMP_HITBOX : PENGUIN_HITBOX,
            PenguinState.TAKING_DAMAGE => PENGUIN_TAKING_DAMAGE_HITBOX,
            _ => PENGUIN_HITBOX,
        };
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        PaletteName = "penguinPalette";
        firstAttack = true;
        hanging = false;
        snowing = false;
        snowingFrameCounter = 0;
        wasShootingIce = false;
        iceCount = 0;
        Direction = Direction.LEFT;
        MaxHealth = BOSS_HP;

        Mist.Spawn();

        SetState(PenguinState.INTRODUCING);
    }

    protected override void OnDeath()
    {
        Sculpture1.Kill();
        Sculpture2.Kill();
        Mist.Kill();
        Lever.Kill();

        base.OnDeath();
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        switch (State)
        {
            case PenguinState.INTRODUCING:
                SetCurrentAnimationByName("LandingIntroducing");
                break;

            case PenguinState.TAKING_DAMAGE:
                State = PenguinState.IDLE;
                break;

            default:
                SetCurrentAnimationByName("Landing");
                break;
        }

        Velocity = Vector.NULL_VECTOR;
    }

    private void FlipSpeedAndDirection()
    {
        Velocity = -Velocity;
        Direction = Direction.Oposite();
    }

    protected override void OnBlockedLeft()
    {
        base.OnBlockedLeft();

        if (State == PenguinState.SLIDING && Direction == Direction.LEFT)
            FlipSpeedAndDirection();
    }

    protected override void OnBlockedRight()
    {
        base.OnBlockedRight();

        if (State == PenguinState.SLIDING && Direction == Direction.RIGHT)
            FlipSpeedAndDirection();
    }

    protected override void OnBlockedUp()
    {
        base.OnBlockedUp();

        Velocity = Vector.NULL_VECTOR;
    }

    private void ApplyKnockback(Sprite attacker)
    {
        FaceToEntity(attacker);
        Velocity = (Direction == DefaultDirection ? PENGUIN_KNOCKBACK_SPEED_X : -PENGUIN_KNOCKBACK_SPEED_X, -PENGUIN_KNOCKBACK_SPEED_Y);
        State = PenguinState.TAKING_DAMAGE;
    }

    protected override void OnDamaged(Sprite attacker, FixedSingle damage)
    {
        base.OnDamaged(attacker, damage);

        if (State is PenguinState.IDLE or PenguinState.JUMPING or PenguinState.HANGING or PenguinState.SHOOTING_ICE)
            ApplyKnockback(attacker);
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        if (frameCounter == 14)
        {
            if (firstAttack)
            {
                firstAttack = false;
                FaceToPlayer();
                State = PenguinState.SHOOTING_ICE;
            }
            else if (wasShootingIce && iceCount < 4)
            {
                FaceToPlayer();
                State = PenguinState.SHOOTING_ICE;
            }
            else
            {
                FaceToPlayer();
                wasShootingIce = false;
                iceCount = 0;

                if (!DONT_ATTACK)
                {
                    int value = Engine.RNG.NextInt(5);
                    while (State == PenguinState.IDLE)
                    {
                        switch (value)
                        {
                            case 0:
                                State = PenguinState.SLIDING;
                                break;

                            case 1:
                                State = PenguinState.SHOOTING_ICE;
                                break;

                            case 2:
                                State = PenguinState.JUMPING;
                                break;

                            case 3:
                                State = PenguinState.HANGING;
                                break;

                            case 4:
                                if (!AllSculpturesAlive())
                                    State = PenguinState.BLOWING;

                                break;
                        }

                        value = Engine.RNG.NextInt(4);
                    }
                }
            }
        }
    }

    private bool AllSculpturesAlive()
    {
        return Sculpture1.Alive && Sculpture2.Alive;
    }

    private void OnShootingIce(EntityState state, long frameCounter)
    {
        if (frameCounter == PENGUIN_SHOT_START_FRAME)
        {
            ShootIce();
            wasShootingIce = true;
            State = PenguinState.IDLE;
        }
    }

    private void OnStartBlowing(EntityState state, EntityState lastState)
    {
    }

    private void PlayBlowingSoundLoop()
    {
        Engine.PlaySound(5, "Chill Penguin Breath", 2.3305, 0.03018);
    }

    private void FinishBlowingSoundLoop()
    {
        Engine.ClearSoundLoopPoint(5, "Chill Penguin Breath", true);
    }

    private void StopBlowingSound()
    {
        Engine.StopSound(5, "Chill Penguin Breath");
    }

    private void OnBlowing(EntityState state, long frameCounter)
    {
        switch (frameCounter)
        {
            case PENGUIN_SHOT_START_FRAME:
                PlayBlowingSoundLoop();
                break;

            case PENGUIN_BLOW_FRAMES_TO_SPAWN_SCULPTURES:
                if (!Sculpture1.Alive)
                {
                    Sculpture1.Origin = Origin + (Direction == Direction.RIGHT ? PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.X : -PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.X, PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.Y);
                    Sculpture1.Spawn();
                }

                if (!Sculpture2.Alive)
                {
                    Sculpture2.Origin = Origin + (Direction == Direction.RIGHT ? PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.X : -PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.X, PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.Y);
                    Sculpture2.Spawn();
                }

                break;

            case PENGUIN_BLOW_FRAMES:
                FinishBlowingSoundLoop();
                State = PenguinState.IDLE;
                break;
        }

        if (frameCounter >= PENGUIN_SHOT_START_FRAME && frameCounter % 8 == 0)
            ShootSnow();
    }

    private void ShootIce()
    {
        PenguinIce ice = Engine.Entities.Create<PenguinIce>(new
        {
            Shooter = this,
            Bump = Engine.RNG.NextInt(2) == 1
        });

        ice.Spawn();
        iceCount++;
    }

    private void ShootSnow()
    {
        PenguinSnow snow = Engine.Entities.Create<PenguinSnow>(new
        {
            Shooter = this
        });

        snow.Spawn();
    }

    private void OnStartSliding(EntityState state, EntityState lastState)
    {
        Invincible = true;
        ReflectShots = true;
    }

    private void OnSliding(EntityState state, long frameCounter)
    {
        if (frameCounter >= 30)
        {
            if (frameCounter == 30)
                Engine.PlaySound(4, "Misc. dash, jump, move (3)");

            Vector v = Velocity;
            Vector a = PENGUIN_SLIDE_DECELARATION * (Direction == DefaultDirection ? Vector.LEFT_VECTOR : Vector.RIGHT_VECTOR);
            v -= a;
            if (v.X > 0 && Velocity.X < 0 || v.X < 0 && Velocity.X > 0 || v.X.Abs < PENGUIN_SLIDE_DECELARATION)
            {
                Velocity = Vector.NULL_VECTOR;
                State = PenguinState.IDLE;
            }
            else
            {
                Velocity = v;
            }
        }
    }

    private void OnEndSliding(EntityState state)
    {
        Invincible = false;
        ReflectShots = false;
    }

    private void OnStartJumping(EntityState state, EntityState lastState)
    {
    }

    private void OnJumping(EntityState state, long frameCounter)
    {
        if (!Landed && Velocity.Y > 0 && CurrentAnimationName != "Falling")
            SetCurrentAnimationByName("Falling", 0);
    }

    private void OnStartHanging(EntityState state, EntityState laststate)
    {
        FaceToScreenCenter();
    }

    private void OnHanging(EntityState state, long frameCounter)
    {
        switch (frameCounter)
        {
            case PENGUIN_FRAMES_BEFORE_HANGING_JUMP:
                SetCurrentAnimationByName("PreJumping", 0);
                break;

            case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG:
                Velocity = Vector.NULL_VECTOR;
                Origin = Lever.Origin + (Lever.Origin.X < Origin.X ? PENGUIN_HANGING_OFFSET : (-PENGUIN_HANGING_OFFSET.X, PENGUIN_HANGING_OFFSET.Y));
                hanging = true;
                SetCurrentAnimationByName("Hanging", 0);
                break;

            case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG + PENGUIN_FRAMES_BEFORE_SNOW_AFTER_HANGING:
                snowing = true;
                snowingFrameCounter = 0;
                Mist.MistDirection = Direction;
                Mist.Play();
                break;

            case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG + PENGUIN_FRAMES_BEFORE_STOP_HANGING:
                Velocity = Vector.NULL_VECTOR;
                hanging = false;
                SetCurrentAnimationByName("Falling", 0);
                break;
        }
    }

    private void OnTakingDamage(EntityState state, long frameCounter)
    {
        hanging = false;
        iceCount = 4;
    }

    private void OnInFlames(EntityState state, long frameCounter)
    {
    }

    private void BreakSculptures()
    {
        if (Sculpture1.Alive)
            Sculpture1.Break();

        if (Sculpture2.Alive)
            Sculpture2.Break();
    }

    private void BreakFrozenBlock()
    {
        if (FrozenBlock.Alive)
            FrozenBlock.Break();
    }

    private void OnStartDying(EntityState state, EntityState lastState)
    {
        BreakSculptures();
        BreakFrozenBlock();
        Lever.Hide();
        Mist.Stop();
        Engine.Player.ResetExternalVelocity();
    }

    protected override void Think()
    {
        if (snowing)
        {
            var adictionalVelocity = (Mist.MistDirection == Direction.LEFT ? -PENGUIN_HANGING_SNOWING_SPEED_X : PENGUIN_HANGING_SNOWING_SPEED_X, 0);
            Engine.Player.AddExternalVelocity(adictionalVelocity);

            if (Sculpture1.Alive && !Sculpture1.MarkedToRemove && !Sculpture1.Broke)
                Sculpture1.AddExternalVelocity(adictionalVelocity);

            if (Sculpture2.Alive && !Sculpture2.MarkedToRemove && !Sculpture2.Broke)
                Sculpture2.AddExternalVelocity(adictionalVelocity);

            snowingFrameCounter++;
            if (snowingFrameCounter == PENGUIN_MIST_FRAMES)
            {
                Mist.Stop();
                snowing = false;
            }
        }

        base.Think();
    }

    protected internal override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        switch (animation.Name)
        {
            case "PreSliding":
                SetCurrentAnimationByName("Sliding");

                if (Direction == DefaultDirection)
                {
                    Velocity = PENGUIN_SLIDE_INITIAL_SPEED * Vector.LEFT_VECTOR;
                    if (WorldCollider.BlockedLeft)
                        FlipSpeedAndDirection();
                }
                else
                {
                    Velocity = PENGUIN_SLIDE_INITIAL_SPEED * Vector.RIGHT_VECTOR;
                    if (WorldCollider.BlockedRight)
                        FlipSpeedAndDirection();
                }

                break;

            case "PreJumping":
                switch (State)
                {
                    case PenguinState.JUMPING:
                    {
                        FixedSingle jumpSpeedX = (Engine.Player.Origin.X - Origin.X) / PENGUIN_JUMP_FRAMES;
                        Velocity = (jumpSpeedX, -PENGUIN_JUMP_SPEED_Y);
                        break;
                    }

                    case PenguinState.HANGING:
                    {
                        FixedSingle jumpSpeedX = (Lever.Origin.X - Origin.X) / PENGUIN_FRAMES_TO_HANG;
                        Velocity = (jumpSpeedX, -PENGUIN_HANGING_JUMP_SPEED_Y);
                        break;
                    }
                }

                SetCurrentAnimationByName("Jumping");
                break;

            case "LandingIntroducing":
                SetCurrentAnimationByName("Introducing");
                break;

            case "Introducing" when !HealthFilling && Health == 0:
                StartHealthFilling();

                Lever.Origin = World.World.GetSceneBoundingBoxFromPos(Origin).MiddleTop + (0, 12);
                Lever.Spawn();
                break;

            case "Landing":
                State = PenguinState.IDLE;
                break;
        }
    }

    protected override void OnStartBattle()
    {
        base.OnStartBattle();

        State = PenguinState.IDLE;
    }

    protected override void OnDying()
    {
        StopBlowingSound();
        hanging = false;
        snowing = false;
        Velocity = Vector.NULL_VECTOR;
        State = PenguinState.DYING;
    }

    public void FreezePlayer()
    {
        if (Alive && !Exploding && !Broke && !FrozenBlock.Alive
            && !Engine.Player.TakingDamage && !Engine.Player.Blinking)
        {
            FrozenBlock.Spawn();
        }
    }
}