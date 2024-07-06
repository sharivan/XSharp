using System;

namespace XSharp.Graphics;

/// <summary>
/// Defines pitch and family settings for fonts.
/// </summary>
[Flags]
public enum FontPitchAndFamily : byte
{
    /// <summary>
    /// Use the Decorative family.
    /// </summary>
    Decorative = 80,
    /// <summary>
    /// Default pitch.
    /// </summary>
    Default = 0,
    /// <summary>
    /// The font family doesn't matter.
    /// </summary>
    DontCare = 0,
    /// <summary>
    /// Fixed pitch.
    /// </summary>
    Fixed = 1,
    /// <summary>
    /// Use the Modern family.
    /// </summary>
    Modern = 0x30,
    /// <summary>
    /// Mono pitch.
    /// </summary>
    Mono = 8,
    /// <summary>
    /// Use the Roman family.
    /// </summary>
    Roman = 0x10,
    /// <summary>
    /// Use the Script family.
    /// </summary>
    Script = 0x40,
    /// <summary>
    /// Use the Swiss family.
    /// </summary>
    Swiss = 0x20,
    /// <summary>
    /// Variable pitch.
    /// </summary>
    Variable = 2
}