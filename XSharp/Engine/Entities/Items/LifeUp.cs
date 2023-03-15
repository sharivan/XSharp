using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public class LifeUp : Item
{
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Weapon));
    }

    public LifeUp()
    {
        SpriteSheetName = "X Weapons";

        SetAnimationNames("LifeUp");
    }

    protected internal override void OnSpawn()
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