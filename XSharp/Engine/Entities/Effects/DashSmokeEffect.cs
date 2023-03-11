using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public class DashSmokeEffect : SpriteEffect
{
    private static Vector GetOrigin(Player player)
    {
        return player.Direction switch
        {
            Direction.LEFT => player.Hitbox.LeftTop + (8, 22),
            Direction.RIGHT => player.Hitbox.RightTop + (-16, 22),
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

    public DashSmokeEffect()
    {
        SpriteSheetName = "X Effects";
        Directional = false;

        SetAnimationNames("DashSmokeEffect");
    }

    protected internal override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}