using SharpDX;
using SharpDX.Direct2D;
using SharpDX.Direct2D1;
using SharpDX.HLSL;
using SharpDX.DirectWrite;

using XSharp.Interop;
using XSharp.Math.Geometry;

using RectangleF = XSharp.Math.Geometry.RectangleF;

using DXWriteFactory = SharpDX.DirectWrite.Factory;
using DX2D1Device = SharpDX.Direct2D1.Device;
using DXWriteFontWeight = SharpDX.DirectWrite.FontWeight;
using DXWriteFontStyle = SharpDX.DirectWrite.FontStyle;
using DX11ColorBGRA = SharpDX.ColorBGRA;
using DX11Rectangle = SharpDX.Rectangle;
using DX11RectangleF = SharpDX.RectangleF;
using SharpDX.DXGI;
using System.Drawing;

namespace XSharp.Graphics;

public class DX11Font(DXWriteDevice device, string fontFamilyName, DXWriteFontWeight fontWeight, DXWriteFontStyle fontStyle, float fontSize) : IFont
{
    private DX2D1Device device;
    private TextFormat format = new TextFormat(device.fac, fontWeight, fontStyle, fontSize);

    public int DrawText(string text, RectangleF rect, FontDrawFlags drawFlags, Color color)
    {
        return font.DrawText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX11FontDrawFlags(), new DX11ColorBGRA(color.ToBgra()));

        using (var renderTarget = device.ImmediateContext.Target)
        {
            device.ImmediateContext.BeginDraw();
            renderTarget.Transform = Matrix3x2.Identity;
            textLayout.Draw(renderTarget, new PointF(100, 100));
            device.ImmediateContext.EndDraw();

            // Apresentação da saída
            SwapChain.Present(SwapChainFlags.None);
        }
    }

    public RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags)
    {
        return ((DX9Rectangle) font.MeasureText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX11FontDrawFlags())).ToRectangleF();
    }

    public RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags, out int textHeight)
    {
        return ((DX9Rectangle) font.MeasureText(Sprite, text, rect.ToDX9RectangleF(), drawFlags.ToDX11FontDrawFlags(), out textHeight)).ToRectangleF();
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