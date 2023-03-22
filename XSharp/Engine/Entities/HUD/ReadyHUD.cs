using System.Reflection;

using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.HUD;

public class ReadyHUD : HUD
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        var readySpriteSheet = Engine.CreateSpriteSheet("Ready", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.HUD.Ready.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            readySpriteSheet.CurrentTexture = texture;
        }

        var sequence = readySpriteSheet.AddFrameSquence("Ready");
        sequence.AddFrame(5, 22, 8, 13, 1, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(21, 22, 16, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(45, 22, 16, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(68, 22, 24, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(107, 22, 24, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(139, 22, 31, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(181, 22, 30, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(220, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(267, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(314, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(367, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 10, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
        sequence.AddFrame(413, 22, 39, 13, 9, false, OriginPosition.LEFT_TOP);

        readySpriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public ReadyHUD()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Offset = READY_OFFSET;
        SpriteSheetName = "Ready";
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CurrentAnimationIndex = 0;
        CurrentAnimation.StartFromBegin();
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        Engine.SpawnPlayer();
        KillOnNextFrame();
    }
}