using XSharp.Engine.Entities.Enemies;
using XSharp.Math;

namespace XSharp.Engine.Entities.Weapons;

public abstract class Weapon : Sprite
{
    private EntityReference<Sprite> shooter;

    public Sprite Shooter
    {
        get => shooter;
        protected set => shooter = Engine.Entities.GetReferenceTo(value);
    }

    public FixedSingle BaseDamage => GetBaseDamage();

    public FixedSingle Damage
    {
        get;
        set;
    }

    protected Weapon()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        CanGoOutOfMapBounds = true;
    }

    protected virtual FixedSingle GetBaseDamage()
    {
        return 1;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = true;
        Damage = GetBaseDamage();
    }

    protected override void OnHurt(Sprite victim, FixedSingle damage)
    {
        base.OnHurt(victim, damage);

        if (victim is Enemy enemy)
            OnHit(enemy, damage);
    }

    protected internal virtual void OnHit(Enemy enemy, FixedSingle damage)
    {
        Engine.PlaySound(1, "Small Hit");
    }

    public void Hit(Enemy enemy)
    {
        if (Damage > 0)
            Hurt(enemy, Damage);
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (Alive && !MarkedToRemove && entity is Enemy enemy)
            Hit(enemy);
    }

    public virtual void Reflect()
    {
    }
}