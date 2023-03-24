using System.Reflection;

using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Effects;

public enum ExplosionEffectSound
{
    NONE = 0,
    ENEMY_DIE_1 = 1,
    ENEMY_DIE_2 = 2,
    ENEMY_DIE_3 = 3,
    ENEMY_DIE_4 = 4
}

internal class ExplosionEffect : SpriteEffect
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        var explosionSpriteSheet = Engine.CreateSpriteSheet("Explosion", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Effects.Explosion.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            explosionSpriteSheet.CurrentTexture = texture;
        }

        var sequence = explosionSpriteSheet.AddFrameSquence("Explosion");
        sequence.AddFrame(0, 0, 38, 48, 1, false, OriginPosition.CENTER);
        sequence.AddFrame(38, 0, 38, 48, 2, false, OriginPosition.CENTER);
        sequence.AddFrame(0, 0, 38, 48, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(76, 0, 38, 48, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(114, 0, 38, 48, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(0, 48, 38, 48, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(38, 48, 38, 48, 3, false, OriginPosition.CENTER);
        sequence.AddFrame(76, 48, 38, 48, 2, false, OriginPosition.CENTER);
        sequence.AddFrame(114, 48, 38, 48, 2, false, OriginPosition.CENTER);

        explosionSpriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    public ExplosionEffectSound EffectSound
    {
        get;
        set;
    } = ExplosionEffectSound.ENEMY_DIE_1;

    public int SoundChannel
    {
        get;
        set;
    } = 2;

    public ExplosionEffect()
    {
        SpriteSheetName = "Explosion";

        SetAnimationNames("Explosion");
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Layer = 1;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        switch (EffectSound)
        {
            case ExplosionEffectSound.ENEMY_DIE_1:
                Engine.PlaySound(SoundChannel, "Enemy Die (1)");
                break;

            case ExplosionEffectSound.ENEMY_DIE_2:
                Engine.PlaySound(SoundChannel, "Enemy Die (2)");
                break;

            case ExplosionEffectSound.ENEMY_DIE_3:
                Engine.PlaySound(SoundChannel, "Enemy Die (3)");
                break;

            case ExplosionEffectSound.ENEMY_DIE_4:
                Engine.PlaySound(SoundChannel, "Enemy Die (4)");
                break;
        }
    }

    protected override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);
        KillOnNextFrame();
    }
}