using SharpDX.Direct3D9;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XSharp.Graphics;
using XSharp.Serialization;

using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;
using Color = XSharp.Graphics.Color;
using DataRectangle = XSharp.Graphics.DataRectangle;
using DataStream = XSharp.Graphics.DataStream;

namespace XSharp.Engine.Graphics;

public class DX9Palette : Palette
{
    public new DX9Texture Texture
    {
        get => (DX9Texture) base.Texture;
        internal set => base.Texture = value;
    }

    public new int Index
    {
        get => base.Index;
        internal set => base.Index = value;
    }

    public new int Count
    {
        get => base.Count;
        internal set => base.Count = value;
    }

    internal DX9Palette()
    {
    }

    internal DX9Palette(ISerializer serializer)
        : base(serializer)
    {
    }

    internal DX9Palette(DX9Texture texture, int index, string name, int count)
    {
        Texture = texture;
        Index = index;
        this.name = name;
        Count = count;
    }

    protected override void SerializeContent(ISerializer serializer)
    {
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.Width;
            int height = Texture.Height;

            using var stream = new DX9DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
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

    public override int LookupColor(Color color, int start, int count)
    {
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.Width;
            int height = Texture.Height;

            using var stream = new DX9DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
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

    public override Color GetColor(int index)
    {
        DataRectangle rect = Texture.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = Texture.Width;
            int height = Texture.Height;

            using var stream = new DX9DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
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

    public override void SetColor(ITexture palette, int index, Color color)
    {
        DX9Texture dxPallete = (DX9Texture) palette;
        DataRectangle rect = dxPallete.LockRectangle(0, D3D9LockFlags.Discard);
        try
        {
            int width = dxPallete.Width;
            int height = dxPallete.Height;

            using var stream = new DX9DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
            stream.Position = index * sizeof(int);
            stream.Write(color.ToBgra());

            if (index >= Count)
                Count = index + 1;
        }
        finally
        {
            dxPallete.UnlockRectangle(0);
        }
    }
}