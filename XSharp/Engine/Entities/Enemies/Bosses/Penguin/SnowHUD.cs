using System;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class SnowHUD : HUD.HUD
    {
        public FixedSingle Speed
        {
            get;
            set;
        } = 2;

        public Direction SnowDirection
        {
            get;
            set;
        } = Direction.RIGHT;

        public bool StartPlaying
        {
            get;
            set;
        } = false;

        public bool Playing => Visible;

        public SnowHUD()
        {
            SpriteSheetIndex = 11;
            Directional = false;

            SetAnimationNames("Snow");
        }

        protected override Box GetBoundingBox()
        {
            return (0, 0, 2 * SCENE_SIZE, 2 * SCENE_SIZE);
        }

        private void ResetOffset()
        {
            Offset = (SnowDirection == Direction.RIGHT ? -SCENE_SIZE : 0, -SCENE_SIZE);
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Visible = StartPlaying;

            ResetOffset();
        }

        protected internal override void UpdateOrigin()
        {
            if (!Visible)
                return;

            Offset += (SnowDirection == Direction.RIGHT ? Speed : -Speed, Speed);
            if (Offset.Y >= 0)
                ResetOffset();

            base.UpdateOrigin();
        }

        protected override void OnCreateAnimation(int animationIndex, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

            switch (frameSequenceName)
            {
                case "Snow":
                    startOn = true;
                    startVisible = true;
                    repeatX = 2;
                    repeatY = 2;
                    break;

                default:
                    add = false;
                    break;
            }
        }

        public void Play()
        {
            Visible = true;
            ResetOffset();
        }

        public void Stop()
        {
            Visible = false;
        }
    }
}