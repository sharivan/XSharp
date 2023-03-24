using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.HUD;

public class XDieExplosion : HUD
{
    private int frameCounter;

    public int MaxFrames
    {
        get;
        set;
    } = 68;

    public int FramesPerCicle
    {
        get;
        set;
    } = 128;

    public double MaxRadius
    {
        get;
        set;
    } = 140;

    public double Phase
    {
        get;
        set;
    } = 0;

    public int SparkCount
    {
        get;
        set;
    } = 8;

    public XDieExplosion()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "X";
        PaletteName = "x1NormalPalette";

        SetAnimationNames(("DyingExplosion", SparkCount));
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        MultiAnimation = true;
        frameCounter = 0;
    }

    protected override void OnPostThink()
    {
        base.OnPostThink();

        double radius = (double) frameCounter / MaxFrames * MaxRadius;
        for (int i = 0; i < SparkCount; i++)
        {
            Animation animation = GetAnimation(i);
            double angle = ((double) frameCounter / FramesPerCicle + (double) i / SparkCount) * 2 * System.Math.PI + Phase;
            animation.Offset = (radius * System.Math.Cos(angle), radius * System.Math.Sin(angle));
        }

        frameCounter++;

        if (frameCounter == MaxFrames)
            Kill();
    }

    protected override bool OnCreateAnimation(string frameSequenceName, ref Vector offset, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn)
    {
        switch (frameSequenceName)
        {
            case "DyingExplosion":
                startOn = true;
                startVisible = true;
                break;

            default:
                return false;
        }

        return base.OnCreateAnimation(frameSequenceName, ref offset, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn);
    }
}