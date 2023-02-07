using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinIceFragment : Sprite
    {
        public PenguinIceFragment()
        {
            Directional = false;
            SpriteSheetIndex = 10;
            PaletteIndex = 7;
            KillOnOffscreen = true;

            SetAnimationNames("IceFragment");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;
        }
    }
}