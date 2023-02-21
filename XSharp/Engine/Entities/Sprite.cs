using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using XSharp.Engine.Graphics;
using XSharp.Engine.World;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;
using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.Entities
{
    public delegate void SpriteEvent(Sprite source);
    public delegate void TakeDamageEvent(Sprite source, Sprite attacker, FixedSingle damage);
    public delegate void HurtEvent(Sprite source, Sprite victim, FixedSingle damage);
    public delegate void HealthChangedEvent(Sprite source, FixedSingle health);
    public delegate void AnimationEndEvent(Sprite source, Animation animation);

    public abstract class Sprite : Entity
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

        private bool visible = true;
        private int layer = 0;
        private List<Animation> animations;
        private int currentAnimationIndex = -1;

        protected SpriteCollider worldCollider;
        protected SpriteCollider spriteCollider;

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

        private EntityList<Sprite> touchingSpritesLeft;
        private EntityList<Sprite> touchingSpritesUp;
        private EntityList<Sprite> touchingSpritesRight;
        private EntityList<Sprite> touchingSpritesDown;

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
                if (Alive && layer != value)
                    Engine.UpdateSpriteLayer(this, value);

                layer = value;
            }
        }

        public bool ResourcesCreated
        {
            get;
            private set;
        } = false;

        private readonly Dictionary<string, List<Animation>> animationsByName;

        public SpriteSheet SpriteSheet => Engine.GetSpriteSheet(SpriteSheetIndex);

        public int SpriteSheetIndex
        {
            get;
            set;
        } = -1;

        public string SpriteSheetName
        {
            get
            {
                var spriteSheet = Engine.GetSpriteSheet(SpriteSheetIndex);
                return spriteSheet?.Name;
            }

            set
            {
                var spriteSheet = Engine.GetSpriteSheetByName(value);
                SpriteSheetIndex = spriteSheet != null ? spriteSheet.Index : -1;
            }
        }

        public bool Directional
        {
            get;
            set;
        } = false;

        public Direction Direction
        {
            get;
            set;
        } = Direction.RIGHT;

        public Direction DefaultDirection
        {
            get;
            set;
        } = Direction.RIGHT;

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

        public int InitialAnimationIndex => InitialAnimationName != null ? GetAnimationIndex(InitialAnimationName) : -1;

        new public SpriteState CurrentState => (SpriteState) base.CurrentState;

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

        public FadingControl FadingSettings
        {
            get;
        }

        public IEnumerable<Animation> Animations => animations;

        public bool InvisibleOnCurrentFrame
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

        public virtual Box BoundingBox
        {
            get
            {
                Box box = Origin + GetBoundingBox();
                return Directional && Direction != DefaultDirection ? box.Mirror(Origin) : box;
            }

            protected set
            {
                Box collisionBox = Directional && Direction != DefaultDirection ? value.Mirror(Origin) : value;
                collisionBox -= Origin;
                SetBoundingBox(collisionBox);
                UpdatePartition();
            }
        }

        public override Box Hitbox
        {
            get
            {
                Box box = Origin + (!Alive && Respawnable ? GetDeadBox() : GetHitbox());
                return Directional && Direction != DefaultDirection ? box.Mirror(Origin) : box;
            }

            protected set
            {
                Box hitbox = Directional && Direction != DefaultDirection ? value.Mirror(Origin) : value;
                hitbox -= Origin;
                SetHitbox(hitbox);
            }
        }

        public override Box TouchingBox
        {
            get
            {
                Box box = Origin + GetTouchingBox();
                return Directional && Direction != DefaultDirection ? box.Mirror(Origin) : box;
            }
        }

        public virtual Box CollisionBox
        {
            get
            {
                Box box = Origin + GetCollisionBox();
                return Directional && Direction != DefaultDirection ? box.Mirror(Origin) : box;
            }
        }

        public SpriteCollider WorldCollider
        {
            get
            {
                worldCollider.Box = CollisionBox;
                return worldCollider;
            }
        }

        public SpriteCollider SpriteCollider
        {
            get
            {
                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;
                return spriteCollider;
            }
        }

        public FixedSingle Gravity => GetGravity();

        public FixedSingle TerminalDownwardSpeed => GetTerminalDownwardSpeed();

        public bool CheckCollisionWithWorld
        {
            get => worldCollider.CheckCollisionWithWorld;
            protected set => worldCollider.CheckCollisionWithWorld = value;
        }

        public bool CheckCollisionWithSolidSprites
        {
            get => spriteCollider.CheckCollisionWithSolidSprites;
            protected set => spriteCollider.CheckCollisionWithSolidSprites = value;
        }

        public bool BlockedUp => !NoClip && (worldCollider.BlockedUp || spriteCollider.BlockedUp);

        public bool BlockedLeft => !NoClip && (worldCollider.BlockedLeft || spriteCollider.BlockedLeft);

        public bool BlockedRight => !NoClip && (worldCollider.BlockedRight || spriteCollider.BlockedRight);

        public bool Landed => !NoClip && (worldCollider.Landed || spriteCollider.Landed) && Velocity.Y >= 0;

        public bool LandedOnSlope => !NoClip && (worldCollider.LandedOnSlope || spriteCollider.LandedOnSlope);

        public bool LandedOnTopLadder => !NoClip && (worldCollider.LandedOnTopLadder || spriteCollider.LandedOnTopLadder);

        public RightTriangle LandedSlope => worldCollider.LandedOnSlope ? worldCollider.LandedSlope : spriteCollider.LandedSlope;

        public bool Underwater => !NoClip && (worldCollider.Underwater || spriteCollider.Underwater);

        public bool CanGoOutOfMapBounds
        {
            get;
            protected set;
        } = true;

        public int PaletteIndex
        {
            get;
            protected set;
        } = -1;

        public Texture Palette => Engine.GetPalette(PaletteIndex);

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
            animations = new List<Animation>();
            animationsByName = new Dictionary<string, List<Animation>>();

            FadingSettings = new FadingControl();

            touchingSpritesLeft = new EntityList<Sprite>();
            touchingSpritesRight = new EntityList<Sprite>();
            touchingSpritesUp = new EntityList<Sprite>();
            touchingSpritesDown = new EntityList<Sprite>();
        }

        public void SetAnimationNames(params string[] animationNames)
        {
            if (animationNames != null && animationNames.Length > 0)
            {
                foreach (var animationName in animationNames)
                    animationsByName.Add(animationName, new());

                InitialAnimationName = animationNames[0];
            }
            else
            {
                foreach (var frameSequenceName in SpriteSheet.FrameSequenceNames)
                    animationsByName.Add(frameSequenceName, new());

                InitialAnimationName = null;
            }
        }

        public int GetAnimationIndex(string animationName)
        {
            return animationsByName.TryGetValue(animationName, out List<Animation> animations) ? animations.Count > 0 ? animations[0].Index : -1 : -1;
        }

        protected override Type GetStateType()
        {
            return typeof(SpriteState);
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

        protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, 0, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, 0, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, 0, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null, 0, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, int animationIndex, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, int animationIndex, int initialFrame = 0) where T : Enum where U : Enum
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

        protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateStartEvent onStart, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, 0, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, 0, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, 0, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null, 0, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateStartEvent onStart, string animationName, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, EntityStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T, U>(T id, string animationName, int initialFrame = 0) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, null, null, Enum.GetNames(typeof(U)).Length, animationName, initialFrame);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            Visible = reader.ReadBoolean();
            currentAnimationIndex = reader.ReadInt32();
            Opacity = reader.ReadSingle();

            int animationCount = reader.ReadInt32();
            for (int i = 0; i < animationCount; i++)
            {
                Animation animation = animations[i];
                animation.LoadState(reader);
            }

            CollisionData = (CollisionData) reader.ReadByte();
            CheckCollisionWithWorld = reader.ReadBoolean();

            vel = new Vector(reader);
            LastVelocity = new Vector(reader);
            NoClip = reader.ReadBoolean();
            moving = reader.ReadBoolean();
            Static = reader.ReadBoolean();
            breakable = reader.ReadBoolean();
            health = new FixedSingle(reader);
            invincible = reader.ReadBoolean();
            invincibilityFrames = reader.ReadInt32();
            invincibilityFrameCounter = reader.ReadInt32();
            blinking = reader.ReadBoolean();
            blinkFrames = reader.ReadInt32();
            blinkFrameCounter = reader.ReadInt32();
            broke = reader.ReadBoolean();
        }

        public override void SaveState(BinaryWriter writer)
        {
            base.SaveState(writer);

            writer.Write(Visible);
            writer.Write(currentAnimationIndex);
            writer.Write(Opacity);

            if (animations != null)
            {
                writer.Write(animations.Count);
                foreach (Animation animation in animations)
                    animation.SaveState(writer);
            }
            else
                writer.Write(0);

            writer.Write((byte) CollisionData);
            writer.Write(CheckCollisionWithWorld);

            vel.Write(writer);
            LastVelocity.Write(writer);
            writer.Write(NoClip);
            writer.Write(moving);
            writer.Write(Static);
            writer.Write(breakable);
            health.Write(writer);
            writer.Write(invincible);
            writer.Write(invincibilityFrames);
            writer.Write(invincibilityFrameCounter);
            writer.Write(blinking);
            writer.Write(blinkFrames);
            writer.Write(blinkFrameCounter);
            writer.Write(broke);
        }

        public Animation CurrentAnimation => GetAnimation(currentAnimationIndex);

        public string CurrentAnimationName => CurrentAnimation?.FrameSequenceName;

        public int CurrentAnimationIndex
        {
            get => currentAnimationIndex;
            set
            {
                Animation animation;
                bool animating = false;
                int animationFrame = -1;
                if (!MultiAnimation)
                {
                    animation = CurrentAnimation;
                    if (animation != null)
                    {
                        animating = animation.Animating;
                        animationFrame = animation.CurrentSequenceIndex;
                        animation.Stop();
                        animation.Visible = false;
                    }
                }

                currentAnimationIndex = value;
                animation = CurrentAnimation;
                if (animation != null)
                {
                    animation.CurrentSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                    animation.Animating = animating;
                    animation.Visible = true;
                }
            }
        }

        protected virtual void OnCreateAnimation(int animationIndex, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn, ref bool add)
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
                VectorKind.BOUDINGBOX_CENTER => BoundingBox.Center,
                VectorKind.COLLISIONBOX_CENTER => CollisionBox.Center,
                _ => base.GetVector(kind)
            };
        }

        public override Vector GetLastVector(VectorKind kind)
        {
            return kind switch
            {
                VectorKind.BOUDINGBOX_CENTER => GetLastBox(BoxKind.BOUDINGBOX).Center,
                VectorKind.COLLISIONBOX_CENTER => GetLastBox(BoxKind.COLLISIONBOX).Center,
                _ => base.GetLastVector(kind)
            };
        }

        public override Box GetBox(BoxKind kind)
        {
            return kind switch
            {
                BoxKind.BOUDINGBOX => BoundingBox,
                BoxKind.COLLISIONBOX => CollisionBox,
                _ => base.GetBox(kind)
            };
        }

        protected virtual Box GetBoundingBox()
        {
            return (Directional && Direction != DefaultDirection ? DrawBox.Mirror(Origin) : DrawBox) - Origin;
        }

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        protected override Box GetHitbox()
        {
            return GetCollisionBox();
        }

        protected override Box GetTouchingBox()
        {
            Box result = GetHitbox();

            if (CheckCollisionWithWorld)
            {
                worldCollider.Box = CollisionBox;
                result |= worldCollider.LeftCollider
                    | worldCollider.UpCollider
                    | worldCollider.RightCollider
                    | worldCollider.DownCollider;
            }

            if (CheckCollisionWithSolidSprites)
            {
                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;
                result |= spriteCollider.LeftCollider
                    | spriteCollider.UpCollider
                    | spriteCollider.RightCollider
                    | spriteCollider.DownCollider;
            }

            return result;
        }

        protected override Box GetDeadBox()
        {
            if (animations.Count == 0)
                return base.GetDeadBox();

            Box drawBox = InitialAnimationName != null
                ? GetFirstAnimationByName(InitialAnimationName).DrawBox
                : InitialAnimationIndex >= 0 ? animations[InitialAnimationIndex].DrawBox : animations[0].DrawBox;

            return (Directional && Direction != DefaultDirection ? drawBox.Mirror(Origin) : drawBox) - Origin;
        }

        protected virtual Box GetCollisionBox()
        {
            if (!MultiAnimation)
                return CurrentAnimation != null ? CurrentAnimation.CurrentFrameCollisionBox : Box.EMPTY_BOX;

            Box result = Box.EMPTY_BOX;
            foreach (var animation in animations)
                if (animation.Visible)
                    result |= animation.CurrentFrameCollisionBox;

            return result;
        }

        protected internal override void OnSpawn()
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
            MultiAnimation = false;

            FadingSettings.Reset();

            CurrentAnimationIndex = InitialAnimationIndex;
            CurrentAnimation?.StartFromBegin();
        }

        protected internal override void PostSpawn()
        {
            base.PostSpawn();

            if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
            {
                worldCollider.Box = CollisionBox;

                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;

                if (BlockedUp)
                    OnBlockedUp();

                if (BlockedLeft)
                    OnBlockedLeft();

                if (BlockedRight)
                    OnBlockedRight();

                if (Landed)
                    OnLanded();
            }
        }

        protected virtual FixedSingle GetMaskSize()
        {
            return MASK_SIZE;
        }

        protected virtual FixedSingle GetHeadHeight()
        {
            return 0;
        }

        protected virtual FixedSingle GetLegsHeight()
        {
            return 0;
        }

        protected virtual bool IsUsingCollisionPlacements()
        {
            return false;
        }

        protected virtual SpriteCollider CreateWorldCollider()
        {
            return new SpriteCollider(this, CollisionBox, GetMaskSize(), GetHeadHeight(), GetLegsHeight(), IsUsingCollisionPlacements(), true, false);
        }

        protected virtual SpriteCollider CreateSpriteCollider()
        {
            return new SpriteCollider(this, Hitbox, GetMaskSize(), 0, 0, IsUsingCollisionPlacements(), false, true);
        }

        protected internal virtual void CreateResources()
        {
            if (ResourcesCreated)
                return;

            worldCollider = CreateWorldCollider();
            spriteCollider = CreateSpriteCollider();

            if (animationsByName.Count == 0 && SpriteSheet.FrameSequenceCount > 0)
                SetAnimationNames();

            int animationIndex = 0;
            var names = new List<string>(animationsByName.Keys);
            foreach (var animationName in names)
            {
                SpriteSheet.FrameSequence sequence = SpriteSheet.GetFrameSequence(animationName);
                string frameSequenceName = sequence.Name;
                Vector offset = Vector.NULL_VECTOR;
                int count = 1;
                int repeatX = 1;
                int repeatY = 1;
                int initialFrame = 0;
                bool startVisible = false;
                bool startOn = true;
                bool add = true;

                OnCreateAnimation(animationIndex, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

                if (add)
                {
                    int ai = animationIndex;
                    for (int i = 0; i < count; i++)
                    {
                        var animation = new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, offset, repeatX, repeatY, initialFrame, startVisible, startOn);
                        animations.Add(animation);
                        animationsByName[animationName].Add(animation);
                        animationIndex++;
                    }
                }
                else
                    animationsByName.Remove(animationName);
            }

            ResourcesCreated = true;
        }

        public override void Spawn()
        {
            base.Spawn();
            CreateResources();
        }

        public override void Place()
        {
            CreateResources();
            base.Place();

            if (!IsOffscreen(VectorKind.ORIGIN))
                Spawn();
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
            if (!Alive || broke || MarkedToRemove)
                return;

            if (!victim.Alive || victim.broke || victim.MarkedToRemove || health <= 0)
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
                spriteCollider.ClearIgnoredSprites();
                spriteCollider.Box = Hitbox;

                if (spriteCollider.DownCollider.IsOverlaping(sprite.Hitbox))
                    touchingSpritesDown.Add(sprite);

                if (spriteCollider.LeftCollider.IsOverlaping(sprite.Hitbox))
                    touchingSpritesLeft.Add(sprite);

                if (spriteCollider.RightCollider.IsOverlaping(sprite.Hitbox))
                    touchingSpritesRight.Add(sprite);

                if (spriteCollider.UpCollider.IsOverlaping(sprite.Hitbox))
                    touchingSpritesUp.Add(sprite);
            }

            return base.CheckTouching(entity);
        }

        private void MoveAlongSlope(SpriteCollider collider, RightTriangle slope, FixedSingle dx, bool gravity = true)
        {
            FixedSingle h = slope.HCathetusVector.X;
            int slopeSign = h.Signal;
            int dxs = dx.Signal;
            bool goingDown = dxs == slopeSign;

            var dy = (FixedSingle) (((FixedDouble) slope.VCathetus * dx / slope.HCathetus).Abs * dxs * slopeSign);
            var delta = new Vector(dx, dy);
            collider.MoveContactSolid(delta, dx.Abs, (goingDown ? Direction.NONE : Direction.UP) | (dxs > 0 ? Direction.RIGHT : Direction.LEFT), CollisionFlags.SLOPE);

            if (gravity)
                collider.MoveContactFloor(TILE_SIZE * 0.5 * QUERY_MAX_DISTANCE);

            if (collider.Landed)
                collider.AdjustOnTheFloor();
        }

        private void MoveX(SpriteCollider collider, FixedSingle deltaX, bool gravity = true, bool followSlopes = true)
        {
            var dx = new Vector(deltaX, 0);

            Box lastBox = collider.Box;
            bool wasLanded = collider.Landed;
            bool wasLandedOnSlope = collider.LandedOnSlope;
            RightTriangle lastSlope = collider.LandedSlope;
            Box lastLeftCollider = collider.LeftCollider;
            Box lastRightCollider = collider.RightCollider;

            collider.Translate(dx);

            if (collider.Landed)
                collider.AdjustOnTheFloor(TILE_SIZE * 0.5 * QUERY_MAX_DISTANCE);
            else if (gravity && wasLanded)
                collider.TryMoveContactSlope(TILE_SIZE * 0.5 * QUERY_MAX_DISTANCE);

            Box boxCollider = deltaX > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
            CollisionFlags collisionFlags = Engine.World.GetCollisionFlags(boxCollider, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites);

            if (!collisionFlags.CanBlockTheMove())
            {
                if (gravity && followSlopes && wasLanded)
                {
                    if (collider.LandedOnSlope)
                    {
                        RightTriangle slope = collider.LandedSlope;
                        FixedSingle h = slope.HCathetusVector.X;
                        if (h > 0 && deltaX > 0 || h < 0 && deltaX < 0)
                        {
                            FixedSingle x = lastBox.Origin.X;
                            FixedSingle stx = deltaX > 0 ? slope.Left : slope.Right;
                            FixedSingle stx_x = stx - x;
                            if (deltaX > 0 && stx_x > 0 && stx_x <= deltaX || deltaX < 0 && stx_x < 0 && stx_x >= deltaX)
                            {
                                deltaX -= stx_x;
                                dx = new Vector(deltaX, 0);

                                collider.Box = lastBox;
                                if (wasLandedOnSlope)
                                    MoveAlongSlope(collider, lastSlope, stx_x);
                                else
                                    collider.Translate(new Vector(stx_x, 0));

                                MoveAlongSlope(collider, slope, deltaX);
                            }
                            else
                            {
                                if (wasLandedOnSlope)
                                {
                                    collider.Box = lastBox;
                                    MoveAlongSlope(collider, lastSlope, deltaX);
                                }
                            }
                        }
                        else
                        {
                            if (wasLandedOnSlope)
                            {
                                collider.Box = lastBox;
                                MoveAlongSlope(collider, lastSlope, deltaX);
                            }
                        }
                    }
                    else if (Engine.World.GetCollisionFlags(collider.DownCollider, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites).HasFlag(CollisionFlags.SLOPE))
                        collider.MoveContactFloor();
                }
            }
            else if (collisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (collider.LandedOnSlope)
                {
                    RightTriangle slope = collider.LandedSlope;
                    FixedSingle x = lastBox.Origin.X;
                    FixedSingle stx = deltaX > 0 ? slope.Left : slope.Right;
                    FixedSingle stx_x = stx - x;
                    if (deltaX > 0 && stx_x < 0 && stx_x >= deltaX || deltaX < 0 && stx_x > 0 && stx_x <= deltaX)
                    {
                        deltaX -= stx_x;
                        dx = new Vector(deltaX, 0);

                        collider.Box = lastBox;
                        if (wasLandedOnSlope)
                            MoveAlongSlope(collider, lastSlope, stx_x);
                        else
                            collider.Translate(new Vector(stx_x, 0));

                        MoveAlongSlope(collider, slope, deltaX);
                    }
                    else
                    {
                        if (wasLandedOnSlope)
                        {
                            collider.Box = lastBox;
                            MoveAlongSlope(collider, lastSlope, deltaX);
                        }
                    }
                }
                else if (!wasLanded)
                {
                    collider.Box = lastBox;
                    if (deltaX > 0)
                        collider.MoveContactSolid(Vector.RIGHT_VECTOR, deltaX.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
                    else
                        collider.MoveContactSolid(Vector.LEFT_VECTOR, (-deltaX).Ceil(), Direction.LEFT, CollisionFlags.NONE);
                }
            }
            else
            {
                collider.Box = lastBox;
                if (deltaX > 0)
                    collider.MoveContactSolid(Vector.RIGHT_VECTOR, deltaX.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
                else
                    collider.MoveContactSolid(Vector.LEFT_VECTOR, (-deltaX).Ceil(), Direction.LEFT, CollisionFlags.NONE);
            }
        }

        private Vector Move(SpriteCollider collider, Vector delta, FixedSingle gravity)
        {
            Vector lastBoxOrigin = collider.Box.Origin;

            if (delta.X != 0)
            {
                if (collider.LandedOnSlope)
                {
                    if (collider.LandedSlope.HCathetusSign == delta.X.Signal)
                    {
                        if (gravity != 0)
                            MoveAlongSlope(collider, collider.LandedSlope, delta.X, gravity != 0);
                    }
                    else
                        MoveAlongSlope(collider, collider.LandedSlope, delta.X, gravity != 0);
                }
                else
                    MoveX(collider, delta.X, gravity != 0);
            }

            if (delta.Y != 0)
            {
                var dy = new Vector(0, delta.Y);
                Box lastBox = collider.Box;
                Box lastUpCollider = collider.UpCollider;
                Box lastDownCollider = collider.DownCollider;
                collider.Translate(dy);

                if (dy.Y > 0)
                {
                    Box union = lastDownCollider | collider.DownCollider;
                    if (Engine.World.GetCollisionFlags(union, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites).CanBlockTheMove())
                    {
                        collider.Box = lastBox;
                        collider.MoveContactFloor(dy.Y.Ceil());
                    }
                }
                else
                {
                    Box union = lastUpCollider | collider.UpCollider;
                    if (Engine.World.GetCollisionFlags(union, collider.IgnoreSprites, CollisionFlags.NONE, collider.CheckCollisionWithWorld, collider.CheckCollisionWithSolidSprites).CanBlockTheMove())
                    {
                        collider.Box = lastBox;
                        collider.MoveContactSolid(dy, (-dy.Y).Ceil(), Direction.UP);
                    }
                }
            }

            return collider.Box.Origin - lastBoxOrigin;
        }

        protected virtual void OnBeforeMove(ref Vector origin)
        {
            BeforeMoveEvent?.Invoke(this);
        }

        public void AdjustOnTheFloor(FixedSingle maxDistance, CollisionFlags ignore = CollisionFlags.NONE)
        {
            Box lastBox = CollisionBox;
            worldCollider.Box = lastBox;
            worldCollider.AdjustOnTheFloor(maxDistance, ignore);

            Vector delta = worldCollider.Box.Origin - lastBox.Origin;
            Origin += delta;

            lastBox = Hitbox;
            spriteCollider.ClearIgnoredSprites();
            spriteCollider.Box = lastBox;
            spriteCollider.AdjustOnTheFloor(maxDistance, ignore);

            delta = spriteCollider.Box.Origin - lastBox.Origin;
            Origin += delta;
        }

        public void AdjustOnTheFloor(CollisionFlags ignore = CollisionFlags.NONE)
        {
            AdjustOnTheFloor(QUERY_MAX_DISTANCE, ignore);
        }

        protected void RestrictIn(Box limitBox, ref Vector origin)
        {
            Vector delta = origin - Origin;

            Box hitbox = Hitbox;
            Box box = limitBox.RestrictIn(hitbox + delta);
            Vector newDelta = box.Origin - hitbox.Origin;

            if (delta != newDelta)
                origin += newDelta - delta;
        }

        private void DoPhysics(Sprite phisycsParent, Vector delta)
        {
            if (NoClip || Static || delta.IsNull)
                return;

            FixedSingle gravity = Gravity;

            if (delta.IsNull)
                return;

            if (CheckCollisionWithWorld)
            {
                worldCollider.Box = CollisionBox;
                delta = Move(worldCollider, delta, gravity);
            }

            if (CheckCollisionWithSolidSprites)
            {
                spriteCollider.ClearIgnoredSprites();

                if (phisycsParent != null)
                    spriteCollider.IgnoreSprites.Add(phisycsParent);

                spriteCollider.Box = Hitbox;

                delta = Move(spriteCollider, delta, gravity);
            }

            if (delta.IsNull)
                return;

            var deltaX = delta.X;
            var deltaY = delta.Y;

            if (deltaX < 0)
            {
                foreach (var sprite in touchingSpritesLeft)
                    if (sprite != phisycsParent)
                        sprite.DoPhysics(this, (0, deltaX));
            }
            else if (deltaX > 0)
            {
                foreach (var sprite in touchingSpritesRight)
                    if (sprite != phisycsParent)
                        sprite.DoPhysics(this, (0, deltaX));
            }

            if (deltaY > 0)
            {
                foreach (var sprite in touchingSpritesDown)
                    if (sprite != phisycsParent)
                        sprite.DoPhysics(this, (0, deltaY));
            }

            foreach (var sprite in touchingSpritesUp)
                if (sprite != phisycsParent)
                {
                    sprite.DoPhysics(this, delta);
                    sprite.AdjustOnTheFloor();
                }

            Vector newOrigin = Origin + delta
                ;
            if (!CanGoOutOfMapBounds)
                RestrictIn(Engine.World.BoundingBox, ref newOrigin);

            OnBeforeMove(ref newOrigin);
            Origin = newOrigin;
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

        protected bool CollisionCheck(Sprite sprite)
        {
            // TODO : Implement call to this
            return ShouldCollide(sprite) || sprite.ShouldCollide(this);
        }

        public bool MultiAnimation
        {
            get;
            protected set;
        }

        public Box DrawBox
        {
            get
            {
                if (!MultiAnimation)
                    return CurrentAnimation != null ? CurrentAnimation.DrawBox : Origin + Box.EMPTY_BOX;

                Box result = Box.EMPTY_BOX;
                foreach (var animation in animations)
                    if (animation.Visible)
                        result |= animation.DrawBox;

                return result;
            }
        }

        public float Opacity
        {
            get;
            set;
        } = 1;

        public Animation GetAnimation(int index)
        {
            return animations == null || index < 0 || index >= animations.Count ? null : animations[index];
        }

        public int GetAnimationIndexByName(string name)
        {
            Animation animation = GetFirstAnimationByName(name);
            return animation != null ? animation.Index : -1;
        }

        public IEnumerable<Animation> GetAnimationsByName(string name)
        {
            return animationsByName.TryGetValue(name, out List<Animation> animations) ? animations : null;
        }

        public Animation GetFirstAnimationByName(string name)
        {
            return animationsByName.TryGetValue(name, out List<Animation> animations) ? animations.Count > 0 ? animations[0] : null : null;
        }

        public void SetCurrentAnimationByName(string name, int startIndex = -1)
        {
            Animation animation = GetFirstAnimationByName(name);
            CurrentAnimationIndex = animation != null ? animation.Index : -1;

            if (startIndex >= 0)
                CurrentAnimation?.Start(startIndex);
        }

        public void SetAnimationsVisibility(string name, bool visible)
        {
            if (!animationsByName.TryGetValue(name, out List<Animation> animations))
                return;

            foreach (var animation in animations)
                animation.Visible = visible;
        }

        public void SetAnimationsVisibilityExclusively(string name, bool visible)
        {
            foreach (var animation in animations)
                animation.Visible = animation.FrameSequenceName == name ? visible : !visible;
        }

        protected override bool PreThink()
        {
            touchingSpritesLeft.Clear();
            touchingSpritesRight.Clear();
            touchingSpritesUp.Clear();
            touchingSpritesDown.Clear();

            InvisibleOnCurrentFrame = false;

            return base.PreThink();
        }

        protected override void Think()
        {
            if (!Static && !Engine.Paused)
            {
                bool lastBlockedUp = false;
                bool lastBlockedLeft = false;
                bool lastBlockedRight = false;
                bool lastLanded = false;

                if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
                {
                    worldCollider.Box = CollisionBox;

                    spriteCollider.ClearIgnoredSprites();
                    spriteCollider.Box = Hitbox;

                    lastBlockedUp = BlockedUp;
                    lastBlockedLeft = BlockedLeft;
                    lastBlockedRight = BlockedRight;
                    lastLanded = Landed;
                }

                FixedSingle gravity = Gravity;

                if (!NoClip && !Static)
                {
                    if (!lastLanded && gravity != 0)
                    {
                        Velocity += gravity * Vector.DOWN_VECTOR;

                        FixedSingle terminalDownwardSpeed = TerminalDownwardSpeed;
                        if (Velocity.Y > terminalDownwardSpeed)
                            Velocity = new Vector(Velocity.X, terminalDownwardSpeed);
                    }

                    if (lastLanded)
                        Velocity = Velocity.XVector;
                }

                if (!lastLanded && Velocity.Y > gravity && Velocity.Y < 2 * gravity)
                    Velocity = new Vector(Velocity.X, gravity);

                Vector vel = Velocity + ExternalVelocity;
                ExternalVelocity = Vector.NULL_VECTOR;

                Vector lastOrigin = Origin;

                DoPhysics(null, vel);

                if (lastOrigin != Origin)
                    StartMoving();
                else if (moving)
                    StopMoving();

                if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
                {
                    worldCollider.Box = CollisionBox;

                    spriteCollider.ClearIgnoredSprites();
                    spriteCollider.Box = Hitbox;

                    if (BlockedUp && (!lastBlockedUp || Velocity.Y < 0))
                        OnBlockedUp();

                    if (BlockedLeft && (!lastBlockedLeft || Velocity.X < 0))
                        OnBlockedLeft();

                    if (BlockedRight && (!lastBlockedRight || Velocity.X > 0))
                        OnBlockedRight();

                    if (Landed && (!lastLanded || Velocity.Y > 0))
                        OnLanded();
                }
            }

            if (Blinking)
                OnBlink(blinkFrameCounter);
        }

        protected internal override void PostThink()
        {
            base.PostThink();

            if (Engine.Paused)
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

            if (Blinking)
            {
                blinkFrameCounter++;

                if (blinkFrames > 0 && blinkFrameCounter >= blinkFrames)
                {
                    Blinking = false;
                    OnEndBlinking();
                }
            }

            if (Animating)
                foreach (Animation animation in animations)
                    animation.NextFrame();
        }

        protected virtual void OnBlink(int frameCounter)
        {
            InvisibleOnCurrentFrame = frameCounter % 2 == 0;
        }

        protected virtual void OnEndInvincibility()
        {
        }

        protected virtual void OnEndBlinking()
        {
        }

        public virtual void Render()
        {
            if (!Alive || MarkedToRemove || !Visible)
                return;

            if (!InvisibleOnCurrentFrame)
                foreach (Animation animation in animations)
                    animation.Render();

            InvisibleOnCurrentFrame = false;
        }

        protected internal virtual void OnAnimationEnd(Animation animation)
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
        protected internal override void Cleanup()
        {
            base.Cleanup();

            CurrentAnimationIndex = -1;
            moving = false;
            invincible = false;
            invincibilityFrames = -1;
            invincibilityFrameCounter = 0;
            blinking = false;
            blinkFrames = -1;
            blinkFrameCounter = 0;
            NoClip = false;
            InvisibleOnCurrentFrame = false;
            FadingSettings.Reset();
        }

        internal void OnDeviceReset()
        {
            foreach (var animation in animations)
                animation.OnDeviceReset();
        }

        protected override BoxKind ComputeBoxKind()
        {
            return (CollisionData.IsSolidBlock() ? BoxKind.COLLISIONBOX : BoxKind.NONE)
                | (Visible ? BoxKind.BOUDINGBOX : BoxKind.NONE)
                | base.ComputeBoxKind();
        }

        public void FaceToPosition(Vector pos)
        {
            var faceDirection = GetHorizontalDirection(pos);

            if (Direction == faceDirection)
                Direction = Direction.Oposite();
        }

        public void FaceToEntity(Entity entity)
        {
            FaceToPosition(entity.Origin);
        }

        public void FaceToPlayer()
        {
            FaceToEntity(Engine.Player);
        }

        public void FaceToScreenCenter()
        {
            FaceToPosition(Engine.World.Camera.Center);
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
}