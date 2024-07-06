namespace XSharp.Graphics;

/// <summary>
/// Specifies quality options for font rendering.
/// </summary>
/// <unmanaged>QUALITY</unmanaged>
public enum FontQuality : byte
{
    /// <summary>
    /// Default
    /// </summary>
    Default,
    /// <summary>
    /// Draft
    /// </summary>
    Draft,
    /// <summary>
    /// Proof
    /// </summary>
    Proof,
    /// <summary>
    /// Non antialiased
    /// </summary>
    NonAntialiased,
    /// <summary>
    /// Antialiased
    /// </summary>
    Antialiased,
    /// <summary>
    /// ClearType
    /// </summary>
    ClearType,
    /// <summary>
    /// ClearTypeNatural
    /// </summary>
    ClearTypeNatural
}