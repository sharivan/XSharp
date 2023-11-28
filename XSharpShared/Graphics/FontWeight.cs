using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Graphics;

/// <summary>
/// Specifies weights for font rendering.
/// </summary>
/// <unmanaged>FW</unmanaged>
public enum FontWeight
{
    /// <summary>
    /// Use a black weight.
    /// </summary>
    Black = 900,
    /// <summary>
    /// Use a bold weight.
    /// </summary>
    Bold = 700,
    /// <summary>
    /// Use a demi-bold weight.
    /// </summary>
    DemiBold = 600,
    /// <summary>
    /// The font weight doesn't matter.
    /// </summary>
    DoNotCare = 0,
    /// <summary>
    /// Use an extra bold weight.
    /// </summary>
    ExtraBold = 800,
    /// <summary>
    /// Make the font extra light.
    /// </summary>
    ExtraLight = 200,
    /// <summary>
    /// Use a heavy weight.
    /// </summary>
    Heavy = 900,
    /// <summary>
    /// Make the font light.
    /// </summary>
    Light = 300,
    /// <summary>
    /// Use a medium weight.
    /// </summary>
    Medium = 500,
    /// <summary>
    /// Use a normal weight.
    /// </summary>
    Normal = 400,
    /// <summary>
    /// Use a regular weight.
    /// </summary>
    Regular = 400,
    /// <summary>
    /// Use a semi-bold weight.
    /// </summary>
    SemiBold = 600,
    /// <summary>
    /// Make the font thin.
    /// </summary>
    Thin = 100,
    /// <summary>
    /// Use an ultra bold weight.
    /// </summary>
    UltraBold = 800,
    /// <summary>
    /// Make the font ultra light.
    /// </summary>
    UltraLight = 200
}