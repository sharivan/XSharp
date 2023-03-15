using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

public class PenguinIceFragment : Sprite
{
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }

    public PenguinIceFragment()
    {
        Directional = false;
        SpriteSheetName = "Penguin";
        PaletteName = "penguinPalette";
        KillOnOffscreen = true;

        SetAnimationNames("IceFragment");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
    }
}