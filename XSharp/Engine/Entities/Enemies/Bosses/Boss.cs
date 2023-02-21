using SharpDX;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Weapons;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses
{
    public delegate void BossDefeatedEvent(Boss boss, Player killer);

    public abstract class Boss : Enemy
    {
        // The interval between each position is five frames.
        private static readonly Vector[] EXPLOSION_ORIGIN_OFFSETS =
        {
            (-28, 10), // 0
            (16, -36),
            (-11, 21),
            (-25, -8),
            (29, -28),
            (-27, 6),
            (-18, -22),
            (-15, -8),
            (-27, -34),
            (-10, -36),
            (-22, 4), // 10
            (5, -33),
            (14, 2),
            (-4, 17),
            (5, -9),
            (26, 8),
            (30, -38),
            (8, -34),
            (-12, -25),
            (-14, -21),
            (28, -12), // 20
            (-2, -30),
            (1, -8),
            (16, -18),
            (9, -29),
            (21, 17),
            (-20, -34),
            (-14, -7),
            (-12, 5),
            (29, 8),
            (-21, -7), // 30 - Blink for two frames, step two frames, blink for more two frames, step two frames and finally blink for more two frames
            (25, 11),
            (-4, 0),
            (-11, -14), // 33 - Start fade out the tilemaps white and boss to black here. Fade out take 60 frames.
            (7, -17),
            (-27, 15),
            (-30, 12),
            (25, -5),
            (5, 4),
            (-30, -29),
            (10, -33), // 40
            (15, -9),
            (-19, 24),
            (22, 24),
            (-19, -30), // 44 - Pause fade out here
            (0, -32),
            (-29, 12),
            (-23, -24),
            (-1, -15),
            (3, 8),
            (-17, 17), // 50
            (-3, 5),
            (6, 8),
            (27, -18),
            (18, 24),
            (-9, 16),
            (-19, -7),
            (-17, 3),
            (-15, 17),
            (-19, 14),
            (-9, -17), // 60
            (25, -35),
            (4, -27),
            (-3, 23),
            (-9, 13),
            (14, -38),
            (-23, -11),
            (-17, 16),
            (-26, -3),
            (24, -38),
            (4, -27), // 70
            (-15, -15), // 71 - Boss start fading to transparent here and take 30 frames
            (17, -38),
            (3, -11),
            (28, 7),
            (29, 16),
            (32, -28),
            (13, -9),
            (-18, -35) // 78
            // Fading back from white start before 30 frames
        };

        public event BossDefeatedEvent BossDefeatedEvent;

        private BossHealthHUD healthHUD;
        private int bossPaletteIndex;

        public bool HealthFilling
        {
            get;
            private set;
        }

        public int HealthFillingFrameCounter
        {
            get;
            private set;
        }

        public FixedSingle MaxHealth
        {
            get;
            protected set;
        }

        public int InvincibilityFrames
        {
            get;
            protected set;
        } = DEFAULT_BOSS_INVINCIBILITY_TIME;

        public bool Exploding
        {
            get;
            private set;
        }

        public int ExplodingFrameCounter
        {
            get;
            private set;
        }

        public bool LockPlayerOnDefeat
        {
            get;
            set;
        } = true;

        protected Boss()
        {
            healthHUD = Engine.CreateEntity<BossHealthHUD>(new
            {
                Boss = this,
                Visible = false
            });
        }

        protected override void OnBlink(int frameCounter)
        {
            if (!Exploding)
            {
                if (frameCounter % 2 == 0)
                    PaletteIndex = PaletteIndex != 4 ? 4 : bossPaletteIndex;
            }
            else
            {
                Blinking = false;
                PaletteIndex = bossPaletteIndex;
            }
        }

        protected override void OnEndBlinking()
        {
            PaletteIndex = bossPaletteIndex;
        }

        protected override void OnDamaged(Sprite attacker, FixedSingle damage)
        {
            Engine.PlaySound(2, 27);
            MakeInvincible(InvincibilityFrames);

            if (!Exploding)
                MakeBlinking(InvincibilityFrames);
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            if (Exploding)
                return false;

            switch (attacker)
            {
                case BusterLemon lemon:
                    damage = lemon.DashLemon ? 2 : 1;
                    break;

                case BusterSemiCharged:
                    damage = 2;
                    break;

                case BusterCharged:
                    damage = 3;
                    break;
            }

            return base.OnTakeDamage(attacker, ref damage);
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Health = 0;
            MaxHealth = BOSS_HP;
            HealthFilling = false;
            KillOnOffscreen = false;
            bossPaletteIndex = PaletteIndex;
            Exploding = false;

            healthHUD.Spawn();
        }

        protected override void OnDeath()
        {
            healthHUD.Kill();

            base.OnDeath();
        }

        protected override void Think()
        {
            base.Think();

            if (HealthFilling)
            {
                if (HealthFillingFrameCounter % 2 == 0)
                {
                    Health++;

                    if (Health == MaxHealth)
                    {
                        HealthFilling = false;
                        OnStartBattle();
                    }
                }

                Engine.PlaySound(0, 19);

                HealthFillingFrameCounter++;
            }
            else if (Exploding)
            {
                if (ExplodingFrameCounter % 5 == 0)
                {
                    int index = ExplodingFrameCounter / 5;
                    if (index < EXPLOSION_ORIGIN_OFFSETS.Length)
                    {
                        Vector origin = EXPLOSION_ORIGIN_OFFSETS[index];
                        Engine.CreateExplosionEffect(Origin + origin, ExplosionEffectSound.NONE);
                    }
                }

                if (ExplodingFrameCounter is >= (30 * 5) and < (30 * 5 + 12)) // Blink white three times between frames 150 and 162, each blink taking two frames, two frames between each blink.
                {
                    int frame = ExplodingFrameCounter - 30 * 5;
                    if (frame % 4 is 0 or 1)
                    {
                        Engine.FadingSettings.FadingLevel = new Vector4(1, 1, 1, 0);
                        Engine.FadingSettings.FadingColor = Color.White;
                    }
                    else
                        Engine.FadingSettings.FadingLevel = Vector4.Zero;
                }
                else if (ExplodingFrameCounter is >= (33 * 5) and < (33 * 5 + 60)) // On frame 162, start tilemaps fading to white and boss fading to black. Fading take 60 frames.
                {
                    float fadingLevel = (ExplodingFrameCounter - 33 * 5) / 59f;

                    Engine.World.FadingSettings.FadingLevel = new Vector4(fadingLevel, fadingLevel, fadingLevel, 0);
                    Engine.World.FadingSettings.FadingColor = Color.White;

                    FadingSettings.FadingLevel = new Vector4(fadingLevel, fadingLevel, fadingLevel, 0);
                    FadingSettings.FadingColor = Color.Black;
                }
                else if (ExplodingFrameCounter is >= (71 * 5) and < (71 * 5 + 30)) // On frame 355, start fading out boss from black to transparent. This fading take 30 frames. 
                {
                    float fadingLevel = (ExplodingFrameCounter - 71 * 5) / 29f;
                    FadingSettings.FadingLevel = new Vector4(1, 1, 1, fadingLevel);
                    FadingSettings.FadingColor = Color.Transparent;
                }
                else if (ExplodingFrameCounter is >= (78 * 5) and < (78 * 5 + 32)) // On frame 390, fade in the tilemaps
                {
                    float fadingLevel = 1 - (ExplodingFrameCounter - 78 * 5) / 31f;
                    Engine.World.FadingSettings.FadingLevel = new Vector4(fadingLevel, fadingLevel, fadingLevel, 0);
                    Engine.World.FadingSettings.FadingColor = Color.White;
                }
                else if (ExplodingFrameCounter >= 78 * 5 + 32 + 2 * 60)
                {
                    Exploding = false;
                    BossDefeatedEvent?.Invoke(this, Engine.Player);
                }

                ExplodingFrameCounter++;
                if (ExplodingFrameCounter == EXPLOSION_ORIGIN_OFFSETS.Length * 5)
                    Engine.PlayBossExplosionEnd();
            }
        }

        protected virtual void OnStartBattle()
        {
            Engine.Player.InputLocked = false;
            Engine.Player.Invincible = false;
            Engine.PlayBossBatleOST();
        }

        protected void StartHealthFilling()
        {
            healthHUD.Visible = true;
            HealthFilling = true;
            HealthFillingFrameCounter = 0;
        }

        protected override bool OnBreak()
        {
            Die();
            return false;
        }

        protected virtual void OnDying()
        {
            Engine.StopAllSounds();
            Explode();
        }

        public void Die()
        {
            Invincible = true;
            Velocity = Vector.NULL_VECTOR;
            Engine.StopBossBattleOST();
            Engine.FreezeSprites(60, Explode);

            OnDying();
        }

        public virtual void Explode()
        {
            if (Alive && !Exploding)
            {
                if (LockPlayerOnDefeat)
                {
                    var player = Engine.Player;
                    player.Invincible = true;
                    player.InputLocked = true;
                    player.Blinking = false;
                    player.StopMoving();
                }

                Exploding = true;
                ExplodingFrameCounter = 0;
                Engine.PlayBossExplosionLoop();
            }
        }
    }
}