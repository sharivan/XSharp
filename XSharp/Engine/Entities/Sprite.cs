using MMX.Geometry;
using MMX.Math;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using static MMX.Engine.Consts;
using static MMX.Engine.World.World;
using Box = MMX.Geometry.Box;

namespace MMX.Engine.Entities
{
    public delegate void SpriteEvent(Sprite source);
    public delegate void TakeDamageEvent(Sprite source, Sprite attacker, FixedSingle damage);
    public delegate void HurtEvent(Sprite source, Sprite victim, FixedSingle damage);
    public delegate void HealthChangedEvent(Sprite source, FixedSingle health);
    public delegate void AnimationEndEvent(Sprite source, Animation animation);

    public abstract class Sprite : Entity, IDisposable
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

        protected List<Animation> animations;
        private int currentAnimationIndex = -1;
        protected bool solid;
        private bool fading;
        private bool fadingIn;
        private int fadingTime;
        private int elapsed;

        protected BoxCollider collider;

        private Vector vel;
        protected bool moving;
        protected bool breakable;
        protected FixedSingle health;
        private bool invincible;
        private int invincibilityFrames;
        private long invincibleExpires;
        protected bool broke;
        private bool blinking;
        private bool blinkOn;
        private long blinkExpires;

        private readonly Dictionary<string, int> animationNames;

        public int SpriteSheetIndex
        {
            get;
            private set;
        }

        public bool Directional
        {
            get;
            private set;
        }

        public Direction Direction { get; set; } = Direction.RIGHT;

        public SpriteSheet Sheet => Engine.GetSpriteSheet(SpriteSheetIndex);

        public string InitialAnimationName
        {
            get;
            set;
        }

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
                if (!invincible && value)
                    MakeInvincible();

