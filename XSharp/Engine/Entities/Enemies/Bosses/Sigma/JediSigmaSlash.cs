using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public class JediSigmaSlash : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<JediSigma>();
    }
    #endregion

    public JediSigmaSlash()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        ContactDamage = JediSigma.SHOT_DAMAGE;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames("Slash");
        InitialAnimationName = "Slash";
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