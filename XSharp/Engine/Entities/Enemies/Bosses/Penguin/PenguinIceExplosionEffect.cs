using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public class PenguinIceExplosionEffect : SpriteEffect
{
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }

    public Vector InitialVelocity
    {
        get;
        internal set;
    }

    public PenguinIceExplosionEffect()
    {
        Directional = false;
        SpriteSheetName = "Penguin";

        SetAnimationNames("IceFragment");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithSolidSprites = false;
        CheckCollisionWithWorld = false;
        HasGravity = true;
        Velocity = InitialVelocity;
        KillOnOffscreen = true;
    }
}