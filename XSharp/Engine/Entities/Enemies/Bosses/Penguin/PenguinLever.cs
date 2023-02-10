﻿using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public enum PenguinLeverState
    {
        SHOWING,
        IDLE,
        PULLED,
        HIDING
    }

    public class PenguinLever : Sprite
    {
        private int showingFrameCounter;

        public PenguinLeverState State
        {
            get;
            private set;
        }

        public PenguinLever()
        {
            Directional = true;
            SpriteSheetIndex = 10;

            SetAnimationNames("Lever");
        }

        public override FixedSingle GetGravity()
        {
            return 0;
        }

        protected override Box GetHitbox()
        {
            return PENGUIN_LEVER_HITBOX;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;

            showingFrameCounter = 0;

            State = PenguinLeverState.SHOWING;
        }

        internal void Hide()
        {
            State = PenguinLeverState.HIDING;
        }

        protected override void Think()
        {
            base.Think();

            switch (State)
            {
                case PenguinLeverState.SHOWING:
                    showingFrameCounter++;
                    Origin += Vector.DOWN_VECTOR;
                    if (showingFrameCounter == PENGUIN_LEVER_MOVING_FRAMES)
                        State = PenguinLeverState.IDLE;

                    break;

                case PenguinLeverState.HIDING:
                    showingFrameCounter++;
                    Origin += Vector.UP_VECTOR;
                    if (showingFrameCounter == 0)
                        State = PenguinLeverState.IDLE;

                    break;
            }
        }
    }
}