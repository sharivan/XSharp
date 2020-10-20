using MMX.Geometry;
using MMX.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MMX.Engine.Consts;

namespace MMX.Engine
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
        private int lives; // Quantidade de vidas.
        private bool inputLocked;
        private Keys[] keyBuffer;
        protected bool death;

        private int currentAnimationIndex;
        private int[,,] animationIndices;

        private bool jumped;
        private bool jumpReleased;
        private bool dashReleased;

        private FixedSingle baseHSpeed;
        private int dashFrameCounter;
        private bool spawing;
        private bool wallJumpStarted;
        private int wallJumpFrameCounter;

        private bool wasBlockedLeft;
        private bool wasBlockedRight;

        private Direction direction = Direction.RIGHT;
        private PlayerState state;
        private Direction stateDirection;
        private bool shooting;
        internal int shots;
        private int shotFrameCounter;
        private bool charging;
        private int chargingFrameCounter;
        internal bool shootingCharged;
        private ChargingEffect chargingEffect;

        /// <summary>
        /// Cria um novo Bomberman
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="name">Nome do Bomberman</param>
        /// <param name="box">Retângulo de desenho do Bomberman</param>
        /// <param name="imageLists">Array de lista de imagens que serão usadas na animação do Bomberman</param>
        internal Player(GameEngine engine, string name, Vector origin, SpriteSheet sheet)
        // Dado o retângulo de desenho do Bomberman, o retângulo de colisão será a metade deste enquanto o de dano será um pouco menor ainda.
        // A posição do retângulo de colisão será aquela que ocupa a metade inferior do retângulo de desenho enquanto o retângulo de dano terá o mesmo centro que o retângulo de colisão.
        : base(engine, name, origin, sheet, false, true)
        {
            CheckCollisionWithWorld = true;

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

            jumpReleased = true;
        }

        public override void SaveState(BinaryWriter writer)
        {
            base.SaveState(writer);

            writer.Write(KEY_BUFFER_COUNT);
            for (int i = 0; i < KEY_BUFFER_COUNT; i++)
                writer.Write((int) keyBuffer[i]);

            writer.Write(lives);
            writer.Write(inputLocked);
            writer.Write(death);

            writer.Write(currentAnimationIndex);

            writer.Write(jumped);
            writer.Write(jumpReleased);
            writer.Write(dashReleased);

            baseHSpeed.Write(writer);
            writer.Write(dashFrameCounter);
            writer.Write(spawing);
            writer.Write(wallJumpStarted);
            writer.Write(wallJumpFrameCounter);

            writer.Write(wasBlockedLeft);
            writer.Write(wasBlockedRight);

            writer.Write((int) direction);
            writer.Write((int) state);
            writer.Write((int) stateDirection);
            writer.Write(shooting);
            writer.Write(shotFrameCounter);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                keyBuffer[i] = (Keys) reader.ReadInt32();

            lives = reader.ReadInt32();
            inputLocked = reader.ReadBoolean();
            death = reader.ReadBoolean();

            currentAnimationIndex = reader.ReadInt32();

            jumped = reader.ReadBoolean();
            jumpReleased = reader.ReadBoolean();
            dashReleased = reader.ReadBoolean();

            baseHSpeed = new FixedSingle(reader);
            dashFrameCounter = reader.ReadInt32();
            spawing = reader.ReadBoolean();
            wallJumpStarted = reader.ReadBoolean();
            wallJumpFrameCounter = reader.ReadInt32();

            wasBlockedLeft = reader.ReadBoolean();
            wasBlockedRight = reader.ReadBoolean();

            direction = (Direction) reader.ReadInt32();
            state = (PlayerState) reader.ReadInt32();
            stateDirection = (Direction) reader.ReadInt32();
            shooting = reader.ReadBoolean();
            shotFrameCounter = reader.ReadInt32();
        }

        protected override Box GetCollisionBox()
        {
            if (Dashing)
                return new Box(new Vector(-DASHING_HITBOX_WIDTH * 0.5, -DASHING_HITBOX_HEIGHT - 3), Vector.NULL_VECTOR, new Vector(DASHING_HITBOX_WIDTH, DASHING_HITBOX_HEIGHT + 3));

            return new Box(new Vector(-HITBOX_WIDTH * 0.5, -HITBOX_HEIGHT - 3), Vector.NULL_VECTOR, new Vector(HITBOX_WIDTH, HITBOX_HEIGHT + 3));
        }

        protected override void OnHealthChanged(FixedSingle health)
        {
            engine.RepaintHP(); // Notifica o engine que o HP do caracter foi alterado para que seja redesenhado.
        }

        public bool Shooting
        {
            get
            {
                return shooting;
            }
        }

        public bool Dashing
        {
            get
            {
                return state == PlayerState.PRE_DASH || state == PlayerState.DASH;
            }
        }

        public bool DashingOnly
        {
            get
            {
                return state == PlayerState.DASH;
            }
        }

        public bool DashingLeft
        {
            get
            {
                return (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.LEFT;
            }
        }

        public bool DashingRight
        {
            get
            {
                return (state == PlayerState.PRE_DASH || state == PlayerState.DASH) && stateDirection == Direction.RIGHT;
            }
        }

        public bool PostDashing
        {
            get
            {
                return state == PlayerState.POST_DASH;
            }
        }

        public bool GoingUp
        {
            get
            {
                return state == PlayerState.GOING_UP;
            }
        }

        public bool Falling
        {
            get
            {
                return state == PlayerState.FALL;
            }
        }

        public bool WallSliding
        {
            get
            {
                return state == PlayerState.WALL_SLIDE;
            }
        }

        public bool WallJumping
        {
            get
            {
                return state == PlayerState.WALL_JUMP;
            }
        }

        public bool WallJumpingToLeft
        {
            get
            {
                return state == PlayerState.WALL_JUMP && stateDirection == Direction.LEFT;
            }
        }

        public bool WallJumpingToRight
        {
            get
            {
                return state == PlayerState.WALL_JUMP && stateDirection == Direction.RIGHT;
            }
        }

        public bool NormalJumping
        {
            get
            {
                return state == PlayerState.JUMP;
            }
        }

        public bool Jumping
        {
            get
            {
                return state == PlayerState.JUMP || state == PlayerState.WALL_JUMP;
            }
        }

        public bool Landing
        {
            get
            {
                return state == PlayerState.LAND;
            }
        }

        public bool Walking
        {
            get
            {
                return state == PlayerState.PRE_WALK || state == PlayerState.WALK;
            }
        }

        public bool PreWalking
        {
            get
            {
                return state == PlayerState.PRE_WALK;
            }
        }

        public bool WalkingOnly
        {
            get
            {
                return state == PlayerState.WALK;
            }
        }

        public bool WalkingLeft
        {
            get
            {
                return (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.LEFT;
            }
        }

        public bool PreWalkingLeft
        {
            get
            {
                return state == PlayerState.PRE_WALK && stateDirection == Direction.LEFT;
            }
        }

        public bool WalkingLeftOnly
        {
            get
            {
                return state == PlayerState.WALK && stateDirection == Direction.LEFT;
            }
        }

        public bool WalkingRight
        {
            get
            {
                return (state == PlayerState.PRE_WALK || state == PlayerState.WALK) && stateDirection == Direction.RIGHT;
            }
        }

        public bool PreWalkingRight
        {
            get
            {
                return state == PlayerState.PRE_WALK && stateDirection == Direction.RIGHT;
            }
        }

        public bool WalkingRightOnly
        {
            get
            {
                return state == PlayerState.WALK && stateDirection == Direction.RIGHT;
            }
        }

        public bool Standing
        {
            get
            {
                return state == PlayerState.STAND;
            }
        }

        public bool PreLadderClimbing
        {
            get
            {
                return state == PlayerState.PRE_LADDER_CLIMB;
            }
        }

        public bool TopLadderClimbing
        {
            get
            {
                return state == PlayerState.TOP_LADDER_CLIMB;
            }
        }

        public bool TopLadderDescending
        {
            get
            {
                return state == PlayerState.TOP_LADDER_DESCEND;
            }
        }

        public bool OnLadder
        {
            get
            {
                return OnLadderOnly || TopLadderDescending || TopLadderClimbing || PreLadderClimbing;
            }
        }

        public bool OnLadderOnly
        {
            get
            {
                return state == PlayerState.LADDER;
            }
        }

        public bool LadderMoving
        {
            get
            {
                return vel.Y != 0 && OnLadder;
            }
        }

        public bool LadderClimbing
        {
            get
            {
                return vel.Y < 0 && OnLadder;
            }
        }

        public bool LadderDescending
        {
            get
            {
                return vel.Y > 0 && OnLadder;
            }
        }

        public Keys Keys
        {
            get
            {
                return GetKeys(0);
            }
        }

        public Keys LastKeys
        {
            get
            {
                return GetLastKeys(0);
            }
        }

        public Keys LastKeysWithoutLatency
        {
            get
            {
                return GetLastKeys(0);
            }
        }

        public bool InputLocked
        {
            get
            {
                return inputLocked;
            }

            set
            {
                inputLocked = true;
            }
        }

        /// <summary>
        /// Quantidade de vidas que o Bomberman possui.
        /// </summary>
        public int Lives
        {
            get
            {
                return lives;
            }
            set
            {
                lives = value;
                engine.RepaintLives();
            }
        }

        protected Animation CurrentAnimation
        {
            get
            {
                return GetAnimation(currentAnimationIndex);
            }
        }

        protected int CurrentAnimationIndex
        {
            get
            {
                return currentAnimationIndex;
            }
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

        public Direction Direction
        {
            get
            {
                return direction;
            }
        }

        public bool PressingNothing
        {
            get
            {
                return Keys == 0;
            }
        }

        public bool PressingNoLeftRight
        {
            get
            {
                return !PressingLeft && !PressingRight;
            }
        }

        public bool PressingLeft
        {
            get
            {
                return !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);
            }
        }

        public bool WasPressingLeft
        {
            get
            {
                return !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.LEFT);
            }
        }

        public bool PressingRight
        {
            get
            {
                return !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);
            }
        }

        public bool WasPressingRight
        {
            get
            {
                return !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.RIGHT);
            }
        }

        public bool PressingDown
        {
            get
            {
                return !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);
            }
        }

        public bool WasPressingDown
        {
            get
            {
                return !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.DOWN);
            }
        }

        public bool PressingUp
        {
            get
            {
                return !inputLocked && GetKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);
            }
        }

        public bool WasPressingUp
        {
            get
            {
                return !inputLocked && GetLastKeys(INPUT_MOVEMENT_LATENCY).HasFlag(Keys.UP);
            }
        }

        public bool PressingShot
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.SHOT);
            }
        }

        public bool WasPressingShot
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.SHOT);
            }
        }

        public bool PressingWeapon
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.WEAPON);
            }
        }

        public bool WasPressingWeapon
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.WEAPON);
            }
        }

        public bool PressingJump
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.JUMP);
            }
        }

        public bool WasPressingJump
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.JUMP);
            }
        }

        public bool PressingDash
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.DASH);
            }
        }

        public bool WasPressingDash
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.DASH);
            }
        }

        public bool PressingLWeaponSwitch
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.LWS);
            }
        }

        public bool WasPressingLWeaponSwitch
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.LWS);
            }
        }

        public bool PressingRWeaponSwitch
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.RWS);
            }
        }

        public bool WasPressingRWeaponSwitch
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.RWS);
            }
        }

        public bool PressingStart
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.START);
            }
        }

        public bool WasPressingStart
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.START);
            }
        }

        public bool PressingSelect
        {
            get
            {
                return !inputLocked && Keys.HasFlag(Keys.SELECT);
            }
        }

        public bool WasPressingSelect
        {
            get
            {
                return !inputLocked && LastKeys.HasFlag(Keys.SELECT);
            }
        }

        protected void SetState(PlayerState state, int startAnimationIndex = -1)
        {
            SetState(state, direction, startAnimationIndex);
        }

        protected void SetState(PlayerState state, Direction direction, int startAnimationIndex = -1)
        {
            this.state = state;
            stateDirection = direction;
            CurrentAnimationIndex = GetAnimationIndex(state, direction, shooting && !(TopLadderClimbing || TopLadderDescending || PreLadderClimbing || PreWalking));
            CurrentAnimation.Start(startAnimationIndex);
        }

        protected int GetAnimationIndex(PlayerState state, Direction direction, bool shooting)
        {
            return animationIndices[(int) state, direction == Direction.LEFT ? 1 : 0, shooting ? 1 : 0];
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
            if (WallJumping && wallJumpFrameCounter >= 4)
            {
                vel = Vector.NULL_VECTOR;
                wallJumpStarted = false;
                SetAirStateAnimation(true);
            }
            else
                vel = vel.XVector;
        }

        protected override void OnBlockedLeft()
        {
            if (Landed)
                SetState(PlayerState.STAND, 0);
            else if (WallJumping && wallJumpFrameCounter >= 4)
            {
                wallJumpStarted = false;
                SetAirStateAnimation(true);
            }
        }

        protected override void OnBlockedRight()
        {
            if (Landed)
                SetState(PlayerState.STAND, 0);
            else if (WallJumping && wallJumpFrameCounter >= 4)
            {
                wallJumpStarted = false;
                SetAirStateAnimation(true);
            }
        }

        protected override void OnLanded()
        {
            wallJumpStarted = false;
            baseHSpeed = WALKING_SPEED;

            if (!spawing)
            {
                if (PressingLeft)
                    TryMoveLeft();
                else if (PressingRight)
                    TryMoveRight();
                else
                {
                    vel = Vector.NULL_VECTOR;
                    SetState(PlayerState.LAND, 0);
                }
            }
            else
                SetState(PlayerState.SPAWN_END, 0);
        }

        protected override FixedSingle GetTerminalDownwardSpeed()
        {
            return WallSliding ? WALL_SLIDE_SPEED : TERMINAL_DOWNWARD_SPEED;
        }

        public override void Spawn()
        {
            base.Spawn();

            spawing = true;

            lives = 2;

            currentAnimationIndex = -1;

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
            // Toda vez que o bomberman morre,
            Lives--; // decrementa sua quantidade de vidas.

            engine.PlaySound("TIME_UP"); // Toca o som de morte do Bomberman.

            base.OnDeath(); // Chama o método OnDeath() da classe base.

            if (lives > 0) // Se ele ainda possuir vidas,
                engine.ScheduleRespawn(this); // respawna o Bomberman.
            else
                engine.OnGameOver(); // Senão, Game Over!
        }

        private void TryMoveLeft(bool standOnly = false)
        {
            if (!standOnly && !BlockedLeft)
                vel = new Vector(-baseHSpeed, vel.Y);
            else
                vel = new Vector(0, vel.Y);

            if (Landed)
            {
                if (standOnly || BlockedLeft)
                {
                    bool wasStanding = Standing;
                    SetState(PlayerState.STAND, Direction.LEFT, !wasStanding ? 0 : -1);
                }
                else
                {
                    if (!shooting && baseHSpeed == PRE_WALKING_SPEED)
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
                    if (BlockedLeft)
                    {
                        if (!WallSliding)
                        {
                            vel = Vector.NULL_VECTOR;
                            SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
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
            if (!standOnly && !BlockedRight)
                vel = new Vector(baseHSpeed, vel.Y);
            else
                vel = new Vector(0, vel.Y);

            if (Landed)
            {
                if (standOnly || BlockedRight)
                {
                    bool wasStanding = Standing;
                    SetState(PlayerState.STAND, Direction.RIGHT, !wasStanding ? 0 : -1);
                }
                else
                {
                    if (!shooting && baseHSpeed == PRE_WALKING_SPEED)
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
                    if (BlockedRight)
                    {
                        if (!WallSliding)
                        {
                            vel = Vector.NULL_VECTOR;
                            SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
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
            if (Walking && LandedOnSlope && LandedSlope.HCathetusSign == vel.X.Signal)
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

        protected override void Think()
        {
            if (engine.Paused)
            {
                if (PressingStart && !WasPressingStart)
                    engine.ContinueGame();
            }
            else
            {
                skipPhysics = Standing;

                if (NoClip)
                {
                    if (spawing)
                    {
                        spawing = false;
                    }

                    bool mirrored = false;
                    Direction direction = PressingLeft ? Direction.LEFT : PressingRight ? Direction.RIGHT : Direction.NONE;
                    if (direction != Direction.NONE && direction != this.direction)
                    {
                        mirrored = true;
                        this.direction = direction;
                        RefreshAnimation();
                    }

                    baseHSpeed = PressingDash ? NO_CLIP_SPEED_BOOST : NO_CLIP_SPEED;
                    vel = new Vector(mirrored ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, PressingUp ? -baseHSpeed : PressingDown ? baseHSpeed : 0);
                    SetAirStateAnimation();
                }
                else if (!spawing)
                {
                    Direction lastDirection = direction;

                    if (baseHSpeed == PRE_WALKING_SPEED)
                        baseHSpeed = WALKING_SPEED;

                    if (!WallJumping)
                    {
                        if (!OnLadder)
                        {
                            if (PressingLeft)
                            {
                                direction = Direction.LEFT;

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
                                    skipPhysics = false;
                                    TryMoveLeft();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedLeft && !Jumping && !GoingUp)
                                    {
                                        if (!WallSliding)
                                        {
                                            vel = Vector.NULL_VECTOR;
                                            SetState(PlayerState.WALL_SLIDE, Direction.LEFT, 0);
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
                                direction = Direction.RIGHT;

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
                                    skipPhysics = false;
                                    TryMoveRight();
                                }
                                else if (!Landed)
                                {
                                    if (BlockedRight && !Jumping && !GoingUp)
                                    {
                                        if (!WallSliding)
                                        {
                                            vel = Vector.NULL_VECTOR;
                                            SetState(PlayerState.WALL_SLIDE, Direction.RIGHT, 0);
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
                                            vel = new Vector(0, vel.Y);

                                        if (!Landing && !PostDashing)
                                        {
                                            SetState(PlayerState.STAND, 0);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!WallJumping)
                                        vel = new Vector(0, vel.Y);

                                    SetAirStateAnimation();
                                }
                            }
                        }

                        if (PressingUp)
                        {
                            if (OnLadderOnly)
                            {
                                if (!shooting)
                                {
                                    collider.Box = CollisionBox;
                                    Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                    CollisionFlags flags = engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true, CollisionSide.CEIL);
                                    if (flags.HasFlag(CollisionFlags.TOP_LADDER))
                                    {
                                        if (!TopLadderClimbing && !TopLadderDescending)
                                            SetState(PlayerState.TOP_LADDER_CLIMB, 0);
                                    }
                                    else if (!TopLadderClimbing && !TopLadderDescending)
                                    {
                                        vel = new Vector(0, -LADDER_CLIMB_SPEED);
                                        CurrentAnimation.Start();
                                    }
                                }
                            }
                            else if (!OnLadder)
                            {
                                collider.Box = CollisionBox;
                                Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                CollisionFlags flags = engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true, CollisionSide.CEIL);
                                if (flags.HasFlag(CollisionFlags.LADDER))
                                {
                                    vel = Vector.NULL_VECTOR;

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
                                if (!shooting)
                                {
                                    collider.Box = CollisionBox;
                                    Box collisionBox = collider.UpCollider + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.DOWN_VECTOR;
                                    CollisionFlags flags = engine.GetCollisionFlags(collisionBox, CollisionFlags.NONE, true, CollisionSide.CEIL);
                                    if (!flags.HasFlag(CollisionFlags.LADDER))
                                    {
                                        if (Landed)
                                        {
                                            if (!Standing)
                                                SetState(PlayerState.LAND, 0);
                                        }
                                        else if (!TopLadderClimbing && !TopLadderDescending)
                                        {
                                            vel = Vector.NULL_VECTOR;
                                            SetAirStateAnimation();
                                        }
                                    }
                                    else if (!TopLadderClimbing && !TopLadderDescending)
                                    {
                                        vel = new Vector(0, LADDER_CLIMB_SPEED);
                                        CurrentAnimation.Start();
                                    }
                                }
                            }
                            else if (LandedOnTopLadder && !TopLadderDescending && !TopLadderClimbing)
                            {
                                vel = Vector.NULL_VECTOR;

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
                            vel = Vector.NULL_VECTOR;
                            CurrentAnimation.Stop();
                        }

                        if (!OnLadder)
                        {
                            if (PressingDash)
                            {
                                if (!WasPressingDash)
                                {
                                    dashReleased = false;
                                    if (Landed && (direction == Direction.LEFT ? !BlockedLeft : !BlockedRight))
                                    {
                                        baseHSpeed = DASH_SPEED;
                                        dashFrameCounter = 0;
                                        vel = new Vector(direction == Direction.LEFT ? -DASH_SPEED : DASH_SPEED, vel.Y);
                                        SetState(PlayerState.PRE_DASH, 0);
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
                                                vel = new Vector(-baseHSpeed, vel.Y);
                                                SetState(PlayerState.WALK, 0);
                                            }
                                            else if (PressingRight && !BlockedRight)
                                            {
                                                vel = new Vector(baseHSpeed, vel.Y);
                                                SetState(PlayerState.WALK, 0);
                                            }
                                            else
                                            {
                                                vel = new Vector(0, vel.Y);
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
                        jumpReleased = false;
                        if (Landed)
                        {
                            if (!BlockedUp)
                            {
                                bool hspeedNull = false;
                                if (PressingDash)
                                    baseHSpeed = DASH_SPEED;
                                else if (baseHSpeed == PRE_WALKING_SPEED)
                                {
                                    baseHSpeed = WALKING_SPEED;
                                    hspeedNull = true;
                                }

                                jumped = true;
                                vel = new Vector(hspeedNull ? 0 : PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, -GetInitialJumpSpeed());
                                SetState(PlayerState.JUMP, 0);
                            }
                        }
                        else if (OnLadder)
                        {
                            vel = Vector.NULL_VECTOR;
                            SetAirStateAnimation();
                        }
                    }
                    else if (WasPressingJump && !PressingJump)
                    {
                        if (!jumpReleased)
                        {
                            jumpReleased = true;

                            if (jumped && !Landed && !WallSliding && vel.Y < 0)
                            {
                                jumped = false;
                                vel = new Vector(PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, 0);
                            }
                        }
                    }

                    if ((!jumped || vel.Y >= 0) && !Landed && (!WallJumping || wallJumpFrameCounter < 4) && !OnLadder && !GetLastKeys(2).HasFlag(Keys.JUMP) && GetKeys(2).HasFlag(Keys.JUMP))
                    {
                        Direction wallJumpDir = GetWallJumpDir();
                        if (wallJumpDir != Direction.NONE)
                        {
                            wallJumpStarted = true;
                            wallJumpFrameCounter = 0;
                            direction = wallJumpDir;
                            baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;

                            jumped = true;
                            vel = Vector.NULL_VECTOR;
                            SetState(PlayerState.WALL_JUMP, 0);
                        }
                    }

                    if (Dashing)
                    {
                        dashFrameCounter++;
                        if (dashFrameCounter > DASH_DURATION)
                        {
                            baseHSpeed = WALKING_SPEED;
                            if (PressingLeft && !BlockedLeft)
                            {
                                vel = new Vector(-baseHSpeed, vel.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else if (PressingRight && !BlockedRight)
                            {
                                vel = new Vector(baseHSpeed, vel.Y);
                                SetState(PlayerState.WALK, 0);
                            }
                            else
                            {
                                vel = new Vector(0, vel.Y);
                                SetState(PlayerState.POST_DASH, 0);
                            }
                        }
                    }

                    if (wallJumpStarted)
                    {
                        wallJumpFrameCounter++;
                        if (wallJumpFrameCounter > WALL_JUMP_DURATION)
                        {
                            wallJumpStarted = false;
                            vel = new Vector(PressingLeft ? -baseHSpeed : PressingRight ? baseHSpeed : 0, vel.Y);
                            SetState(PlayerState.GOING_UP, 0);
                        }
                        else if (wallJumpFrameCounter == 4)
                        {
                            baseHSpeed = PressingDash ? DASH_SPEED : WALKING_SPEED;
                            vel = new Vector(WallJumpingToLeft ? baseHSpeed : -baseHSpeed, -INITIAL_UPWARD_SPEED_FROM_JUMP);
                        }
                        else if (wallJumpFrameCounter < 4)
                            vel = Vector.NULL_VECTOR;
                    }
                }

                if (PressingShot)
                {
                    if (!WasPressingShot)
                    {
                        if (shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                        {
                            shooting = true;
                            shotFrameCounter = 0;

                            if (OnLadderOnly)
                            {
                                vel = Vector.NULL_VECTOR;

                                if (PressingLeft)
                                    direction = Direction.LEFT;
                                else if (PressingRight)
                                    direction = Direction.RIGHT;

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
                        if (!shooting && !charging && !shootingCharged && shots < MAX_SHOTS)
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
                                if ((frame & 2) == 0 || (frame & 2) == 1)
                                    Palette = engine.ChargeLevel1Palette;
                                else
                                    Palette = engine.X1NormalPalette;

                                if (chargingEffect == null)
                                    chargingEffect = engine.StartChargeEffect(this);

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

                    Palette = engine.X1NormalPalette;

                    if (chargingEffect != null)
                    {
                        chargingEffect.Kill();
                        chargingEffect = null;
                    }

                    if (charging && chargingFrameCounter >= 4 && shots < MAX_SHOTS && !PreLadderClimbing && !TopLadderClimbing && !TopLadderDescending)
                    {
                        shooting = true;
                        shootingCharged = true;
                        shotFrameCounter = 0; 

                        if (OnLadderOnly)
                        {
                            vel = Vector.NULL_VECTOR;

                            if (PressingLeft)
                                direction = Direction.LEFT;
                            else if (PressingRight)
                                direction = Direction.RIGHT;

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

                if (shooting)
                {
                    shotFrameCounter++;
                    if (shotFrameCounter > SHOT_DURATION)
                    {
                        shooting = false;
                        RefreshAnimation();
                    }
                }

                if (PressingStart && !WasPressingStart)
                    engine.PauseGame();
            }

            base.Think();
        }

        private Vector GetShotOrigin()
        {
            switch (state)
            {
                case PlayerState.STAND:
                case PlayerState.LAND:
                    return new Vector(9, 8);

                case PlayerState.WALK:
                    return new Vector(18, 6);

                case PlayerState.JUMP:
                case PlayerState.WALL_JUMP:
                case PlayerState.GOING_UP:
                case PlayerState.FALL:
                    return new Vector(18, 7);

                case PlayerState.PRE_DASH:
                    return new Vector(21, -4);

                case PlayerState.DASH:
                    return new Vector(26, 0);

                case PlayerState.POST_DASH:
                    return new Vector(24, 8);

                case PlayerState.LADDER:
                    return new Vector(9, 5);
            }

            return new Vector(9, 8);
        }

        public void ShootLemon()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            engine.ShootLemon(this, direction == Direction.RIGHT ? CollisionBox.RightTop + shotOrigin : CollisionBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction, baseHSpeed == DASH_SPEED);
        }

        public void ShootSemiCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            engine.ShootSemiCharged(this, direction == Direction.RIGHT ? CollisionBox.RightTop + shotOrigin : CollisionBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction);
        }

        public void ShootCharged()
        {
            shots++;

            Vector shotOrigin = GetShotOrigin() + new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5);
            Direction direction = Direction;
            if (state == PlayerState.WALL_SLIDE)
                direction = direction == Direction.RIGHT ? Direction.LEFT : Direction.RIGHT;

            engine.ShootCharged(this, direction == Direction.RIGHT ? CollisionBox.RightTop + shotOrigin : CollisionBox.LeftTop + new Vector(-shotOrigin.X, shotOrigin.Y), direction);
        }

        public Direction GetWallJumpDir()
        {
            FixedSingle vclip;
            int slopeSign;
            if (LandedOnSlope)
            {
                RightTriangle slopeTriangle = LandedSlope;
                FixedSingle h = slopeTriangle.HCathetusVector.X;
                vclip = (slopeTriangle.VCathetusVector.Y * CollisionBox.Width / h).Abs;
                slopeSign = h.Signal;
            }
            else
            {
                vclip = 0;
                slopeSign = 0;
            }

            Box collisionBox = Origin + GetCollisionBox().ClipTop(-2).ClipBottom(2 + (slopeSign == 1 ? vclip : 0)) + WALL_MAX_DISTANCE_TO_WALL_JUMP * Vector.LEFT_VECTOR;
            if (engine.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE, true, CollisionSide.LEFT_WALL).HasFlag(CollisionFlags.BLOCK))
                return Direction.LEFT;

            collisionBox = Origin + GetCollisionBox().ClipTop(-2).ClipBottom(2 + (slopeSign == -1 ? vclip : 0)) + WALL_MAX_DISTANCE_TO_WALL_JUMP * Vector.RIGHT_VECTOR;
            if (engine.GetCollisionFlags(collisionBox, CollisionFlags.SLOPE, true, CollisionSide.RIGHT_WALL).HasFlag(CollisionFlags.BLOCK))
                return Direction.RIGHT;

            return Direction.NONE;
        }

        private void SetAirStateAnimation(bool forceGoingUp = false)
        {
            if (vel.Y >= FALL_ANIMATION_MINIMAL_SPEED)
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

        /// <summary>
        /// Obtém a primeira direção possível para um dado conjunto de teclas pressionadas.
        /// </summary>
        /// <param name="bits">Conjunto de bits que indicam quais teclas estão sendo pressionadas.</param>
        /// <returns></returns>
        private static Direction FirstDirection(int bits, bool leftRightOnly = true)
        {
            for (int i = 0; i < 4; i++)
            {
                int mask = 1 << i;

                if ((bits & mask) != 0)
                {
                    Direction direction = Utils.IntToDirection(mask);
                    if (leftRightOnly && direction != Direction.LEFT && direction != Direction.RIGHT)
                        continue;

                    return direction;
                }
            }

            return Direction.NONE;
        }

        /// <summary>
        /// Obtém o vetor unitário numa direção dada
        /// </summary>
        /// <param name="direction">Direção que o vetor derá ter</param>
        /// <returns>Vetor unitário na direção de direction</returns>
        public static Vector GetVectorDir(Direction direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    return Vector.LEFT_VECTOR;

                case Direction.UP:
                    return Vector.UP_VECTOR;

                case Direction.RIGHT:
                    return Vector.RIGHT_VECTOR;

                case Direction.DOWN:
                    return Vector.DOWN_VECTOR;
            }

            return Vector.NULL_VECTOR;
        }

        /// <summary>
        /// Atualiza o conjunto de teclas que estão sendo pressionadas.
        /// </summary>
        /// <param name="value">Conjunto de teclas pressionadas.</param>
        internal void PushKeys(Keys value)
        {
            if (spawing || death)
                return;

            Array.Copy(keyBuffer, 0, keyBuffer, 1, keyBuffer.Length - 1);
            keyBuffer[0] = value;
        }

        private void RefreshAnimation()
        {
            CurrentAnimationIndex = GetAnimationIndex(state, direction, shooting);
        }

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

                    if (direction == Direction.LEFT)
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
                    vel = new Vector(0, shooting ? 0 : -LADDER_CLIMB_SPEED);
                else if (PressingDown)
                    vel = new Vector(0, shooting ? 0 : LADDER_CLIMB_SPEED);
                else
                    CurrentAnimation.Stop();

                if (shooting)
                    CurrentAnimation.Stop();
            }
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animation.Index, true, true))
            {
                Box collisionBox = CollisionBox;
                Vector delta = Origin - collisionBox.Origin;
                collider.Box = collisionBox + (HITBOX_HEIGHT - LADDER_BOX_VCLIP) * Vector.UP_VECTOR;
                collider.MoveContactFloor(MAP_SIZE);

                Origin = collider.Box.Origin + delta;

                vel = Vector.NULL_VECTOR;
                SetState(PlayerState.STAND, 0);
            }
            else if (ContainsAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animation.Index, true, true))
            {
                Origin += (HITBOX_HEIGHT - LADDER_BOX_VCLIP + MAP_SIZE) * Vector.DOWN_VECTOR;

                SetState(PlayerState.LADDER, 0);

                if (PressingUp)
                    vel = new Vector(0, shooting ? 0 : -LADDER_CLIMB_SPEED);
                else if (PressingDown)
                    vel = new Vector(0, shooting ? 0 : LADDER_CLIMB_SPEED);
                else
                    CurrentAnimation.Stop();

                if (shooting)
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

            if (frameSequenceName == "Spawn")
                SetAnimationIndex(PlayerState.SPAWN, animationIndex, false, false);
            else if (frameSequenceName == "SpawnEnd")
                SetAnimationIndex(PlayerState.SPAWN_END, animationIndex, false, false);
            else if (frameSequenceName == "Stand")
                SetAnimationIndex(PlayerState.STAND, animationIndex, true, false);
            else if (frameSequenceName == "Shooting")
                SetAnimationIndex(PlayerState.STAND, animationIndex, true, true);
            else if (frameSequenceName == "PreWalking")
                SetAnimationIndex(PlayerState.PRE_WALK, animationIndex, true, false);
            else if (frameSequenceName == "Walking")
                SetAnimationIndex(PlayerState.WALK, animationIndex, true, false);
            else if (frameSequenceName == "ShootWalking")
                SetAnimationIndex(PlayerState.WALK, animationIndex, true, true);
            else if (frameSequenceName == "Jumping")
                SetAnimationIndex(PlayerState.JUMP, animationIndex, true, false);
            else if (frameSequenceName == "ShootJumping")
                SetAnimationIndex(PlayerState.JUMP, animationIndex, true, true);
            else if (frameSequenceName == "GoingUp")
                SetAnimationIndex(PlayerState.GOING_UP, animationIndex, true, false);
            else if (frameSequenceName == "ShootGoingUp")
                SetAnimationIndex(PlayerState.GOING_UP, animationIndex, true, true);
            else if (frameSequenceName == "Falling")
                SetAnimationIndex(PlayerState.FALL, animationIndex, true, false);
            else if (frameSequenceName == "ShootFalling")
                SetAnimationIndex(PlayerState.FALL, animationIndex, true, true);
            else if (frameSequenceName == "Landing")
                SetAnimationIndex(PlayerState.LAND, animationIndex, true, false);
            else if (frameSequenceName == "ShootLanding")
                SetAnimationIndex(PlayerState.LAND, animationIndex, true, true);
            else if (frameSequenceName == "PreDashing")
                SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, true, false);
            else if (frameSequenceName == "ShootPreDashing")
                SetAnimationIndex(PlayerState.PRE_DASH, animationIndex, true, true);
            else if (frameSequenceName == "Dashing")
                SetAnimationIndex(PlayerState.DASH, animationIndex, true, false);
            else if (frameSequenceName == "ShootDashing")
                SetAnimationIndex(PlayerState.DASH, animationIndex, true, true);
            else if (frameSequenceName == "PostDashing")
                SetAnimationIndex(PlayerState.POST_DASH, animationIndex, true, false);
            else if (frameSequenceName == "ShootPostDashing")
                SetAnimationIndex(PlayerState.POST_DASH, animationIndex, true, true);
            else if (frameSequenceName == "WallSliding")
                SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, true, false);
            else if (frameSequenceName == "ShootWallSliding")
                SetAnimationIndex(PlayerState.WALL_SLIDE, animationIndex, true, true);
            else if (frameSequenceName == "WallJumping")
                SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, true, false);
            else if (frameSequenceName == "ShootWallJumping")
                SetAnimationIndex(PlayerState.WALL_JUMP, animationIndex, true, true);
            else if (frameSequenceName == "PreLadderClimbing")
                SetAnimationIndex(PlayerState.PRE_LADDER_CLIMB, animationIndex, true, false);
            else if (frameSequenceName == "LadderMoving")
                SetAnimationIndex(PlayerState.LADDER, animationIndex, true, false);
            else if (frameSequenceName == "ShootLadder")
                SetAnimationIndex(PlayerState.LADDER, animationIndex, true, true);
            else if (frameSequenceName == "TopLadderClimbing")
                SetAnimationIndex(PlayerState.TOP_LADDER_CLIMB, animationIndex, true, false);
            else if (frameSequenceName == "TopLadderDescending")
                SetAnimationIndex(PlayerState.TOP_LADDER_DESCEND, animationIndex, true, false);
            else
                add = false;
        }

        public override FixedSingle GetGravity()
        {
            if (wallJumpStarted && wallJumpFrameCounter <= 2 || OnLadder)
                return 0;

            return base.GetGravity();
        }
    }
}
