using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public class JediSigmaBeamSaber : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<JediSigma>();
    }
    #endregion

    public JediSigmaBeamSaber()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        ContactDamage = JediSigma.SHOT_DAMAGE;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames("BeamSaber");
        InitialAnimationName = "BeamSaber";
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