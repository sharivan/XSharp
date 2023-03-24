using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public class PenguinIceFragment : Sprite
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }
    #endregion

    public PenguinIceFragment()
    {
        SpriteSheetName = "Penguin";
        PaletteName = "penguinPalette";
        KillOnOffscreen = true;

        SetAnimationNames("IceFragment");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
    }
}