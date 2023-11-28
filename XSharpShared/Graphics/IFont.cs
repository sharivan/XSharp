﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Math.Geometry;

namespace XSharp.Graphics;

public interface IFont : IDisposable
{
    /// <summary>	
    /// Load formatted text into video memory to improve the efficiency of rendering to the device. This method supports ANSI and Unicode strings.	
    /// </summary>	
    /// <remarks>	
    /// The compiler setting also determines the function version. If Unicode is defined, the function call resolves to PreloadTextW. Otherwise, the function call resolves to PreloadTextA because ANSI strings are being used. This method generates textures that contain glyphs that represent the input text. The glyphs are drawn as a series of triangles. Text will not be rendered to the device; ID3DX10Font::DrawText must still be called to render the text. However, by preloading text into video memory, ID3DX10Font::DrawText will use substantially fewer CPU resources. This method internally converts characters to glyphs using the GDI function {{GetCharacterPlacement}}. 	
    /// </remarks>	
    /// <param name="stringRef">Pointer to a string of characters to be loaded into video memory. If the compiler settings require Unicode, the data type LPCTSTR resolves to LPCWSTR; otherwise, the data type resolves to LPCSTR. See Remarks. </param>
    /// <returns>If the method succeeds, the return value is S_OK. If the method fails, the return value can be one of the following: D3DERR_INVALIDCALL, D3DXERR_INVALIDDATA. </returns>
    public void PreloadText(string stringRef);

    /// <summary>	
    /// Draws formatted text. 
    /// </summary>
    /// <param name="text"><para>Pointer to a string to draw. If the Count parameter is -1, the string must be null-terminated.</para></param>	
    /// <param name="rect"><para>Pointer to a <see cref="RectangleF"/> structure that contains the rectangle, in logical coordinates, in which the text is to be formatted. The coordinate value of the rectangle's right side must be greater than that of its left side. Likewise, the coordinate value of the bottom must be greater than that of the top.</para></param>	
    /// <param name="drawFlags"><para>Specifies the method of formatting the text. It can be any combination of the following values:</para>  ValueMeaning <list> <item><term>DT_BOTTOM</term> </item></list>  <para>Justifies the text to the bottom of the rectangle. This value must be combined with DT_SINGLELINE.</para>  <list> <item><term>DT_CALCRECT</term></item> </list>  <para>Determines the width and height of the rectangle. If there are multiple lines of text, DrawText uses the width of the rectangle pointed to by the pRect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, DrawText modifies the right side of the rectangle so that it bounds the last character in the line. In either case, DrawText returns the height of the formatted text but does not draw the text.</para>  <list> <item><term>DT_CENTER</term></item> </list>  <para>Centers text horizontally in the rectangle.</para>  <list> <item><term>DT_EXPANDTABS</term></item> </list>  <para>Expands tab characters. The default number of characters per tab is eight.</para>  <list> <item><term>DT_LEFT</term></item> </list>  <para>Aligns text to the left.</para>  <list> <item><term>DT_NOCLIP</term></item> </list>  <para>Draws without clipping. DrawText is somewhat faster when DT_NOCLIP is used.</para>  <list> <item><term>DT_RIGHT</term></item> </list>  <para>Aligns text to the right.</para>  <list> <item><term>DT_RTLREADING</term></item> </list>  <para>Displays text in right-to-left reading order for bidirectional text when a Hebrew or Arabic font is selected. The default reading order for all text is left-to-right.</para>  <list> <item><term>DT_SINGLELINE</term></item> </list>  <para>Displays text on a single line only. Carriage returns and line feeds do not break the line.</para>  <list> <item><term>DT_TOP</term></item> </list>  <para>Top-justifies text.</para>  <list> <item><term>DT_VCENTER</term></item> </list>  <para>Centers text vertically (single line only).</para>  <list> <item><term>DT_WORDBREAK</term></item> </list>  <para>Breaks words. Lines are automatically broken between words if a word would extend past the edge of the rectangle specified by the pRect parameter. A carriage return/line feed sequence also breaks the line.</para>   <para>?</para></param>	
    /// <param name="color"><para>Color of the text. For more information, see <see cref="Color"/>.</para></param>	
    /// <returns>If the function succeeds, the return value is the height of the text in logical units. If DT_VCENTER or DT_BOTTOM is specified, the return value is the offset from pRect (top to the bottom) of the drawn text. If the function fails, the return value is zero.</returns>
    public int DrawText(string text, RectangleF rect, FontDrawFlags drawFlags, Color color);

