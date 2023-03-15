using System.Reflection;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public abstract class SpriteEffect : Sprite
{
    [Precache]
    internal static void Precache()
    {
        var xEffectsSpriteSheet = Engine.CreateSpriteSheet("X Effects", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.X.Effects.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            xEffectsSpriteSheet.CurrentTexture = texture;
        }

        var sequence = xEffectsSpriteSheet.AddFrameSquence("WallKickEffect");
        sequence.AddFrame(0, 201, 11, 12, 2, false, OriginPosition.LEFT_TOP);

        sequence = xEffectsSpriteSheet.AddFrameSquence("PreDashSparkEffect");
        sequence.AddFrame(19, 124, 16, 32, 2, false, OriginPosition.LEFT_BOTTOM);

        sequence = xEffectsSpriteSheet.AddFrameSquence("DashSparkEffect");
        sequence.AddFrame(103, 124, 18, 32, 3, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(139, 124, 23, 32, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(178, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(219, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(260, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(301, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(346, 124, 27, 32, 1, false, OriginPosition.LEFT_BOTTOM);

        sequence = xEffectsSpriteSheet.AddFrameSquence("DashSmokeEffect");
        sequence.AddFrame(3, 164, 8, 28, 4, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(17, 164, 8, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(44, 164, 10, 28, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(58, 164, 10, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(71, 164, 13, 28, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(85, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(99, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(112, 164, 14, 28, 2, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(126, 164, 14, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(140, 164, 14, 28, 1, false, OriginPosition.LEFT_BOTTOM);
        sequence.AddFrame(154, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);

        sequence = xEffectsSpriteSheet.AddFrameSquence("WallSlideEffect");
        sequence.AddFrame(0, 228, 8, 8, 4, false, OriginPosition.CENTER);
        sequence.AddFrame(12, 227, 10, 11, 5, false, OriginPosition.CENTER);
        sequence.AddFrame(29, 226, 13, 13, 5, false, OriginPosition.CENTER);
        sequence.AddFrame(49, 226, 14, 14, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(70, 226, 14, 14, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(90, 226, 14, 14, 3, false, OriginPosition.CENTER);

        xEffectsSpriteSheet.ReleaseCurrentTexture();
    }

    public bool HasGravity
    {
        get;
        protected set;
    } = false;

    protected SpriteEffect()
    {
    }

    public override FixedSingle GetGravity()
    {
        return HasGravity ? base.GetGravity() : FixedSingle.ZERO;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        CheckTouchingEntities = false;
    }
}