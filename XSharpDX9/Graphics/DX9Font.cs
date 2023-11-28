using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XSharp.Interop;
using XSharp.Math.Geometry;

using DX9ColorBGRA = SharpDX.ColorBGRA;
using DX9Rectangle = SharpDX.Rectangle;
using DX9RectangleF = SharpDX.RectangleF;

namespace XSharp.Graphics;

public class DX9Font : IFont
{
    private Font font;

    public Sprite Sprite
    {
        get;
        set;
    }

    public DX9Font(Sprite sprite, Font font)
    {
        Sprite = sprite;
        this.font = font;
    }

    public int DrawText(string text, RectangleF rect, FontDrawFlags drawFlags, Color color)
    {
        return font.DrawText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX9FontDrawFlags(), new DX9ColorBGRA(color.ToBgra()));
    }

    public RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags)
    {
        return ((DX9Rectangle) font.MeasureText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX9FontDrawFlags())).ToRectangleF();
    }

    public RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags, out int textHeight)
    {
        return ((DX9Rectangle) font.MeasureText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX9FontDrawFlags(), out textHeight)).ToRectangleF();
    }

    public void PreloadText(string stringRef)
    {
        font.PreloadText(stringRef);
    }

    public void Dispose()
    {
        font?.Dispose();
    }
}