using XSharp.Engine.Entities.HUD;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses;

public class BossHealthHUD : HealthHUD
{
    public Boss Boss
    {
        get;
        internal set;
    }

    public BossHealthHUD()
    {
        Left = BOSS_HP_LEFT;
        Image = HUDImage.BOSS;
    }

    protected internal override void PostThink()
    {
        Capacity = Boss.MaxHealth;
        Value = Boss.Health;

        base.PostThink();
    }
}