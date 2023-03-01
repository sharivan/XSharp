namespace XSharp.Engine.Entities.Effects
{
    public enum ExplosionEffectSound
    {
        NONE = 0,
        ENEMY_DIE_1 = 1,
        ENEMY_DIE_2 = 2,
        ENEMY_DIE_3 = 3,
        ENEMY_DIE_4 = 4
    }

    internal class ExplosionEffect : SpriteEffect
    {
        public ExplosionEffectSound EffectSound
        {
            get;
            set;
        } = ExplosionEffectSound.ENEMY_DIE_1;

        public int SoundChannel
        {
            get;
            set;
        } = 2;

        public ExplosionEffect()
        {
            SpriteSheetName = "Explosion";
            Directional = false;

            SetAnimationNames("Explosion");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            switch (EffectSound)
            {
                case ExplosionEffectSound.ENEMY_DIE_1:
                    Engine.PlaySound(SoundChannel, 12);
                    break;

                case ExplosionEffectSound.ENEMY_DIE_2:
                    Engine.PlaySound(SoundChannel, 13);
                    break;

                case ExplosionEffectSound.ENEMY_DIE_3:
                    Engine.PlaySound(SoundChannel, 14);
                    break;

                case ExplosionEffectSound.ENEMY_DIE_4:
                    Engine.PlaySound(SoundChannel, 15);
                    break;
            }
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}