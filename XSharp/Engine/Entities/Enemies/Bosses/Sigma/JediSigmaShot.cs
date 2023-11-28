using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public class JediSigmaShot : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<JediSigma>();
    }
    #endregion

    internal EntityReference<JediSigma> shooter;

    public JediSigma Shooter => shooter;

    public JediSigmaShot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        ContactDamage = JediSigma.SHOT_DAMAGE;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames("Shot");
        InitialAnimationName = "Shot";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return JediSigma.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
    }
}