                invincible = value;
            }
        }

        public bool Blinking
        {
            get => blinking;
            set
            {
                if (!blinking && value)
                    MakeBlinking();

                blinking = value;
            }
        }

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
                vel = value;
            }
        }

        public Vector LastVelocity
        {
            get;
            private set;
        }

        public BoxCollider Collider
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
            get;
            set;
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
            set;
        }

        public int PaletteIndex
        {
            get;
            set;
        }

        public Texture Palette => Engine.GetPalette(PaletteIndex);

        protected Sprite(GameEngine engine, string name, Vector origin, int spriteSheetIndex, string[] animationNames = null, string initialAnimationName = null, bool directional = false) :
            base(engine, name, origin)
        {
            SpriteSheetIndex = spriteSheetIndex;
            InitialAnimationName = initialAnimationName;
            Directional = directional;

            PaletteIndex = -1;
            Opacity = 1;

            animations = new List<Animation>();            

            this.animationNames = new Dictionary<string, int>();

            if (animationNames != null)
                foreach (var animationName in animationNames)
                    this.animationNames.Add(animationName, -1);
            else
                foreach (var frameSequenceName in Sheet.FrameSequenceNames)
                    this.animationNames.Add(frameSequenceName, -1);
        }

        protected Sprite(GameEngine engine, Vector origin, int spriteSheetIndex, string[] animationNames = null, string initialAnimationName = null, bool directional = false)
            : this(engine, engine.GetExclusiveName("Sprite"), origin, spriteSheetIndex, animationNames, initialAnimationName, directional)
        {
        }

        protected Sprite(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional = false, params string[] animationNames)
            : this(engine, name, origin, spriteSheetIndex, animationNames.Length > 0 ? animationNames : null, animationNames.Length > 0 ? animationNames[0] : null, directional)
        {
        }

        protected Sprite(GameEngine engine, Vector origin, int spriteSheetIndex, bool directional = false, params string[] animationNames)
            : this(engine, engine.GetExclusiveName("Sprite"), origin, spriteSheetIndex, directional, animationNames)
        {
        }

        public int GetAnimationIndex(string animationName)
        {
            return animationNames.TryGetValue(animationName, out int result) ? result : -1;
        }

        protected override Type GetStateType()
        {
            return typeof(SpriteState);
        }

        protected SpriteState RegisterState(int id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd, int animationIndex, int initialFrame = 0)
        {
            var state = (SpriteState) RegisterState(id, onStart, onFrame, onEnd);
            state.AnimationIndex = animationIndex;
            state.InitialFrame = initialFrame;
            return state;
        }

        protected SpriteState RegisterState(int id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd, string animationName, int initialFrame = 0)
        {
            var state = (SpriteState) RegisterState(id, onStart, onFrame, onEnd);
            state.AnimationName = animationName;
            state.InitialFrame = initialFrame;
            return state;
        }

        protected SpriteState RegisterState<T>(T id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, animationName, initialFrame);
        }

        protected SpriteState RegisterState(int id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0)
        {
            return RegisterState(id, null, onFrame, null, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState(int id, int animationIndex, int initialFrame = 0)
        {
            return RegisterState(id, null, null, null, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState(int id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0)
        {
            return RegisterState(id, null, onFrame, null, animationName, initialFrame);
        }

        protected SpriteState RegisterState(int id, string animationName, int initialFrame = 0)
        {
            return RegisterState(id, null, null, null, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, int animationIndex, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null, animationIndex, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, EntityStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, animationName, initialFrame);
        }

        protected SpriteState RegisterState<T>(T id, string animationName, int initialFrame = 0) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null, animationName, initialFrame);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            currentAnimationIndex = reader.ReadInt32();
            Opacity = reader.ReadSingle();

            int animationCount = reader.ReadInt32();
            for (int i = 0; i < animationCount; i++)
            {
                Animation animation = animations[i];
                animation.LoadState(reader);
            }

            solid = reader.ReadBoolean();
            fading = reader.ReadBoolean();
            fadingIn = reader.ReadBoolean();
            fadingTime = reader.ReadInt32();
            elapsed = reader.ReadInt32();            
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

            writer.Write(solid);
            writer.Write(fading);
            writer.Write(fadingIn);
            writer.Write(fadingTime);
            writer.Write(elapsed);            
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

        protected internal Animation CurrentAnimation => GetAnimation(currentAnimationIndex);

        protected internal int CurrentAnimationIndex
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

        protected virtual void OnCreateAnimation(int animationIndex, SpriteSheet sheet, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn, ref bool add)
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name}[{Name}, {Origin}]";
        }

        public void FadeIn(int time)
        {
            fading = true;
            fadingIn = true;
            fadingTime = time;
            elapsed = 0;
        }

        public void FadeOut(int time)
        {
            fading = true;
            fadingIn = false;
            fadingTime = time;
            elapsed = 0;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Visible = true;
            CheckCollisionWithWorld = true;
            solid = true;
            Velocity = Vector.NULL_VECTOR;
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

            CurrentAnimationIndex = InitialAnimationIndex;
            CurrentAnimation?.StartFromBegin();
        }

        protected internal override void PostSpawn()
        {
            base.PostSpawn();

            if (CheckCollisionWithWorld)
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

        public override void Spawn()
        {
            base.Spawn();

            collider = new BoxCollider(Engine.World, CollisionBox, GetMaskSize(), GetSideColliderTopOffset(), GetSideColliderBottomOffset(), IsUsingCollisionPlacements());

            int animationIndex = 0;
            var names = new List<string>(animationNames.Keys);
            foreach (var animationName in names)
            {
                SpriteSheet.FrameSequence sequence = Sheet.GetFrameSequence(animationName);
                string frameSequenceName = sequence.Name;
                Vector offset = Vector.NULL_VECTOR;
                int count = 1;
                int repeatX = 1;
                int repeatY = 1;
                int initialFrame = 0;
                bool startVisible = false;
                bool startOn = true;
                bool add = true;

                OnCreateAnimation(animationIndex, Sheet, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

                if (add)
                {
                    int ai = animationIndex;
                    for (int i = 0; i < count; i++)
                    {
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, offset, repeatX, repeatY, initialFrame, startVisible, startOn));
                        animationNames[animationName] = ai;
                        animationIndex++;
                    }
                }
                else
                    animationNames.Remove(animationName);
            }
        }

        protected virtual bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            return true;
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

            if (!victim.Invincible && !victim.NoClip && victim.OnTakeDamage(this, ref damage))
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
            blinkOn = false;
            blinkExpires = frames > 0 ? Engine.FrameCounter + frames : 0;
        }

        protected virtual bool ShouldCollide(Sprite sprite)
        {
            return false;
        }

        protected override Box GetBoundingBox()
        {
            return DrawBox - Origin;
        }

        protected override Box GetHitBox()
        {
            return GetCollisionBox();
        }

        public Box CollisionBox => Origin + GetCollisionBox();

        public Box LastCollisionBox => LastOrigin + GetCollisionBox();

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

        private void MoveAlongSlope(BoxCollider collider, RightTriangle slope, FixedSingle dx, bool gravity = true)
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

        private void MoveX(BoxCollider collider, FixedSingle deltaX, bool gravity = true, bool followSlopes = true)
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
            CollisionFlags collisionFlags = Engine.GetCollisionFlags(union, CollisionFlags.NONE, true);

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
                    else if (Engine.GetCollisionFlags(collider.DownCollider, CollisionFlags.NONE, false).HasFlag(CollisionFlags.SLOPE))
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

            if (CheckCollisionWithWorld)
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

            if (Velocity.IsNull && moving)
                StopMoving();

            Vector delta = !Static && !Velocity.IsNull ? Velocity : Vector.NULL_VECTOR;
            if (!delta.IsNull)
            {
                if (!NoClip && CheckCollisionWithWorld)
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
                            if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true)))
                            {
                                collider.Box = lastBox;
                                collider.MoveContactFloor(dy.Y.Ceil());
                            }
                        }
                        else
                        {
                            Box union = lastUpCollider | collider.UpCollider;
                            if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true)))
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
                Vector newOrigin = Origin + delta;

                if (!CanGoOutOfMapBounds)
                    OnBeforeMove(ref newOrigin);

                Vector lastOrigin = Origin;
                Origin = newOrigin;

                if (lastOrigin != newOrigin)
                    StartMoving();
            }
            else if (moving)
                StopMoving();

            if (CheckCollisionWithWorld)
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
        }

        public Animation GetAnimation(int index)
        {
            return animations == null || index < 0 || index >= animations.Count ? null : animations[index];
        }

        public int GetAnimationIndexByName(string name)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                var animation = animations[i];
                if (animation.FrameSequenceName == name)
                    return i;
            }

            return -1;
        }

        public Animation GetAnimationByName(string name)
        {
            int index = GetAnimationIndexByName(name);
            return index >= 0 ? animations[index] : null;
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
        }

        protected internal override void PostThink()
        {
            base.PostThink();

            if (Engine.Paused)
                return;

            if (Invincible && invincibleExpires > 0 && Engine.FrameCounter >= invincibleExpires)
                Invincible = false;

            if (Blinking && blinkExpires > 0 && Engine.FrameCounter >= blinkExpires)
                Blinking = false;

            foreach (Animation animation in animations)
                animation.NextFrame();
        }

        protected virtual void OnBlink(bool blinkOn)
        {
            InvisibleOnCurrentFrame = !blinkOn;
        }

        public virtual void Render()
        {
            if (!Alive || !Visible)
                return;

            if (Blinking)
            {
                blinkOn = !blinkOn;
                OnBlink(blinkOn);
            }

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

        internal void OnDeviceReset()
        {
            foreach (var animation in animations)
                animation.OnDeviceReset();
        }
    }
}