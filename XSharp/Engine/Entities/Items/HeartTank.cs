﻿using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public class HeartTank : Item
    {
        public HeartTank(GameEngine engine, string name, Vector origin) : base(engine, name, origin, -1, 1, false, "HeartTank")
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
            Engine.StartHeartTankAcquiring();
        }
    }
}