using System;
using System.Linq;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.Entities;

public delegate void SpriteEvent(Sprite source);
public delegate void TakeDamageEvent(Sprite source, Sprite attacker, FixedSingle damage);
public delegate void HurtEvent(Sprite source, Sprite victim, FixedSingle damage);
public delegate void HealthChangedEvent(Sprite source, FixedSingle health);
public delegate void AnimationEndEvent(Sprite source, Animation animation);

public abstract class Sprite : Entity, IRenderable
{
    public event TakeDamageEvent TakeDamageEvent;
    public event HurtEvent HurtEvent;
    public event HealthChangedEvent HealthChangedEvent;
    public event AnimationEndEvent AnimationEndEvent;
    public event SpriteEvent BeforeMoveEvent;
    public event SpriteEvent MoveEvent;
    public event SpriteEvent StartMovingEvent;
    public event SpriteEvent StopMovingEvent;
    public event SpriteEvent BlockedLeftEvent;
    public event SpriteEvent BlockedRightEvent;
    public event SpriteEvent BlockedUpEvent;
    public event SpriteEvent LandedEvent;
    public event SpriteEvent BrokeEvent;

    private string paletteName = null;

    [NotSerializable]
    private Palette palette = null;

    private string spriteSheetName = null;

    [NotSerializable]
    private SpriteSheet spriteSheet = null;

    private bool visible = true;
    internal int layer = 0;
    internal int priority = -1;

    private (string name, int count, bool startWithCounterSuffix, int startCounterSuffix)[] animationNames;
    private AnimationReference currentAnimation = null;

    protected SpriteCollider worldCollider;
    protected SpriteCollider spriteCollider;

    private bool lastBlockedUp = false;
    private bool lastBlockedLeft = false;
    private bool lastBlockedRight = false;
    private bool lastLanded = false;

    private Vector vel;
    protected bool moving;
    protected bool breakable;
    protected FixedSingle health;
    private bool invincible;
    private int invincibilityFrames;
    private int invincibilityFrameCounter = 0;
    protected bool broke;
    private bool blinking;
    private int blinkFrames;
    private int blinkFrameCounter = 0;

    private EntitySet<Sprite> touchingSpritesLeft;
    private EntitySet<Sprite> touchingSpritesUp;
    private EntitySet<Sprite> touchingSpritesRight;
    private EntitySet<Sprite> touchingSpritesDown;

    public AnimationFactory Animations
    {
        get;
        private set;
    }

    public bool UpsideDown
    {
        get;
        set;
    } = false;

    public NinetyRotation Rotation
    {
        get;
        set;
    } = NinetyRotation.ANGLE_0;

    public bool Visible
    {
        get => visible;

        set
        {
            visible = value;
            UpdatePartition();
        }
    }

    public int Layer
    {
        get => layer;

        set
        {
            if (!Alive)
                layer = value;
            else if (layer != value)
                Engine.UpdateSpriteLayer(this, value);
        }
    }

    public int Priority
    {
        get => priority;

        set
        {
            if (!Alive)
                priority = value;
            else if (priority != value)
                Engine.UpdateSpritePriority(this, value);
        }
    }

    public bool ResourcesCreated
    {
        get;
        private set;
    } = false;

    public bool MultiAnimation
    {
        get;
        protected set;
    } = false;

    public Box DrawBox
    {
        get
        {
            if (!MultiAnimation)
                return CurrentAnimation != null ? CurrentAnimation.DrawBox : IntegerOrigin + Box.EMPTY_BOX;

            Box result = Box.EMPTY_BOX;
            foreach (var animation in Animations)
            {
                if (animation.Visible)
                    result |= animation.DrawBox;
            }

            return result;
        }
    }

    public string CurrentAnimationName
    {
        get => currentAnimation?.TargetName;
        set => SetCurrentAnimationByName(value);
    }

    public int CurrentAnimationIndex
    {
        get => currentAnimation is not null ? currentAnimation.TargetIndex : -1;
        set => SetCurrentAnimationByIndex(value);
    }

    public Animation CurrentAnimation
    {
        get => currentAnimation;
        set
        {
            Animation animation;
            bool animating = false;
            int animationFrame = -1;
            if (!MultiAnimation)
            {
                animation = currentAnimation;
                if (animation != null)
                {
                    animating = animation.Animating;
                    animationFrame = animation.CurrentFrame;
                    animation.Stop();
                    animation.Visible = false;
                }
            }

            currentAnimation = value;
            animation = currentAnimation;
            if (animation != null)
            {
                animation.CurrentFrame = animationFrame != -1 ? animationFrame : 0;
                animation.Animating = animating;
                animation.Visible = true;
            }
        }
    }

    public bool MirrorAnimationFromDirection
    {
        get;
        set;
    } = true;

    public float Opacity
    {
        get;
        set;
    } = 1;

    public CollisionData CollisionData
    {
        get;
        set;
    } = CollisionData.NONE;

    public bool Inertial
    {
        get;
        set;
    } = false;

    public bool CanSmash
    {
        get;
        set;
    } = false;

    public string InitialAnimationName
    {
        get;
        set;
    } = null;

    public int InitialAnimationIndex => InitialAnimationName is not null and not "" ? GetAnimationIndex(InitialAnimationName) : -1;

    new public SpriteState CurrentState
    {
        get => (SpriteState) base.CurrentState;
        set => base.CurrentState = value;
    }

    public bool Static
    {
        get;
        set;
    }

    public bool NoClip
    {
        get;
        set;
    }

    public bool Moving => moving;

    public bool Broke => broke;

    public bool Invincible
    {
        get => invincible;
        set
        {
            bool lastInvincible = invincible;

            if (value)
                MakeInvincible();

            invincible = value;

            if (lastInvincible && !value)
                OnEndInvincibility();
        }
    }

    public bool Blinking
    {
        get => blinking;
        set
        {
            bool lastBlinking = blinking;

            if (value)
                MakeBlinking();

            blinking = value;

            if (lastBlinking && !value)
                OnEndBlinking();
        }
    }

    public FadingControl FadingControl
    {
        get;
        private set;
    }

    public bool InvisibleOnNextFrame
    {
        get;
        set;
    }

    public FixedSingle Health
    {
        get => health;
        set
        {
            health = value;
            OnHealthChanged(health);

            if (health == 0)
                Break();
        }
    }

    public Vector Velocity
    {
        get => vel;
        set
        {
            LastVelocity = vel;
            vel = value.TruncFracPart();
        }
    }

    public Vector LastVelocity
    {
        get;
        private set;
    }

    public Vector ExternalVelocity
    {
        get;
        private set;
    } = Vector.NULL_VECTOR;

    public bool ResetExternalVelocityOnFrame
    {
        get;
        set;
    } = true;

