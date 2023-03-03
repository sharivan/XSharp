using XSharp.Engine.Collision;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies
{
    public enum ScriverState
    {
        IDLE = 0,
        DRILLING = 1,
        JUMPING = 2,
        LANDING = 3
    }

    public class Scriver : Enemy
    {
        private bool flashing;
        private bool jumping;
        private int jumpCounter;

        public ScriverState State
        {
            get => GetState<ScriverState>();
            set => SetState(value);
        }

        public Scriver()
        {
            SpriteSheetName = "Scriver";

            SetAnimationNames("Idle", "Drilling", "Jumping", "Landing");

            SetupStateArray(typeof(ScriverState));
            RegisterState(ScriverState.IDLE, OnIdle, "Idle");
            RegisterState(ScriverState.DRILLING, OnDrilling, "Drilling");
            RegisterState(ScriverState.JUMPING, OnJumping, "Jumping");
            RegisterState(ScriverState.LANDING, OnLanding, "Landing");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            flashing = false;
            jumping = false;
            jumpCounter = 0;

            PaletteName = "scriverPalette";
            Health = SCRIVER_HEALTH;
            ContactDamage = SCRIVER_CONTACT_DAMAGE;
            CollisionData = CollisionData.NONE;

            NothingDropOdd = 79;
            SmallHealthDropOdd = 10;
            BigHealthDropOdd = 10;
            SmallAmmoDropOdd = 0;
            BigAmmoDropOdd = 0;
            LifeUpDropOdd = 1;

            State = ScriverState.IDLE;
        }

        protected override FixedSingle GetLegsHeight()
        {
            return SCRIVER_SIDE_COLLIDER_BOTTOM_CLIP;
        }

        protected override Box GetCollisionBox()
        {
            return SCRIVER_COLLISION_BOX;
        }

        protected override Box GetHitbox()
        {
            return State == ScriverState.DRILLING ? SCRIVER_DRILLING_HITBOX : SCRIVER_HITBOX;
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            flashing = true;
            PaletteName = "flashingPalette";

            return base.OnTakeDamage(attacker, ref damage);
        }

        protected override void OnLanded()
        {
            if (State == ScriverState.JUMPING)
            {
                jumpCounter++;
                jumping = false;
                Velocity = Vector.NULL_VECTOR;
                State = ScriverState.LANDING;
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

                    if ((Engine.Player.Origin.X - Origin.X).Abs <= 50 && (Engine.Player.Origin.Y - Origin.Y).Abs <= 24)
                        State = ScriverState.DRILLING;
                }

                if (frameCounter >= 40 && State != ScriverState.DRILLING)
                {
                    State = ScriverState.JUMPING;
                    jumpCounter = 0;
                }
            }
        }

        private void OnJumping(EntityState state, long frameCounter)
        {
            if (frameCounter == 10)
            {
                Velocity = (Direction == Direction.RIGHT ? SCRIVER_JUMP_VELOCITY_X : -SCRIVER_JUMP_VELOCITY_X, SCRIVER_JUMP_VELOCITY_Y);
                jumping = true;
            }
            else if (!jumping)
            {
                Velocity = Vector.NULL_VECTOR;
            }
        }

        private void OnLanding(EntityState state, long frameCounter)
        {
            if (frameCounter >= 12)
                State = jumpCounter >= 2 ? ScriverState.IDLE : ScriverState.JUMPING;
        }

        private void OnDrilling(EntityState state, long frameCounter)
        {
            if (frameCounter >= 12 && (Engine.Player.Origin.X - Origin.X).Abs > 50)
                State = ScriverState.IDLE;
            else
                FaceToPlayer();
        }

        protected override bool PreThink()
        {
            if (flashing)
            {
                flashing = false;
                PaletteName = "scriverPalette";
            }

            return base.PreThink();
        }
    }
}