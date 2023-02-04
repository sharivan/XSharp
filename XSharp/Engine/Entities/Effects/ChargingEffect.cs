namespace MMX.Engine.Entities.Effects
{
    public class ChargingEffect : SpriteEffect
    {
        private int level;

        private bool soundPlayed;

        public Player Charger
        {
            get;
        }

        public int Level
        {
            get => level;

            set
            {
                if (level is < 1 or > 2)
                    return;

                level = value;
                CurrentAnimationIndex = level - 1;
            }
        }

        public ChargingEffect(GameEngine engine, string name, Player charger) : base(engine, name, charger.HitBox.Center, 3, true, "ChargingLevel1", "ChargingLevel2")
        {
            Charger = charger;
            PaletteIndex = 3;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            level = 1;
        }

        protected override void Think()
        {
            if (!soundPlayed)
            {
                Engine.PlaySound(2, 3, 3.350, 1.585);
                soundPlayed = true;
            }

            Origin = Charger.HitBox.Center;
            Direction = Charger.Direction;

            base.Think();
        }

        protected override void OnDeath()
        {
            Engine.StopSound(2, 3);
            base.OnDeath();
        }
    }
}
