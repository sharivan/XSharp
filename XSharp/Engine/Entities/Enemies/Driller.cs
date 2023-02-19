using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies
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

        public Driller()
        {
            SpriteSheetIndex = 4;

            SetAnimationNames("Idle", "Drilling", "Jumping", "Landing");

            SetupStateArray(typeof(DrillerState));
            RegisterState(DrillerState.IDLE, OnIdle, "Idle");
            RegisterState(DrillerState.DRILLING, OnDrilling, "Drilling");
            RegisterState(DrillerState.JUMPING, OnJumping, "Jumping");
            RegisterState(DrillerState.LANDING, OnLanding, "Landing");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            flashing = false;
            jumping = false;
            jumpCounter = 0;

            PaletteIndex = 5;
            Health = DRILLER_HEALTH;
            ContactDamage = 0; //DRILLER_CONTACT_DAMAGE;
            CollisionData = CollisionData.SOLID;

            NothingDropOdd = 79;
            SmallHealthDropOdd = 10;
            BigHealthDropOdd = 10;
            SmallAmmoDropOdd = 0;
            BigAmmoDropOdd = 0;
            LifeUpDropOdd = 1;

            State = DrillerState.IDLE;
        }

        protected override FixedSingle GetLegsHeight()
        {
            return DRILLER_SIDE_COLLIDER_BOTTOM_CLIP;
        }

        protected override Box GetCollisionBox()
        {
            return DRILLER_COLLISION_BOX;
        }

        protected override Box GetHitbox()
        {
            return State == DrillerState.DRILLING ? DRILLER_DRILLING_HITBOX : DRILLER_HITBOX;
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            flashing = true;
            PaletteIndex = 4;

            return base.OnTakeDamage(attacker, ref damage);
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

        protected override void OnBlockedUp()
        {
            base.OnBlockedUp();

            Velocity = Vector.NULL_VECTOR;
        }

        private void OnIdle(EntityState state, long frameCounter)
        {
            if (Landed && Engine.Player != null)
            {
                if (frameCounter >= 12)
                {
                    FaceToPlayer();

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
                FaceToPlayer();
        }

        protected override bool PreThink()
        {
            if (flashing)
            {
                flashing = false;
                PaletteIndex = 5;
            }

            return base.PreThink();
        }
    }
}