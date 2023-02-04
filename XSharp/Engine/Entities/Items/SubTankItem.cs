using XSharp.Geometry;

namespace XSharp.Engine.Entities.Items
{
    public class SubTankItem : Item
    {
        public SubTankItem(string name, Vector origin) : base(name, origin, -1, 1, false, "SubTank")
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
