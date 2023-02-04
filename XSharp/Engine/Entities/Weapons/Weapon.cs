using XSharp.Engine.Entities.Enemies;
using XSharp.Geometry;
using XSharp.Math;

namespace XSharp.Engine.Entities.Weapons
{
    public abstract class Weapon : Sprite
    {
        public Sprite Shooter
        {
            get;
        }

        public FixedSingle BaseDamage => GetBaseDamage();

        public FixedSingle Damage
        {
            get;
            set;
        }

        protected Weapon(Sprite shooter, string name, Vector origin, Direction direction, int spriteSheetIndex) : base(name, origin, spriteSheetIndex, true)
        {
            Shooter = shooter;
            Direction = direction;

            CanGoOutOfMapBounds = true;
        }

        protected virtual FixedSingle GetBaseDamage()
        {
            return 1;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Damage = GetBaseDamage();
        }

        public virtual void Hit(Enemy enemy)
        {
            Hurt(enemy, Damage);
        }

        protected override void OnStartTouch(Entity entity)
        {
            if (entity is Enemy enemy)
                Hit(enemy);

            base.OnStartTouch(entity);
        }

        protected override void Think()
        {
            base.Think();

            if (Offscreen)
                KillOnNextFrame();
        }
    }
}