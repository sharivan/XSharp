using System;
using System.Collections.Generic;
using System.IO;

using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;

using static MMX.Engine.Consts;
using static MMX.Engine.World.World;

using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.Entities
{
    public abstract class Sprite : Entity, IDisposable
    {
        protected List<Animation> animations;
        private int currentAnimationIndex;
        protected bool solid;
        private bool fading;
        private bool fadingIn;
        private int fadingTime;
        private int elapsed;     

        protected BoxCollider collider;

        protected Vector vel;
        protected bool moving;
        protected bool isStatic;
        protected bool breakable;
        protected int health;
        protected bool invincible;
        protected int invincibilityTime;
        private long invincibleExpires;
        protected bool broke;

        protected bool skipPhysics;

        public string Name
        {
            get;
            private set;
        }

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

        public SpriteSheet Sheet => Engine.GetSpriteSheet(SpriteSheetIndex);

        protected Sprite(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional = false) :
            base(engine, origin)
        {
            Name = name;
            SpriteSheetIndex = spriteSheetIndex;
            Directional = directional;

            PaletteIndex = -1;
            Opacity = 1;

            animations = new List<Animation>();
            collider = new BoxCollider(engine.World, CollisionBox);
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
            CheckCollisionWithSprites = reader.ReadBoolean();
            CheckCollisionWithWorld = reader.ReadBoolean();

            vel = new Vector(reader);
            NoClip = reader.ReadBoolean();
            moving = reader.ReadBoolean();
            isStatic = reader.ReadBoolean();
            breakable = reader.ReadBoolean();
            health = reader.ReadInt32();
            invincible = reader.ReadBoolean();
            invincibilityTime = reader.ReadInt32();
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
            writer.Write(CheckCollisionWithSprites);
            writer.Write(CheckCollisionWithWorld);

            vel.Write(writer);
            writer.Write(NoClip);
            writer.Write(moving);
            writer.Write(isStatic);
            writer.Write(breakable);
            writer.Write(health);
            writer.Write(invincible);
            writer.Write(invincibilityTime);
            writer.Write(invincibleExpires);
            writer.Write(broke);
        }

        protected Animation CurrentAnimation => GetAnimation(currentAnimationIndex);

        protected int CurrentAnimationIndex
        {
            get => currentAnimationIndex;
            set
            {
                Animation animation = CurrentAnimation;
                bool animating;
                int animationFrame;
                if (animation != null)
                {
                    animating = animation.Animating;
                    animationFrame = animation.CurrentSequenceIndex;
                    animation.Stop();
                    animation.Visible = false;
                }
                else
                {
                    animating = false;
                    animationFrame = -1;
                }

                currentAnimationIndex = value;
                animation = CurrentAnimation;
                animation.CurrentSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                animation.Animating = animating;
                animation.Visible = true;
            }
        }

        public bool Static => isStatic;

        public bool NoClip
        {
            get;
            set;
        }

        public bool Moving => moving;

        public bool Broke => broke;

        public bool Invincible => invincible;

        public int Health
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
            set => vel = value;
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

        public bool CheckCollisionWithSprites
        {
            get;
            set;
        }

        public bool CheckCollisionWithWorld
        {
            get;
            set;
        }

        public bool BlockedUp => !NoClip && collider.BlockedUp;

        public bool BlockedLeft => !NoClip && collider.BlockedLeft;

        public bool BlockedRight => !NoClip && collider.BlockedRight;

        public bool Landed => !NoClip && collider.Landed && vel.Y >= 0;

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

        protected virtual void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn, ref bool add)
        {
        }

        public override string ToString() => "Sprite [" + Name + ", " + Origin + "]";

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

        public override void Spawn()
        {
            base.Spawn();

            solid = true;

            Dictionary<string, SpriteSheet.FrameSequence>.Enumerator sequences = Sheet.GetFrameSequenceEnumerator();
            int animationIndex = 0;
            while (sequences.MoveNext())
            {
                var pair = sequences.Current;
                SpriteSheet.FrameSequence sequence = pair.Value;
                string frameSequenceName = sequence.Name;
                int initialFrame = 0;
                bool startVisible = false;
                bool startOn = true;
                bool add = true;

                OnCreateAnimation(animationIndex, Sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);

                if (add)
                {
                    if (frameSequenceName != sequence.Name)
                        sequence = Sheet.GetFrameSequence(frameSequenceName);

                    if (Directional)
                    {
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn));
                        animationIndex++;
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn, true, false));
                        animationIndex++;
                    }
                    else
                    {
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn));
                        animationIndex++;
                    }
                }
            }

            vel = Vector.NULL_VECTOR;
            NoClip = false;
            moving = false;
            isStatic = false;
            breakable = true;
            health = DEFAULT_HEALTH;
            invincible = false;
            invincibilityTime = DEFAULT_INVINCIBLE_TIME;
            broke = false;

            currentAnimationIndex = -1;
        }

        protected virtual bool OnTakeDamage(Sprite attacker, MMXBox region, ref int damage) => true;

        protected virtual void OnTakeDamagePost(Sprite attacker, MMXBox region, FixedSingle damage)
        {
        }

        protected virtual void OnHealthChanged(FixedSingle health)
        {
        }

        public void Hurt(Sprite victim, FixedSingle damage)
        {
        }

        public void Hurt(Sprite victim, MMXBox region, int damage)
        {
            if (victim.broke || victim.markedToRemove || health <= 0)
                return;

            MMXBox intersection = region;

            if (!intersection.IsValid())
                return;

            if (!victim.invincible && victim.OnTakeDamage(this, region, ref damage))
            {
                int h = victim.health;
                h -= damage;

                if (h < 0)
                    h = 0;

                victim.health = h;
                victim.OnHealthChanged(h);
                victim.OnTakeDamagePost(this, region, damage);

                if (victim.health == 0)
                    victim.Break();
                else
                    victim.MakeInvincible();
            }
        }

        public void MakeInvincible(int time = 0)
        {
            invincible = true;
            invincibleExpires = Engine.GetEngineTime() + (time <= 0 ? invincibilityTime : time);
        }

        protected virtual bool ShouldCollide(Sprite sprite) => false;

        protected Vector DoCheckCollisionWithSprites(Vector delta)
        {
            Vector result = delta;

            // TODO: Implement

            return result;
        }

        protected override MMXBox GetHitBox() => CollisionBox;

        public MMXBox CollisionBox => Origin + GetCollisionBox();

        protected abstract MMXBox GetCollisionBox();

        protected override MMXBox GetBoundingBox() => DrawBox;

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

            MMXBox lastBox = collider.Box;
            bool wasLanded = collider.Landed;
            bool wasLandedOnSlope = collider.LandedOnSlope;
            RightTriangle lastSlope = collider.LandedSlope;
            MMXBox lastLeftCollider = collider.LeftCollider;
            MMXBox lastRightCollider = collider.RightCollider;

            collider.Translate(dx);

            if (collider.Landed)
                collider.AdjustOnTheFloor(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);
            else if (gravity && wasLanded)
                collider.TryMoveContactSlope(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);

            MMXBox union = deltaX > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
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

        private void DoPhysics()
        {
            collider.Box = CollisionBox;

            bool lastBlockedUp = BlockedUp;
            bool lastBlockedLeft = BlockedLeft;
            bool lastBlockedRight = BlockedRight;
            bool lastLanded = Landed;

            FixedSingle gravity = Gravity;
            if (!skipPhysics)
            {
                if (vel.IsNull && moving)
                    StopMoving();

                Vector delta = !isStatic && !vel.IsNull ? vel : Vector.NULL_VECTOR;

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
                            MMXBox lastBox = collider.Box;
                            MMXBox lastUpCollider = collider.UpCollider;
                            MMXBox lastDownCollider = collider.DownCollider;
                            collider.Translate(dy);

                            if (dy.Y > 0)
                            {
                                MMXBox union = lastDownCollider | collider.DownCollider;
                                if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true)))
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactFloor(dy.Y.Ceil());
                                }
                            }
                            else
                            {
                                MMXBox union = lastUpCollider | collider.UpCollider;
                                if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true)))
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactSolid(dy, (-dy.Y).Ceil(), Direction.UP);
                                }
                            }
                        }

                        delta = collider.Box.Origin - CollisionBox.Origin;
                    }

                    if (!NoClip && CheckCollisionWithSprites)
                        delta = DoCheckCollisionWithSprites(delta);
                }

                if (delta != Vector.NULL_VECTOR)
                {
                    Vector newOrigin = Origin + delta;

                    FixedSingle x = newOrigin.X;
                    FixedSingle y = newOrigin.Y;

                    if (!CanGoOutOfMapBounds)
                    {
                        MMXBox limit = Engine.World.BoundingBox;
                        if (!Engine.noCameraConstraints)
                            limit &= Engine.cameraConstraintsBox.ClipTop(-2 * HITBOX_HEIGHT).ClipBottom(-2 * HITBOX_HEIGHT);

                        MMXBox collisionBox = newOrigin + GetCollisionBox();

                        FixedSingle minX = collisionBox.Left;
                        FixedSingle limitLeft = limit.Left;
                        if (minX < limitLeft)
                            x -= minX - limitLeft;

                        FixedSingle minY = collisionBox.Top;
                        FixedSingle limitTop = limit.Top;
                        if (minY < limitTop)
                            y -= minY - limitTop;
                        
                        FixedSingle maxX = collisionBox.Right;
                        FixedSingle limitRight = limit.Right;
                        if (maxX > limitRight)
                            x += limitRight - maxX;

                        FixedSingle maxY = collisionBox.Bottom;
                        FixedSingle limitBottom = limit.Bottom;
                        if (maxY > limitBottom)
                            y += limitBottom - maxY;
                    }

                    Origin = new Vector(x, y);

                    StartMoving();
                }
                else if (moving)
                {
                    StopMoving();
                }

                if (!NoClip && !isStatic)
                {
                    if (gravity != 0)
                    {
                        vel += gravity * Vector.DOWN_VECTOR;

                        FixedSingle terminalDownwardSpeed = TerminalDownwardSpeed;
                        if (vel.Y > terminalDownwardSpeed)
                            vel = new Vector(vel.X, terminalDownwardSpeed);
                    }

                    if (CheckCollisionWithWorld && collider.Landed && vel.Y > 0)
                        vel = vel.XVector;
                }
            }

            if (!Landed && vel.Y > gravity && vel.Y < 2 * gravity)
                vel = new Vector(vel.X, gravity);

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

        public virtual FixedSingle GetGravity() => Underwater ? UNDERWATER_GRAVITY : GRAVITY;

        protected virtual FixedSingle GetTerminalDownwardSpeed() => Underwater ? UNDERWATER_TERMINAL_DOWNWARD_SPEED : TERMINAL_DOWNWARD_SPEED;

        protected virtual void OnBlockedRight()
        {
        }

        protected virtual void OnBlockedLeft()
        {
        }

        protected virtual void OnBlockedUp()
        {
        }

        protected virtual void OnLanded()
        {
        }

        protected bool CollisionCheck(Sprite sprite) => ShouldCollide(sprite) || sprite.ShouldCollide(this);

        public MMXBox DrawBox => CurrentAnimation != null ? CurrentAnimation.DrawBox : Origin + MMXBox.EMPTY_BOX;

        public float Opacity
        {
            get;
            set;
        }

        public Animation GetAnimation(int index) => animations == null || index < 0 || index >= animations.Count ? null : animations[index];

        protected override bool PreThink()
        {
            skipPhysics = false;
            return base.PreThink();
        }

        protected override void Think()
        {
            if (!isStatic && !Engine.Paused)
                DoPhysics();
        }

        protected override void PostThink()
        {
            base.PostThink();

            if (Engine.Paused)
                return;

            if (invincible && Engine.GetEngineTime() >= invincibleExpires)
                invincible = false;

            foreach (Animation animation in animations)
                animation.OnFrame();
        }

        public virtual void Render()
        {
            if (!alive || markedToRemove)
                return;

            foreach (Animation animation in animations)
                animation.Render();
        }

        internal virtual void OnAnimationEnd(Animation animation)
        {
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
        }

        protected virtual void OnStopMoving()
        {
        }

        protected virtual bool OnBreak() => true;

        protected virtual void OnBroke()
        {
        }

        public void Break()
        {
            if (alive && !broke && !markedToRemove && breakable && OnBreak())
            {
                broke = true;
                OnBroke();
                Kill();
            }
        }

        protected override void OnDeath()
        {
            animations.Clear();
            base.OnDeath();
        }

        internal void OnDeviceReset()
        {
            foreach (var animation in animations)
                animation.OnDeviceReset();
        }
    }
}
