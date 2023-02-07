using XSharp.Geometry;

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
        IN_FLAMES = 8
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
            SpriteSheetIndex = 10;
            PaletteIndex = 7;

            ContactDamage = 6;

            SetAnimationNames("FallingIntroducing", "LandingIntroducing", "Introducing", "IntroducingEnd", "Idle", "ShootingIce",
                "PreSliding", "Sliding", "Blowing", "PreJumping", "Jumping", "Falling", "Landing", "Hanging", "TakingDamage", "InFlames");

            SetupStateArray(typeof(PenguinState));
            RegisterState(PenguinState.IDLE, OnIdle, "Idle");
            RegisterState(PenguinState.INTRODUCING, "FallingIntroducing");
            RegisterState(PenguinState.SHOOTING_ICE, OnShootingIce, "ShootingIce");
            RegisterState(PenguinState.BLOWING, OnShootingIce, "Blowing");
            RegisterState(PenguinState.SLIDING, OnSliding, "PreSliding");
            RegisterState(PenguinState.JUMPING, OnJumping, "PreJumping");
            RegisterState(PenguinState.HANGING, OnHanging, "PreJumping");
            RegisterState(PenguinState.TAKING_DAMAGE, OnTakingDamage, "TakingDamage");
            RegisterState(PenguinState.IN_FLAMES, OnInFlames, "InFlames");  
        }

        protected override Box GetCollisionBox()
        {
            return PENGUIN_COLLISION_BOX;
        }

        protected override Box GetHitbox()
        {
            return State == PenguinState.INTRODUCING ? PENGUIN_COLLISION_BOX : State == PenguinState.SLIDING ? PENGUIN_SLIDE_HITBOX : PENGUIN_HITBOX;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            PaletteIndex = 7;
            firstAttack = true;
            wasShootingIce = false;
            iceCount = 0;

            SetState(PenguinState.INTRODUCING);
        }

        protected override void OnLanded()
        {
            base.OnLanded();

            if (State == PenguinState.INTRODUCING)
                SetCurrentAnimationByName("LandingIntroducing");
            else
                SetCurrentAnimationByName("Landing");
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
                    State = PenguinState.SHOOTING_ICE;
                }
                else if (wasShootingIce && iceCount < 4)
                    State = PenguinState.SHOOTING_ICE;
                else
                {
                    wasShootingIce = false;
                    iceCount = 0;
                    int value = Engine.RNG.Next(2);
                    switch (value)
                    {
                        case 0:
                            State = PenguinState.SLIDING;
                            break;

                        case 1:
                            State = PenguinState.SHOOTING_ICE;
                            break;

                            /*case 2:
                                State = PenguinState.JUMPING;
                                break;

                            case 3:
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

        private void ShootIce()
        {
            var ice = new PenguinIce()
            {
                Shooter = this
            };

            ice.Spawn();
            iceCount++;
        }

        private void OnSliding(EntityState state, long frameCounter)
        {
            if (frameCounter >= 30)
            {
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

        private void OnJumping(EntityState state, long frameCounter)
        {
            if (!Landed && Velocity.Y > 0)
                SetCurrentAnimationByName("Falling");
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
    }
}