namespace XSharp.Graphics;

/// <summary>
/// Defines precision levels for font rendering.
/// </summary>
/// <unmanaged>OutPrecision</unmanaged>
public enum FontPrecision : byte
{
    /// <summary>
    /// Default
    /// </summary>
    Default,
    /// <summary>
    /// String
    /// </summary>
    String,
    /// <summary>
    /// Character
    /// </summary>
    Character,
    /// <summary>
    /// Stroke
    /// </summary>
    Stroke,
    /// <summary>
    /// TrueType
    /// </summary>
    TrueType,
    /// <summary>
    /// Device
    /// </summary>
    Device,
    /// <summary>
    /// Raster
    /// </summary>
    Raster,
    /// <summary>
    /// TrueTypeOnly
    /// </summary>
    TrueTypeOnly,
    /// <summary>
    /// Outline
    /// </summary>
    Outline,
    /// <summary>
    /// ScreenOutline
    /// </summary>
    ScreenOutline,
    /// <summary>
    /// PostScriptOnly
    /// </summary>
    PostScriptOnly
}