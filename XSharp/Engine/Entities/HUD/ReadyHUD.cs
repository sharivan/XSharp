using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD
{
    public class ReadyHUD : HUD
    {
        public ReadyHUD()
        {
            Offset = READY_OFFSET;
            SpriteSheetIndex = 7;
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