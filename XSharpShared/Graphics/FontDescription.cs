using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XSharp.Util;

namespace XSharp.Graphics;

/// <summary>	
/// <p>Defines font attributes.</p>	
/// </summary>
public struct FontDescription
{

    /// <summary>	
    /// <dd> <p>Height, in logical units, of the font's character cell or character.</p> </dd>	
    /// </summary>	
    public int Height;

    /// <summary>	
    /// <dd> <p>Width, in logical units, of characters in the font.</p> </dd>	
    /// </summary>
    public int Width;

    /// <summary>	
    /// <dd> <p>Weight of the font in the range from 0 through 1000.</p> </dd>	
    /// </summary>	
    public FontWeight Weight;

    /// <summary>	
    /// <dd> <p>Set to <strong>TRUE</strong> for an Italic font.</p> </dd>	
    /// </summary>
    public bool Italic;

    /// <summary>	
    /// <dd> <p>Character set.</p> </dd>	
    /// </summary>
    public FontCharacterSet CharacterSet;

    /// <summary>	
    /// <dd> <p>Output precision. The output precision defines how closely the output must match the requested font height, width, character orientation, escapement, pitch, and font type.</p> </dd>	
    /// </summary>
    /// <unmanaged-short>D3DX10_FONT_PRECISION OutputPrecision</unmanaged-short>	
    public FontPrecision OutputPrecision;

    /// <summary>	
    /// <dd> <p>Output quality.</p> </dd>	
    /// </summary>
    public FontQuality Quality;

    /// <summary>	
    /// <dd> <p>Pitch and family of the font.</p> </dd>	
    /// </summary>	
    public FontPitchAndFamily PitchAndFamily;

    /// <summary>	
    /// <dd> <p>A <c>null</c>-terminated string that specifies the typeface name of the font. The length of the string must not exceed 32 characters, including the terminating <strong><c>null</c></strong> character. If FaceName is an empty string, the first font that matches the other specified attributes will be used. If the compiler settings require Unicode, the data type TCHAR resolves to WCHAR; otherwise, the data type resolves to CHAR. See Remarks.</p> </dd>	
    /// </summary>
    public string FaceName;
}