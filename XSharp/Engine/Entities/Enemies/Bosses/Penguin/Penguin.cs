using System;
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
        private bool hanging;
        private bool snowing;
        private int snowingFrameCounter;
        private bool wasShootingIce;
        private int iceCount;
        private PenguinLever lever;
        private SnowHUD snow;
        private PenguinSculpture sculpture1;
        private PenguinSculpture sculpture2;
        private PenguinFrozenBlock frozenBlock;

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
            RegisterState(PenguinState.HANGING, OnStartHanging, OnHanging, null, "Idle");
            RegisterState(PenguinState.TAKING_DAMAGE, OnTakingDamage, "TakingDamage");
            RegisterState(PenguinState.IN_FLAMES, OnInFlames, "InFlames");
            RegisterState(PenguinState.DYING, OnStartDying, "Dying");

            lever = new PenguinLever();
            snow = new SnowHUD();

            sculpture1 = new PenguinSculpture()
            {
                Shooter = this
            };

            sculpture2 = new PenguinSculpture()
            {
                Shooter = this
            };

            frozenBlock = new PenguinFrozenBlock();
        }

        public override FixedSingle GetGravity()
        {
            return hanging || State == PenguinState.DYING ? 0 : base.GetGravity();
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
                PenguinState.TAKING_DAMAGE => PENGUIN_TAKING_DAMAGE_HITBOX,
                _ => PENGUIN_HITBOX,
            };
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            PaletteIndex = 7;
            firstAttack = true;
            hanging = false;
            snowing = false;
            snowingFrameCounter = 0;
            wasShootingIce = false;
            iceCount = 0;
            Direction = Direction.LEFT;
            MaxHealth = BOSS_HP;

            snow.Spawn();

            SetState(PenguinState.INTRODUCING);
        }

        protected override void OnDeath()
        {
            sculpture1.Kill();
            sculpture2.Kill();
            snow.Kill();
            lever.Kill();

            base.OnDeath();
        }

        protected override void OnLanded()
        {
            base.OnLanded();

            switch (State)
            {
                case PenguinState.INTRODUCING:
                    SetCurrentAnimationByName("LandingIntroducing");
                    break;

                case PenguinState.TAKING_DAMAGE:
                    State = PenguinState.IDLE;
                    break;

                default:
                    SetCurrentAnimationByName("Landing");
                    break;
            }

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

        private void ApplyKnockback(Sprite attacker)
        {
            FaceToEntity(attacker);
            Velocity = (Direction == DefaultDirection ? PENGUIN_KNOCKBACK_SPEED_X : -PENGUIN_KNOCKBACK_SPEED_X, -PENGUIN_KNOCKBACK_SPEED_Y);
            State = PenguinState.TAKING_DAMAGE;
        }

        protected override void OnDamaged(Sprite attacker, FixedSingle damage)
        {
            base.OnDamaged(attacker, damage);

            if (State is PenguinState.IDLE or PenguinState.JUMPING or PenguinState.HANGING or PenguinState.SHOOTING_ICE)
                ApplyKnockback(attacker);
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

                    int value = Engine.RNG.Next(5);
                    while (State == PenguinState.IDLE)
                    {                        
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

                            case 3:
                                State = PenguinState.HANGING;
                                break;

                            case 4:
                                if (!(sculpture1.Alive && !sculpture1.MarkedToRemove && !sculpture1.Broke
                                    || sculpture2.Alive && !sculpture2.MarkedToRemove && !sculpture2.Broke))
                                    State = PenguinState.BLOWING;

                                break;
                        }

                        value = Engine.RNG.Next(4);
                    }
                }
            }
        }

        private void OnShootingIce(EntityState state, long frameCounter)
        {
            if (frameCounter == PENGUIN_SHOT_START_FRAME)
            {
                ShootIce();
                wasShootingIce = true;
                State = PenguinState.IDLE;
            }
        }

        private void OnStartBlowing(EntityState state, EntityState lastState)
        {
        }

        private void OnBlowing(EntityState state, long frameCounter)
        {
            switch (frameCounter)
            {
                case PENGUIN_SHOT_START_FRAME:
                    Engine.PlaySound(4, 29);
                    break;

                case PENGUIN_BLOW_FRAMES_TO_SPAWN_SCULPTURES:
                    BreakSculptures();

                    sculpture1.Origin = Origin + (Direction == Direction.RIGHT ? PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.X : -PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.X, PENGUIN_SCUPTURE_ORIGIN_OFFSET_1.Y);
                    sculpture1.Spawn();

                    sculpture2.Origin = Origin + (Direction == Direction.RIGHT ? PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.X : -PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.X, PENGUIN_SCUPTURE_ORIGIN_OFFSET_2.Y);
                    sculpture2.Spawn();
                    break;

                case PENGUIN_BLOW_FRAMES:
                    State = PenguinState.IDLE;
                    break;
            }

            if (frameCounter >= PENGUIN_SHOT_START_FRAME && frameCounter % 8 == 0)
                ShootSnow();
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

        private void ShootSnow()
        {
            var snow = new PenguinSnow()
            {
                Shooter = this
            };

            snow.Spawn();
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

        private void OnStartHanging(EntityState state, EntityState laststate)
        {
            FaceToScreenCenter();
        }

        private void OnHanging(EntityState state, long frameCounter)
        {
            switch (frameCounter)
            {
                case PENGUIN_FRAMES_BEFORE_HANGING_JUMP:
                    SetCurrentAnimationByName("PreJumping", 0);
                    break;

                case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG:
                    Velocity = Vector.NULL_VECTOR;
                    Origin = lever.Origin + (lever.Origin.X < Origin.X ? PENGUIN_HANGING_OFFSET : (-PENGUIN_HANGING_OFFSET.X, PENGUIN_HANGING_OFFSET.Y));
                    hanging = true;
                    SetCurrentAnimationByName("Hanging", 0);
                    break;

                case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG + PENGUIN_FRAMES_BEFORE_SNOW_AFTER_HANGING:
                    snowing = true;
                    snowingFrameCounter = 0;
                    snow.SnowDirection = Direction;
                    snow.Play();
                    PlaySnowSoundLoop();
                    break;

                case PENGUIN_FRAMES_BEFORE_HANGING_JUMP + PENGUIN_FRAMES_TO_HANG + PENGUIN_FRAMES_BEFORE_STOP_HANGING:
                    Velocity = Vector.NULL_VECTOR;
                    hanging = false;
                    SetCurrentAnimationByName("Falling", 0);
                    break;
            }
        }

        private void OnTakingDamage(EntityState state, long frameCounter)
        {
            hanging = false;
            iceCount = 4;
        }

        private void OnInFlames(EntityState state, long frameCounter)
        {
        }

        private void BreakSculptures()
        {
            if (sculpture1.Alive && !sculpture1.MarkedToRemove && !sculpture1.Broke)
                sculpture1.Break();

            if (sculpture2.Alive && !sculpture2.MarkedToRemove && !sculpture2.Broke)
                sculpture2.Break();
        }

        private void OnStartDying(EntityState state, EntityState lastState)
        {
            BreakSculptures();
        }

        private void PlaySnowSoundLoop()
        {
            Engine.PlaySound(5, 36, 1.2, 0.128);
        }

        private void FinishSnowSoundLoop()
        {
            Engine.ClearSoundLoopPoint(5, 36, true);
        }

        protected override void Think()
        {
            if (snowing)
            {
                var adictionalVelocity = (Direction == Direction.LEFT ? -PENGUIN_HANGING_SNOWING_SPEED_X : PENGUIN_HANGING_SNOWING_SPEED_X, 0);
                Engine.Player.AdictionalVelocity = adictionalVelocity;

                if (sculpture1.Alive && !sculpture1.MarkedToRemove && !sculpture1.Broke)
                    sculpture1.AdictionalVelocity = adictionalVelocity;

                if (sculpture2.Alive && !sculpture2.MarkedToRemove && !sculpture2.Broke)
                    sculpture2.AdictionalVelocity = adictionalVelocity;

                snowingFrameCounter++;
                if (snowingFrameCounter == PENGUIN_SNOW_FRAMES)
                {
                    FinishSnowSoundLoop();
                    snow.Stop();
                    snowing = false;
                }
            }

            base.Think();
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
                    switch (State)
                    {
                        case PenguinState.JUMPING:
                        {
                            FixedSingle jumpSpeedX = (Engine.Player.Origin.X - Origin.X) / PENGUIN_JUMP_FRAMES;
                            Velocity = (jumpSpeedX, -PENGUIN_JUMP_SPEED_Y);
                            break;
                        }

                        case PenguinState.HANGING:
                        {
                            FixedSingle jumpSpeedX = (lever.Origin.X - Origin.X) / PENGUIN_FRAMES_TO_HANG;
                            Velocity = (jumpSpeedX, -PENGUIN_HANGING_JUMP_SPEED_Y);
                            break;
                        }
                    }

                    SetCurrentAnimationByName("Jumping");
                    break;

                case "LandingIntroducing":
                    SetCurrentAnimationByName("Introducing");
                    break;

                case "Introducing" when !HealthFilling && Health == 0:
                    StartHealthFilling();

                    lever.Origin = World.World.GetSceneBoundingBoxFromPos(Origin).MiddleTop + (0, 12);
                    lever.Spawn();
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

        public void FreezePlayer()
        {
            if (!frozenBlock.Alive)
                frozenBlock.Spawn();
        }
    }
}