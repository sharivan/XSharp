using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public class LifeUp : Item
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }
    #endregion

    public LifeUp()
    {
        SpriteSheetName = "X Weapons";

        SetAnimationNames("LifeUp");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CurrentAnimationIndex = 0;
        CurrentAnimation.StartFromBegin();
    }

    protected override void OnCollecting(Player player)
    {
        if (player.Lives <= MAX_LIVES)
        {
            player.Lives++;
            Engine.PlaySound(0, "X Extra Life", true);
        }
    }
}