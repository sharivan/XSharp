using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Items;

public class SubTankItem : Item
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }
    #endregion

    public SubTankItem()
    {
        SpriteSheetName = "X Weapons";
        DurationFrames = 0;

        SetAnimationNames("SubTank");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CurrentAnimationIndex = 0;
        CurrentAnimation.StartFromBegin();
    }

    protected override void OnCollecting(Player player)
    {
        Engine.StartSubTankAcquiring();
    }
}
