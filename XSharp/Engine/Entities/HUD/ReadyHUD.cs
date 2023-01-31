using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.HUD
{
    public class ReadyHUD : HUD
    {
        public ReadyHUD(GameEngine engine, string name) : base(engine, name, READY_OFFSET, 7)
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CurrentAnimationIndex = 0;
            CurrentAnimation.StartFromBegin();
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            Engine.SpawnPlayer();
            KillOnNextFrame();
        }
    }
}
