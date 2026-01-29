using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.HUD;

public abstract class HUD : Sprite
{
    public Vector Offset
    {
        get;
        set;
    }

    protected HUD()
    {
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckTouchingEntities = false;
        CheckCollisionWithWorld = false;
        Static = true;
    }

    protected override void OnPostThink()
    {
        base.OnPostThink();

        Origin = Engine.Camera.LeftTop + Offset;
    }
}