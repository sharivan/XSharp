using MMX.Geometry;

using MMX.Engine.Entities.Weapons;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Enemies
{
    public class Driller : Enemy
    {
        private DrillerState state;
        private bool flashing;
        private bool jumping;
        private int jumpCounter;
        private bool stateChanged;
        private int frameCounter;

        public enum DrillerState
        {
            IDLE = 0,
            DRILLING = 1,
            JUMPING = 2,
            LANDING = 3
        }

        public DrillerState State
        {
            get => state;
            set
            {
                frameCounter = 0;
                state = value;
                stateChanged = true;
            }
        }

        public Driller(GameEngine engine, string name, Vector origin, int spriteSheetIndex) : base(engine, name, origin, spriteSheetIndex) { }

        public override void OnSpawn()
        {
            base.OnSpawn();
           
            flashing = false;
            jumping = false;
            jumpCounter = 0;
            stateChanged = false;
            frameCounter = 0;

            PaletteIndex = 5;
            State = DrillerState.IDLE;
            Health = DRILLER_HEALTH;

            CheckCollisionWithWorld = true;
            CheckCollisionWithSprites = false;
            CurrentAnimationIndex = 0;
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

        private void UpdateAnimation()
        {
            switch (State)
            {
                case DrillerState.IDLE:
                    CurrentAnimationIndex = Direction == Direction.RIGHT ? 0 : 1;
                    break;

                case DrillerState.JUMPING:
                    CurrentAnimationIndex = Direction == Direction.RIGHT ? 2 : 3;
                    break;

                case DrillerState.LANDING:
                    CurrentAnimationIndex = Direction == Direction.RIGHT ? 4 : 5;
                    break;

                case DrillerState.DRILLING:
                    CurrentAnimationIndex = Direction == Direction.RIGHT ? 6 : 7;
                    break;               
            }

            if (stateChanged)
            {
                stateChanged = false;
                CurrentAnimation.StartFromBegin();
            }
        }

        protected override void OnLanded()
        {
            if (State == DrillerState.JUMPING)
            {
                jumpCounter++;
                jumping = false;
                Velocity = Vector.NULL_VECTOR;
                State = DrillerState.LANDING;
                UpdateAnimation();
            }
        }

        protected override void Think()
        {
            switch (State)
            {
                case DrillerState.IDLE:
                {
                    if (Landed && Engine.Player != null)
                    {
                        if (frameCounter >= 12)
                            FaceToPlayer(Engine.Player);
                        
                        if (frameCounter >= 40)
                        {
                            State = DrillerState.JUMPING;
                            jumpCounter = 0;
                        }
                    }

                    break;
                }

                case DrillerState.JUMPING:
                {
                    if (frameCounter == 10)
                    {
                        Velocity = (Direction == Direction.RIGHT ? DRILLER_JUMP_VELOCITY_X : -DRILLER_JUMP_VELOCITY_X, DRILLER_JUMP_VELOCITY_Y);
                        jumping = true;
                    }
                    else if (!jumping)
                        Velocity = Vector.NULL_VECTOR;

                    break;
                }

                case DrillerState.LANDING when frameCounter >= 12:
                    State = jumpCounter >= 2 ? DrillerState.IDLE : DrillerState.JUMPING;
                    break;

                case DrillerState.DRILLING:
                    break;
            }

            frameCounter++;
            UpdateAnimation();

            base.Think();
        }

        protected override void PostThink()
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
