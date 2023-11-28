using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D9;
using SharpDX;

using XSharp.Graphics;
using XSharp.Interop;

using MMXBox = XSharp.Math.Geometry.Box;
using DX9Point = SharpDX.Point;
using Color = XSharp.Graphics.Color;
using DataRectangle = XSharp.Graphics.DataRectangle;
using Format = XSharp.Graphics.Format;
using DX9Format = SharpDX.Direct3D9.Format;
using DataStream = XSharp.Graphics.DataStream;

namespace XSharp.Engine.Graphics;

public class DX9SpriteSheet : SpriteSheet
{
    public new static DX9Engine Engine => (DX9Engine) SpriteSheet.Engine;

    public new DX9Texture CurrentTexture => (DX9Texture) base.CurrentTexture;

    internal DX9SpriteSheet(bool disposeTexture = false, bool precache = false)
        : base(disposeTexture, precache)
    {
    }

    internal DX9SpriteSheet(ITexture texture, bool disposeTexture = false, bool precache = false)
        : base(texture, disposeTexture, precache)
    {
    }

    internal DX9SpriteSheet(string imageFileName, bool precache = false)
        : base(imageFileName, precache)
    {
    }

    public override Frame AddFrame(MMXBox boudingBox, MMXBox hitbox)
    {
        Frame frame;

        if (Precache)
        {
            var description = CurrentTexture.GetLevelDescription(0);
            int srcWidth = description.Width;
            int srcHeight = description.Height;
            int srcX = (int) boudingBox.Left;
            int srcY = (int) boudingBox.Top;
            int width = (int) boudingBox.Width;
            int height = (int) boudingBox.Height;
            int width1 = (int) BaseEngine.NextHighestPowerOfTwo((uint) width);
            int height1 = (int) BaseEngine.NextHighestPowerOfTwo((uint) height);

            DX9Texture texture;
            if (CurrentPalette == null)
            {
                texture = new DX9Texture(Engine.Device, width1, height1, 1, Usage.None, description.Format.ToFormat(), Pool.Default);

                Surface src = CurrentTexture.GetSurfaceLevel(0);
                Surface dst = texture.GetSurfaceLevel(0);

                Engine.Device.UpdateSurface(src, boudingBox.ToRectangleF().ToDX9RectangleF(), dst, new DX9Point(0, 0));
            }
            else
            {
                texture = new DX9Texture(Engine.Device, width1, height1, 1, Usage.None, Format.L8, Pool.Managed);

                DataRectangle srcRect = CurrentTexture.LockRectangle();
                DataRectangle dstRect = texture.LockRectangle(true);

                using (var dstDS = new DX9DataStream(dstRect.DataPointer, width1 * height1 * sizeof(byte), true, true))
                {
                    using var srcDS = new DX9DataStream(srcRect.DataPointer, srcWidth * srcHeight * sizeof(int), true, true);
                    using var reader = new BinaryReader(srcDS);
                    for (int y = srcY; y < srcY + height; y++)
                    {
                        for (int x = srcX; x < srcX + width; x++)
                        {
                            srcDS.Seek(y * srcRect.Pitch + x * sizeof(int), SeekOrigin.Begin);
                            int bgra = reader.ReadInt32();
                            var color = Color.FromBgra(bgra);
                            int index = CurrentPalette.LookupColor(color);
                            dstDS.Write((byte) (index != -1 ? index : 0));
                        }

                        for (int x = width; x < width1; x++)
                            dstDS.Write((byte) 0);
                    }

                    for (int y = height; y < height1; y++)
                    {
                        for (int x = 0; x < width1; x++)
                            dstDS.Write((byte) 0);
                    }
                }

                CurrentTexture.UnlockRectangle(0);
                texture.UnlockRectangle(0);
            }

            frame = CreateFrame(frames.Count, boudingBox - boudingBox.Origin, hitbox, texture, true);
            frames.Add(frame);
            return frame;
        }

        frame = CreateFrame(frames.Count, boudingBox - boudingBox.Origin, hitbox, CurrentTexture, false);
        frames.Add(frame);
        return frame;
    }
}