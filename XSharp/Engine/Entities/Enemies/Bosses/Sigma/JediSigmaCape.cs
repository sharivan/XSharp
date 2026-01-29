using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public class JediSigmaCape : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<JediSigma>();
    }
    #endregion

    internal EntityReference<JediSigma> sigma;

    public JediSigma Sigma => sigma;

    public JediSigmaCape()
    {
    }

    protected override Box GetHitbox()
    {
        return JediSigma.CAPE_HITBOX;
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames("Cape");
        InitialAnimationName = "Cape";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Origin = (Sigma.Origin.X + -Sigma.Direction.GetHorizontalSignal() * JediSigma.CAPE_OFFSET_X, Sigma.Origin.Y + JediSigma.CAPE_OFFSET_Y);
        Velocity = (-Sigma.Direction.GetHorizontalSignal() * JediSigma.CAPE_SPEED_X, JediSigma.CAPE_SPEED_Y);
        Blinking = true;
    }
}