using XSharp.Math;

namespace XSharp.Engine.Entities.Effects;

public abstract class SpriteEffect : Sprite
{
    public bool HasGravity
    {
        get;
        protected set;
    } = false;

    protected SpriteEffect()
    {
    }

    public override FixedSingle GetGravity()
    {
        return HasGravity ? base.GetGravity() : FixedSingle.ZERO;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        CheckTouchingEntities = false;
    }
}