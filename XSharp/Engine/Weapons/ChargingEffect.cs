﻿using MMX.Math;
using MMX.Geometry;

namespace MMX.Engine.Weapons
{
    public class ChargingEffect : Sprite
    {
        private readonly Player charger;
        private int level;
        private readonly int[] animationIndices;

        public ChargingEffect(GameEngine engine, string name, Player charger, SpriteSheet sheet) : base(engine, name, charger.CollisionBox.Center, sheet)
        {
            Parent = charger;

            CheckCollisionWithWorld = false;

            animationIndices = new int[2];
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

        public override FixedSingle GetGravity() => FixedSingle.ZERO;

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