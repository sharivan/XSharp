using XSharp.Engine.Entities.HUD;
using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses
{
    public abstract class Boss : Enemy
    {
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

        protected Boss()
        {
            healthHUD = new BossHealthHUD
            {
                Boss = this,
                Visible = false
            };
        }

        protected override void OnBlink(int frameCounter)
        {
            if (frameCounter % 2 == 0)
                PaletteIndex = PaletteIndex != 4 ? 4 : bossPaletteIndex;
        }

        protected override void OnEndBlink()
        {
            PaletteIndex = bossPaletteIndex;
        }

        protected override void OnDamaged()
        {           
            Engine.PlaySound(2, 27);
            MakeInvincible(InvincibilityFrames);
            MakeBlinking(InvincibilityFrames);
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
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

            return true;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Health = 0;
            MaxHealth = BOSS_HP;
            HealthFilling = false;
            KillOnOffscreen = false;
            bossPaletteIndex = PaletteIndex;

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
                HealthFillingFrameCounter++;
                if (HealthFillingFrameCounter % 2 == 0)
                {
                    Health++;
                    Engine.PlaySound(0, 19);

                    if (Health == BOSS_HP)
                    {
                        HealthFilling = false;
                        OnStartBattle();
                    }
                }
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
    }
}