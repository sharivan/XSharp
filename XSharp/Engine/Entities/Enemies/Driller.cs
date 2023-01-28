using MMX.Geometry;

using MMX.Engine.Entities.Weapons;

using static MMX.Engine.Consts;
using System;

namespace MMX.Engine.Entities.Enemies
{
    public enum DrillerState
    {
        IDLE = 0,
        DRILLING = 1,
        JUMPING = 2,
        LANDING = 3
    }

    public class Driller : Enemy
    {
        private bool flashing;
        private bool jumping;
        private int jumpCounter;

        public DrillerState State
        {
            get => GetState<DrillerState>();
            set => SetState(value);
        }

        public Driller(GameEngine engine, string name, Vector origin) : base(engine, name, origin, 4)
        {
            SetupStateArray(typeof(DrillerState));
            RegisterState(DrillerState.IDLE, OnIdle, "Idle");
            RegisterState(DrillerState.DRILLING, OnDrilling, "Drilling");
            RegisterState(DrillerState.JUMPING, OnJumping, "Jumping");
            RegisterState(DrillerState.LANDING, OnLanding, "Landing");
        }

        internal override void OnSpawn()
        {
            base.OnSpawn();
           
            flashing = false;
            jumping = false;
            jumpCounter = 0;

            PaletteIndex = 5;
            Health = DRILLER_HEALTH;

            CheckCollisionWithWorld = true;
            CheckCollisionWithSprites = false;

            State = DrillerState.IDLE;
        }

        protected override void OnStartTouch(Entity entity)
        {
            if (entity is Weapon)
            {
                flashing = true;
                PaletteIndex = 4;
                Engine.PlaySound(2, 8);
            }

            base.OnStartTouch(entity);
        }

        public void FaceToPlayer(Player player)
        {
            var playerOrigin = player.Origin;
            if (Direction == Direction.LEFT && Origin.X < playerOrigin.X)
                Direction = Direction.RIGHT;
            else if (Direction == Direction.RIGHT && Origin.X > playerOrigin.X)
                Direction = Direction.LEFT;
        }

        protected override void OnLanded()
        {
            if (State == DrillerState.JUMPING)
            {
                jumpCounter++;
                jumping = false;
                Velocity = Vector.NULL_VECTOR;
                State = DrillerState.LANDING;
            }
        }

        private void OnIdle(EntityState state, long frameCounter)
        {
            if (Landed && Engine.Player != null)
            {
                if (frameCounter >= 12)
                {
                    FaceToPlayer(Engine.Player);

                    if ((Engine.Player.Origin.X - Origin.X).Abs <= 50)
                        State = DrillerState.DRILLING;
                }

                if (frameCounter >= 40 && State != DrillerState.DRILLING)
                {
                    State = DrillerState.JUMPING;
                    jumpCounter = 0;
                }
            }
        }

        private void OnJumping(EntityState state, long frameCounter)
        {
            if (frameCounter == 10)
            {
                Velocity = (Direction == Direction.RIGHT ? DRILLER_JUMP_VELOCITY_X : -DRILLER_JUMP_VELOCITY_X, DRILLER_JUMP_VELOCITY_Y);
                jumping = true;
            }
            else if (!jumping)
                Velocity = Vector.NULL_VECTOR;
        }

        private void OnLanding(EntityState state, long frameCounter)
        {
            if (frameCounter >= 12)
                State = jumpCounter >= 2 ? DrillerState.IDLE : DrillerState.JUMPING;
        }

        private void OnDrilling(EntityState state, long frameCounter)
        {
            if (frameCounter >= 12 && (Engine.Player.Origin.X - Origin.X).Abs > 50)
                State = DrillerState.IDLE;
            else
                FaceToPlayer(Engine.Player);
        }

        protected internal override void PostThink()
        {
            base.PostThink();

            if (flashing)
            {
                flashing = false;
                PaletteIndex = 5;
            }
        }
    }
}
