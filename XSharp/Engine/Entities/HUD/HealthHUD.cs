using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD;

public enum HUDImage
{
    NONE = -1,
    RIDE_ARMOR = 0,
    ZERO = 1,
    X1_BOSS = 2,
    BOSS = 3,
    DOPPLER = 4,
    W = 5,
    DOPPLER_PROTOTYPE = 6,
    X = 7
}

public class HealthHUD : HUD
{
    private static FixedSingle GetHeight(FixedSingle capacity)
    {
        return 4 + 2 * capacity + 16;
    }

    private static FixedSingle GetTop(FixedSingle capacity)
    {
        return HP_BOTTOM - GetHeight(capacity);
    }

    private HUDImage image = HUDImage.NONE;

    private int topAnimationIndex;
    private int middleAnimationIndex;
    private int middleEmptyAnimationIndex;
    private int bottomAnimationIndex;
    private int[] hudImageAnimationIndex;

    public FixedSingle Left
    {
        get;
        protected set;
    }

    public FixedSingle Capacity
    {
        get;
        protected set;
    }

    public FixedSingle Value
    {
        get;
        protected set;
    }

    public HUDImage Image
    {
        get => image;

        protected set
        {
            if (ResourcesCreated && value != image)
            {
                if (image != HUDImage.NONE)
                {
                    Animation animation = GetAnimation(hudImageAnimationIndex[(int) image]);
                    animation.Visible = false;
                }

                if (value != HUDImage.NONE)
                {
                    Animation animation = GetAnimation(hudImageAnimationIndex[(int) value]);
                    animation.Visible = true;
                }
            }

            image = value;
        }
    }

    protected Animation TopAnimation => GetAnimation(topAnimationIndex);

    protected Animation MiddleAnimation => GetAnimation(middleAnimationIndex);

    protected Animation MiddleEmptyAnimation => GetAnimation(middleEmptyAnimationIndex);

    protected Animation BottomAnimation => GetAnimation(bottomAnimationIndex);

    protected Animation ImageAnimation => Image != HUDImage.NONE ? GetAnimation(hudImageAnimationIndex[(int) Image]) : null;

    protected HealthHUD()
    {
        SpriteSheetName = "HP";

        hudImageAnimationIndex = new int[8];
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        MultiAnimation = true;

        if (Image != HUDImage.NONE)
        {
            Animation animation = GetAnimation(hudImageAnimationIndex[(int) Image]);
            animation.Visible = true;
        }
    }

    protected internal override void PostThink()
    {
        Offset = (Left, GetTop(Capacity));

        base.PostThink();

        var imageAnimation = ImageAnimation;
        if (imageAnimation != null)
            imageAnimation.Offset = (1, 2 * Capacity + 6);

        BottomAnimation.Offset = (0, 2 * Capacity + 4);
        MiddleEmptyAnimation.RepeatY = (int) (Capacity - Value);
        MiddleAnimation.Offset = (0, 2 * (Capacity - Value) + 4);
        MiddleAnimation.RepeatY = (int) Value;
    }

    protected override void OnCreateAnimation(int animationIndex, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
    {
        base.OnCreateAnimation(animationIndex, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

        switch (frameSequenceName)
        {
            case "HPTop":
                startOn = true;
                startVisible = true;
                topAnimationIndex = animationIndex;
                break;

            case "HPMiddle":
                startOn = true;
                startVisible = true;
                middleAnimationIndex = animationIndex;
                break;

            case "HPMiddleEmpty":
                startOn = true;
                startVisible = true;
                middleEmptyAnimationIndex = animationIndex;
                offset = (0, 4);
                break;

            case "HPBottom":
                startOn = true;
                startVisible = true;
                bottomAnimationIndex = animationIndex;
                break;

            case "RideArmor":
                startOn = true;
                startVisible = Image == HUDImage.RIDE_ARMOR;
                hudImageAnimationIndex[(int) HUDImage.RIDE_ARMOR] = animationIndex;
                break;

            case "Zero":
                startOn = true;
                startVisible = Image == HUDImage.ZERO;
                hudImageAnimationIndex[(int) HUDImage.ZERO] = animationIndex;
                break;

            case "X1Boss":
                startOn = true;
                startVisible = Image == HUDImage.X1_BOSS;
                hudImageAnimationIndex[(int) HUDImage.X1_BOSS] = animationIndex;
                break;

            case "Boss":
                startOn = true;
                startVisible = Image == HUDImage.BOSS;
                hudImageAnimationIndex[(int) HUDImage.BOSS] = animationIndex;
                break;

            case "Doppler":
                startOn = true;
                startVisible = Image == HUDImage.DOPPLER;
                hudImageAnimationIndex[(int) HUDImage.DOPPLER] = animationIndex;
                break;

            case "W":
                startOn = true;
                startVisible = Image == HUDImage.W;
                hudImageAnimationIndex[(int) HUDImage.W] = animationIndex;
                break;

            case "DopplerPrototype":
                startOn = true;
                startVisible = Image == HUDImage.DOPPLER_PROTOTYPE;
                hudImageAnimationIndex[(int) HUDImage.DOPPLER_PROTOTYPE] = animationIndex;
                break;

            case "X":
                startOn = true;
                startVisible = Image == HUDImage.X;
                hudImageAnimationIndex[(int) HUDImage.X] = animationIndex;
                break;

            default:
                add = false;
                break;
        }
    }
}