namespace XSharp.Engine.Entities.Effects
{
    public class ChargingEffect : SpriteEffect
    {
        private int level;

        private bool soundPlayed;

        private Player charger;

        public Player Charger
        {
            get => charger;
            set
            {
                charger = value;
                if (value != null)
                {
                    Origin = value.Hitbox.Center;
                    Direction = value.Direction;
                }
            }
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

        public ChargingEffect()
        {
            SpriteSheetIndex = 3;
            Directional = true;
            PaletteIndex = 3;

            SetAnimationNames("ChargingLevel1", "ChargingLevel2");
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

            Origin = Charger.Hitbox.Center;
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