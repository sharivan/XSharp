using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Items;

public class HeartTank : Item
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction<Weapon>();
    }
    #endregion

    public HeartTank()
    {
        DurationFrames = 0;
        SpriteSheetName = "X Weapons";

        SetAnimationNames("HeartTank");
    }

    protected override void OnSpawn()
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