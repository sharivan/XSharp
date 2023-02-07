using XSharp.Engine.Entities.HUD;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses
{
    public class BossHealthHUD : HealthHUD
    {
        public Boss Boss
        {
            get;
            internal set;
        }

        internal BossHealthHUD()
        {
            Left = BOSS_HP_LEFT;
            Image = HUDImage.BOSS;
        }

        protected internal override void UpdateOrigin()
        {
            Capacity = Boss.MaxHealth;
            Value = Boss.Health;

            base.UpdateOrigin();
        }
    }
}
