using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities;

public class Checkpoint : Entity
{
    private Box hitbox;

    new public Box Hitbox
    {
        get => base.Hitbox;
        set => base.Hitbox = value;
    }

    public ushort Point
    {
        get;
        set;
    }

    public Vector CharacterPos
    {
        get;
        set;
    }

    public Vector CameraPos
    {
        get;
        set;
    }

    public Vector BackgroundPos
    {
        get;
        set;
    }

    public Vector ForceBackground
    {
        get;
        set;
    }

    public uint Scroll
    {
        get;
        set;
    }

    public Checkpoint()
    {
        Respawnable = true;
    }

    protected override Box GetHitbox()
    {
        return hitbox;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckTouchingEntities = false;
    }

    protected override void SetHitbox(Box hitbox)
    {
        this.hitbox = hitbox;
    }

    public override string ToString()
    {
        return $"Checkpoint #{Point} {Origin} {Hitbox}";
    }
}