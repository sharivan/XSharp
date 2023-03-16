using System.Reflection;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

internal class Mist : HUD.HUD
{
    [Precache]
    internal static void Precache()
    {
        Engine.PrecacheSound("Enemy Sound (05)", @"resources\sounds\mmx\68 - MMX - Enemy Sound (05).wav");

        var mistSpriteSheet = Engine.CreateSpriteSheet("Mist", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Effects.Mist.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            mistSpriteSheet.CurrentTexture = texture;
        }

        var sequence = mistSpriteSheet.AddFrameSquence("Mist");
        sequence.OriginOffset = Vector.NULL_VECTOR;
        sequence.Hitbox = (Vector.NULL_VECTOR, Vector.NULL_VECTOR, (SCENE_SIZE, SCENE_SIZE));
        sequence.AddFrame(0, 0, 0, 0, 256, 256, 1, true);

        mistSpriteSheet.ReleaseCurrentTexture();
    }

    public FixedSingle Speed
    {
        get;
        set;
    } = 2;

    public Direction MistDirection
    {
        get;
        set;
    } = Direction.RIGHT;

    public bool StartPlaying
    {
        get;
        set;
    } = false;

    public bool Playing => Visible;

    public Mist()
    {
        SpriteSheetName = "Mist";
        Directional = false;

        SetAnimationNames("Mist");
    }

    protected override Box GetBoundingBox()
    {
        return (0, 0, 2 * SCENE_SIZE, 2 * SCENE_SIZE);
    }

    private void ResetOffset()
    {
        Offset = (MistDirection == Direction.RIGHT ? -SCENE_SIZE : 0, -SCENE_SIZE);
    }

    private void PlaySnowSoundLoop()
    {
        Engine.PlaySound(5, "Enemy Sound (05)", 1.2, 0.128);
    }

    private void FinishSnowSoundLoop()
    {
        Engine.ClearSoundLoopPoint(5, "Enemy Sound (05)", true);
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        Visible = StartPlaying;

        ResetOffset();
    }

    protected internal override void PostThink()
    {
        if (!Visible)
            return;

        Offset += (MistDirection == Direction.RIGHT ? Speed : -Speed, Speed);
        if (Offset.Y >= 0)
            ResetOffset();

        base.PostThink();
    }

    protected override bool OnCreateAnimation(string frameSequenceName, ref Vector offset, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn)
    {
        switch (frameSequenceName)
        {
            case "Mist":
                startOn = true;
                startVisible = true;
                repeatX = 2;
                repeatY = 2;
                break;

            default:
                return false;
        }

        return base.OnCreateAnimation(frameSequenceName, ref offset, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn);
    }

    public void Play()
    {
        if (!Visible)
        {
            Visible = true;
            PlaySnowSoundLoop();
            ResetOffset();
        }
    }

    public void Stop()
    {
        if (Visible)
        {
            Visible = false;
            FinishSnowSoundLoop();
        }
    }
}