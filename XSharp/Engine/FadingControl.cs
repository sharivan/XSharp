using System;
using XSharp.Graphics;
using XSharp.Math.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine;

[Flags]
public enum FadingFlags
{
    NONE = 0,
    ALPHA = 1,
    RED = 2,
    GREEN = 4,
    BLUE = 8,

    ALL = ALPHA | RED | GREEN | BLUE,
    COLORS = RED | GREEN | BLUE
}

public class FadingControl : ISerializable
{
    public bool Fading
    {
        get;
        private set;
    }

    public FadingFlags Flags
    {
        get;
        set;
    } = FadingFlags.COLORS;

    public bool Paused
    {
        get;
        set;
    }

    public Vector4 FadingLevel
    {
        get;
        set;
    } = Vector4.Zero;

    public Color FadingColor
    {
        get;
        set;
    } = Color.Black;

    public FadingFlags FadeIn
    {
        get;
        set;
    } = FadingFlags.COLORS;

    public long FadingFrames
    {
        get;
        set;
    }

    public long FadingTick
    {
        get;
        set;
    }

    public Action OnFadingComplete
    {
        get;
        set;
    }

    internal void DoFrame()
    {
        OnFrame();
    }

    protected virtual void OnFrame()
    {
        if (Fading && !Paused)
        {
            FadingTick++;
            if (FadingTick > FadingFrames)
            {
                FadingLevel = new Vector4(
                        Flags.HasFlag(FadingFlags.RED) ? (FadeIn.HasFlag(FadingFlags.RED) ? 0 : 1) : FadingLevel.X,
                        Flags.HasFlag(FadingFlags.GREEN) ? (FadeIn.HasFlag(FadingFlags.GREEN) ? 0 : 1) : FadingLevel.Y,
                        Flags.HasFlag(FadingFlags.BLUE) ? (FadeIn.HasFlag(FadingFlags.BLUE) ? 0 : 1) : FadingLevel.Z,
                        Flags.HasFlag(FadingFlags.ALPHA) ? (FadeIn.HasFlag(FadingFlags.ALPHA) ? 0 : 1) : FadingLevel.W
                    );

                Fading = false;
                OnFadingComplete?.Invoke();
            }
            else
            {
                float level = (float) FadingTick / FadingFrames;
                FadingLevel = new Vector4(
                    Flags.HasFlag(FadingFlags.RED) ? (FadeIn.HasFlag(FadingFlags.RED) ? 1 - level : level) : FadingLevel.X,
                    Flags.HasFlag(FadingFlags.GREEN) ? (FadeIn.HasFlag(FadingFlags.GREEN) ? 1 - level : level) : FadingLevel.Y,
                    Flags.HasFlag(FadingFlags.BLUE) ? (FadeIn.HasFlag(FadingFlags.BLUE) ? 1 - level : level) : FadingLevel.Z,
                    Flags.HasFlag(FadingFlags.ALPHA) ? (FadeIn.HasFlag(FadingFlags.ALPHA) ? 1 - level : level) : FadingLevel.W
                    );
            }
        }
    }

    public void Reset()
    {
        Fading = false;
        Paused = false;
        FadingColor = Color.Transparent;
        FadingFrames = 0;
        FadingLevel = Vector4.Zero;
        Flags = FadingFlags.COLORS;
        FadeIn = FadingFlags.COLORS;
        FadingTick = 0;
        OnFadingComplete = null;
    }

