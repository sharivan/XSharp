using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
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

    public class Penguin : Boss
    {
        private bool firstAttack;
        private bool wasShootingIce;
        private int iceCount;

        public PenguinState State
        {
            get => GetState<PenguinState>();
            set => SetState(value);
        }

        public Penguin()
        {
            Directional = true;
            DefaultDirection = Direction.LEFT;
            SpriteSheetIndex = 10;
            PaletteIndex = 7;

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
            RegisterState(PenguinState.HANGING, OnHanging, "PreJumping");
            RegisterState(PenguinState.TAKING_DAMAGE, OnTakingDamage, "TakingDamage");
            RegisterState(PenguinState.IN_FLAMES, OnInFlames, "InFlames");
            RegisterState(PenguinState.DYING, "Dying");
        }

        public override FixedSingle GetGravity()
        {
            return State == PenguinState.DYING ? 0 : base.GetGravity();
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
                _ => PENGUIN_HITBOX,
            };
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            PaletteIndex = 7;
            firstAttack = true;
            wasShootingIce = false;
            iceCount = 0;
            Direction = Direction.LEFT;
            MaxHealth = BOSS_HP;

            SetState(PenguinState.INTRODUCING);
        }

        protected override void OnLanded()
        {
            base.OnLanded();

            if (State == PenguinState.INTRODUCING)
                SetCurrentAnimationByName("LandingIntroducing");
            else
                SetCurrentAnimationByName("Landing");

            Velocity = Vector.NULL_VECTOR;
        }

        protected override void OnBlockedLeft()
        {
            base.OnBlockedLeft();

            if (State == PenguinState.SLIDING)
            {
                Velocity = (-Velocity.X, 0);
                Direction = Direction.Oposite();
            }
        }

        protected override void OnBlockedRight()
        {
            base.OnBlockedRight();

            if (State == PenguinState.SLIDING)
            {
                Velocity = (-Velocity.X, 0);
                Direction = Direction.Oposite();
            }
        }

        protected override void OnBlockedUp()
        {
            base.OnBlockedUp();

            Velocity = Vector.NULL_VECTOR;
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
                    int value = Engine.RNG.Next(3);
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

                            /*case 3:
                                State = PenguinState.BLOWING;
                                break;

                            case 4:
                                State = PenguinState.HANGING;
                                break;*/
                    }
                }
            }
        }

        private void OnShootingIce(EntityState state, long frameCounter)
        {
            if (frameCounter == 16)
            {
                ShootIce();
                wasShootingIce = true;
                State = PenguinState.IDLE;
            }
        }

        private void OnStartBlowing(EntityState state, EntityState lastState)
        {
            Engine.PlaySound(4, 29);
        }

        private void OnBlowing(EntityState state, long frameCounter)
        {
        }

        private void ShootIce()
        {
            var ice = new PenguinIce()
            {
                Shooter = this,
                Bump = Engine.RNG.Next(2) == 1
            };

            ice.Spawn();
            iceCount++;
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
                    Engine.PlaySound(4, 28);

                Vector v = Velocity;
                Vector a = PENGUIN_SLIDE_DECELARATION * (Direction == DefaultDirection ? Vector.LEFT_VECTOR : Vector.RIGHT_VECTOR);
                v -= a;
                if (v.X > 0 && Velocity.X < 0 || v.X < 0 && Velocity.X > 0 || v.X.Abs < PENGUIN_SLIDE_DECELARATION)
                {
                    Velocity = Vector.NULL_VECTOR;
                    State = PenguinState.IDLE;
                }
                else
                    Velocity = v;
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

        private void OnHanging(EntityState state, long frameCounter)
        {
        }

        private void OnTakingDamage(EntityState state, long frameCounter)
        {
        }

        private void OnInFlames(EntityState state, long frameCounter)
        {
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            switch (animation.FrameSequenceName)
            {
                case "PreSliding":
                    SetCurrentAnimationByName("Sliding");
                    Velocity = PENGUIN_SLIDE_INITIAL_SPEED * (Direction == DefaultDirection ? Vector.LEFT_VECTOR : Vector.RIGHT_VECTOR);
                    break;

                case "PreJumping":
                    SetCurrentAnimationByName("Jumping");

                    FixedSingle jumpSpeedX = (Engine.Player.Origin.X - Origin.X) / PENGUIN_JUMP_FRAMES;
                    Velocity = (jumpSpeedX, -PENGUIN_JUMP_SPEED_Y);
                    break;

                case "LandingIntroducing":
                    SetCurrentAnimationByName("Introducing");
                    break;

                case "Introducing" when !HealthFilling && Health == 0:
                    StartHealthFilling();
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
            Velocity = Vector.NULL_VECTOR;
            State = PenguinState.DYING;
        }
    }
}