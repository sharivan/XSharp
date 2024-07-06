using System;

using XSharp.Graphics;
using XSharp.Serialization;

namespace XSharp.Engine.Graphics;

public abstract class Palette : IDisposable, ISerializable
{
    protected internal string name;

    public int Index
    {
        get;
        protected set;
    }

    public string Name
    {
        get => name;
        set => BaseEngine.Engine.UpdatePaletteName(this, value);
    }

    public ITexture Texture
    {
        get;
        protected set;
    }

    public int Width => Texture.Width;

    public int Height => Texture.Height;

    public int Count
    {
        get;
        protected set;
    } = 0;

    public int Capacity => Width * Height;

    protected Palette()
    {
    }

    protected Palette(ISerializer serializer)
    {
        Deserialize(serializer);
    }

    public void Deserialize(ISerializer serializer)
    {
        name = serializer.ReadString();
        Index = serializer.ReadInt();

        Count = serializer.ReadInt();
        var colors = new Color[Count];
        for (int i = 0; i < Count; i++)
        {
            int raw = serializer.ReadInt();
            var color = new Color(raw);
            colors[i] = color;
        }

        BaseEngine.Engine.PrecachePalette(name, colors, Count);
    }

    protected abstract void SerializeContent(ISerializer serializer);

    public void Serialize(ISerializer serializer)
    {
        serializer.WriteString(name);
        serializer.WriteInt(Index);

        serializer.WriteInt(Count);
        SerializeContent(serializer);
    }

    public void Dispose()
    {
        Texture?.Dispose();
        GC.SuppressFinalize(this);
    }

    public int LookupColor(Color color)
    {
        return LookupColor(color, 0, 256);
    }

    public abstract int LookupColor(Color color, int start, int count);

    public abstract Color GetColor(int index);

    public abstract void SetColor(ITexture palette, int index, Color color);
}