    public void Start(Color color, int frames, FadingFlags flags = FadingFlags.COLORS, FadingFlags fadeIn = FadingFlags.NONE, Action onFadingComplete = null)
    {
        Fading = true;
        Paused = false;
        FadingColor = new Color(
                    flags.HasFlag(FadingFlags.RED) ? color.R : FadingColor.R,
                    flags.HasFlag(FadingFlags.GREEN) ? color.G : FadingColor.G,
                    flags.HasFlag(FadingFlags.BLUE) ? color.B : FadingColor.B,
                    flags.HasFlag(FadingFlags.ALPHA) ? color.A : FadingColor.A
                    );
        FadingFrames = frames;
        FadingLevel = new Vector4(
                    flags.HasFlag(FadingFlags.RED) ? (fadeIn.HasFlag(FadingFlags.RED) ? 1 : 0) : FadingLevel.X,
                    flags.HasFlag(FadingFlags.GREEN) ? (fadeIn.HasFlag(FadingFlags.GREEN) ? 1 : 0) : FadingLevel.Y,
                    flags.HasFlag(FadingFlags.BLUE) ? (fadeIn.HasFlag(FadingFlags.BLUE) ? 1 : 0) : FadingLevel.Z,
                    flags.HasFlag(FadingFlags.ALPHA) ? (fadeIn.HasFlag(FadingFlags.ALPHA) ? 1 : 0) : FadingLevel.W
                    );
        Flags = flags;
        FadeIn = fadeIn;
        FadingTick = 0;
        OnFadingComplete = onFadingComplete;
    }

    public void Start(Color color, int frames, Action onFadingComplete = null)
    {
        Start(color, frames, FadingFlags.COLORS, FadingFlags.NONE, onFadingComplete);
    }

    public void SeekToStart()
    {
        SeekToPosition(Vector4.Zero);
    }

    public void SeekToEnd()
    {
        SeekToPosition(Vector4.One);
    }

    public void SeekToPosition(Vector4 position)
    {
        if (Fading)
        {
            Fading = false;
            FadingLevel = new Vector4(
                    Flags.HasFlag(FadingFlags.RED) ? (FadeIn.HasFlag(FadingFlags.RED) ? 1 - position.X : position.X) : FadingLevel.X,
                    Flags.HasFlag(FadingFlags.GREEN) ? (FadeIn.HasFlag(FadingFlags.GREEN) ? 1 - position.Y : position.Y) : FadingLevel.Y,
                    Flags.HasFlag(FadingFlags.BLUE) ? (FadeIn.HasFlag(FadingFlags.BLUE) ? 1 - position.Z : position.Z) : FadingLevel.Z,
                    Flags.HasFlag(FadingFlags.ALPHA) ? (FadeIn.HasFlag(FadingFlags.ALPHA) ? 1 - position.W : position.W) : FadingLevel.W
                    );
            OnFadingComplete?.Invoke();
        }
    }

    public void Deserialize(ISerializer serializer)
    {
        Fading = serializer.ReadBool();
        Flags = serializer.ReadEnum<FadingFlags>();
        Paused = serializer.ReadBool();
        float x = serializer.ReadFloat();
        float y = serializer.ReadFloat();
        float z = serializer.ReadFloat();
        float w = serializer.ReadFloat();
        FadingLevel = new Vector4(x, y, z, w);
        FadingColor = Color.FromRgba(serializer.ReadInt());
        FadeIn = serializer.ReadEnum<FadingFlags>();
        FadingFrames = serializer.ReadLong();
        FadingTick = serializer.ReadLong();
        serializer.DeserializeProperty(nameof(OnFadingComplete), typeof(FadingControl), this);
    }

    public void Serialize(ISerializer serializer)
    {
        serializer.WriteBool(Fading);
        serializer.WriteEnum(Flags);
        serializer.WriteBool(Paused);
        serializer.WriteFloat(FadingLevel.X);
        serializer.WriteFloat(FadingLevel.Y);
        serializer.WriteFloat(FadingLevel.Z);
        serializer.WriteFloat(FadingLevel.W);
        serializer.WriteInt(FadingColor.ToRgba());
        serializer.WriteEnum(FadeIn);
        serializer.WriteLong(FadingFrames);
        serializer.WriteLong(FadingTick);
        serializer.WriteDelegate(OnFadingComplete);
    }
}