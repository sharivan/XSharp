using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Tombot;

public class TombotLoad : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Tombot>();
    }
    #endregion

    public TombotLoad()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        DefaultDirection = Direction.LEFT;

        PaletteName = "tombotPalette";
        SpriteSheetName = "Tombot";

        SetAnimationNames("Load");
        InitialAnimationName = "Load";
    }

    protected override Box GetHitbox()
    {
        return Tombot.LOAD_HITBOX;
    }

    protected override void OnStopMoving()
    {
        base.OnStopMoving();

        Break();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        HasGravity = true;
        KillOnOffscreen = true;
        Blinking = true;
    }
}