﻿using MMX.Geometry;
using MMX.Math;
using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.HUD
{
    public class HealthHUD : HUD
    {
        private static FixedSingle GetValue(GameEngine engine)
        {
            return engine.Player != null ? engine.Player.Health : 0;
        }

        private static FixedSingle GetHeight(GameEngine engine)
        {
            return 4 + 2 * engine.HealthCapacity + 16;
        }

        private static FixedSingle GetTop(GameEngine engine)
        {
            return HP_BOTTOM - GetHeight(engine);
        }

        private int topAnimationIndex;
        private int middleAnimationIndex;
        private int middleEmptyAnimationIndex;
        private int bottomAnimationIndex;

        public FixedSingle Capacity => Engine.HealthCapacity;

        public FixedSingle Value => GetValue(Engine);

        protected Animation TopAnimation => GetAnimation(topAnimationIndex);

        protected Animation MiddleAnimation => GetAnimation(middleAnimationIndex);

        protected Animation MiddleEmptyAnimation => GetAnimation(middleEmptyAnimationIndex);

        protected Animation BottomAnimation => GetAnimation(bottomAnimationIndex);

        public HealthHUD(GameEngine engine, string name) : base(engine, name, (HP_LEFT, GetTop(engine)), 6)
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            MultiAnimation = true;
        }

        protected internal override void UpdateOrigin()
        {
            Offset = (HP_LEFT, GetTop(Engine));

            base.UpdateOrigin();

            BottomAnimation.Offset = (0, 2 * Capacity + 4);
            MiddleEmptyAnimation.RepeatY = (int) (Capacity - Value);
            MiddleAnimation.Offset = (0, 2 * (Capacity - Value) + 4);
            MiddleAnimation.RepeatY = (int) Value;
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

            switch (frameSequenceName)
            {
                case "HPTop":
                    startOn = true;
                    startVisible = true;
                    topAnimationIndex = animationIndex;
                    break;

                case "HPMiddle":
                    startOn = true;
                    startVisible = true;
                    middleAnimationIndex = animationIndex;
                    break;

                case "HPMiddleEmpty":
                    startOn = true;
                    startVisible = true;
                    middleEmptyAnimationIndex = animationIndex;
                    offset = (0, 4);
                    break;

                case "HPBottom":
                    startOn = true;
                    startVisible = true;
                    bottomAnimationIndex = animationIndex;
                    break;

                default:
                    add = false;
                    break;
            }
        }
    }
}