    public override Box Hitbox
    {
        get
        {
            Box box = base.Hitbox;

            if (UpsideDown)
                box = box.Flip(Origin);

            if (Rotation != NinetyRotation.ANGLE_0)
                box = box.Rotate(Origin, Rotation);

            //if (Scale != 1)
            //    box = box.Scale(Origin, Scale);

            return box;
        }

        protected set
        {
            Box box = value;

            //if (Scale != 1)
            //    box = box.ScaleInverse(Origin, Scale);

            if (Rotation != NinetyRotation.ANGLE_0)
                box = box.Rotate(Origin, Rotation.Inverse());

            if (UpsideDown)
                box = box.Flip(Origin);

            base.Hitbox = box;
        }
    }

    public virtual Box CollisionBox
    {
        get
        {
            Box box = Origin + GetCollisionBox();

            if (Direction != DefaultDirection)
                box = box.Mirror(Origin);

            if (UpsideDown)
                box = box.Flip(Origin);

            if (Rotation != NinetyRotation.ANGLE_0)
                box = box.Rotate(Origin, Rotation);

            //if (Scale != 1)
            //    box = box.Scale(Origin, Scale);

            return box;
        }
    }

    public SpriteCollider WorldCollider
    {
        get
        {
            if (worldCollider != null)
                worldCollider.Box = CollisionBox;

            return worldCollider;
        }
    }

    public SpriteCollider SpriteCollider
    {
        get
        {
            if (spriteCollider != null)
            {
                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;
            }

            return spriteCollider;
        }
    }

    public FixedSingle Gravity => GetGravity();

    public FixedSingle TerminalDownwardSpeed => GetTerminalDownwardSpeed();

    public bool CheckCollisionWithWorld
    {
        get => worldCollider != null && worldCollider.CheckCollisionWithWorld;

        protected set
        {
            if (worldCollider != null)
                worldCollider.CheckCollisionWithWorld = value;
        }
    }

    public bool CheckCollisionWithSolidSprites
    {
        get => spriteCollider != null && spriteCollider.CheckCollisionWithSolidSprites;

        protected set
        {
            if (spriteCollider != null)
                spriteCollider.CheckCollisionWithSolidSprites = value;
        }
    }

    public bool CanBeCarriedWhenLanded
    {
        get;
        set;
    } = true;

    public bool CanBePushedHorizontally
    {
        get;
        set;
    } = true;

    public bool CanBePushedVertically
    {
        get;
        set;
    } = true;

    public bool AutoAdjustOnTheFloor
    {
        get;
        set;
    } = true;

    public bool BlockedUp => !NoClip && (CheckCollisionWithWorld && WorldCollider.BlockedUp || CheckCollisionWithSolidSprites && SpriteCollider.BlockedUp);

    public bool BlockedLeft => !NoClip && (CheckCollisionWithWorld && WorldCollider.BlockedLeft || CheckCollisionWithSolidSprites && SpriteCollider.BlockedLeft);

    public bool BlockedRight => !NoClip && (CheckCollisionWithWorld && WorldCollider.BlockedRight || CheckCollisionWithSolidSprites && SpriteCollider.BlockedRight);

