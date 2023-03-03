using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies
{
    public enum BattonBoneGState
    {
        IDLE = 0,
        ATTACKING = 1,
        ESCAPING = 2
    }

    public class BattonBoneG : Enemy
    {
        private bool flashing;

        public BattonBoneGState State
        {
            get => GetState<BattonBoneGState>();
            set
            {
                CheckCollisionWithWorld = value == BattonBoneGState.ESCAPING;
                SetState(value);
            }
        }

        public BattonBoneG()
        {
            SpriteSheetName = "BattonBoneG";

            SetAnimationNames("Idle", "Attacking");

            SetupStateArray(typeof(BattonBoneGState));
            RegisterState(BattonBoneGState.IDLE, OnIdle, "Idle");
            RegisterState(BattonBoneGState.ATTACKING, OnAttacking, "Attacking");
            RegisterState(BattonBoneGState.ESCAPING, OnEscaping, "Attacking");
        }

        public override FixedSingle GetGravity()
        {
            return 0;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;

            flashing = false;

            PaletteName = "battonBoneGPalette";
            Health = BATTON_BONE_G_HEALTH;
            ContactDamage = BATTON_BONE_G_CONTACT_DAMAGE;

            NothingDropOdd = 79;
            SmallHealthDropOdd = 5;
            BigHealthDropOdd = 5;
            SmallAmmoDropOdd = 5;
            BigAmmoDropOdd = 5;
            LifeUpDropOdd = 1;

            State = BattonBoneGState.IDLE;
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            flashing = true;
            PaletteName = "flashingPalette";

            return base.OnTakeDamage(attacker, ref damage);
        }

        protected override void OnHurt(Sprite victim, FixedSingle damage)
        {
            Velocity = BATTON_BONE_G_ESCAPE_SPEED * Vector.UP_VECTOR;
            State = BattonBoneGState.ESCAPING;
        }

        protected override void OnStopMoving()
        {
            base.OnStopMoving();

            if (State == BattonBoneGState.ESCAPING)
            {
                Velocity = Vector.NULL_VECTOR;
                State = BattonBoneGState.IDLE;
            }
        }

        private void OnIdle(EntityState state, long frameCounter)
        {
            if (Engine.Player != null && frameCounter >= 60 && Origin.DistanceTo(Engine.Player.Origin) <= SCENE_SIZE * 0.5)
                State = BattonBoneGState.ATTACKING;
            else
                Velocity = Vector.NULL_VECTOR;
        }

        private void OnAttacking(EntityState state, long frameCounter)
        {
            if (Engine.Player != null)
            {
                Vector delta = Engine.Player.Origin - Origin;
                Velocity = BATTON_BONE_G_ATTACK_SPEED * delta.Versor();
            }
            else
                State = BattonBoneGState.ESCAPING;
        }

        private void OnEscaping(EntityState state, long frameCounter)
        {
            if (BlockedUp)
            {
                Velocity = Vector.NULL_VECTOR;
                State = BattonBoneGState.IDLE;
            }
            else
                Velocity = BATTON_BONE_G_ESCAPE_SPEED * Vector.UP_VECTOR;
        }

        protected override bool PreThink()
        {
            if (flashing)
            {
                flashing = false;
                PaletteName = "battonBoneGPalette";
            }

            return base.PreThink();
        }
    }
}