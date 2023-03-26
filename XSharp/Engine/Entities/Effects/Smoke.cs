using SharpDX;

using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Effects;

public class Smoke : SpriteEffect
{
    #region StaticFields
    public static readonly Color[] PALETTE = new Color[]
    {
        Color.Transparent,          // 0
        Color.FromBgra(0xFFF8F8F8), // 1
        Color.FromBgra(0xFFF8E070), // 2
        Color.FromBgra(0xFFF0B038), // 3
        Color.FromBgra(0xFFF09060), // 4
        Color.FromBgra(0xFFF87038), // 5           
        Color.FromBgra(0xFFF81810), // 6
        Color.FromBgra(0xFFA0A0A0), // 7
        Color.FromBgra(0xFF28D0F8), // 8
        Color.FromBgra(0xFF2878C0), // 9
        Color.FromBgra(0xFF1840B0), // A
        Color.FromBgra(0xFFF8C000), // B
        Color.FromBgra(0xFFD04008), // C
        Color.FromBgra(0xFFC0C0C0), // D
        Color.FromBgra(0xFF888888), // E
        Color.FromBgra(0xFF181818)  // F
    };
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var palette = Engine.PrecachePalette("SmokePalette", PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("Smoke", true, true);

        spriteSheet.CurrentTexture = Engine.CreateImageTextureFromEmbeddedResource("Sprites.Effects.Smoke.png");
        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Smoke");
        sequence.AddFrame(0, 0, 14, 14, 5);
        sequence.AddFrame(14, 0, 14, 14, 5);
        sequence.AddFrame(28, 0, 14, 14, 5);
        sequence.AddFrame(42, 0, 14, 14, 3);
        sequence.AddFrame(56, 0, 14, 14, 3);
        sequence.AddFrame(70, 0, 14, 14, 3);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public Smoke()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "SmokePalette";
        SpriteSheetName = "Smoke";

        SetAnimationNames("Smoke");
        InitialAnimationName = "Smoke";
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = true;
        HasGravity = false;
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}