    /// <summary>
    /// Draws formatted text.
    /// </summary>
    /// <param name="text">Pointer to a string to draw. If the Count parameter is -1, the string must be null-terminated.</param>
    /// <param name="x">The x position to draw the text.</param>
    /// <param name="y">The y position to draw the text.</param>
    /// <param name="color">Color of the text. For more information, see <see cref="Color"/>.</param>
    /// <returns>
    /// If the function succeeds, the return value is the height of the text in logical units. If DT_VCENTER or DT_BOTTOM is specified, the return value is the offset from pRect (top to the bottom) of the drawn text. If the function fails, the return value is zero.
    /// </returns>
    public int DrawText(string text, int x, int y, Color color)
    {
        return DrawText(text, new RectangleF(x, y, 0, 0), FontDrawFlags.NoClip, color);
    }

    /// <summary>
    /// Measures the specified sprite.
    /// </summary>
    /// <param name="text"><para>Pointer to a string to draw. If the Count parameter is -1, the string must be null-terminated.</para></param>	
    /// <param name="drawFlags"><para>Specifies the method of formatting the text. It can be any combination of the following values:</para>  ValueMeaning <list> <item><term>DT_BOTTOM</term></item> </list>  <para>Justifies the text to the bottom of the rectangle. This value must be combined with DT_SINGLELINE.</para>  <list> <item><term>DT_CALCRECT</term></item> </list>  <para>Determines the width and height of the rectangle. If there are multiple lines of text, DrawText uses the width of the rectangle pointed to by the pRect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, DrawText modifies the right side of the rectangle so that it bounds the last character in the line. In either case, DrawText returns the height of the formatted text but does not draw the text.</para>  <list> <item><term>DT_CENTER</term></item> </list>  <para>Centers text horizontally in the rectangle.</para>  <list> <item><term>DT_EXPANDTABS</term></item> </list>  <para>Expands tab characters. The default number of characters per tab is eight.</para>  <list> <item><term>DT_LEFT</term></item> </list>  <para>Aligns text to the left.</para>  <list> <item><term>DT_NOCLIP</term></item> </list>  <para>Draws without clipping. DrawText is somewhat faster when DT_NOCLIP is used.</para>  <list> <item><term>DT_RIGHT</term></item> </list>  <para>Aligns text to the right.</para>  <list> <item><term>DT_RTLREADING</term></item> </list>  <para>Displays text in right-to-left reading order for bidirectional text when a Hebrew or Arabic font is selected. The default reading order for all text is left-to-right.</para>  <list> <item><term>DT_SINGLELINE</term></item> </list>  <para>Displays text on a single line only. Carriage returns and line feeds do not break the line.</para>  <list> <item><term>DT_TOP</term></item> </list>  <para>Top-justifies text.</para>  <list> <item><term>DT_VCENTER</term></item> </list>  <para>Centers text vertically (single line only).</para>  <list> <item><term>DT_WORDBREAK</term></item> </list>  <para>Breaks words. Lines are automatically broken between words if a word would extend past the edge of the rectangle specified by the pRect parameter. A carriage return/line feed sequence also breaks the line.</para>   <para>?</para></param>	
    /// <returns>Determines the width and height of the rectangle. If there are multiple lines of text, this function uses the width of the rectangle pointed to by the rect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, this method modifies the right side of the rectangle so that it bounds the last character in the line. </returns>
    public unsafe RectangleF MeasureText(string text, FontDrawFlags drawFlags)
    {
        return MeasureText(text, new RectangleF(), drawFlags);
    }

