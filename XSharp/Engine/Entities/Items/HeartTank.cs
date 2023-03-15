using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Items;

public class HeartTank : Item
{
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }

    public HeartTank()
    {
        DurationFrames = 0;
        SpriteSheetName = "X Weapons";

        SetAnimationNames("HeartTank");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CurrentAnimationIndex = 0;
        CurrentAnimation.StartFromBegin();
    }

    protected override void OnCollecting(Player player)
    {
        Engine.StartHeartTankAcquiring();
    }
}