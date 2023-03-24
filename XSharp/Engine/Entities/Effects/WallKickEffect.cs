using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public class WallKickEffect : SpriteEffect
{
    private static Vector GetOrigin(Player player)
    {
        return player.Direction switch
        {
            Direction.LEFT => player.Hitbox.LeftTop + (-14, 27),
            Direction.RIGHT => player.Hitbox.RightTop + (14 - 11, 27),
            _ => Vector.NULL_VECTOR,
        };
    }

    private Player player;

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

    public WallKickEffect()
    {
        SpriteSheetName = "X Effects";

        SetAnimationNames("WallKickEffect");
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}