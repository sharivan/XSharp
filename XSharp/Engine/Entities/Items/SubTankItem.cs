using MMX.Geometry;

namespace MMX.Engine.Entities.Items
{
    public class SubTankItem : Item
    {
        public SubTankItem(GameEngine engine, string name, Vector origin) : base(engine, name, origin, -1, 1, false, "SubTank")
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CurrentAnimationIndex = 0;
            CurrentAnimation.StartFromBegin();
        }

        protected override void OnCollecting(Player player)
        {
            Engine.StartSubTankAcquiring();
        }
    }
}
