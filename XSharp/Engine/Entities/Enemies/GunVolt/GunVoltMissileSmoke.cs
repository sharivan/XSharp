using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.GunVolt;

public class GunVoltMissileSmoke : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<GunVolt>();
    }
    #endregion

    public GunVoltMissileSmoke()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;

        PaletteName = "GunVoltPalette";
        SpriteSheetName = "GunVolt";
        Layer = 1;

        SetAnimationNames("MissileSmoke");
        InitialAnimationName = "MissileSmoke";
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = true;
        HasGravity = false;
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}