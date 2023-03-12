﻿using XSharp.Engine.Graphics;
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

    private AnimationReference topAnimation;
    private AnimationReference middleAnimation;
    private AnimationReference middleEmptyAnimation;
    private AnimationReference bottomAnimation;
    private AnimationReference[] hudImageAnimation;

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
                    Animation animation = hudImageAnimation[(int) image];
                    animation.Visible = false;
                }

                if (value != HUDImage.NONE)
                {
                    Animation animation = hudImageAnimation[(int) value];
                    animation.Visible = true;
                }
            }

            image = value;
        }
    }

    protected Animation TopAnimation => topAnimation;

    protected Animation MiddleAnimation => middleAnimation;

    protected Animation MiddleEmptyAnimation => middleEmptyAnimation;

    protected Animation BottomAnimation => bottomAnimation;

    protected Animation ImageAnimation => Image != HUDImage.NONE ? hudImageAnimation[(int) Image] : null;

    protected HealthHUD()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "HP";

        hudImageAnimation = new AnimationReference[8];
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        MultiAnimation = true;

        topAnimation = GetAnimationByName("HPTop");
        middleAnimation = GetAnimationByName("HPMiddle");
        middleEmptyAnimation = GetAnimationByName("HPMiddleEmpty");
        bottomAnimation = GetAnimationByName("HPBottom");

        hudImageAnimation[(int) HUDImage.RIDE_ARMOR] = GetAnimationByName("RideArmor");
        hudImageAnimation[(int) HUDImage.ZERO] = GetAnimationByName("Zero");
        hudImageAnimation[(int) HUDImage.X1_BOSS] = GetAnimationByName("X1Boss");
        hudImageAnimation[(int) HUDImage.BOSS] = GetAnimationByName("Boss");
        hudImageAnimation[(int) HUDImage.DOPPLER] = GetAnimationByName("Doppler");
        hudImageAnimation[(int) HUDImage.W] = GetAnimationByName("W");
        hudImageAnimation[(int) HUDImage.DOPPLER_PROTOTYPE] = GetAnimationByName("DopplerPrototype");
        hudImageAnimation[(int) HUDImage.X] = GetAnimationByName("X");

        if (Image != HUDImage.NONE)
        {
            Animation animation = hudImageAnimation[(int) Image];
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

    protected override bool OnCreateAnimation(string animationName, ref Vector offset, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn)
    {
        switch (animationName)
        {
            case "HPTop":
                startOn = true;
                startVisible = true;
                break;

            case "HPMiddle":
                startOn = true;
                startVisible = true;
                break;

            case "HPMiddleEmpty":
                startOn = true;
                startVisible = true;
                offset = (0, 4);
                break;

            case "HPBottom":
                startOn = true;
                startVisible = true;
                break;

            case "RideArmor":
                startOn = true;
                startVisible = Image == HUDImage.RIDE_ARMOR;
                break;

            case "Zero":
                startOn = true;
                startVisible = Image == HUDImage.ZERO;
                break;

            case "X1Boss":
                startOn = true;
                startVisible = Image == HUDImage.X1_BOSS;
                break;

            case "Boss":
                startOn = true;
                startVisible = Image == HUDImage.BOSS;
                break;

            case "Doppler":
                startOn = true;
                startVisible = Image == HUDImage.DOPPLER;
                break;

            case "W":
                startOn = true;
                startVisible = Image == HUDImage.W;
                break;

            case "DopplerPrototype":
                startOn = true;
                startVisible = Image == HUDImage.DOPPLER_PROTOTYPE;
                break;

            case "X":
                startOn = true;
                startVisible = Image == HUDImage.X;
                break;

            default:
                return false;
        }

        return base.OnCreateAnimation(animationName, ref offset, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn);
    }
}