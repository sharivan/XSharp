using System;
using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using Color = SharpDX.Color;
using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;

namespace XSharp.Engine.Graphics;

public class Palette : IDisposable
{
    internal string name;

    public int Index
    {
        get;
        internal set;
    }

    public string Name
    {
        get => name;
        set => GameEngine.Engine.UpdatePaletteName(this, value);
    }

    public Texture Texture
    {
        get;
        internal set;
    }

    internal Palette()
    {
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

    public int LookupColor(Color color, int start, int count)
    {
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.GetLevelDescription(0).Width;
            int height = Texture.GetLevelDescription(0).Height;

            using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
            using var reader = new BinaryReader(stream);
            for (int i = start; i < start + count; i++)
            {
                int bgra = reader.ReadInt32();
                var c = Color.FromBgra(bgra);
                if (color == c)
                    return i;
            }
        }
        finally
        {
            Texture.UnlockRectangle(0);
        }

        return -1;
    }

    public Color GetColor(int index)
    {
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.GetLevelDescription(0).Width;
            int height = Texture.GetLevelDescription(0).Height;

            using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
            using var reader = new BinaryReader(stream);
            stream.Position = index * sizeof(int);
            int bgra = reader.ReadInt32();
            var c = Color.FromBgra(bgra);
            return c;
        }
        finally
        {
            Texture.UnlockRectangle(0);
        }
    }

    public void SetColor(Texture palette, int index, Color color)
    {
        DataRectangle rect = palette.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = palette.GetLevelDescription(0).Width;
            int height = palette.GetLevelDescription(0).Height;

            using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
            stream.Position = index * sizeof(int);
            stream.Write(color.ToBgra());
        }
        finally
        {
            palette.UnlockRectangle(0);
        }
    }
}