﻿using System;
using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Serialization;

using Color = SharpDX.Color;
using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;

namespace XSharp.Engine.Graphics;

public class Palette : IDisposable, ISerializable
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

    public int Width => Texture.GetLevelDescription(0).Width;

    public int Height => Texture.GetLevelDescription(0).Height;

    public int Count
    {
        get;
        internal set;
    } = 0;

    public int Capacity => Width * Height;

    internal Palette()
    {
    }

    internal Palette(BinarySerializer serializer)
    {
        Deserialize(serializer);
    }

    public void Deserialize(BinarySerializer serializer)
    {
        name = serializer.ReadString();
        Index = serializer.ReadInt();

        Count = serializer.ReadInt();
        var colors = new Color[Count];
        for (int i = 0; i < Count; i++)
        {
            int bgra = serializer.ReadInt();
            var color = Color.FromBgra(bgra);
            colors[i] = color;
        }

        GameEngine.Engine.PrecachePalette(name, colors, Count);
    }

    public void Serialize(BinarySerializer serializer)
    {
        serializer.WriteString(name);
        serializer.WriteInt(Index);

        serializer.WriteInt(Count);
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.GetLevelDescription(0).Width;
            int height = Texture.GetLevelDescription(0).Height;

            using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
            using var reader = new BinaryReader(stream);
            for (int i = 0; i < Count; i++)
            {
                int bgra = reader.ReadInt32();
                serializer.WriteInt(bgra);
            }
        }
        finally
        {
            Texture.UnlockRectangle(0);
        }
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

            if (index >= Count)
                Count = index + 1;
        }
        finally
        {
            palette.UnlockRectangle(0);
        }
    }
}