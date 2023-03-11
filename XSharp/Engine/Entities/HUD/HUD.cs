using XSharp.Math.Geometry;

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

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Directional = false;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckTouchingEntities = false;
        CheckCollisionWithWorld = false;
        Static = true;
    }

    protected internal override void PostThink()
    {
        base.PostThink();

        Origin = Engine.Camera.LeftTop + Offset;
    }

    protected internal override void UpdatePartition(bool force = false)
    {
    }
}