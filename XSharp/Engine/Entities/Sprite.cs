using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using XSharp.Engine.Graphics;
using XSharp.Engine.World;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;
using Box = XSharp.Geometry.Box;

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
        internal int layer = 0;
        private List<Animation> animations;
        private int currentAnimationIndex = -1;

        protected SpriteCollider collider;

        private Vector vel;
        protected bool moving;
        protected bool breakable;
        protected FixedSingle health;
        private bool invincible;
        private int invincibilityFrames;
        private long invincibleExpires;
        protected bool broke;
        private bool blinking;
        private int blinkFrameCounter = 0;
        private long blinkFrames;

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
                    Engine.UpdateSpriteLayer(this, layer);
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
            protected set;
        } = -1;

        public string SpriteSheetName
        {
            get
            {
                var spriteSheet = Engine.GetSpriteSheet(SpriteSheetIndex);
                return spriteSheet?.Name;
            }

            protected set
            {
                var spriteSheet = Engine.GetSpriteSheetByName(value);
                SpriteSheetIndex = spriteSheet != null ? spriteSheet.Index : -1;
            }
        }

        public bool Directional
        {
            get;
            protected set;
        } = false;

        public Direction Direction
        {
            get;
            protected set;
        } = Direction.RIGHT;

        public Direction DefaultDirection
        {
            get;
            protected set;
        } = Direction.RIGHT;

        public CollisionData CollisionData
        {
            get;
            protected set;
        } = CollisionData.NONE;

        public bool PushSprites
        {
            get;
            protected set;
        } = false;

        public bool Inertial
        {
            get;
            protected set;
        } = false;

        public bool CanSmash
        {
            get;
            protected set;
        } = false;

        public string InitialAnimationName
        {
            get;
            protected set;
        } = null;

        public int InitialAnimationIndex => InitialAnimationName != null ? GetAnimationIndex(InitialAnimationName) : -1;

        new public SpriteState CurrentState => (SpriteState) base.CurrentState;

        public bool Static
        {
            get;
            protected set;
        }

        public bool NoClip
        {
            get;
            protected set;
        }

        public bool Moving => moving;

        public bool Broke => broke;

        public bool Invincible
        {
            get => invincible;
            protected set
            {
                if (!invincible && value)
                    MakeInvincible();

                invincible = value;
            }
        }

        public bool Blinking
        {
            get => blinking;
            protected set
            {
                if (!blinking && value)
                    MakeBlinking();

                blinking = value;
            }
        }

        public FadingSettings FadingSettings
        {
            get;
        }

        public IEnumerable<Animation> Animations => animations;

        public bool InvisibleOnCurrentFrame
        {
            get;
            protected set;
        }

        public FixedSingle Health
        {
            get => health;
            protected set
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
            protected set
            {
                LastVelocity = vel;
                vel = value;
            }
        }

        public Vector LastVelocity
        {
            get;
            private set;
        }

        public Vector AdictionalVelocity
        {
            get;
            internal set;
        }

        public virtual Box BoundingBox
        {
            get => Origin + (Directional && Direction == DefaultDirection ? GetBoundingBox() : GetBoundingBox().Mirror());
            protected set
            {
                Box collisionBox = value - value.Origin;
                BeginUpdate();
                SetBoundingBox(Directional && Direction == DefaultDirection ? collisionBox : collisionBox.Mirror());
                SetOrigin(value.Origin);
                EndUpdate();
                UpdatePartition();
            }
        }

        public override Box Hitbox
        {
            get
            {
                Box box = !Alive && Respawnable ? GetDeadBox() : GetHitbox();
                return Origin + (Directional && Direction == DefaultDirection ? box : box.Mirror());
            }

            protected set
            {
                Box hitbox = value - value.Origin;
                BeginUpdate();
                SetHitbox(Directional && Direction == DefaultDirection ? hitbox : hitbox.Mirror());
                SetOrigin(value.Origin);
                EndUpdate();
                UpdatePartition();
            }
        }

        public virtual Box CollisionBox => Origin + (Directional && Direction == DefaultDirection ? GetCollisionBox() : GetCollisionBox().Mirror());

        public SpriteCollider Collider
        {
            get
            {
                collider.Box = CollisionBox;
                return collider;
            }
        }

        public FixedSingle Gravity => GetGravity();

        public FixedSingle TerminalDownwardSpeed => GetTerminalDownwardSpeed();

        public bool CheckCollisionWithWorld
        {
            get => collider.CheckCollisionWithWorld;
            protected set => collider.CheckCollisionWithWorld = value;
        }

        public bool CheckCollisionWithSolidSprites
        {
            get => collider.CheckCollisionWithSolidSprites;
            protected set => collider.CheckCollisionWithSolidSprites = value;
        }

        public bool BlockedUp => !NoClip && collider.BlockedUp;

        public bool BlockedLeft => !NoClip && collider.BlockedLeft;

        public bool BlockedRight => !NoClip && collider.BlockedRight;

        public bool Landed => !NoClip && collider.Landed && Velocity.Y >= 0;

        public bool LandedOnSlope => !NoClip && collider.LandedOnSlope;

        public bool LandedOnTopLadder => !NoClip && collider.LandedOnTopLadder;

        public RightTriangle LandedSlope => collider.LandedSlope;

        public bool Underwater => !NoClip && collider.Underwater;

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
            protected set;
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

            FadingSettings = new FadingSettings();
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
            invincibleExpires = reader.ReadInt64();
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
            writer.Write(invincibleExpires);
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
            return (Directional && Direction == DefaultDirection ? DrawBox : DrawBox.Mirror()) - Origin;
        }

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        protected override Box GetHitbox()
        {
            return GetCollisionBox();
        }

        protected override Box GetDeadBox()
        {
            if (animations.Count == 0)
                return base.GetDeadBox();

            Box drawBox = InitialAnimationName != null
                ? GetFirstAnimationByName(InitialAnimationName).DrawBox
                : InitialAnimationIndex >= 0 ? animations[InitialAnimationIndex].DrawBox : animations[0].DrawBox;

            return (Directional && Direction == DefaultDirection ? drawBox : drawBox.Mirror()) - Origin;
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
            AdictionalVelocity = Vector.NULL_VECTOR;
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
                collider.Box = CollisionBox;

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

        protected virtual FixedSingle GetSideColliderTopOffset()
        {
            return 0;
        }

        protected virtual FixedSingle GetSideColliderBottomOffset()
        {
            return 0;
        }

        protected virtual bool IsUsingCollisionPlacements()
        {
            return false;
        }

        protected virtual SpriteCollider CreateCollider()
        {
            return new SpriteCollider(this, CollisionBox, GetMaskSize(), GetSideColliderTopOffset(), GetSideColliderBottomOffset(), IsUsingCollisionPlacements(), true, false);
        }

        protected internal virtual void CreateResources()
        {
            if (ResourcesCreated)
                return;

            collider = CreateCollider();

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
            invincibleExpires = frames < 0 ? Engine.FrameCounter + invincibilityFrames : frames > 0 ? Engine.FrameCounter + frames : 0;

            if (blink)
                MakeBlinking(frames < 0 ? invincibilityFrames : frames);
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
                collider.MoveContactFloor(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);

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
                collider.AdjustOnTheFloor(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);
            else if (gravity && wasLanded)
                collider.TryMoveContactSlope(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);

            Box union = deltaX > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
            CollisionFlags collisionFlags = Engine.World.GetCollisionFlags(union, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites);

            if (!CanBlockTheMove(collisionFlags))
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
                    else if (Engine.World.GetCollisionFlags(collider.DownCollider, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites).HasFlag(CollisionFlags.SLOPE))
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

        protected virtual void OnBeforeMove(ref Vector origin)
        {
            BeforeMoveEvent?.Invoke(this);
        }

        private void DoPhysics()
        {
            bool lastBlockedUp = false;
            bool lastBlockedLeft = false;
            bool lastBlockedRight = false;
            bool lastLanded = false;

            if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
            {
                collider.Box = CollisionBox;

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

            Vector vel = Velocity + AdictionalVelocity;
            AdictionalVelocity = Vector.NULL_VECTOR;

            if (vel.IsNull && moving)
                StopMoving();

            Vector delta = !Static && !vel.IsNull ? vel : Vector.NULL_VECTOR;
            if (!delta.IsNull)
            {
                if (!NoClip && (CheckCollisionWithWorld || CheckCollisionWithSolidSprites))
                {
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
                            if (CanBlockTheMove(Engine.World.GetCollisionFlags(union, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites)))
                            {
                                collider.Box = lastBox;
                                collider.MoveContactFloor(dy.Y.Ceil());
                            }
                        }
                        else
                        {
                            Box union = lastUpCollider | collider.UpCollider;
                            if (CanBlockTheMove(Engine.World.GetCollisionFlags(union, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites)))
                            {
                                collider.Box = lastBox;
                                collider.MoveContactSolid(dy, (-dy.Y).Ceil(), Direction.UP);
                            }
                        }
                    }

                    delta = collider.Box.Origin - CollisionBox.Origin;
                }
            }

            if (delta != Vector.NULL_VECTOR)
            {
                Vector newOrigin = CanGoOutOfMapBounds ? Origin + delta : Engine.World.BoundingBox.RestrictIn(CollisionBox + delta).Origin;

                OnBeforeMove(ref newOrigin);

                Vector lastOrigin = Origin;
                Origin = newOrigin;

                if (lastOrigin != newOrigin)
                    StartMoving();
            }
            else if (moving)
                StopMoving();

            if (CheckCollisionWithWorld || CheckCollisionWithSolidSprites)
            {
                if (BlockedUp && !lastBlockedUp)
                    OnBlockedUp();

                if (BlockedLeft && !lastBlockedLeft)
                    OnBlockedLeft();

                if (BlockedRight && !lastBlockedRight)
                    OnBlockedRight();

                if (Landed && !lastLanded)
                    OnLanded();
            }
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
            InvisibleOnCurrentFrame = false;
            return base.PreThink();
        }

        protected override void Think()
        {
            if (!Static && !Engine.Paused)
                DoPhysics();

            if (Blinking)
                OnBlink(blinkFrameCounter);
        }

        protected internal override void PostThink()
        {
            base.PostThink();

            if (Engine.Paused)
                return;

            if (Invincible && invincibleExpires > 0 && Engine.FrameCounter >= invincibleExpires)
                Invincible = false;

            if (Blinking && blinkFrames > 0 && blinkFrameCounter >= blinkFrames)
            {
                Blinking = false;
                OnEndBlink();
            }
            else
                blinkFrameCounter++;

            if (Animating)
                foreach (Animation animation in animations)
                    animation.NextFrame();
        }

        protected virtual void OnBlink(int frameCounter)
        {
            InvisibleOnCurrentFrame = frameCounter % 2 == 0;
        }

        protected virtual void OnEndBlink()
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
            invincibleExpires = -1;
            blinking = false;
            blinkFrames = -1;
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
            return (CollisionChecker.IsSolidBlock(CollisionData) ? BoxKind.COLLISIONBOX : BoxKind.NONE)
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
    }
}