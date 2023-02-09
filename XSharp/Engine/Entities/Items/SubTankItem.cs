namespace XSharp.Engine.Entities.Items
{
    public class SubTankItem : Item
    {
        public SubTankItem()
        {
            SpriteSheetIndex = 1;
            DurationFrames = 0;

            SetAnimationNames("SubTank");
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
