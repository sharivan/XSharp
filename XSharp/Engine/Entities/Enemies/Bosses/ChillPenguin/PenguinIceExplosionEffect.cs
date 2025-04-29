using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin;

public class PenguinIceExplosionEffect : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<ChillPenguin>();
    }
    #endregion

    public Vector InitialVelocity
    {
        get;
        internal set;
    }

    public PenguinIceExplosionEffect()
    {
        SpriteSheetName = "ChillPenguin";

        SetAnimationNames("IceFragment");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithSolidSprites = false;
        CheckCollisionWithWorld = false;
        HasGravity = true;
        Velocity = InitialVelocity;
        KillOnOffscreen = true;
    }
}