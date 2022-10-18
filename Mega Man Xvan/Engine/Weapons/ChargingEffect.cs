using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Math;
using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Weapons
{
    public class ChargingEffect : Sprite
    {
        private Player charger;
        private int level;
        private int[] animationIndices;

        public ChargingEffect(GameEngine engine, string name, Player charger, SpriteSheet sheet) : base(engine, name, charger.CollisionBox.Center, sheet)
        {
            Parent = charger;

            CheckCollisionWithWorld = false;

            animationIndices = new int[2];
        }

        public int Level
        {
            get
            {
                return level;
            }

            set
            {
                if (level < 1 || level > 2)
                    return;

                level = value;
                CurrentAnimationIndex = level - 1;
            }
        }

        public override FixedSingle GetGravity()
        {
            return FixedSingle.ZERO;
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        public override void Spawn()
        {
            base.Spawn();

            level = 1;
            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);
            startOn = false;
            startVisible = false;

            if (frameSequenceName == "ChargingLevel1")
                animationIndices[0] = animationIndex;
            else if (frameSequenceName == "ChargingLevel2")
                animationIndices[1] = animationIndex;
            else
                add = false;
        }
    }
}
