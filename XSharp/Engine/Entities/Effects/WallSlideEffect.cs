using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public class WallSlideEffect : SpriteEffect
{
    private static Vector GetOrigin(Player player)
    {
        return player.Direction switch
        {
            Direction.LEFT => player.Hitbox.LeftTop + (-5, 25),
            Direction.RIGHT => player.Hitbox.RightTop + (5, 25),
            _ => Vector.NULL_VECTOR,
        };
    }

    private EntityReference<Player> player;

    public Player Player
    {
        get => player;
        set
        {
            player = value;
            if (value != null)
                Origin = GetOrigin(value);
        }
    }

    public WallSlideEffect()
    {
        SpriteSheetName = "X Effects";

        SetAnimationNames("WallSlideEffect");
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}