    /// <summary>
    /// Measures the specified sprite.
    /// </summary>
    /// <param name="text"><para>Pointer to a string to draw. If the Count parameter is -1, the string must be null-terminated.</para></param>	
    /// <param name="rect"><para>Pointer to a <see cref="RectangleF"/> structure that contains the rectangle, in logical coordinates, in which the text is to be formatted. The coordinate value of the rectangle's right side must be greater than that of its left side. Likewise, the coordinate value of the bottom must be greater than that of the top.</para></param>	
    /// <param name="drawFlags"><para>Specifies the method of formatting the text. It can be any combination of the following values:</para>  ValueMeaning <list> <item><term>DT_BOTTOM</term></item> </list>  <para>Justifies the text to the bottom of the rectangle. This value must be combined with DT_SINGLELINE.</para>  <list> <item><term>DT_CALCRECT</term></item> </list>  <para>Determines the width and height of the rectangle. If there are multiple lines of text, DrawText uses the width of the rectangle pointed to by the pRect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, DrawText modifies the right side of the rectangle so that it bounds the last character in the line. In either case, DrawText returns the height of the formatted text but does not draw the text.</para>  <list> <item><term>DT_CENTER</term></item> </list>  <para>Centers text horizontally in the rectangle.</para>  <list> <item><term>DT_EXPANDTABS</term></item> </list>  <para>Expands tab characters. The default number of characters per tab is eight.</para>  <list> <item><term>DT_LEFT</term></item> </list>  <para>Aligns text to the left.</para>  <list> <item><term>DT_NOCLIP</term></item> </list>  <para>Draws without clipping. DrawText is somewhat faster when DT_NOCLIP is used.</para>  <list> <item><term>DT_RIGHT</term></item> </list>  <para>Aligns text to the right.</para>  <list> <item><term>DT_RTLREADING</term></item> </list>  <para>Displays text in right-to-left reading order for bidirectional text when a Hebrew or Arabic font is selected. The default reading order for all text is left-to-right.</para>  <list> <item><term>DT_SINGLELINE</term></item> </list>  <para>Displays text on a single line only. Carriage returns and line feeds do not break the line.</para>  <list> <item><term>DT_TOP</term></item> </list>  <para>Top-justifies text.</para>  <list> <item><term>DT_VCENTER</term></item> </list>  <para>Centers text vertically (single line only).</para>  <list> <item><term>DT_WORDBREAK</term></item> </list>  <para>Breaks words. Lines are automatically broken between words if a word would extend past the edge of the rectangle specified by the pRect parameter. A carriage return/line feed sequence also breaks the line.</para>   <para>?</para></param>	
    /// <returns>Determines the width and height of the rectangle. If there are multiple lines of text, this function uses the width of the rectangle pointed to by the rect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, this method modifies the right side of the rectangle so that it bounds the last character in the line. </returns>
    public unsafe RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags);

    /// <summary>
    /// Measures the specified sprite.
    /// </summary>
    /// <param name="text"><para>Pointer to a string to draw. If the Count parameter is -1, the string must be null-terminated.</para></param>	
    /// <param name="rect"><para>Pointer to a <see cref="RectangleF"/> structure that contains the rectangle, in logical coordinates, in which the text is to be formatted. The coordinate value of the rectangle's right side must be greater than that of its left side. Likewise, the coordinate value of the bottom must be greater than that of the top.</para></param>	
    /// <param name="drawFlags"><para>Specifies the method of formatting the text. It can be any combination of the following values:</para>  ValueMeaning <list> <item><term>DT_BOTTOM</term></item> </list>  <para>Justifies the text to the bottom of the rectangle. This value must be combined with DT_SINGLELINE.</para>  <list> <item><term>DT_CALCRECT</term></item> </list>  <para>Determines the width and height of the rectangle. If there are multiple lines of text, DrawText uses the width of the rectangle pointed to by the pRect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, DrawText modifies the right side of the rectangle so that it bounds the last character in the line. In either case, DrawText returns the height of the formatted text but does not draw the text.</para>  <list> <item><term>DT_CENTER</term></item> </list>  <para>Centers text horizontally in the rectangle.</para>  <list> <item><term>DT_EXPANDTABS</term></item> </list>  <para>Expands tab characters. The default number of characters per tab is eight.</para>  <list> <item><term>DT_LEFT</term></item> </list>  <para>Aligns text to the left.</para>  <list> <item><term>DT_NOCLIP</term></item> </list>  <para>Draws without clipping. DrawText is somewhat faster when DT_NOCLIP is used.</para>  <list> <item><term>DT_RIGHT</term></item> </list>  <para>Aligns text to the right.</para>  <list> <item><term>DT_RTLREADING</term></item> </list>  <para>Displays text in right-to-left reading order for bidirectional text when a Hebrew or Arabic font is selected. The default reading order for all text is left-to-right.</para>  <list> <item><term>DT_SINGLELINE</term></item> </list>  <para>Displays text on a single line only. Carriage returns and line feeds do not break the line.</para>  <list> <item><term>DT_TOP</term></item> </list>  <para>Top-justifies text.</para>  <list> <item><term>DT_VCENTER</term></item> </list>  <para>Centers text vertically (single line only).</para>  <list> <item><term>DT_WORDBREAK</term></item> </list>  <para>Breaks words. Lines are automatically broken between words if a word would extend past the edge of the rectangle specified by the pRect parameter. A carriage return/line feed sequence also breaks the line.</para>   <para>?</para></param>
    /// <param name="textHeight">The height of the formatted text but does not draw the text.</param>	
    /// <returns>Determines the width and height of the rectangle. If there are multiple lines of text, this function uses the width of the rectangle pointed to by the rect parameter and extends the base of the rectangle to bound the last line of text. If there is only one line of text, this method modifies the right side of the rectangle so that it bounds the last character in the line. </returns>
    public unsafe RectangleF MeasureText(string text, RectangleF rect, FontDrawFlags drawFlags, out int textHeight);
}