    public bool TouchingLethalSpikeLeft => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingLethalSpikeLeft || CheckCollisionWithSolidSprites && SpriteCollider.TouchingLethalSpikeLeft);

    public bool TouchingNonLethalSpikeLeft => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingNonLethalSpikeLeft || CheckCollisionWithSolidSprites && SpriteCollider.TouchingNonLethalSpikeLeft);

    public bool TouchingLethalSpikeUp => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingLethalSpikeUp || CheckCollisionWithSolidSprites && SpriteCollider.TouchingLethalSpikeUp);

    public bool TouchingNonLethalSpikeUp => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingNonLethalSpikeUp || CheckCollisionWithSolidSprites && SpriteCollider.TouchingNonLethalSpikeUp);

    public bool TouchingLethalSpikeRight => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingLethalSpikeRight || CheckCollisionWithSolidSprites && SpriteCollider.TouchingLethalSpikeRight);

    public bool TouchingNonLethalSpikeRight => !NoClip && (CheckCollisionWithWorld && WorldCollider.TouchingNonLethalSpikeRight || CheckCollisionWithSolidSprites && SpriteCollider.TouchingNonLethalSpikeRight);

    public bool Landed => !NoClip && (CheckCollisionWithWorld && WorldCollider.Landed || CheckCollisionWithSolidSprites && SpriteCollider.Landed) && Velocity.Y >= 0;

    public bool LandedOnSlope => !NoClip && (CheckCollisionWithWorld && WorldCollider.LandedOnSlope || CheckCollisionWithSolidSprites && SpriteCollider.LandedOnSlope);

    public bool LandedOnTopLadder => !NoClip && CheckCollisionWithWorld && (WorldCollider.LandedOnTopLadder || CheckCollisionWithSolidSprites && SpriteCollider.LandedOnTopLadder);

    public bool LandedOnLethalSpike => !NoClip && (CheckCollisionWithWorld && WorldCollider.LandedOnLethalSpike || CheckCollisionWithSolidSprites && SpriteCollider.LandedOnLethalSpike);

    public bool LandedOnNonLethalSpike => !NoClip && (CheckCollisionWithWorld && WorldCollider.LandedOnNonLethalSpike || CheckCollisionWithSolidSprites && SpriteCollider.LandedOnNonLethalSpike);

    public RightTriangle LandedSlope => CheckCollisionWithWorld && WorldCollider.LandedOnSlope ? WorldCollider.LandedSlope : CheckCollisionWithSolidSprites && SpriteCollider.LandedOnSlope ? SpriteCollider.LandedSlope : RightTriangle.EMPTY;

    public bool Underwater => !NoClip && (CheckCollisionWithWorld && WorldCollider.Underwater || CheckCollisionWithSolidSprites && SpriteCollider.Underwater);

    public bool CanGoOutOfMapBounds
    {
        get;
        protected set;
    } = true;

    public string PaletteName
    {
        get => paletteName;

        set
        {
            paletteName = value;
            palette = paletteName == null ? null : Engine.GetPaletteByName(value);
        }
    }

    public Palette Palette
    {
        get
        {
            if (palette == null)
            {
                if (PaletteName != null)
                    palette = Engine.GetPaletteByName(PaletteName);
            }

            return palette;
        }

        set
        {
            palette = value;
            paletteName = value != null ? palette.Name : null;
        }
    }

    public string SpriteSheetName
    {
        get => spriteSheetName;

        protected set
        {
            spriteSheetName = value;
            spriteSheet = spriteSheetName == null ? null : Engine.GetSpriteSheetByName(value);
        }
    }

    public SpriteSheet SpriteSheet
    {
        get
        {
            if (spriteSheet == null)
            {
                if (SpriteSheetName != null)
                    spriteSheet = Engine.GetSpriteSheetByName(SpriteSheetName);
            }

            return spriteSheet;
        }

        set
        {
            spriteSheet = value;
            spriteSheetName = value != null ? spriteSheet.Name : null;
        }
    }

    public bool Animating
    {
        get;
        set;
    } = true;

    public bool KnockPlayerOnHurt
    {
        get;
        set;
    } = true;

    protected Sprite()
    {
        FadingControl = new FadingControl();

        touchingSpritesLeft = [];
        touchingSpritesRight = [];
        touchingSpritesUp = [];
        touchingSpritesDown = [];
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Animations = new AnimationFactory(this);
    }

    public void SetAnimationNames(params string[] animationNames)
    {
        (string name, int count, bool startWithCounterSuffix, int startCounterSuffix)[] @params = new (string, int, bool, int)[animationNames.Length];
        for (int i = 0; i < animationNames.Length; i++)
            @params[i] = (animationNames[i], 1, false, 1);

        SetAnimationNames(@params);
    }

    public void SetAnimationNames(params (string name, int count)[] animationNames)
    {
        (string name, int count, bool startWithCounterSuffix, int startCounterSuffix)[] @params = new (string, int, bool, int)[animationNames.Length];
        for (int i = 0; i < animationNames.Length; i++)
            @params[i] = (animationNames[i].name, animationNames[i].count, false, 1);

        SetAnimationNames(@params);
    }

    public void SetAnimationNames(params (string name, int count, bool startWithCounterSuffix, int startCounterSuffix)[] animationNames)
    {
        if (animationNames != null && animationNames.Length > 0)
        {
            this.animationNames = animationNames;
            InitialAnimationName = animationNames[0].name;
        }
        else
        {
            SetAnimationNames();
        }
    }

    public void SetAnimationNames()
    {
        var names = SpriteSheet.FrameSequenceNames;
        animationNames = new (string, int, bool, int)[names.Count()];

        int i = 0;
        foreach (var name in names)
            animationNames[i++] = (name, 1, false, 1);

        InitialAnimationName = null;
    }

    public int GetAnimationIndex(string animationName)
    {
        var animation = Animations[animationName];
        return animation != null ? animation.Index : -1;
    }

    protected override Type GetStateBaseType()
    {
        return typeof(SpriteState);
    }

    protected new StateClass RegisterState<StateClass>() where StateClass : SpriteState
    {
        return (StateClass) RegisterState(typeof(StateClass));
    }

    protected new StateClass RegisterState<StateClass>(int id) where StateClass : SpriteState
    {
        return (StateClass) RegisterState(id, typeof(StateClass));
    }

    protected SpriteState RegisterState(int id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int subStateCount, int animationIndex, int initialFrame = 0)
    {
        var state = (SpriteState) base.RegisterState(id, onStart, onFrame, onEnd, subStateCount);
        state.AnimationIndex = animationIndex;
        state.InitialFrame = initialFrame;
        return state;
    }

    protected SpriteState RegisterState(int id, EntityStateStartEvent onStart, int subStateCount, int animationIndex, int initialFrame = 0)
    {
        return RegisterState(id, onStart, null, null, subStateCount, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState(int id, EntityStateFrameEvent onFrame, int subStateCount, int animationIndex, int initialFrame = 0)
    {
        return RegisterState(id, null, onFrame, null, subStateCount, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState(int id, EntityStateEndEvent onEnd, int subStateCount, int animationIndex, int initialFrame = 0)
    {
        return RegisterState(id, null, null, onEnd, subStateCount, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState(int id, int subStateCount, int animationIndex, int initialFrame = 0)
    {
        return RegisterState(id, null, null, null, subStateCount, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, 0, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, 0, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, 0, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, 0, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, int animationIndex, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, int animationIndex, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
    }

    protected SpriteState RegisterState(int id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int subStateCount, string animationName, int initialFrame = 0)
    {
        var state = (SpriteState) base.RegisterState(id, onStart, onFrame, onEnd, subStateCount);
        state.AnimationName = animationName;
        state.InitialFrame = initialFrame;
        return state;
    }

    protected SpriteState RegisterState(int id, EntityStateStartEvent onStart, int subStateCount, string animationName, int initialFrame = 0)
    {
        return RegisterState(id, onStart, null, null, subStateCount, animationName, initialFrame);
    }

    protected SpriteState RegisterState(int id, EntityStateFrameEvent onFrame, int subStateCount, string animationName, int initialFrame = 0)
    {
        return RegisterState(id, null, onFrame, null, subStateCount, animationName, initialFrame);
    }

    protected SpriteState RegisterState(int id, EntityStateEndEvent onEnd, int subStateCount, string animationName, int initialFrame = 0)
    {
        return RegisterState(id, null, null, onEnd, subStateCount, animationName, initialFrame);
    }

    protected SpriteState RegisterState(int id, int subStateCount, string animationName, int initialFrame = 0)
    {
        return RegisterState(id, null, null, null, subStateCount, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, 0, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, 0, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, 0, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T>(T id, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, 0, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, string animationName, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
    }

    protected SpriteState RegisterState<T, U>(T id, string animationName, int initialFrame = 0) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
    }

    protected virtual bool OnCreateAnimation(string animationName, ref Vector offset, ref int repeatX, ref int repeatY, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn)
    {
        return true;
    }

    protected virtual void OnAnimationCreated(Animation animation)
    {
    }

    public override string ToString()
    {
        return $"{GetType().Name}[{Name}, {Origin}]";
    }

    public override Vector GetVector(VectorKind kind)
    {
        return kind switch
        {
            VectorKind.COLLISIONBOX_CENTER => CollisionBox.Center,
            _ => base.GetVector(kind)
        };
    }

    public override Vector GetLastVector(VectorKind kind)
    {
        return kind switch
        {
            VectorKind.COLLISIONBOX_CENTER => GetLastBox(BoxKind.COLLISIONBOX).Center,
            _ => base.GetLastVector(kind)
        };
    }

    public override Box GetBox(BoxKind kind)
    {
        return kind switch
        {
            BoxKind.COLLISIONBOX => CollisionBox,
            _ => base.GetBox(kind)
        };
    }

    protected override Box GetHitbox()
    {
        if (!MultiAnimation)
            return CurrentAnimation != null ? CurrentAnimation.CurrentFrameHitbox : Box.EMPTY_BOX;

        Box result = Box.EMPTY_BOX;
        foreach (var animation in Animations)
        {
            if (animation.Visible)
                result |= animation.CurrentFrameHitbox;
        }

        return result;
    }

    protected override Box GetTouchingBox()
    {
        return GetHitbox().ClipLeft(-STEP_SIZE).ClipTop(-STEP_SIZE).ClipRight(-STEP_SIZE).ClipBottom(-STEP_SIZE);
    }

    protected override Box GetDeadBox()
    {
        if (Animations.Count == 0)
            return base.GetDeadBox();

        Animation animation;
        Box drawBox = InitialAnimationName is not null and not "" && (animation = Animations[InitialAnimationName]) != null
            ? animation.DrawBox
            : InitialAnimationIndex >= 0 && InitialAnimationIndex < Animations.Count ? Animations[InitialAnimationIndex].DrawBox : Animations.Count > 0 ? Animations[0].DrawBox : Box.EMPTY_BOX;

        return (MirrorAnimationFromDirection && Direction != DefaultDirection ? drawBox.Mirror(Origin) : drawBox) - Origin;
    }

    protected virtual Box GetCollisionBox()
    {
        return GetHitbox();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Visible = true;
        Animating = true;
        CheckCollisionWithWorld = true;
        CheckCollisionWithSolidSprites = false;
        CollisionData = CollisionData.NONE;
        Velocity = Vector.NULL_VECTOR;
        ExternalVelocity = Vector.NULL_VECTOR;
        NoClip = false;
        moving = false;
        Static = false;
        breakable = true;
        health = DEFAULT_HEALTH;
        Invincible = false;
        Blinking = false;
        invincibilityFrames = DEFAULT_INVINCIBLE_TIME;
        broke = false;

        FadingControl.Reset();

        CurrentAnimationIndex = InitialAnimationIndex;
        CurrentAnimation?.StartFromBegin();
    }

    protected override void OnPostSpawn()
    {
        base.OnPostSpawn();

        if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
        {
            worldCollider.Box = CollisionBox;

            spriteCollider.ClearIgnoredSprites();
            spriteCollider.Box = Hitbox;

            if (BlockedUp)
            {
                OnBlockedUp();
                lastBlockedUp = true;
            }
            else
            {
                lastBlockedUp = false;
            }

            if (BlockedLeft)
            {
                OnBlockedLeft();
                lastBlockedLeft = true;
            }
            else
            {
                lastBlockedLeft = false;
            }

            if (BlockedRight)
            {
                OnBlockedRight();
                lastBlockedRight = true;
            }
            else
            {
                lastBlockedRight = false;
            }

            if (Landed)
            {
                OnLanded();
                lastLanded = true;
            }
            else
            {
                lastLanded = false;
            }
        }
    }

    protected virtual FixedSingle GetCollisionBoxHeadHeight()
    {
        return 0;
    }

    protected virtual FixedSingle GetCollisionBoxLegsHeight()
    {
        return 0;
    }

    protected virtual FixedSingle GetHitboxHeadHeight()
    {
        return 0;
    }

    protected virtual FixedSingle GetHitboxLegsHeight()
    {
        return 0;
    }

    protected virtual bool IsUsingCollisionPlacements()
    {
        return false;
    }

    protected virtual SpriteCollider CreateWorldCollider()
    {
        return new SpriteCollider(this, CollisionBox, GetCollisionBoxHeadHeight(), GetCollisionBoxLegsHeight(), IsUsingCollisionPlacements(), true, false);
    }

    protected virtual SpriteCollider CreateSpriteCollider()
    {
        return new SpriteCollider(this, Hitbox, GetHitboxHeadHeight(), GetHitboxLegsHeight(), IsUsingCollisionPlacements(), false, true);
    }

    internal void CreateResources()
    {
        OnCreateResources();
    }

    protected virtual void OnCreateResources()
    {
        if (ResourcesCreated)
            return;

        worldCollider = CreateWorldCollider();
        spriteCollider = CreateSpriteCollider();

        if ((animationNames == null || animationNames.Length == 0) && SpriteSheet != null && SpriteSheet.FrameSequenceCount > 0)
            SetAnimationNames();

        if (animationNames != null)
        {
            foreach (var (name, count, startWithCounterSuffix, startCounterSuffix) in animationNames)
            {
                SpriteSheet.FrameSequence sequence = SpriteSheet.GetFrameSequence(name);
                string frameSequenceName = sequence.Name;
                Vector offset = Vector.NULL_VECTOR;
                int repeatX = 1;
                int repeatY = 1;
                int initialFrame = 0;
                bool startVisible = false;
                bool startOn = true;

                bool add = OnCreateAnimation(frameSequenceName, ref offset, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn);
                if (add)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var animation = Animations.Create(frameSequenceName, offset, repeatX, repeatY, initialFrame, startVisible, startOn);
                        animation.Name = Animations.GetExclusiveName(name, startWithCounterSuffix, startCounterSuffix);
                        OnAnimationCreated(animation);
                    }
                }
            }
        }

        ResourcesCreated = true;
    }

    public override void Spawn()
    {
        base.Spawn();
        CreateResources();
    }

    public override void Place(bool respawnable = true, Direction spawnOnlyOnDirection = Direction.BOTH)
    {
        CreateResources();
        base.Place(respawnable, spawnOnlyOnDirection);

        if (IsInSpawnArea(VectorKind.ORIGIN))
            Spawn();
    }

    public void BringToFront()
    {
        int count = this is HUD.HUD ? Engine.GetHUDs(Layer).Count : Engine.GetSprites(Layer).Count;
        Priority = count - 1;
    }

    public void SendToBack()
    {
        Priority = 0;
    }

    protected virtual bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        return !Invincible && !NoClip;
    }

    protected virtual void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
    {
        TakeDamageEvent?.Invoke(this, attacker, damage);
    }

    protected virtual void OnHealthChanged(FixedSingle health)
    {
        HealthChangedEvent?.Invoke(this, health);
    }

    protected virtual void OnHurt(Sprite victim, FixedSingle damage)
    {
        HurtEvent?.Invoke(this, victim, damage);
    }

    public void Hurt(Sprite victim, FixedSingle damage)
    {
        if (!Alive || MarkedToRemove)
            return;

        if (!victim.Alive || victim.broke || victim.MarkedToRemove || victim.health <= 0)
            return;

        if (victim.OnTakeDamage(this, ref damage))
        {
            FixedSingle h = victim.health;
            h -= damage;

            if (h < 0)
                h = 0;

            victim.health = h;
            victim.OnHealthChanged(h);
            victim.OnTakeDamagePost(this, damage);

            OnHurt(victim, damage);

            if (victim.health == 0)
                victim.Break();
        }
    }

    public void MakeInvincible(int frames = 0, bool blink = false)
    {
        invincible = true;
        invincibilityFrames = frames;
        invincibilityFrameCounter = 0;

        if (blink)
            MakeBlinking(frames);
    }

    public void MakeBlinking(int frames = 0)
    {
        blinking = true;
        blinkFrameCounter = 0;
        blinkFrames = frames;
    }

    protected virtual bool ShouldCollide(Sprite sprite)
    {
        return false;
    }

    protected override bool CheckTouching(Entity entity)
    {
        if (CollisionData.IsSolidBlock() && entity is Sprite sprite && sprite.Alive && sprite.CheckCollisionWithSolidSprites)
        {
            Box box = Hitbox.RoundOriginToFloor();
            SpriteCollider collider = sprite.SpriteCollider;

            if (box.IsOverlaping(collider.UpCollider - (0, STEP_SIZE)))
                touchingSpritesDown.Add(sprite);

            if (box.IsOverlaping(collider.RightCollider + (STEP_SIZE, 0)))
                touchingSpritesLeft.Add(sprite);

            if (box.IsOverlaping(collider.LeftCollider - (STEP_SIZE, 0)))
                touchingSpritesRight.Add(sprite);

            if (box.IsOverlaping(collider.DownCollider + (0, STEP_SIZE)))
                touchingSpritesUp.Add(sprite);
        }

        return base.CheckTouching(entity);
    }

    private static void MoveAlongSlope(SpriteCollider collider, RightTriangle slope, FixedSingle dx, bool gravity, bool autoAdjustOnTheFloor)
    {
        var lastLeftCollider = collider.LeftCollider;
        var lastRightCollider = collider.RightCollider;

        var h = slope.HCathetusVector.X;
        int slopeSign = h.Signal;
        int dxs = dx.Signal;
        bool goingDown = dxs == slopeSign;

        var dy = (FixedSingle) (((FixedDouble) slope.VCathetus * dx / slope.HCathetus).Abs * dxs * slopeSign);
        Vector delta = (dx, dy);

        collider.Translate(delta);

        var boxCollider = dx > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
        var collisionFlags = Engine.World.GetCollisionFlags(boxCollider, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites);

        if (collisionFlags.CanBlockTheMove(delta.GetDirection()))
        {
            collider.Translate((delta.X > 0 ? (-delta.X).Floor() : (-delta.X).Ceil(), delta.Y > 0 ? (-delta.Y).Floor() : (-delta.Y).Ceil()));
            collider.MoveContactSolid(delta, dx.Abs.Ceil(), (goingDown ? Direction.NONE : Direction.UP) | (dxs > 0 ? Direction.RIGHT : Direction.LEFT), CollisionFlags.SLOPE);
        }

        if (gravity)
            collider.TryMoveContactFloor();

        if (collider.Landed && autoAdjustOnTheFloor)
            collider.AdjustOnTheFloor();
    }

    // TODO : Slope collision detection inside this method must be refined and some trash code should be removed.
    private static void MoveX(SpriteCollider collider, FixedSingle dx, bool gravity, bool followSlopes, bool autoAdjustOnTheFloor)
    {
        Vector delta = (dx, 0);

        var lastBox = collider.Box;
        bool wasLanded = collider.Landed;
        bool wasLandedOnSlope = collider.LandedOnSlope;
        var lastSlope = collider.LandedSlope;
        var lastLeftCollider = collider.LeftCollider;
        var lastRightCollider = collider.RightCollider;

        collider.Translate(delta);

        var boxCollider = dx > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
        var collisionFlags = Engine.World.GetCollisionFlags(boxCollider, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites);

        if (collisionFlags.CanBlockTheMove(delta.GetDirection()))
        {
            if (collisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (collider.LandedOnSlope)
                {
                    var slope = collider.LandedSlope;
                    var x = lastBox.Origin.X;
                    var stx = dx > 0 ? slope.Left : slope.Right;
                    var stx_x = stx - x;
                    if (dx > 0 && stx_x < 0 && stx_x >= dx || dx < 0 && stx_x > 0 && stx_x <= dx)
                    {
                        dx -= stx_x;
                        delta = (dx, 0);

                        if (delta.X > 0)
                            collider.Translate((-delta).RoundToFloor());
                        else
                            collider.Translate((-delta).RoundToCeil());

                        if (wasLandedOnSlope)
                            MoveAlongSlope(collider, lastSlope, stx_x, gravity, autoAdjustOnTheFloor);
                        else
                            collider.Translate((stx_x, 0));

                        MoveAlongSlope(collider, slope, dx, gravity, autoAdjustOnTheFloor);
                    }
                    else if (wasLandedOnSlope)
                    {
                        if (delta.X > 0)
                            collider.Translate((-delta).RoundToFloor());
                        else
                            collider.Translate((-delta).RoundToCeil());

                        MoveAlongSlope(collider, lastSlope, dx, gravity, autoAdjustOnTheFloor);
                    }
                }
                else if (!wasLanded)
                {
                    if (dx > 0)
                    {
                        collider.Translate((-delta).RoundToFloor());
                        collider.MoveContactSolid(delta, dx.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
                    }
                    else
                    {
                        collider.Translate((-delta).RoundToCeil());
                        collider.MoveContactSolid(delta, (-dx).Ceil(), Direction.LEFT, CollisionFlags.NONE);
                    }
                }
            }
            else if (dx > 0)
            {
                collider.Translate((-delta).RoundToFloor());
                collider.MoveContactSolid(delta, dx.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
            }
            else
            {
                collider.Translate((-delta).RoundToCeil());
                collider.MoveContactSolid(delta, (-dx).Ceil(), Direction.LEFT, CollisionFlags.NONE);
            }
        }
        else if (gravity && followSlopes && wasLanded)
        {
            if (collider.LandedOnSlope)
            {
                var slope = collider.LandedSlope;
                var h = slope.HCathetusVector.X;
                if (h > 0 && dx > 0 || h < 0 && dx < 0)
                {
                    var x = lastBox.Origin.X;
                    var stx = dx > 0 ? slope.Left : slope.Right;
                    var stx_x = stx - x;
                    if (dx > 0 && stx_x > 0 && stx_x <= dx || dx < 0 && stx_x < 0 && stx_x >= dx)
                    {
                        dx -= stx_x;
                        delta = (dx, 0);

                        if (delta.X > 0)
                            collider.Translate((-delta).RoundToFloor());
                        else
                            collider.Translate((-delta).RoundToCeil());

                        if (wasLandedOnSlope)
                            MoveAlongSlope(collider, lastSlope, stx_x, gravity, autoAdjustOnTheFloor);
                        else
                            collider.Translate((stx_x, 0));

                        MoveAlongSlope(collider, slope, dx, gravity, autoAdjustOnTheFloor);
                    }
                    else
                    {
                        if (wasLandedOnSlope)
                        {
                            if (delta.X > 0)
                                collider.Translate((-delta).RoundToFloor());
                            else
                                collider.Translate((-delta).RoundToCeil());

                            MoveAlongSlope(collider, lastSlope, dx, gravity, autoAdjustOnTheFloor);
                        }
                    }
                }
                else
                {
                    if (wasLandedOnSlope)
                    {
                        if (delta.X > 0)
                            collider.Translate((-delta).RoundToFloor());
                        else
                            collider.Translate((-delta).RoundToCeil());

                        MoveAlongSlope(collider, lastSlope, dx, gravity, autoAdjustOnTheFloor);
                    }
                }
            }
            else if (Engine.World.GetCollisionFlags(collider.DownCollider, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites).HasFlag(CollisionFlags.SLOPE))
            {
                collider.TryMoveContactFloor();
            }
        }
    }

    private static void MoveX(SpriteCollider collider, Vector delta, bool gravity, bool wasLanded, bool autoAdjustOnTheFloor)
    {
        var dx = delta.X;

        if (collider.LandedOnSlope)
        {
            if (collider.LandedSlope.HCathetusSign == dx.Signal)
            {
                if (gravity)
                    MoveAlongSlope(collider, collider.LandedSlope, dx, gravity, autoAdjustOnTheFloor);
            }
            else
            {
                MoveAlongSlope(collider, collider.LandedSlope, dx, gravity, autoAdjustOnTheFloor);
            }
        }
        else
        {
            MoveX(collider, dx, gravity, true, autoAdjustOnTheFloor);
        }

        if (delta.Y >= 0)
        {
            if (collider.Landed)
            {
                if (autoAdjustOnTheFloor)
                    collider.AdjustOnTheFloor();
            }
            else if (gravity && wasLanded)
                collider.TryMoveContactSlope(QUERY_MAX_DISTANCE * FixedSingle.HALF);
        }
    }

    private static void MoveY(SpriteCollider collider, Vector delta)
    {
        var deltaY = delta.YVector;
        var lastUpCollider = collider.UpCollider;
        var lastDownCollider = collider.DownCollider;
        collider.Translate(deltaY);

        var union = deltaY.Y > 0 ? lastDownCollider | collider.DownCollider : lastUpCollider | collider.UpCollider;

        if (Engine.World.GetCollisionFlags(union, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites).CanBlockTheMove(deltaY.GetDirection()))
        {
            if (deltaY.Y > 0)
            {
                collider.Translate((-deltaY).RoundToFloor());
                collider.MoveContactFloor(deltaY.Y.Ceil());
            }
            else
            {
                collider.Translate((-deltaY).RoundToCeil());
                collider.MoveContactSolid(deltaY, (-deltaY.Y).Ceil(), Direction.UP);
            }
        }
    }

    private Vector Move(SpriteCollider collider, Vector delta, bool gravity, bool autoAdjustOnTheFloor)
    {
        var lastBoxOrigin = collider.Box.Origin;
        bool wasLanded = collider.Landed;

        if (delta.Y > 0)
        {
            if (delta.X != 0)
                MoveX(collider, delta, gravity, wasLanded, autoAdjustOnTheFloor);

            MoveY(collider, delta);
        }
        else if (delta.Y < 0)
        {
            MoveY(collider, delta);

            if (delta.X != 0)
                MoveX(collider, delta, gravity, wasLanded, autoAdjustOnTheFloor);
        }
        else if (delta.X != 0)
        {
            MoveX(collider, delta, gravity, wasLanded, autoAdjustOnTheFloor);
        }

        return collider.Box.Origin - lastBoxOrigin;
    }

    protected virtual void OnBeforeMove(ref Vector origin)
    {
        BeforeMoveEvent?.Invoke(this);
    }

    public bool MoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        bool worldContact = false;
        bool spriteContact = false;
        FixedSingle worldDelta = maxDistance;
        FixedSingle spriteDelta = maxDistance;

        if (world && CheckCollisionWithWorld)
        {
            var lastBox = CollisionBox;
            worldCollider.Box = lastBox;
            worldContact = worldCollider.MoveContactFloor(maxDistance, ignore);

            worldDelta = worldCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        if (sprite && CheckCollisionWithSolidSprites)
        {
            var lastBox = Hitbox;
            spriteCollider.ClearIgnoredSprites();
            spriteCollider.Box = lastBox;
            spriteContact = spriteCollider.MoveContactFloor(maxDistance, ignore);

            spriteDelta = spriteCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        FixedSingle delta = worldContact ? spriteContact ? FixedSingle.Min(worldDelta, spriteDelta) : worldDelta : spriteContact ? spriteDelta : 0;
        if (delta != 0)
            Origin += (0, delta);

        return worldContact || spriteContact;
    }

    public bool MoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        return MoveContactFloor(QUERY_MAX_DISTANCE, ignore, world, sprite);
    }

    public bool TryMoveContactFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        bool worldContact = false;
        bool spriteContact = false;
        FixedSingle worldDelta = maxDistance;
        FixedSingle spriteDelta = maxDistance;

        if (world && CheckCollisionWithWorld)
        {
            var lastBox = CollisionBox;
            worldCollider.Box = lastBox;
            worldContact = worldCollider.TryMoveContactFloor(maxDistance, ignore);

            worldDelta = worldCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        if (sprite && CheckCollisionWithSolidSprites)
        {
            var lastBox = Hitbox;
            spriteCollider.ClearIgnoredSprites();
            spriteCollider.Box = lastBox;
            spriteContact = spriteCollider.TryMoveContactFloor(maxDistance, ignore);

            spriteDelta = spriteCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        FixedSingle delta = worldContact ? spriteContact ? FixedSingle.Min(worldDelta, spriteDelta) : worldDelta : spriteContact ? spriteDelta : 0;
        if (delta != 0)
            Origin += (0, delta);

        return worldContact || spriteContact;
    }

    public bool TryMoveContactFloor(CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        return TryMoveContactFloor(QUERY_MAX_DISTANCE * FixedSingle.HALF, ignore, world, sprite);
    }

    public bool AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        bool worldContact = false;
        bool spriteContact = false;
        FixedSingle worldDelta = 0;
        FixedSingle spriteDelta = 0;

        if (world && CheckCollisionWithWorld)
        {
            var lastBox = CollisionBox;
            worldCollider.Box = lastBox;
            worldContact = worldCollider.AdjustOnTheFloor(maxDistance, ignore);

            worldDelta = worldCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        if (sprite && CheckCollisionWithSolidSprites)
        {
            var lastBox = Hitbox;
            spriteCollider.ClearIgnoredSprites();
            spriteCollider.Box = lastBox;
            spriteContact = spriteCollider.AdjustOnTheFloor(maxDistance, ignore);

            spriteDelta = spriteCollider.Box.Origin.Y - lastBox.Origin.Y;
        }

        FixedSingle delta = worldContact ? spriteContact ? FixedSingle.Max(worldDelta, spriteDelta) : worldDelta : spriteContact ? spriteDelta : 0;
        if (delta != 0)
            Origin += (0, delta);

        return worldContact || spriteContact;
    }

    public bool AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE, bool world = true, bool sprite = true)
    {
        return AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore, world, sprite);
    }

    protected void Clamp(Box limitBox, ref Vector origin)
    {
        var delta = origin - Origin;

        var hitbox = Hitbox;
        var box = limitBox.RestrictIn(hitbox + delta);
        var newDelta = box.Origin - hitbox.Origin;

        if (delta != newDelta)
            origin += newDelta - delta;
    }

    private Vector DoPhysics(Sprite physicsParent, Vector delta)
    {
        if (Static)
            return Vector.NULL_VECTOR;

        Vector lastOrigin = Origin;

        if (!NoClip)
        {
            var gravity = Gravity;

            if (CheckCollisionWithWorld)
            {
                worldCollider.Box = CollisionBox;
                delta = Move(worldCollider, delta, gravity != 0, AutoAdjustOnTheFloor);
            }

            if (CheckCollisionWithSolidSprites)
            {
                spriteCollider.ClearIgnoredSprites();

                if (physicsParent != null)
                    spriteCollider.IgnoreSprites.Add(physicsParent);

                spriteCollider.Box = Hitbox;
                delta = Move(spriteCollider, delta, gravity != 0, AutoAdjustOnTheFloor);
            }
        }

        if (delta == Vector.NULL_VECTOR)
        {
            if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
            {
                if (Landed && !BlockedUp && AutoAdjustOnTheFloor)
                    AdjustOnTheFloor();
                //else
                //    TryMoveContactFloor();
            }

            return Vector.NULL_VECTOR;
        }

        var newOrigin = Origin + delta;

        if (!CanGoOutOfMapBounds)
            Clamp(Engine.World.ForegroundLayout.BoundingBox.ClipTop(-2 * BLOCK_SIZE).ClipBottom(-2 * BLOCK_SIZE), ref newOrigin);

        OnBeforeMove(ref newOrigin);
        Origin = newOrigin;

        if (lastOrigin != Origin)
            StartMoving();
        else if (moving)
            StopMoving();

        delta = Origin - lastOrigin;
        var deltaX = delta.X > 0 ? delta.X.Ceil() : delta.X.Floor();
        var deltaY = delta.Y > 0 ? delta.Y.Ceil() : delta.Y.Floor();

        if (CollisionData.IsSolidBlock())
        {
            if (deltaX < 0)
            {
                foreach (var sprite in touchingSpritesLeft)
                {
                    if (sprite != physicsParent && sprite.CanBePushedHorizontally)
                        sprite.DoPhysics(this, (deltaX, 0));
                }
            }
            else if (deltaX > 0)
            {
                foreach (var sprite in touchingSpritesRight)
                {
                    if (sprite != physicsParent && sprite.CanBePushedHorizontally)
                        sprite.DoPhysics(this, (deltaX, 0));
                }
            }

            if (deltaY > 0)
            {
                foreach (var sprite in touchingSpritesDown)
                {
                    if (sprite != physicsParent && sprite.CanBePushedVertically)
                        sprite.DoPhysics(this, (0, deltaY));
                }
            }

            if (delta != Vector.NULL_VECTOR)
            {
                foreach (var sprite in touchingSpritesUp)
                {
                    if (sprite != physicsParent && sprite.CanBeCarriedWhenLanded)
                    {
                        sprite.DoPhysics(this, delta);
                        sprite.TryMoveContactFloor();

                        if (sprite.Landed && !sprite.BlockedUp && sprite.AutoAdjustOnTheFloor)
                            sprite.AdjustOnTheFloor();
                    }
                }
            }
        }

        if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
        {
            if (CheckCollisionWithWorld)
                worldCollider.Box = CollisionBox;

            if (CheckCollisionWithSolidSprites)
            {
                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;
            }

            if (BlockedUp)
            {
                if (!lastBlockedUp)
                {
                    OnBlockedUp();
                    lastBlockedUp = true;
                }
            }
            else
            {
                lastBlockedUp = false;
            }

            if (BlockedLeft)
            {
                if (!lastBlockedLeft)
                {
                    OnBlockedLeft();
                    lastBlockedLeft = true;
                }
            }
            else
            {
                lastBlockedLeft = false;
            }

            if (BlockedRight)
            {
                if (!lastBlockedRight)
                {
                    OnBlockedRight();
                    lastBlockedRight = true;
                }
            }
            else
            {
                lastBlockedRight = false;
            }

            if (Landed)
            {
                if (!lastLanded)
                {
                    OnLanded();
                    lastLanded = true;
                }
            }
            else
            {
                lastLanded = false;
            }
        }

        return delta;
    }

    public virtual FixedSingle GetGravity()
    {
        return Underwater ? UNDERWATER_GRAVITY : GRAVITY;
    }

    protected virtual FixedSingle GetTerminalDownwardSpeed()
    {
        return Underwater ? UNDERWATER_TERMINAL_DOWNWARD_SPEED : TERMINAL_DOWNWARD_SPEED;
    }

    protected virtual void OnBlockedRight()
    {
        BlockedRightEvent?.Invoke(this);
    }

    protected virtual void OnBlockedLeft()
    {
        BlockedLeftEvent?.Invoke(this);
    }

    protected virtual void OnBlockedUp()
    {
        BlockedUpEvent?.Invoke(this);
    }

    protected virtual void OnLanded()
    {
        LandedEvent?.Invoke(this);
    }

    // TODO : Implement call to this
    protected bool CollisionCheck(Sprite sprite)
    {
        return ShouldCollide(sprite) && sprite.ShouldCollide(this);
    }

    public Animation GetAnimation(int index)
    {
        return Animations[index];
    }

    public int GetAnimationIndexByName(string name)
    {
        Animation animation = GetAnimationByName(name);
        return animation != null ? animation.Index : -1;
    }

    public AnimationReference GetAnimationByName(string name)
    {
        return Animations[name];
    }

    public void SetCurrentAnimationByIndex(int index, int initialFrame = -1)
    {
        var lastAnimation = CurrentAnimation;

        var animation = Animations[index];
        CurrentAnimation = animation;

        if (animation == null)
            return;

        switch (initialFrame)
        {
            case -1: // current animation will continue from the current frame from the last animation
                initialFrame = lastAnimation != null ? lastAnimation.CurrentFrame : 0;
                break;

            case -2: // current animation will continue from the current frame from the current animation
                initialFrame = animation.CurrentFrame;
                break;

            default: // current animation will continue from the specified frame
                if (initialFrame < 0)
                    throw new ArgumentException($"Invalid negative initial frame '{initialFrame}'.");

                break;
        }

        animation.Start(initialFrame);
    }

    public void SetCurrentAnimationByName(string name, int initialFrame = -1)
    {
        var lastAnimation = CurrentAnimation;

        var animation = Animations[name];
        CurrentAnimation = animation;

        if (animation == null)
            return;

        switch (initialFrame)
        {
            case -1: // current animation will continue from the current frame from the last animation
                initialFrame = lastAnimation != null ? lastAnimation.CurrentFrame : 0;
                break;

            case -2: // current animation will continue from the current frame from the current animation
                initialFrame = animation.CurrentFrame;
                break;

            default: // current animation will continue from the specified frame
                if (initialFrame < 0)
                    throw new ArgumentException($"Invalid negative initial frame '{initialFrame}'.");

                break;
        }

        animation.Start(initialFrame);
    }

    public void SetAnimationsVisibility(string name, bool visible)
    {
        var animation = Animations[name];
        if (animation == null)
            return;

        animation.Visible = visible;
    }

    public void SetAnimationsVisibilityExclusively(string name, bool visible)
    {
        foreach (var animation in Animations)
            animation.Visible = animation.Name == name ? visible : !visible;
    }

    protected override bool OnPreThink()
    {
        if (!Engine.Paused)
        {
            touchingSpritesLeft.Clear();
            touchingSpritesRight.Clear();
            touchingSpritesUp.Clear();
            touchingSpritesDown.Clear();
        }

        return base.OnPreThink();
    }

    protected override void OnThink()
    {
    }

    protected virtual void OnPostRender()
    {
        InvisibleOnNextFrame = false;

        if (Blinking)
        {
            OnBlink(blinkFrameCounter);

            blinkFrameCounter++;

            if (blinkFrames > 0 && blinkFrameCounter >= blinkFrames)
            {
                Blinking = false;
                OnEndBlinking();
            }
        }

        if (Animating)
        {
            foreach (Animation animation in Animations)
                animation.NextFrame();
        }
    }

    protected virtual void DoPhysics()
    {
        if (Static)
            return;

        bool landed = false;
        bool blockedUp = false;
        bool blockedLeft = false;
        bool blockedRight = false;

        if (!NoClip)
        {
            if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
            {
                landed = Landed;
                blockedUp = BlockedUp;
            }

            FixedSingle gravity = Gravity;

            if (!(landed && Velocity.Y < 0) && gravity != 0)
            {
                Velocity += gravity * Vector.DOWN_VECTOR;

                FixedSingle terminalDownwardSpeed = TerminalDownwardSpeed;
                if (Velocity.Y > terminalDownwardSpeed)
                    Velocity = (Velocity.X, terminalDownwardSpeed);
            }

            if (landed && Velocity.Y > 0 || blockedUp && Velocity.Y < 0)
                Velocity = Velocity.XVector;

            if (Velocity.Y > gravity && Velocity.Y < 2 * gravity)
                Velocity = (Velocity.X, gravity);
        }

        Vector vel = Velocity + (!NoClip ? ExternalVelocity : Vector.NULL_VECTOR);
        ExternalVelocity = Vector.NULL_VECTOR;

        Vector delta = DoPhysics(null, vel);

        if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
        {
            blockedLeft = BlockedLeft;
            blockedRight = BlockedRight;
        }

        if (delta.X.Abs < STEP_SIZE && delta.Y.Abs < STEP_SIZE && (blockedLeft && Velocity.X < 0 || blockedRight && Velocity.X > 0))
            Velocity = Velocity.YVector;
    }

    protected override void OnPostThink()
    {
        base.OnPostThink();

        if (!Alive || MarkedToRemove || Engine.Paused)
            return;

        if (Invincible)
        {
            invincibilityFrameCounter++;

            if (invincibilityFrames > 0 && invincibilityFrameCounter >= invincibilityFrames)
            {
                Invincible = false;
                OnEndInvincibility();
            }
        }

        OnPostRender();
        DoPhysics();
    }

    protected virtual void OnBlink(int frameCounter)
    {
        InvisibleOnNextFrame = frameCounter % 2 == 0;
    }

    protected virtual void OnEndInvincibility()
    {
    }

    protected virtual void OnEndBlinking()
    {
    }

    public virtual void Render(IRenderTarget target)
    {
        if (Engine.Editing)
        {
            Animation animation = null;
            if (InitialAnimationName is not null and not "")
                animation = Animations[InitialAnimationName];
            else if (InitialAnimationIndex >= 0 && InitialAnimationIndex < Animations.Count)
                animation = Animations[InitialAnimationIndex];
            else if (Animations.Count > 0)
                animation = Animations[0];

            animation?.Render(target);
        }
        else if (Alive && !MarkedToRemove && Visible && !InvisibleOnNextFrame)
        {
            if (MultiAnimation)
            {
                foreach (var animation in Animations)
                    animation.Render(target);
            }
            else
            {
                var animation = CurrentAnimation;
                animation?.Render(target);
            }
        }
    }

    internal void NotifyAnimationEnd(Animation animation)
    {
        OnAnimationEnd(animation);
    }

    protected virtual void OnAnimationEnd(Animation animation)
    {
        AnimationEndEvent?.Invoke(this, animation);
    }

    private void StartMoving()
    {
        if (moving)
            return;

        moving = true;
        OnStartMoving();
    }

    private void StopMoving()
    {
        if (!moving)
            return;

        moving = false;
        OnStopMoving();
    }

    protected virtual void OnStartMoving()
    {
        StartMovingEvent?.Invoke(this);
    }

    protected virtual void OnStopMoving()
    {
        StopMovingEvent?.Invoke(this);
    }

    protected virtual bool OnBreak()
    {
        return true;
    }

    protected virtual void OnBroke()
    {
        BrokeEvent?.Invoke(this);
    }

    public void Break()
    {
        if (Alive && !broke && !MarkedToRemove && breakable && OnBreak())
        {
            health = 0;
            broke = true;
            OnBroke();
            Kill();
        }
    }
    protected override void OnCleanup()
    {
        base.OnCleanup();

        CurrentAnimationIndex = -1;
        moving = false;
        invincible = false;
        invincibilityFrames = -1;
        invincibilityFrameCounter = 0;
        blinking = false;
        blinkFrames = -1;
        blinkFrameCounter = 0;
        NoClip = false;
        InvisibleOnNextFrame = false;
        FadingControl.Reset();
    }

    internal void OnDeviceReset()
    {
        palette = null;
        spriteSheet = null;

        foreach (var animation in Animations)
            animation.OnDeviceReset();
    }

    public void FaceToPosition(Vector pos)
    {
        var faceDirection = GetHorizontalDirection(pos);
        Direction = faceDirection;
    }

    public void FaceToEntity(Entity entity)
    {
        FaceToPosition(entity.Origin);
    }

    public void FaceToPlayer()
    {
        if (Engine.Player != null)
            FaceToEntity(Engine.Player);
    }

    public void FaceToScreenCenter()
    {
        FaceToPosition(Engine.Camera.Origin);
    }

    public void ResetExternalVelocity()
    {
        ExternalVelocity = Vector.NULL_VECTOR;
    }

    public void AddExternalVelocity(Vector velocity)
    {
        ExternalVelocity += velocity;
    }

    public void SubtractExternalVelocity(Vector velocity)
    {
        ExternalVelocity -= velocity;
    }
}