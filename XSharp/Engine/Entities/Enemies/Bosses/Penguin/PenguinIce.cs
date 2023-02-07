using XSharp.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinIce : Enemy
    {
        public Penguin Shooter
        {
            get;
            internal set;
        }

        public PenguinIce()
        {
            Directional = true;
            SpriteSheetIndex = 10;
            ContactDamage = 2;

            SetAnimationNames("Ice");
        }

        protected override Box GetHitbox()
        {
            return PENGUIN_ICE_HITBOX;
        }

        public void Explode()
        {
            // TODO : Implement
            Kill();
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Direction = Shooter.Direction;
            Origin = Shooter.Origin + (Shooter.Direction == Shooter.DefaultDirection ? -PENGUIN_ICE_SHOT_ORIGIN_OFFSET.X : PENGUIN_ICE_SHOT_ORIGIN_OFFSET.X, PENGUIN_ICE_SHOT_ORIGIN_OFFSET.Y);

            SetCurrentAnimationByName("Ice");
        }

        protected override void OnBlockedLeft()
        {
            base.OnBlockedLeft();

            Explode();
        }

        protected override void OnBlockedRight()
        {
            base.OnBlockedRight();

            Explode();
        }

        protected override void OnContactDamage(Player player)
        {
            base.OnContactDamage(player);

            Explode();
        }

        protected override void Think()
        {
            base.Think();

            Velocity = PENGUIN_ICE_SPEED * (Shooter.Direction == Shooter.DefaultDirection ? Vector.LEFT_VECTOR : Vector.RIGHT_VECTOR);
        }
    }
}