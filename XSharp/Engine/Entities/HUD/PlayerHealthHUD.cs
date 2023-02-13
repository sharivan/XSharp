using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD
{
    public class PlayerHealthHUD : HealthHUD
    {
        public PlayerHealthHUD()
        {
            Left = HP_LEFT;
            Image = HUDImage.X;
        }

        protected internal override void UpdateOrigin()
        {
            Capacity = Engine.HealthCapacity;
            Value = Engine.Player != null ? Engine.Player.Health : 0;

            base.UpdateOrigin();
        }
    }
}
