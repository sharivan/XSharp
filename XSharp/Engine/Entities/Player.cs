using System;
using System.IO;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.World;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities
{
    public enum PlayerState
    {
        NONE = -1,
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
        TOP_LADDER_DESCEND = 17,
        TAKING_DAMAGE = 18,
        DYING = 19,
        VICTORY = 20,
        PRE_TELEPORTING = 21,
        TELEPORTING = 22
    }

    // TODO : This class needs a huge refactor
    public class Player : Sprite
    {
        private int lives;

        private readonly Keys[] keyBuffer = new Keys[KEY_BUFFER_COUNT];
        protected bool death;

        private readonly int[,] animationIndices = new int[Enum.GetNames(typeof(PlayerState)).Length, 3];

        private bool jumping;
        private bool dashReleased;
        private bool teleporting;

        private FixedSingle baseHSpeed = WALKING_SPEED;
        private int dashFrameCounter;
        private bool spawning;
        private int wallJumpFrameCounter;
        private int wallSlideFrameCounter;

        private bool wasBlockedLeft;
        private bool wasBlockedRight;
        private PlayerState state = PlayerState.NONE;
        private PlayerState forcedState = PlayerState.NONE;
        private Direction stateDirection;
        internal int shots;
        private int shotFrameCounter;
        private bool charging;
        private int chargingFrameCounter;
        private int chargingFrameCounter2;
        internal bool shootingCharged;

        private bool spawnSoundPlayed;

        private DashSparkEffect dashSparkEffect = null;

        public bool CanWallJump => GetWallJumpDir() != Direction.NONE;

        public bool CrossingBossDoor
        {
            get;
            private set;
        }

        public bool CanGoOutOfCameraBounds
        {
            get;
            set;
        } = false;

        public PlayerState ForcedState
        {
            get => forcedState;
            set
            {
                forcedState = value;
                SetState(forcedState, 0);
            }
        }

        public PlayerState ForcedStateException
        {
            get;
            set;
        } = PlayerState.NONE;

        internal Player()
        {
            SpriteSheetIndex = 0;
            Directional = true;
        }

        public override void SaveState(BinaryWriter writer)
        {
            base.SaveState(writer);

            for (int i = 0; i < KEY_BUFFER_COUNT; i++)
                writer.Write((int) keyBuffer[i]);

            writer.Write(Lives);
            writer.Write(InputLocked);
            writer.Write(death);

            writer.Write(jumping);
            writer.Write(dashReleased);

            baseHSpeed.Write(writer);
            writer.Write(dashFrameCounter);
            writer.Write(spawning);
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

            for (int i = 0; i < KEY_BUFFER_COUNT; i++)
                keyBuffer[i] = (Keys) reader.ReadInt32();

            Lives = reader.ReadInt32();
            InputLocked = reader.ReadBoolean();
            death = reader.ReadBoolean();

            jumping = reader.ReadBoolean();
            dashReleased = reader.ReadBoolean();

            baseHSpeed = new FixedSingle(reader);
            dashFrameCounter = reader.ReadInt32();
            spawning = reader.ReadBoolean();
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

        protected override Box GetCollisionBox()
        {
            return COLLISION_BOX;
        }

        protected override Box GetHitbox()
        {
            return Dashing ? DASHING_HITBOX : HITBOX;
        }

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

        public bool PlayerSpawning => state == PlayerState.SPAWN;

        public bool Teleporting => state == PlayerState.TELEPORTING;

        public bool VictoryPosing => state == PlayerState.VICTORY;

        public bool Standing => state == PlayerState.STAND;

        public bool PreLadderClimbing => state == PlayerState.PRE_LADDER_CLIMB;

        public bool TopLadderClimbing => state == PlayerState.TOP_LADDER_CLIMB;

        public bool TopLadderDescending => state == PlayerState.TOP_LADDER_DESCEND;

        public bool OnLadder => OnLadderOnly || TopLadderDescending || TopLadderClimbing || PreLadderClimbing;

        public bool OnLadderOnly => state == PlayerState.LADDER;

        public bool LadderMoving => Velocity.Y != 0 && OnLadder;

        public bool LadderClimbing => Velocity.Y < 0 && OnLadder;

        public bool LadderDescending => Velocity.Y > 0 && OnLadder;

        public bool TakingDamage => state == PlayerState.TAKING_DAMAGE;

        public int TakingDamageFrameCounter
        {
            get;
            private set;
        } = 0;

        public bool Dying => state == PlayerState.DYING;

        public bool Freezed
        {
            get;
            private set;
        }

        public Keys Keys => GetKeys(0);

        public Keys LastKeys => GetLastKeys(0);

        public Keys LastKeysWithoutLatency => GetLastKeys(0);

        public bool InputLocked
        {
            get;
            set;
        }

        public int Lives
        {
            get => lives;
            set
            {
                if (value is < MIN_LIVES or > MAX_LIVES)
                    return;

                lives = value;
            }
        }

        public bool Tired => Health / Engine.HealthCapacity < X_TIRED_PERCENTAGE;

        public bool PressingNothing => InputLocked || Keys == 0;

        public bool PressingNoLeftRight => !InputLocked && !PressingLeft && !PressingRight;

        public bool PressingLeft => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

        public bool WasPressingLeft => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);

        public bool PressedLeft => !WasPressingLeft && PressingLeft;

        public bool PressingRight => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

        public bool WasPressingRight => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);

        public bool PressedRight => !WasPressingRight && PressingRight;

        public bool PressingDown => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

        public bool WasPressingDown => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);

        public bool PressedDown => !WasPressingDown && PressingDown;

        public bool PressingUp => !InputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

        public bool WasPressingUp => !InputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);

        public bool PressedUp => !WasPressingUp && PressingUp;

        public bool PressingShot => !InputLocked && Keys.HasFlag(Keys.SHOT);

        public bool WasPressingShot => !InputLocked && LastKeys.HasFlag(Keys.SHOT);

        public bool PressedShot => !WasPressingShot && PressingShot;

        public bool PressingWeapon => !InputLocked && Keys.HasFlag(Keys.WEAPON);

        public bool WasPressingWeapon => !InputLocked && LastKeys.HasFlag(Keys.WEAPON);

        public bool PressedWeapon => !WasPressingWeapon && PressingWeapon;

        public bool PressingJump => !InputLocked && Keys.HasFlag(Keys.JUMP);

        public bool WasPressingJump => !InputLocked && LastKeys.HasFlag(Keys.JUMP);

        public bool PressedJump => !WasPressingJump && PressingJump;

        public bool PressingDash => !InputLocked && Keys.HasFlag(Keys.DASH);

        public bool WasPressingDash => !InputLocked && LastKeys.HasFlag(Keys.DASH);

        public bool PressedDash => !WasPressingDash && PressingDash;

        public bool PressingLWeaponSwitch => !InputLocked && Keys.HasFlag(Keys.LWS);

        public bool WasPressingLWeaponSwitch => !InputLocked && LastKeys.HasFlag(Keys.LWS);

        public bool PressedLWeaponSwitch => !WasPressingLWeaponSwitch && PressingLWeaponSwitch;

        public bool PressingRWeaponSwitch => !InputLocked && Keys.HasFlag(Keys.RWS);

        public bool WasPressingRWeaponSwitch => !InputLocked && LastKeys.HasFlag(Keys.RWS);

        public bool PressedRWeaponSwitch => !WasPressingRWeaponSwitch && PressingRWeaponSwitch;

        public bool PressingStart => !InputLocked && Keys.HasFlag(Keys.START);

        public bool WasPressingStart => !InputLocked && LastKeys.HasFlag(Keys.START);

        public bool PressedStart => !WasPressingStart && PressingStart;

        public bool PressingSelect => !InputLocked && Keys.HasFlag(Keys.SELECT);

        public bool WasPressingSelect => !InputLocked && LastKeys.HasFlag(Keys.SELECT);

        public bool PressedSelect => !WasPressingSelect && PressingSelect;

        public bool DyeByAbiss
        {
            get;
            private set;
        }

        public ChargingEffect ChargingEffect
        {
            get;
            private set;
        }

        protected override FixedSingle GetTerminalDownwardSpeed()
        {
            return spawning || teleporting ?
                TELEPORT_SPEED :
                WallSliding ?
                (Underwater ? UNDERWATER_WALL_SLIDE_SPEED : WALL_SLIDE_SPEED) :
                base.GetTerminalDownwardSpeed();
        }

        protected void SetState(PlayerState state, int startAnimationIndex = -1)
        {
            SetState(state, Direction, startAnimationIndex);
        }

        protected void SetState(PlayerState state, Direction direction, int startAnimationIndex = -1)
        {
            if (state == PlayerState.NONE || forcedState != PlayerState.NONE && state != ForcedStateException && state != forcedState)
                return;

            if (state == PlayerState.STAND)
                TakingDamageFrameCounter = 0;

            this.state = state;
            stateDirection = direction;
            CurrentAnimationIndex = GetAnimationIndex(state, Shooting);
            CurrentAnimation.Start(startAnimationIndex);
        }

        protected int GetAnimationIndex(PlayerState state, bool shooting)
        {
            return animationIndices[(int) state, shooting && !(spawning || teleporting || TopLadderClimbing || TopLadderDescending || PreLadderClimbing || PreWalking || TakingDamage || Dying || VictoryPosing) ? 1 : state == PlayerState.STAND && Tired ? 2 : 0];
        }

        public Keys GetKeys(int latency)
        {
            return keyBuffer[latency];
        }

        public Keys GetLastKeys(int latency)
        {
            return keyBuffer[latency + 1];
        }

        protected override void OnStartMoving()
        {
        }

        protected override void OnStopMoving()
        {
        }

        protected override void OnBlockedUp()
        {
            if (!CrossingBossDoor && !teleporting && !TakingDamage && !Dying && WallJumping && wallJumpFrameCounter >= 7)
            {
                Velocity = Vector.NULL_VECTOR;
                WallJumping = false;
                jumping = false;
                SetAirStateAnimation(true);
            }
            else
                Velocity = Velocity.XVector;
        }

        public void SetStandState(Direction direction, int startAnimationIndex = 0)
        {
            if (LandedOnTopLadder)
            {
                Box collisionBox = CollisionBox;
                collider.Box = collisionBox;
                collider.AdjustOnTheFloor(QUERY_MAX_DISTANCE);
                Vector delta = collider.Box.Origin - collisionBox.Origin;
                Origin += delta;
            }

            Velocity = Vector.NULL_VECTOR;
            SetState(PlayerState.STAND, direction, startAnimationIndex);
        }

        public void SetStandState(int startAnimationIndex = 0)
        {
            SetStandState(Direction, startAnimationIndex);
        }

        public void StopMoving()
        {
            WallJumping = false;
            jumping = false;
            Velocity = Vector.NULL_VECTOR;

            if (Landed)
                SetStandState();
            else
                SetAirStateAnimation();
        }

        protected override void OnBlockedLeft()
        {
            if (!CrossingBossDoor && !teleporting && !Dying)
            {
                if (TakingDamage)
                {
                    if (PressingLeft && TakingDamageFrameCounter >= 4)
                        OnKnockbackEnd();
                }
                else if (Landed)
                    SetStandState();
                /*else if (WallJumping && wallJumpFrameCounter >= 7)
                {
                    WallJumping = false;
                    SetAirStateAnimation(true);
                }*/
            }
        }

        protected override void OnBlockedRight()
        {
            if (!CrossingBossDoor && !teleporting && !Dying)
            {
                if (TakingDamage)
                {
                    if (PressingRight && TakingDamageFrameCounter >= 4)
                        OnKnockbackEnd();
                }
                else if (Landed)
                    SetStandState();
                /*else if (WallJumping && wallJumpFrameCounter >= 7)
                {
                    WallJumping = false;
                    SetAirStateAnimation(true);
                }*/
            }
        }

        protected override void OnLanded()
        {
            WallJumping = false;
            baseHSpeed = WALKING_SPEED;

            if (CrossingBossDoor || Dying || teleporting)
                return;

            if (!spawning)
            {
                if (!TakingDamage)
                {
                    if (!ContainsAnimationIndex(PlayerState.LAND, CurrentAnimationIndex, true))
                        PlaySound(6);

                    if (PressingLeft)
                        TryMoveLeft();
                    else if (PressingRight)
                        TryMoveRight();
                    else
                        SetState(PlayerState.LAND, 0);
                }
                else
                    Velocity = Velocity.XVector;
            }
            else
                SetState(PlayerState.SPAWN_END, 0);
        }

        protected override FixedSingle GetSideColliderTopClip()
        {
            return 1;
        }

        protected override FixedSingle GetSideColliderBottomClip()
        {
            return 9;
        }

        protected override bool IsUsingCollisionPlacements()
        {
            return true;
        }

        protected override void OnOffScreen()
        {
            if (teleporting)
                Engine.OnPlayerTeleported();
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            spawning = true;

            CheckCollisionWithWorld = false;
            CheckCollisionWithSolidSprites = false;
            PaletteIndex = 0;
            Velocity = TELEPORT_SPEED * Vector.DOWN_VECTOR;
            Lives = X_INITIAL_LIVES;
            Health = Engine.HealthCapacity;
            Freezed = false;
            CrossingBossDoor = false;
            CanGoOutOfCameraBounds = false;
            DyeByAbiss = false;
            TakingDamageFrameCounter = 0;

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
            // TODO : Implement the remaining

            Lives--;

            base.OnDeath();
        }

        private void TryMoveLeft(bool standOnly = false)
        {
            Velocity = !standOnly && !BlockedLeft ? new Vector(-baseHSpeed, Velocity.Y) : new Vector(0, Velocity.Y);

            if (Landed)
            {
                if (standOnly || BlockedLeft)
                {
                    bool wasStanding = Standing;
                    SetStandState(Direction.LEFT, !wasStanding ? 0 : -1);
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
                            wallSlideFrameCounter = 0;
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
                    SetStandState(Direction.RIGHT, !wasStanding ? 0 : -1);
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
                            wallSlideFrameCounter = 0;
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
            if (Walking)
            {
                if (baseHSpeed == SLOPE_DOWNWARD_WALKING_SPEED_1)
                    return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1;

                if (baseHSpeed == SLOPE_DOWNWARD_WALKING_SPEED_2)
                    return INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2;
            }

            return INITIAL_UPWARD_SPEED_FROM_JUMP;
        }

        internal void PlaySound(int index, bool ignoreUpdatesUntilFinished = false)
        {
            Engine.PlaySound(0, index, ignoreUpdatesUntilFinished);
        }

        protected override void OnBeforeMove(ref Vector origin)
        {
            Box limit = Engine.World.BoundingBox;
            if (!CanGoOutOfCameraBounds && !CrossingBossDoor && !Engine.NoCameraConstraints && !Engine.World.Camera.NoConstraints)
                limit &= Engine.CameraConstraintsBox.ClipTop(-2 * BLOCK_SIZE).ClipBottom(-2 * BLOCK_SIZE);

            Vector delta = origin - Origin;
            origin = limit.RestrictIn(CollisionBox + delta).Origin;
        }

        protected override void Think()
        {
            base.Think();

            if (Engine.Paused)
            {
                if (PressedStart)
                    Engine.ContinueGame();
            }
            else if (!CrossingBossDoor && !Dying && !teleporting && !VictoryPosing)
            {
                if (spawning)
                {
                    if (!spawnSoundPlayed)
                    {
                        PlaySound(7);
                        spawnSoundPlayed = true;
                    }

                    if (!CheckCollisionWithWorld)
                    {
                        if (Engine.CurrentCheckpoint != null)
                        {
                            if (Origin.Y >= Engine.CurrentCheckpoint.Hitbox.Top + SCREEN_HEIGHT * 0.5)
                            {
                                CheckCollisionWithWorld = true;
                                CheckCollisionWithSolidSprites = true;
                            }
                            else
                            {
                                CheckCollisionWithWorld = false;
                                CheckCollisionWithSolidSprites = false;
                            }
                        }
                    }
                }
                else
                {
                    CheckCollisionWithWorld = true;
                    CheckCollisionWithSolidSprites = true;
                }

                if (NoClip)
                {
                    spawning = false;

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
                else if (!spawning)
                {
                    if (Origin.Y > Engine.World.Camera.RightBottom.Y + BLOCK_SIZE)
                    {
                        DyeByAbiss = true;
                        Die();
                        return;
                    }

                    Direction lastDirection = Direction;

                    if (baseHSpeed == PRE_WALKING_SPEED)
                        baseHSpeed = WALKING_SPEED;

                    if (!TakingDamage)
                    {
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
                                                wallSlideFrameCounter = 0;
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
                                                wallSlideFrameCounter = 0;
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
                                                SetStandState();
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
                                        Box ladderCollisionBox = Hitbox.ClipTop(HITBOX.Height - 5);
                                        CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites);
                                        if (flags.HasFlag(CollisionFlags.TOP_LADDER))
                                        {
                                            if (!TopLadderClimbing && !TopLadderDescending)
                                            {
                                                Box collisionBox = CollisionBox;
                                                collider.Box = collisionBox + LADDER_MOVE_OFFSET * Vector.UP_VECTOR;
                                                collider.AdjustOnTheFloor(MAP_SIZE);
                                                Vector delta = collider.Box.Origin - collisionBox.Origin;
                                                Origin += delta;
                                                Velocity = Vector.NULL_VECTOR;
                                                SetState(PlayerState.TOP_LADDER_CLIMB, 0);
                                            }
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
                                    Box ladderCollisionBox = Hitbox.ClipTop(15);
                                    CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites);
                                    if (flags.HasFlag(CollisionFlags.LADDER))
                                    {
                                        Velocity = Vector.NULL_VECTOR;

                                        Box collisionBox = CollisionBox;
                                        collider.Box = CollisionBox;
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
                                        if (collider.Landed)
                                        {
                                            if (!Standing)
                                                SetState(PlayerState.LAND, 0);
                                        }
                                        else
                                        {
                                            Box ladderCollisionBox = Hitbox.ClipTop(15);
                                            CollisionFlags flags = Engine.World.GetCollisionFlags(ladderCollisionBox, CollisionFlags.NONE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites);
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
                            else if (OnLadderOnly)
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
                                else if (Dashing)
                                {
                                    if (!dashReleased)
                                    {
                                        dashReleased = true;

                                        if (Landed)
                                        {
                                            baseHSpeed = WALKING_SPEED;

                                            if (Dashing)
                                            {
                                                if (dashSparkEffect != null)
                                                {
                                                    dashSparkEffect.KillOnNextFrame();
                                                    dashSparkEffect = null;
                                                }

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

                        if (PressedJump)
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
                        else if (!PressingJump && !WallJumping && jumping && !Landed && !WallSliding && Velocity.Y < 0)
                        {
                            jumping = false;
                            WallJumping = false;
                            Velocity = Velocity.XVector;
                        }
                    }

                    if (Dashing)
                    {
                        if (dashFrameCounter == 0)
                            dashSparkEffect = Engine.StartDashSparkEffect(this);

                        dashFrameCounter++;

                        if (dashFrameCounter % 4 == 0 && !Underwater)
                            Engine.StartDashSmokeEffect(this);

                        if (dashFrameCounter > DASH_DURATION)
                        {
                            if (dashSparkEffect != null)
                            {
                                dashSparkEffect.KillOnNextFrame();
                                dashSparkEffect = null;
                            }

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
                    else if (dashSparkEffect != null)
                    {
                        dashSparkEffect.KillOnNextFrame();
                        dashSparkEffect = null;
                    }

                    if (WallJumping)
                    {
                        wallJumpFrameCounter++;
                        collider.Box = CollisionBox;

                        if (wallJumpFrameCounter < 7)
                        {
                            if (wallJumpFrameCounter == 3)
                                SetState(PlayerState.WALL_JUMP, 0);
                            else if (wallJumpFrameCounter == 4)
                            {
                                PlaySound(5);
                                Engine.StartWallKickEffect(this);
                            }

                            Velocity = Vector.NULL_VECTOR;
                        }
                        else if (wallJumpFrameCounter == 7)
                        {
                            baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;
                            Velocity = new Vector(WallJumpingToLeft ? baseHSpeed : -baseHSpeed, -INITIAL_UPWARD_SPEED_FROM_JUMP);
                        }
                        else if (/*collider.Landed || */wallJumpFrameCounter > WALL_JUMP_DURATION)
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
                    }

                    if (WallSliding)
                    {
                        wallSlideFrameCounter++;
                        Velocity = wallSlideFrameCounter < 8 ? Vector.NULL_VECTOR : (Underwater ? UNDERWATER_WALL_SLIDE_SPEED : WALL_SLIDE_SPEED) * Vector.DOWN_VECTOR;
                        baseHSpeed = WALKING_SPEED;

                        if (wallSlideFrameCounter >= 11)
                        {
                            int diff = wallSlideFrameCounter - 11;
                            if (!Underwater && diff % 4 == 0)
                                Engine.StartWallSlideEffect(this);
                        }
                    }

                    if (TakingDamage)
                        TakingDamageFrameCounter++;
                }

                if (!TakingDamage && !InputLocked)
                {
                    if (PressingShot)
                    {
                        if (!WasPressingShot)
                        {
                            if (!spawning && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
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
                                    SetStandState();
                                else
                                    RefreshAnimation();

                                ShootLemon();
                            }
                        }
                        else if (!Shooting && !charging && !shootingCharged && shots < MAX_SHOTS)
                        {
                            charging = true;
                            chargingFrameCounter = 0;
                            chargingFrameCounter2 = 0;
                        }
                    }

                    if (!InputLocked && charging && !PressingShot)
                    {
                        bool charging = this.charging;
                        int chargingFrameCounter = this.chargingFrameCounter;
                        this.charging = false;
                        this.chargingFrameCounter = 0;
                        chargingFrameCounter2 = 0;

                        PaletteIndex = 0;

                        if (ChargingEffect != null)
                        {
                            ChargingEffect.KillOnNextFrame();
                            ChargingEffect = null;
                        }

                        if (!spawning && charging && chargingFrameCounter >= 4 && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
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
                                SetStandState();
                            else
                                RefreshAnimation();

                            if (chargingFrameCounter >= 60)
                                ShootCharged();
                            else
                                ShootSemiCharged();
                        }
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

                if (PressedStart)
                    Engine.PauseGame();
            }

            if (charging)
            {
                if (!InputLocked)
                    chargingFrameCounter++;

                chargingFrameCounter2++;

                if (chargingFrameCounter >= 4)
                {
                    int frame = chargingFrameCounter2 - 4;
                    PaletteIndex = (frame & 2) is 0 or 1 ? 1 : 0;

                    ChargingEffect ??= Engine.StartChargingEffect(this);

                    if (chargingFrameCounter == 64)
                        ChargingEffect.Level = 2;
                }
            }
        }

        private Vector GetShotOrigin()
        {
            return state switch
            {
                PlayerState.STAND or PlayerState.LAND => new Vector(7, 9),
                PlayerState.WALK => new Vector(16, 8),
                PlayerState.JUMP or PlayerState.WALL_JUMP or PlayerState.GOING_UP or PlayerState.FALL => new Vector(16, 9),
                PlayerState.PRE_DASH => new Vector(19, -3),
                PlayerState.DASH => new Vector(24, 1),
                PlayerState.POST_DASH => new Vector(22, 9),
                PlayerState.LADDER => new Vector(7, 6),
                PlayerState.WALL_SLIDE when wallSlideFrameCounter >= 11 => new Vector(7, 11),
                _ => new Vector(7, 9),
            };
        }

        public void ShootLemon()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootLemon(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), baseHSpeed == DASH_SPEED);
        }

        public void ShootSemiCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootSemiCharged(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y));
        }

        public void ShootCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX.Width * 0.5, LEMON_HITBOX.Height * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            Engine.ShootCharged(this, direction == Direction.RIGHT ? Hitbox.RightTop + shotOrigin : Hitbox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y));
        }

        private bool CanWallJumpLeft()
        {
            Box collisionBox = Collider.LeftCollider.ExtendLeftFixed(8).ClipTop(-2 + (256 - 32) * MASK_SIZE);
            return Engine.World.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites).HasFlag(CollisionFlags.BLOCK);
        }

        private bool CanWallJumpRight()
        {
            Box collisionBox = Collider.RightCollider.ExtendRightFixed(8).ClipTop(-2 + (256 - 32) * MASK_SIZE);
            return Engine.World.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE | CollisionFlags.UNCLIMBABLE, this, CheckCollisionWithWorld, CheckCollisionWithSolidSprites).HasFlag(CollisionFlags.BLOCK);
        }

        public Direction GetWallJumpDir()
        {
            bool cwjl = CanWallJumpLeft();
            bool cwjr = CanWallJumpRight();

            return PressingLeft && cwjl
                ? Direction.LEFT
                : PressingRight && cwjr ? Direction.RIGHT : cwjr ? Direction.RIGHT : cwjl ? Direction.LEFT : Direction.NONE;
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

        public static Vector GetVectorDir(Direction direction)
        {
            return direction switch
            {
                Direction.LEFT => Vector.LEFT_VECTOR,
                Direction.UP => Vector.UP_VECTOR,
                Direction.RIGHT => Vector.RIGHT_VECTOR,
                Direction.DOWN => Vector.DOWN_VECTOR,
                _ => Vector.NULL_VECTOR,
            };
        }

        internal void PushKeys(Keys value)
        {
            if (death)
                return;

            Array.Copy(keyBuffer, 0, keyBuffer, 1, keyBuffer.Length - 1);
            keyBuffer[0] = value;
        }

        private void RefreshAnimation()
        {
            CurrentAnimationIndex = GetAnimationIndex(state, Shooting);
        }

        protected bool ContainsAnimationIndex(PlayerState state, int index, bool checkShooting = false)
        {
            return animationIndices[(int) state, 0] == index || checkShooting && animationIndices[(int) state, 1] == index;
        }

        private void OnKnockbackEnd()
        {
            baseHSpeed = WALKING_SPEED;
            if (PressingLeft)
                TryMoveLeft();
            else if (PressingRight)
                TryMoveRight();
            else if (Landed)
                SetStandState();
            else
                SetAirStateAnimation();

            MakeInvincible(60, true);
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            if (CrossingBossDoor && !ContainsAnimationIndex(PlayerState.PRE_DASH, animation.Index, true))
                return;

            if (ContainsAnimationIndex(PlayerState.SPAWN_END, animation.Index))
            {
                spawning = false;
                SetStandState();
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
            else if (ContainsAnimationIndex(PlayerState.JUMP, animation.Index, true))
                SetState(PlayerState.GOING_UP, 0);
            else if (ContainsAnimationIndex(PlayerState.LAND, animation.Index, true))
                SetStandState();
            else if (ContainsAnimationIndex(PlayerState.PRE_DASH, animation.Index, true))
            {
                if (dashSparkEffect != null)
                    dashSparkEffect.State = DashingSparkEffectState.DASHING;

                SetState(PlayerState.DASH, 0);
            }
            else if (ContainsAnimationIndex(PlayerState.POST_DASH, animation.Index, true))
            {
                if (Landed)
                {
                    baseHSpeed = WALKING_SPEED;
                    if (PressingLeft)
                        TryMoveLeft();
                    else if (PressingRight)
                        TryMoveRight();
                    else
                        SetStandState();
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
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animation.Index, true))
                SetStandState();
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animation.Index, true))
            {
                Origin += LADDER_MOVE_OFFSET * Vector.DOWN_VECTOR;

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
            else if (ContainsAnimationIndex(PlayerState.TAKING_DAMAGE, animation.Index, false))
                OnKnockbackEnd();
            else if (ContainsAnimationIndex(PlayerState.DYING, animation.Index, false))
            {
                Freezed = false;
                Engine.PlaySound(2, 10);
                PaletteIndex = 4;

                Engine.StartDyingEffect();
                KillOnNextFrame();
            }
            else if (ContainsAnimationIndex(PlayerState.VICTORY, animation.Index, false))
            {
                if (teleporting)
                {
                    InputLocked = true;
                    Invincible = true;
                    SetState(PlayerState.PRE_TELEPORTING, 0);
                    PlaySound(17);
                }
                else
                {
                    Invincible = false;
                    InputLocked = false;
                    SetStandState();
                }
            }
            else if (ContainsAnimationIndex(PlayerState.PRE_TELEPORTING, animation.Index, false))
            {
                Invincible = true;
                Velocity = TELEPORT_SPEED * Vector.UP_VECTOR;
                CheckCollisionWithSolidSprites = false;
                CheckCollisionWithWorld = false;
                SetState(PlayerState.TELEPORTING, 0);
            }
        }

        public void StartVictoryPosing()
        {
            Invincible = true;
            InputLocked = true;
            SetState(PlayerState.VICTORY, 0);
            PlaySound(16);
        }

        public void StartTeleporting(bool withVictoryPose)
        {
            CanGoOutOfCameraBounds = true;
            teleporting = true;

            if (withVictoryPose)
                StartVictoryPosing();
            else
            {
                Invincible = true;
                InputLocked = true;
                SetState(PlayerState.PRE_TELEPORTING, 0);
                PlaySound(17);
            }
        }

        private void SetAnimationIndex(PlayerState state, int animationIndex, bool shooting, bool tired = false)
        {
            animationIndices[(int) state, shooting ? 1 : tired ? 2 : 0] = animationIndex;
        }

        protected override void OnCreateAnimation(int animationIndex, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);
            startOn = false; // Por padrão, a animação de um jogador começa parada.
            startVisible = false;

            switch (frameSequenceName)
            {
                case "Spawn":
                    SetAnimationIndex(PlayerState.SPAWN, animationIndex, false);
                    SetAnimationIndex(PlayerState.TELEPORTING, animationIndex, false);
                    break;

                case "SpawnEnd":
                    SetAnimationIndex(PlayerState.SPAWN_END, animationIndex, false);
                    break;

                case "Stand":
                    SetAnimationIndex(PlayerState.STAND, animationIndex, false);
                    break;

                case "Shooting":
                    SetAnimationIndex(PlayerState.STAND, animationIndex, true);
                    break;

                case "Tired":
                    SetAnimationIndex(PlayerState.STAND, animationIndex, false, true);
                    break;

                case "PreWalking":
                    SetAnimationIndex(PlayerState.PRE_WALK, animationIndex, false);
                    break;

                case "Walking":
                    SetAnimationIndex(PlayerState.WALK, animationIndex, false);
                    break;

                case "ShootWalking":
                    SetAnimationIndex(PlayerState.WALK, animationIndex, true);
                    break;

                case "Jumping":
                    SetAnimationIndex(PlayerState.JUMP, animationIndex, false);
                    break;

                case "ShootJumping":
                    SetAnimationIndex(PlayerState.JUMP, animationIndex, true);
                    break;

                case "GoingUp":
                    SetAnimationIndex(PlayerState.GOING_UP, animationIndex, false);
                    break;

                case "ShootGoingUp":
                    SetAnimationIndex(PlayerState.GOING_UP, animationIndex, true);
                    break;

                case "Falling":
                    SetAnimationIndex(PlayerState.FALL, animationIndex, false);
                    break;

                case "ShootFalling":
                    SetAnimationIndex(PlayerState.FALL, animationIndex, true);
                    break;

                case "Landing":
                    SetAnimationIndex(PlayerState.LAND, animationIndex, false);
                    break;

                case "ShootLanding":
                    SetAnimationIndex(PlayerState.LAND, animationIndex, true);
                    break;

                case "PreDashing":
                    SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, false);
                    break;

                case "ShootPreDashing":
                    SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, true);
                    break;

                case "Dashing":
                    SetAnimationIndex(PlayerState.DASH, animationIndex, false);
                    break;

                case "ShootDashing":
                    SetAnimationIndex(PlayerState.DASH, animationIndex, true);
                    break;

                case "PostDashing":
                    SetAnimationIndex(PlayerState.POST_DASH, animationIndex, false);
                    break;

                case "ShootPostDashing":
                    SetAnimationIndex(PlayerState.POST_DASH, animationIndex, true);
                    break;

                case "WallSliding":
                    SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, false);
                    break;

                case "ShootWallSliding":
                    SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, true);
                    break;

                case "WallJumping":
                    SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, false);
                    break;

                case "ShootWallJumping":
                    SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, true);
                    break;

                case "PreLadderClimbing":
                    SetAnimationIndex(PlayerState.PRE_LADDER_CLIMB, animationIndex, false);
                    break;

                case "LadderMoving":
                    SetAnimationIndex(PlayerState.LADDER, animationIndex, false);
                    break;

                case "ShootLadder":
                    SetAnimationIndex(PlayerState.LADDER, animationIndex, true);
                    break;

                case "TopLadderClimbing":
                    SetAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animationIndex, false);
                    break;

                case "TopLadderDescending":
                    SetAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animationIndex, false);
                    break;

                case "TakingDamage":
                    SetAnimationIndex(PlayerState.TAKING_DAMAGE, animationIndex, false);
                    break;

                case "Dying":
                    SetAnimationIndex(PlayerState.DYING, animationIndex, false);
                    break;

                case "Victory":
                    SetAnimationIndex(PlayerState.VICTORY, animationIndex, false);
                    break;

                case "PreTeleporting":
                    SetAnimationIndex(PlayerState.PRE_TELEPORTING, animationIndex, false);
                    break;

                default:
                    add = false;
                    break;
            }
        }

        public override FixedSingle GetGravity()
        {
            return spawning || teleporting || WallJumping && wallJumpFrameCounter < 7 || WallSliding || OnLadder || Dying || CrossingBossDoor ? 0 : base.GetGravity();
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            if (TakingDamage || Dying || Invincible || NoClip)
                return false;

            //Invincible = true;

            Direction direction = GetHorizontalDirection(attacker).Oposite();
            if (direction != Direction.NONE)
                Direction = direction;

            Engine.PlaySound(2, 9);

            WallJumping = false;

            if (attacker.KnockPlayerOnHurt)
            {
                Velocity = (Direction == Direction.RIGHT ? INITIAL_DAMAGE_RECOIL_SPEED_X : -INITIAL_DAMAGE_RECOIL_SPEED_X, INITIAL_DAMAGE_RECOIL_SPEED_Y);
                SetState(PlayerState.TAKING_DAMAGE, 0);
            }

            if (Health > 0 && Health - damage <= 0)
                Velocity = Vector.NULL_VECTOR;

            return true;
        }

        public void Die()
        {
            Invincible = true;
            WallJumping = false;
            Velocity = Vector.NULL_VECTOR;
            Freezed = true;
            charging = false;

            if (ChargingEffect != null)
            {
                ChargingEffect.KillOnNextFrame();
                ChargingEffect = null;
            }

            SetState(PlayerState.DYING, 0);
        }

        protected override bool OnBreak()
        {
            Die();
            return false;
        }

        public void Heal(int amount)
        {
            FixedSingle health = Health;
            health += amount;
            if (health > Engine.HealthCapacity)
                health = Engine.HealthCapacity;

            if (health > Health)
                Engine.StartHealthRecovering((int) (health - Health));
        }

        public void ReloadAmmo(int amount)
        {
            // TODO : Implement
        }

        internal void StartBossDoorCrossing()
        {
            CrossingBossDoor = true;
            CheckCollisionWithWorld = false;
            CheckCollisionWithSolidSprites = false;
            Invincible = true;
            Blinking = false;
            WallJumping = false;
            jumping = false;
            baseHSpeed = WALKING_SPEED;
            Velocity = Vector.NULL_VECTOR;
        }

        internal void StopBossDoorCrossing()
        {
            CrossingBossDoor = false;
            CheckCollisionWithWorld = true;
            CheckCollisionWithSolidSprites = true;
            Invincible = false;
            Velocity = Vector.NULL_VECTOR;

            if (Landed)
                SetStandState();
            else
                SetAirStateAnimation();
        }
    }
}