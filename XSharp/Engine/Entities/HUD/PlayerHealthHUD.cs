using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD;

public class PlayerHealthHUD : HealthHUD
{
    public PlayerHealthHUD()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Left = HP_LEFT;
        Image = HUDImage.X;
    }

    protected override void OnPostThink()
    {
        Capacity = Engine.HealthCapacity;
        Value = Engine.Player != null ? Engine.Player.Health : 0;

        base.OnPostThink();
    }
}
