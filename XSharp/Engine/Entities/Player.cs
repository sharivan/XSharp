using System;
using System.IO;

using MMX.Geometry;
using MMX.Math;
using MMX.Engine.Entities.Weapons;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities
{
    public enum PlayerState
    {
        SPAWN = 0,
        SPAWN_END = 1,
        STAND = 2,
        PRE_WALK = 3,
        WALK = 4,
        JUMP = 5,
        GOING_UP = 6,
        FALL = 7,
        LAND = 8,
        PRE_DASH = 9,
        DASH = 10,
        POST_DASH = 11,
        WALL_SLIDE = 12,
        WALL_JUMP = 13,
        PRE_LADDER_CLIMB = 14,
        LADDER = 15,
        TOP_LADDER_CLIMB = 16,
        TOP_LADDER_DESCEND = 17
    }

    public class Player : Sprite
    {
        private bool inputLocked;
        private readonly Keys[] keyBuffer;
        protected bool death;

        private readonly int[,,] animationIndices;

        private bool jumping;
        private bool dashReleased;

        private FixedSingle baseHSpeed;
        private int dashFrameCounter;
        private bool spawing;
        private int wallJumpFrameCounter;

        private bool wasBlockedLeft;
        private bool wasBlockedRight;
        private PlayerState state;
        private Direction stateDirection;
        internal int shots;
        private int shotFrameCounter;
        private bool charging;
        private int chargingFrameCounter;
        internal bool shootingCharged;
        private ChargingEffect chargingEffect;

        private bool spawnSoundPlayed;

        public bool CanWallJump => GetWallJumpDir() != Direction.NONE;

        internal Player(GameEngine engine, string name, Vector origin, int spriteSheetIndex) : base(engine, name, origin, spriteSheetIndex, true)
        {
            CheckCollisionWithWorld = false;

            baseHSpeed = WALKING_SPEED;

            keyBuffer = new Keys[KEY_BUFFER_COUNT];

            animationIndices = new int[ANIMATION_COUNT, 2, 2];
            for (int i = 0; i < ANIMATION_COUNT; i++)
            {
                animationIndices[i, 0, 0] = -1;
                animationIndices[i, 0, 1] = -1;
                animationIndices[i, 1, 0] = -1;
                animationIndices[i, 1, 1] = -1;
            }
        }

        public override void SaveState(BinaryWriter writer)
        {
            base.SaveState(writer);

            writer.Write(KEY_BUFFER_COUNT);
            for (int i = 0; i < KEY_BUFFER_COUNT; i++)
                writer.Write((int) keyBuffer[i]);

            writer.Write(Lives);
            writer.Write(inputLocked);
            writer.Write(death);

            writer.Write(jumping);
            writer.Write(dashReleased);

            baseHSpeed.Write(writer);
            writer.Write(dashFrameCounter);
            writer.Write(spawing);
            writer.Write(WallJumping);
            writer.Write(wallJumpFrameCounter);

            writer.Write(wasBlockedLeft);
            writer.Write(wasBlockedRight);

            writer.Write((int) Direction);
            writer.Write((int) state);
            writer.Write((int) stateDirection);
            writer.Write(Shooting);
            writer.Write(shotFrameCounter);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                keyBuffer[i] = (Keys) reader.ReadInt32();

            Lives = reader.ReadInt32();
            inputLocked = reader.ReadBoolean();
            death = reader.ReadBoolean();

            jumping = reader.ReadBoolean();
            dashReleased = reader.ReadBoolean();

            baseHSpeed = new FixedSingle(reader);
            dashFrameCounter = reader.ReadInt32();
            spawing = reader.ReadBoolean();
            WallJumping = reader.ReadBoolean();
            wallJumpFrameCounter = reader.ReadInt32();

            wasBlockedLeft = reader.ReadBoolean();
            wasBlockedRight = reader.ReadBoolean();

            Direction = (Direction) reader.ReadInt32();
            state = (PlayerState) reader.ReadInt32();
            stateDirection = (Direction) reader.ReadInt32();
            Shooting = reader.ReadBoolean();
            shotFrameCounter = reader.ReadInt32();
        }

        protected override Box GetCollisionBox() => ((-HITBOX_WIDTH * 0.5, -HITBOX_HEIGHT - 4), Vector.NULL_VECTOR, (HITBOX_WIDTH, HITBOX_HEIGHT + 4));

        protected override Box GetHitBox() => new Box(Dashing
                ? (DASHING_HITBOX_OFFSET, (-DASHING_HITBOX_WIDTH * 0.5, -DASHING_HITBOX_HEIGHT * 0.5), (DASHING_HITBOX_WIDTH * 0.5, DASHING_HITBOX_HEIGHT * 0.5))
                : (HITBOX_OFFSET, (-HITBOX_WIDTH * 0.5, -HITBOX_HEIGHT * 0.5), (HITBOX_WIDTH * 0.5, HITBOX_HEIGHT * 0.5))) + GetVector(VectorKind.PLAYER_ORIGIN);

        public bool Shooting
        {
            get;
            private set;
        }

        public bool Dashing => state is PlayerState.PRE_DASH or PlayerState.DASH;

        public bool DashingOnly => state == PlayerState.DASH;

        public bool DashingLeft => (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.LEFT;

        public bool DashingRight => (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.RIGHT;

        public bool PostDashing => state == PlayerState.POST_DASH;

        public bool GoingUp => state == PlayerState.GOING_UP;

        public bool Falling => state == PlayerState.FALL;

        public bool WallSliding => state == PlayerState.WALL_SLIDE;

        public bool WallJumping
        {
            get;
            private set;
        }

        public bool WallJumpingToLeft => state == PlayerState.WALL_JUMP && stateDirection == Direction.LEFT;

        public bool WallJumpingToRight => state == PlayerState.WALL_JUMP && stateDirection == Direction.RIGHT;

        public bool NormalJumping => state == PlayerState.JUMP;

        public bool Jumping => state is PlayerState.JUMP or PlayerState.WALL_JUMP;

        public bool Landing => state == PlayerState.LAND;

        public bool Walking => state is PlayerState.PRE_WALK or PlayerState.WALK;

        public bool PreWalking => state == PlayerState.PRE_WALK;

        public bool WalkingOnly => state == PlayerState.WALK;

        public bool WalkingLeft => (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.LEFT;

        public bool PreWalkingLeft => state == PlayerState.PRE_WALK && stateDirection == Direction.LEFT;

        public bool WalkingLeftOnly => state == PlayerState.WALK && stateDirection == Direction.LEFT;

        public bool WalkingRight => (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.RIGHT;

        public bool PreWalkingRight => state == PlayerState.PRE_WALK && stateDirection == Direction.RIGHT;

        public bool WalkingRightOnly => state == PlayerState.WALK && stateDirection == Direction.RIGHT;

        public bool Spawning => state == PlayerState.SPAWN;

        public bool Standing => state == PlayerState.STAND;

        public bool PreLadderClimbing => state == PlayerState.PRE_LADDER_CLIMB;

        public bool TopLadderClimbing => state == PlayerState.TOP_LADDER_CLIMB;

        public bool TopLadderDescending => state == PlayerState.TOP_LADDER_DESCEND;

        public bool OnLadder => OnLadderOnly || TopLadderDescending || TopLadderClimbing || PreLadderClimbing;

        public bool OnLadderOnly => state == PlayerState.LADDER;

        public bool LadderMoving => Velocity.Y != 0 && OnLadder;

        public bool LadderClimbing => Velocity.Y < 0 && OnLadder;

        public bool LadderDescending => Velocity.Y > 0 && OnLadder;

        public Keys Keys => GetKeys(0);

        public Keys LastKeys => GetLastKeys(0);

        public Keys LastKeysWithoutLatency => GetLastKeys(0);

        public bool InputLocked
        {
            get => inputLocked;

            set => inputLocked = true;
        }

        public int Lives
        {
            get;
            set;
        }

        public bool PressingNothing => Keys == 0;

        public bool PressingNoLeftRight => !PressingLeft && !PressingRight;

        public bool PressingLeft => !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

        public bool WasPressingLeft => !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

        public bool PressingRight => !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

        public bool WasPressingRight => !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

        public bool PressingDown => !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

        public bool WasPressingDown => !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

        public bool PressingUp => !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

        public bool WasPressingUp => !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

        public bool PressingShot => !inputLocked && Keys.HasFlag(Keys.SHOT);

        public bool WasPressingShot => !inputLocked && LastKeys.HasFlag(Keys.SHOT);

        public bool PressingWeapon => !inputLocked && Keys.HasFlag(Keys.WEAPON);

        public bool WasPressingWeapon => !inputLocked && LastKeys.HasFlag(Keys.WEAPON);

        public bool PressingJump => !inputLocked && Keys.HasFlag(Keys.JUMP);

        public bool WasPressingJump => !inputLocked && LastKeys.HasFlag(Keys.JUMP);

        public bool PressingDash => !inputLocked && Keys.HasFlag(Keys.DASH);

        public bool WasPressingDash => !inputLocked && LastKeys.HasFlag(Keys.DASH);

        public bool PressingLWeaponSwitch => !inputLocked && Keys.HasFlag(Keys.LWS);

        public bool WasPressingLWeaponSwitch => !inputLocked && LastKeys.HasFlag(Keys.LWS);

        public bool PressingRWeaponSwitch => !inputLocked && Keys.HasFlag(Keys.RWS);

        public bool WasPressingRWeaponSwitch => !inputLocked && LastKeys.HasFlag(Keys.RWS);

        public bool PressingStart => !inputLocked && Keys.HasFlag(Keys.START);

        public bool WasPressingStart => !inputLocked && LastKeys.HasFlag(Keys.START);

        public bool PressingSelect => !inputLocked && Keys.HasFlag(Keys.SELECT);

        public bool WasPressingSelect => !inputLocked && LastKeys.HasFlag(Keys.SELECT);

        protected void SetState(PlayerState state, int startAnimationIndex = -1) => SetState(state, Direction, startAnimationIndex);

        protected void SetState(PlayerState state, Direction direction, int startAnimationIndex = -1)
        {
            this.state = state;
            stateDirection = direction;
            CurrentAnimationIndex = GetAnimationIndex(state, direction, Shooting && !(TopLadderClimbing || TopLadderDescending || PreLadderClimbing || PreWalking));
            CurrentAnimation.Start(startAnimationIndex);
        }

        protected int GetAnimationIndex(PlayerState state, Direction direction, bool shooting) => animationIndices[(int) state, direction == Direction.LEFT ? 1 : 0, shooting ? 1 : 0];

        public Keys GetKeys(int latency) => keyBuffer[latency];

        public Keys GetLastKeys(int latency) => keyBuffer[latency + 1];

        protected override void OnStartMoving()
        {
        }

        protected override void OnStopMoving()
        {
        }

        protected override void OnBlockedUp()
        {
            if (WallJumping && wallJumpFrameCounter >= 7)
            {
                Velocity = Vector.NULL_VECTOR;
                WallJumping = false;
                jumping = false;
                SetAirStateAnimation(true);
            }
            else
                Velocity = Velocity.XVector;
        }

        protected override void OnBlockedLeft()
        {
            if (Landed)
                SetState(PlayerState.STAND, 0);
            else if (WallJumping && wallJumpFrameCounter >= 7)
            {
                WallJumping = false;
                jumping = false;
                SetAirStateAnimation(true);
            }
        }

        protected override void OnBlockedRight()
        {
            if (Landed)
                SetState(PlayerState.STAND, 0);
            else if (WallJumping && wallJumpFrameCounter >= 7)
            {
                WallJumping = false;
                jumping = false;
                SetAirStateAnimation(true);
            }
        }

        protected override void OnLanded()
        {
            WallJumping = false;
            baseHSpeed = WALKING_SPEED;

            if (!spawing)
            {
                PlaySound(6);

                if (PressingLeft)
                    TryMoveLeft();
                else if (PressingRight)
                    TryMoveRight();
                else
                {
                    Velocity = Vector.NULL_VECTOR;
                    SetState(PlayerState.LAND, 0);
                }
            }
            else
                SetState(PlayerState.SPAWN_END, 0);
        }

        protected override FixedSingle GetTerminalDownwardSpeed() => WallSliding ? WALL_SLIDE_SPEED : base.GetTerminalDownwardSpeed();

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            spawing = true;
            Velocity = TERMINAL_DOWNWARD_SPEED * Vector.DOWN_VECTOR;
            Lives = 2;
            Health = X_BASE_HEALTH;

            ResetKeys();

            SetState(PlayerState.SPAWN, 0);
        }

        private void ResetKeys()
        {
            for (int i = 0; i < keyBuffer.Length; i++)
                keyBuffer[i] = 0;
        }

        protected override void OnDeath()
        {
            Lives--;

            base.OnDeath();

            Engine.OnGameOver();
        }

        private void TryMoveLeft(bool standOnly = false)
        {
            Velocity = !standOnly && !BlockedLeft ? new Vector(-baseHSpeed, Velocity.Y) : new Vector(0, Velocity.Y);

            if (Landed)
            {
                if (standOnly || BlockedLeft)
                {
                    bool wasStanding = Standing;
                    SetState(PlayerState.STAND, Direction.LEFT, !wasStanding ? 0 : -1);
                }
                else
                {
                    if (!Shooting && baseHSpeed == PRE_WALKING_SPEED)
                    {
                        bool wasPreWalkingLeft = PreWalkingLeft;
                        SetState(PlayerState.PRE_WALK, Direction.LEFT, !wasPreWalkingLeft ? 0 : -1);
                    }
                    else
                    {
                        baseHSpeed = GetWalkingSpeed();
                        bool wasWalkingLeftOnly = WalkingLeftOnly;
                        SetState(PlayerState.WALK, Direction.LEFT, !wasWalkingLeftOnly ? 0 : -1);
                    }
                }
            }
            else if (!OnLadder)
            {
                if (!Jumping && !GoingUp)
                {
                    if (BlockedLeft && GetWallJumpDir() == Direction.LEFT)
                    {
                        if (!WallSliding)
                        {
                            Velocity = Vector.NULL_VECTOR;
                            SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
                            PlaySound(6);
                        }
                    }
                    else if (!WallJumping)
                        SetAirStateAnimation();
                }
                else if (!WallJumping)
                    SetAirStateAnimation();
            }
        }

        private void TryMoveRight(bool standOnly = false)
        {
            Velocity = !standOnly && !BlockedRight ? new Vector(baseHSpeed, Velocity.Y) : new Vector(0, Velocity.Y);

            if (Landed)
            {
                if (standOnly || BlockedRight)
                {
                    bool wasStanding = Standing;
                    SetState(PlayerState.STAND, Direction.RIGHT, !wasStanding ? 0 : -1);
                }
                else
                {
                    if (!Shooting && baseHSpeed == PRE_WALKING_SPEED)
                    {
                        bool wasPreWalkingRight = PreWalkingRight;
                        SetState(PlayerState.PRE_WALK, Direction.RIGHT, !wasPreWalkingRight ? 0 : -1);
                    }
                    else
                    {
                        baseHSpeed = GetWalkingSpeed();
                        bool wasWalkingRightOnly = WalkingRightOnly;
                        SetState(PlayerState.WALK, Direction.RIGHT, !wasWalkingRightOnly ? 0 : -1);
                    }
                }
            }
            else if (!OnLadder)
            {
                if (!Jumping && !GoingUp)
                {
                    if (BlockedRight && GetWallJumpDir() == Direction.RIGHT)
                    {
                        if (!WallSliding)
                        {
                            Velocity = Vector.NULL_VECTOR;
                            SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
                            PlaySound(6);
                        }
                    }
                    else if (!WallJumping)
                        SetAirStateAnimation();
                }
                else if (!WallJumping)
                    SetAirStateAnimation();
            }
        }

        protected override bool PreThink()
        {
            wasBlockedLeft = BlockedLeft;
            wasBlockedRight = BlockedRight;
            return base.PreThink();
        }

        private FixedSingle GetWalkingSpeed()
        {
            if (Walking && LandedOnSlope && LandedSlope.HCathetusSign == Velocity.X.Signal)
            {
                RightTriangle slope = LandedSlope;
                FixedSingle ratio = slope.HCathetus / slope.VCathetus;

                if (ratio == 4)
                    return SLOPE_DOWNWARD_WALKING_SPEED_1;

                if (ratio == 2)
                    return SLOPE_DOWNWARD_WALKING_SPEED_2;
            }

            return WALKING_SPEED;
        }

        private FixedSingle GetInitialJumpSpeed()
        {
            if (Walking && LandedOnSlope && baseHSpeed > WALKING_SPEED)
            {
                RightTriangle slope = LandedSlope;
                FixedSingle ratio = slope.HCathetus / slope.VCathetus;

                if (ratio == 4)
                    return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1;

                if (ratio == 2)
                    return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2;
            }

            return INITIAL_UPWARD_SPEED_FROM_JUMP;
        }

        internal void PlaySound(int index) => Engine.PlaySound(0, index);

        protected override void OnBeforeMove(ref Vector origin)
        {
            FixedSingle x = origin.X;
            FixedSingle y = origin.Y;

            Box limit = Engine.World.BoundingBox;
            if (!Engine.noCameraConstraints)
                limit &= Engine.cameraConstraintsBox.ClipTop(-2 * HITBOX_HEIGHT).ClipBottom(-2 * HITBOX_HEIGHT);

            Box collisionBox = origin + GetCollisionBox();

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

            origin = new Vector(x, y);
        }

        protected override void Think()
        {
            base.Think();

            if (Engine.Paused)
            {
                if (PressingStart && !WasPressingStart)
                    Engine.ContinueGame();
            }
            else
            {
                if (Spawning && !CheckCollisionWithWorld)
                {
                    if (!spawnSoundPlayed)
                    {
                        PlaySound(7);
                        spawnSoundPlayed = true;
                    }

                    if (Engine.CurrentCheckpoint != null)
                    {
                        if (GetVector(VectorKind.PLAYER_ORIGIN).Y >= Engine.CurrentCheckpoint.BoundingBox.Top + SCREEN_HEIGHT / 2)
                            CheckCollisionWithWorld = true;
                    }
                    else
                        CheckCollisionWithWorld = true;
                }
                else
                    CheckCollisionWithWorld = true;

                if (NoClip)
                {
                    if (spawing)
                    {
                        spawing = false;
                    }

                    bool mirrored = false;
                    Direction direction = PressingLeft ? Direction.LEFT : PressingRight ? Direction.RIGHT : Direction.NONE;
                    if (direction != Direction.NONE && direction != Direction)
                    {
                        mirrored = true;
                        Direction = direction;
                        RefreshAnimation();
                    }

                    baseHSpeed = PressingDash ? NO_CLIP_SPEED_BOOST : NO_CLIP_SPEED;
                    Velocity = new Vector(mirrored ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, PressingUp ? -baseHSpeed : PressingDown ? baseHSpeed : 0);
                    SetAirStateAnimation();
                }
                else if (!spawing)
                {
                    Direction lastDirection = Direction;

                    if (baseHSpeed == PRE_WALKING_SPEED)
                        baseHSpeed = WALKING_SPEED;

                    if (!WallJumping)
                    {
                        if (!OnLadder)
                        {
                            if (PressingLeft)
                            {
                                Direction = Direction.LEFT;

                                bool mirrored = false;
                                if (lastDirection == Direction.RIGHT && !WalkingRight && !DashingRight)
                                {
                                    mirrored = true;
                                    RefreshAnimation();
                                }

                                if (Standing || PostDashing || WalkingRight || DashingRight)
                                {
                                    baseHSpeed = Standing ? PRE_WALKING_SPEED : WALKING_SPEED;
                                    TryMoveLeft(mirrored);
                                }
                                else if (WalkingLeftOnly && Landed)
                                {
                                    baseHSpeed = GetWalkingSpeed();
                                    TryMoveLeft();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedLeft && !Jumping && !GoingUp && GetWallJumpDir() == Direction.LEFT)
                                    {
                                        if (!WallSliding)
                                        {
                                            Velocity = Vector.NULL_VECTOR;
                                            SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
                                            PlaySound(6);
                                        }
                                        else if (!WallJumping)
                                            TryMoveLeft(mirrored);
                                    }
                                    else if (!WallJumping)
                                        TryMoveLeft(mirrored);
                                }
                            }
                            else if (PressingRight)
                            {
                                Direction = Direction.RIGHT;

                                bool mirrored = false;
                                if (lastDirection == Direction.LEFT && !WalkingLeft && !DashingLeft)
                                {
                                    mirrored = true;
                                    RefreshAnimation();
                                }

                                if (Standing || PostDashing || WalkingLeft || DashingLeft)
                                {
                                    baseHSpeed = Standing ? PRE_WALKING_SPEED : WALKING_SPEED;
                                    TryMoveRight(mirrored);
                                }
                                else if (WalkingRightOnly && Landed)
                                {
                                    baseHSpeed = GetWalkingSpeed();
                                    TryMoveRight();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedRight && !Jumping && !GoingUp && GetWallJumpDir() == Direction.RIGHT)
                                    {
                                        if (!WallSliding)
                                        {
                                            Velocity = Vector.NULL_VECTOR;
                                            SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
                                            PlaySound(6);
                                        }
                                        else if (!WallJumping)
                                            TryMoveRight(mirrored);
                                    }
                                    else if (!WallJumping)
                                        TryMoveRight(mirrored);
                                }
                            }
                            else
                            {
                                if (Landed)
                                {
                                    if (!Standing && !Dashing)
                                    {
                                        if (!WallJumping)
                                            Velocity = new Vector(0, Velocity.Y);

                                        if (!Landing && !PostDashing)
                                            SetState(PlayerState.STAND, 0);
                                    }
                                }
                                else
                                {
                                    if (!WallJumping)
                                        Velocity = new Vector(0, Velocity.Y);

                                    SetAirStateAnimation();
                                }
                            }
                        }

                        if (PressingUp)
                        {
                            if (OnLadderOnly)
                            {
                                if (!Shooting)
                                {
                                    collider.Box = CollisionBox;
                                    Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                    CollisionFlags flags = Engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true);
                                    if (flags.HasFlag(CollisionFlags.TOP_LADDER))
                                    {
                                        if (!TopLadderClimbing && !TopLadderDescending)
                                            SetState(PlayerState.TOP_LADDER_CLIMB, 0);
                                    }
                                    else if (!TopLadderClimbing && !TopLadderDescending)
                                    {
                                        Velocity = new Vector(0, -LADDER_CLIMB_SPEED);
                                        CurrentAnimation.Start();
                                    }
                                }
                            }
                            else if (!OnLadder)
                            {
                                collider.Box = CollisionBox;
                                Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                CollisionFlags flags = Engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true);
                                if (flags.HasFlag(CollisionFlags.LADDER))
                                {
                                    Velocity = Vector.NULL_VECTOR;

                                    collider.Box = collisionBox;
                                    collider.AdjustOnTheLadder();
                                    Vector delta = collider.Box.Origin - collisionBox.Origin;
                                    Origin += delta;

                                    SetState(PlayerState.PRE_LADDER_CLIMB, 0);
                                }
                            }
                        }
                        else if (PressingDown)
                        {
                            if (OnLadderOnly)
                            {
                                if (!Shooting)
                                {
                                    collider.Box = CollisionBox;
                                    Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                    CollisionFlags flags = Engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true);
                                    if (!flags.HasFlag(CollisionFlags.LADDER))
                                    {
                                        if (Landed)
                                        {
                                            if (!Standing)
                                                SetState(PlayerState.LAND, 0);
                                        }
                                        else if (!TopLadderClimbing && !TopLadderDescending)
                                        {
                                            Velocity = Vector.NULL_VECTOR;
                                            SetAirStateAnimation();
                                        }
                                    }
                                    else if (!TopLadderClimbing && !TopLadderDescending)
                                    {
                                        Velocity = new Vector(0, LADDER_CLIMB_SPEED);
                                        CurrentAnimation.Start();
                                    }
                                }
                            }
                            else if (LandedOnTopLadder && !TopLadderDescending && !TopLadderClimbing)
                            {
                                Velocity = Vector.NULL_VECTOR;

                                Box collisionBox = CollisionBox;
                                collider.Box = collisionBox;
                                collider.AdjustOnTheLadder();
                                Vector delta = collider.Box.Origin - collisionBox.Origin;
                                Origin += delta;

                                SetState(PlayerState.TOP_LADDER_DESCEND, 0);
                            }
                        }
                        else if (OnLadderOnly && (WasPressingUp || WasPressingDown))
                        {
                            Velocity = Vector.NULL_VECTOR;
                            CurrentAnimation.Stop();
                        }

                        if (!OnLadder)
                        {
                            if (PressingDash)
                            {
                                if (!WasPressingDash)
                                {
                                    dashReleased = false;
                                    if (Landed && (Direction == Direction.LEFT ? !BlockedLeft : !BlockedRight))
                                    {
                                        baseHSpeed = DASH_SPEED;
                                        dashFrameCounter = 0;
                                        Velocity = new Vector(Direction == Direction.LEFT ? -DASH_SPEED : DASH_SPEED, Velocity.Y);
                                        SetState(PlayerState.PRE_DASH, 0);
                                        PlaySound(4);
                                    }
                                }
                                else if (!Landed && !WallJumping && !WallSliding && !OnLadder)
                                    SetAirStateAnimation();
                            }
                            else if (WasPressingDash && !PressingDash)
                            {
                                if (!dashReleased)
                                {
                                    dashReleased = true;

                                    if (Landed)
                                    {
                                        baseHSpeed = WALKING_SPEED;

                                        if (Dashing)
                                        {
                                            if (PressingLeft && !BlockedLeft)
                                            {
                                                Velocity = new Vector(-baseHSpeed, Velocity.Y);
                                                SetState(PlayerState.WALK, 0);
                                            }
                                            else if (PressingRight && !BlockedRight)
                                            {
                                                Velocity = new Vector(baseHSpeed, Velocity.Y);
                                                SetState(PlayerState.WALK, 0);
                                            }
                                            else
                                            {
                                                Velocity = new Vector(0, Velocity.Y);
                                                SetState(PlayerState.POST_DASH, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!WasPressingJump && PressingJump)
                    {
                        if (collider.Landed || collider.TouchingWaterSurface && !CanWallJump)
                        {
                            bool hspeedNull = false;
                            if (PressingDash)
                                baseHSpeed = DASH_SPEED;
                            else if (baseHSpeed == PRE_WALKING_SPEED)
                            {
                                baseHSpeed = WALKING_SPEED;
                                hspeedNull = true;
                            }

                            if (!BlockedUp)
                            {
                                jumping = true;
                                Velocity = (hspeedNull ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, -GetInitialJumpSpeed());
                                SetState(PlayerState.JUMP, 0);
                                PlaySound(5);
                            }
                            else if (Velocity.Y < 0)
                                Velocity = Velocity.XVector;
                        }
                        else if (OnLadder)
                        {
                            Velocity = Vector.NULL_VECTOR;
                            SetAirStateAnimation();
                        }
                        else if (BlockedUp && Velocity.Y < 0)
                            Velocity = Velocity.XVector;
                        else if (!WallJumping || wallJumpFrameCounter >= 3)
                        {
                            Direction wallJumpDir = GetWallJumpDir();
                            if (wallJumpDir != Direction.NONE)
                            {
                                WallJumping = true;
                                wallJumpFrameCounter = 0;
                                Direction = wallJumpDir;
                                baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;

                                jumping = true;
                                Velocity = Vector.NULL_VECTOR;
                            }
                        }
                    }
                    else if (WasPressingJump && !PressingJump && !WallJumping && jumping && !Landed && !WallSliding && Velocity.Y < 0)
                    {
                        jumping = false;
                        WallJumping = false;
                        Velocity = Velocity.XVector;
                    }

                    if (Dashing)
                    {
                        dashFrameCounter++;
                        if (dashFrameCounter > DASH_DURATION)
                        {
                            baseHSpeed = WALKING_SPEED;
                            if (PressingLeft && !BlockedLeft)
                            {
                                Velocity = new Vector(-baseHSpeed, Velocity.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else if (PressingRight && !BlockedRight)
                            {
                                Velocity = new Vector(baseHSpeed, Velocity.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else
                            {
                                Velocity = Velocity.YVector;
                                SetState(PlayerState.POST_DASH, 0);
                            }
                        }
                    }

                    if (WallJumping)
                    {
                        wallJumpFrameCounter++;
                        if (wallJumpFrameCounter > WALL_JUMP_DURATION)
                        {
                            WallJumping = false;
                            FixedSingle vy;
                            if (!PressingJump)
                            {
                                jumping = false;
                                vy = !Landed && !WallSliding && Velocity.Y < 0 ? 0 : Velocity.Y;
                            }
                            else
                                vy = Velocity.Y;

                            Velocity = new Vector(PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, vy);
                            SetState(PlayerState.GOING_UP, 0);
                        }
                        else if (wallJumpFrameCounter == 7)
                        {
                            baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;
                            Velocity = new Vector(WallJumpingToLeft ? baseHSpeed : -baseHSpeed, -INITIAL_UPWARD_SPEED_FROM_JUMP);
                        }
                        else if (wallJumpFrameCounter < 7)
                        {
                            if (wallJumpFrameCounter == 3)
                            {
                                SetState(PlayerState.WALL_JUMP, 0);
                                PlaySound(5);
                            }

                            Velocity = Vector.NULL_VECTOR;
                        }
                    }
                }

                if (PressingShot)
                {
                    if (!WasPressingShot)
                    {
                        if (shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                        {
                            Shooting = true;
                            shotFrameCounter = 0;

                            if (OnLadderOnly)
                            {
                                Velocity = Vector.NULL_VECTOR;

                                if (PressingLeft)
                                    Direction = Direction.LEFT;
                                else if (PressingRight)
                                    Direction = Direction.RIGHT;

                                RefreshAnimation();
                            }
                            else if (Standing || PreWalking)
                                SetState(PlayerState.STAND, 0);
                            else
                                RefreshAnimation();

                            ShootLemon();
                        }
                    }
                    else
                    {
                        if (!Shooting && !charging && !shootingCharged && shots < MAX_SHOTS)
                        {
                            charging = true;
                            chargingFrameCounter = 0;
                        }

                        if (charging)
                        {
                            chargingFrameCounter++;
                            if (chargingFrameCounter >= 4)
                            {
                                int frame = chargingFrameCounter - 4;
                                PaletteIndex = (frame & 2) is 0 or 1 ? 1 : 0;

                                chargingEffect ??= Engine.StartChargeEffect(this);

                                if (frame == 60)
                                    chargingEffect.Level = 2;
                            }
                        }
                    }
                }
                else if (WasPressingShot && !PressingShot)
                {
                    bool charging = this.charging;
                    int chargingFrameCounter = this.chargingFrameCounter;
                    this.charging = false;
                    this.chargingFrameCounter = 0;

                    PaletteIndex = 0;

                    if (chargingEffect != null)
                    {
                        chargingEffect.Kill();
                        chargingEffect = null;
                    }

                    if (charging && chargingFrameCounter >= 4 && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                    {
                        Shooting = true;
                        shootingCharged = true;
                        shotFrameCounter = 0;

                        if (OnLadderOnly)
                        {
                            Velocity = Vector.NULL_VECTOR;

                            if (PressingLeft)
                                Direction = Direction.LEFT;
                            else if (PressingRight)
                                Direction = Direction.RIGHT;

                            RefreshAnimation();
                        }
                        else if (Standing || PreWalking)
                            SetState(PlayerState.STAND, 0);
                        else
                            RefreshAnimation();

                        if (chargingFrameCounter >= 60)
                            ShootCharged();
                        else
                            ShootSemiCharged();
                    }
                }

                if (Shooting)
                {
                    shotFrameCounter++;
                    if (shotFrameCounter > SHOT_DURATION)
                    {
                        Shooting = false;
                        RefreshAnimation();
                    }
                }

                if (PressingStart && !WasPressingStart)
                    Engine.PauseGame();
            }
        }

        private Vector GetShotOrigin() => state switch
        {
            PlayerState.STAND or PlayerState.LAND => new Vector(9, 9),
            PlayerState.WALK => new Vector(18, 8),
            PlayerState.JUMP or PlayerState.WALL_JUMP or PlayerState.GOING_UP or PlayerState.FALL => new Vector(18, 9),
            PlayerState.PRE_DASH => new Vector(21, -3),
            PlayerState.DASH => new Vector(26, 1),
            PlayerState.POST_DASH => new Vector(24, 9),
            PlayerState.LADDER => new Vector(9, 6),
            _ => new Vector(9, 9),
        };

        public void ShootLemon()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootLemon(this, direction == Direction.RIGHT ? HitBox.RightTop + shotOrigin : HitBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction, baseHSpeed == DASH_SPEED);
        }

        public void ShootSemiCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootSemiCharged(this, direction == Direction.RIGHT ? HitBox.RightTop + shotOrigin : HitBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction);
        }

        public void ShootCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootCharged(this, direction == Direction.RIGHT ? HitBox.RightTop + shotOrigin : HitBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction);
        }

        public Direction GetWallJumpDir()
        {
            Box collisionBox = Collider.LeftCollider.ExtendLeftFixed(8).ClipTop(-2 + (256 - 32) * MASK_SIZE);
            if (Engine.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE, true).HasFlag(CollisionFlags.BLOCK))
                return Direction.LEFT;

            collisionBox = Collider.RightCollider.ExtendRightFixed(8).ClipTop(-2 + (256 - 32) * MASK_SIZE);
            return Engine.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE, true).HasFlag(CollisionFlags.BLOCK)
                ? Direction.RIGHT
                : Direction.NONE;
        }

        private void SetAirStateAnimation(bool forceGoingUp = false)
        {
            if (Velocity.Y >= FALL_ANIMATION_MINIMAL_SPEED)
            {
                if (!Falling)
                {
                    if (WallSliding)
                        baseHSpeed = WALKING_SPEED;

                    SetState(PlayerState.FALL, 0);
                }
            }
            else if (forceGoingUp || !Jumping && !GoingUp)
                SetState(PlayerState.GOING_UP, 0);
        }

        public static Vector GetVectorDir(Direction direction) => direction switch
        {
            Direction.LEFT => Vector.LEFT_VECTOR,
            Direction.UP => Vector.UP_VECTOR,
            Direction.RIGHT => Vector.RIGHT_VECTOR,
            Direction.DOWN => Vector.DOWN_VECTOR,
            _ => Vector.NULL_VECTOR,
        };

        internal void PushKeys(Keys value)
        {
            if (spawing || death)
                return;

            Array.Copy(keyBuffer, 0, keyBuffer, 1, keyBuffer.Length - 1);
            keyBuffer[0] = value;
        }

        private void RefreshAnimation() => CurrentAnimationIndex = GetAnimationIndex(state, Direction, Shooting);

        protected bool ContainsAnimationIndex(PlayerState state, int index, bool directional = false, bool checkShooting = false)
        {
            if (animationIndices[(int) state, 0, 0] == index)
                return true;

            if (checkShooting && animationIndices[(int) state, 0, 1] == index)
                return true;

            if (directional)
            {
                if (animationIndices[(int) state, 1, 0] == index)
                    return true;

                if (checkShooting && animationIndices[(int) state, 1, 1] == index)
                    return true;
            }

            return false;
        }

        internal override void OnAnimationEnd(Animation animation)
        {
            if (ContainsAnimationIndex(PlayerState.SPAWN_END, animation.Index))
            {
                spawing = false;
                SetState(PlayerState.STAND, 0);
            }
            else if (ContainsAnimationIndex(PlayerState.PRE_WALK, animation.Index, true))
            {
                if (Landed && Walking)
                {
                    baseHSpeed = GetWalkingSpeed();

                    if (Direction == Direction.LEFT)
                        TryMoveLeft();
                    else
                        TryMoveRight();
                }
            }
            else if (ContainsAnimationIndex(PlayerState.JUMP, animation.Index, true, true))
                SetState(PlayerState.GOING_UP, 0);
            else if (ContainsAnimationIndex(PlayerState.LAND, animation.Index, true, true))
                SetState(PlayerState.STAND, 0);
            else if (ContainsAnimationIndex(PlayerState.PRE_DASH, animation.Index, true, true))
                SetState(PlayerState.DASH, 0);
            else if (ContainsAnimationIndex(PlayerState.POST_DASH, animation.Index, true, true))
            {
                if (Landed)
                {
                    baseHSpeed = WALKING_SPEED;
                    if (PressingLeft)
                        TryMoveLeft();
                    else if (PressingRight)
                        TryMoveRight();
                    else
                        SetState(PlayerState.STAND, 0);
                }
                else
                    SetAirStateAnimation();
            }
            else if (ContainsAnimationIndex(PlayerState.PRE_LADDER_CLIMB, animation.Index, true))
            {
                SetState(PlayerState.LADDER, 0);

                if (PressingUp)
                    Velocity = new Vector(0, Shooting ? 0 : -LADDER_CLIMB_SPEED);
                else if (PressingDown)
                    Velocity = new Vector(0, Shooting ? 0 : LADDER_CLIMB_SPEED);
                else
                    CurrentAnimation.Stop();

                if (Shooting)
                    CurrentAnimation.Stop();
            }
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animation.Index, true, true))
            {
                Box collisionBox = CollisionBox;
                Vector delta = Origin - collisionBox.Origin;
                collider.Box = collisionBox + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.UP_VECTOR;
                collider.MoveContactFloor(MAP_SIZE);

                Origin = collider.Box.Origin + delta;

                Velocity = Vector.NULL_VECTOR;
                SetState(PlayerState.STAND, 0);
            }
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animation.Index, true, true))
            {
                Origin += (HITBOX_HEIGHT - LADDER_BOX_VCLIP + MAP_SIZE) * Vector.DOWN_VECTOR;

                SetState(PlayerState.LADDER, 0);

                if (PressingUp)
                    Velocity = new Vector(0, Shooting ? 0 : -LADDER_CLIMB_SPEED);
                else if (PressingDown)
                    Velocity = new Vector(0, Shooting ? 0 : LADDER_CLIMB_SPEED);
                else
                    CurrentAnimation.Stop();

                if (Shooting)
                    CurrentAnimation.Stop();
            }
        }

        private void SetAnimationIndex(PlayerState state, int animationIndex, bool directional, bool shooting)
        {
            if (shooting)
            {
                animationIndices[(int) state, 0, 1] = animationIndex;

                if (directional)
                    animationIndices[(int) state, 1, 1] = animationIndex + 1;
            }
            else
            {
                animationIndices[(int) state, 0, 0] = animationIndex;

                if (directional)
                    animationIndices[(int) state, 1, 0] = animationIndex + 1;
            }
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);
            startOn = false; // Por padrão, a animação de um jogador começa parada.
            startVisible = false;

            switch (frameSequenceName)
            {
                case "Spawn":
                    SetAnimationIndex(PlayerState.SPAWN, animationIndex, false, false);
                    break;

                case "SpawnEnd":
                    SetAnimationIndex(PlayerState.SPAWN_END, animationIndex, false, false);
                    break;

                case "Stand":
                    SetAnimationIndex(PlayerState.STAND, animationIndex, true, false);
                    break;

                case "Shooting":
                    SetAnimationIndex(PlayerState.STAND, animationIndex, true, true);
                    break;

                case "PreWalking":
                    SetAnimationIndex(PlayerState.PRE_WALK, animationIndex, true, false);
                    break;

                case "Walking":
                    SetAnimationIndex(PlayerState.WALK, animationIndex, true, false);
                    break;

                case "ShootWalking":
                    SetAnimationIndex(PlayerState.WALK, animationIndex, true, true);
                    break;

                case "Jumping":
                    SetAnimationIndex(PlayerState.JUMP, animationIndex, true, false);
                    break;

                case "ShootJumping":
                    SetAnimationIndex(PlayerState.JUMP, animationIndex, true, true);
                    break;

                case "GoingUp":
                    SetAnimationIndex(PlayerState.GOING_UP, animationIndex, true, false);
                    break;

                case "ShootGoingUp":
                    SetAnimationIndex(PlayerState.GOING_UP, animationIndex, true, true);
                    break;

                case "Falling":
                    SetAnimationIndex(PlayerState.FALL, animationIndex, true, false);
                    break;

                case "ShootFalling":
                    SetAnimationIndex(PlayerState.FALL, animationIndex, true, true);
                    break;

                case "Landing":
                    SetAnimationIndex(PlayerState.LAND, animationIndex, true, false);
                    break;

                case "ShootLanding":
                    SetAnimationIndex(PlayerState.LAND, animationIndex, true, true);
                    break;

                case "PreDashing":
                    SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, true, false);
                    break;

                case "ShootPreDashing":
                    SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, true, true);
                    break;

                case "Dashing":
                    SetAnimationIndex(PlayerState.DASH, animationIndex, true, false);
                    break;

                case "ShootDashing":
                    SetAnimationIndex(PlayerState.DASH, animationIndex, true, true);
                    break;

                case "PostDashing":
                    SetAnimationIndex(PlayerState.POST_DASH, animationIndex, true, false);
                    break;

                case "ShootPostDashing":
                    SetAnimationIndex(PlayerState.POST_DASH, animationIndex, true, true);
                    break;

                case "WallSliding":
                    SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, true, false);
                    break;

                case "ShootWallSliding":
                    SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, true, true);
                    break;

                case "WallJumping":
                    SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, true, false);
                    break;

                case "ShootWallJumping":
                    SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, true, true);
                    break;

                case "PreLadderClimbing":
                    SetAnimationIndex(PlayerState.PRE_LADDER_CLIMB, animationIndex, true, false);
                    break;

                case "LadderMoving":
                    SetAnimationIndex(PlayerState.LADDER, animationIndex, true, false);
                    break;

                case "ShootLadder":
                    SetAnimationIndex(PlayerState.LADDER, animationIndex, true, true);
                    break;

                case "TopLadderClimbing":
                    SetAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animationIndex, true, false);
                    break;

                case "TopLadderDescending":
                    SetAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animationIndex, true, false);
                    break;

                default:
                    add = false;
                    break;
            }
        }

        public override FixedSingle GetGravity() => WallJumping && wallJumpFrameCounter < 7 || OnLadder ? (FixedSingle) 0 : base.GetGravity();
    }
}
