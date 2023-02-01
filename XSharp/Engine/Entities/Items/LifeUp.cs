using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public class LifeUp : Item
    {
        public LifeUp(GameEngine engine, string name, Vector origin, int durationFrames = 0) : base(engine, name, origin, durationFrames, 1, false, "LifeUp")
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = true;
            CheckCollisionWithSprites = false;

            CurrentAnimationIndex = 0;
            CurrentAnimation.StartFromBegin();
        }

        protected override void OnCollecting(Player player)
        {
            if (player.Lives <= MAX_LIVES)
            {
                player.Lives++;
                Engine.PlaySound(0, 20, true);
            }
        }
    }
}