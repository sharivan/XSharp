using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Graphics;

/// <summary>
/// Specifies formatting options for text rendering.
/// </summary>
/// <unmanaged>DT</unmanaged>
[Flags]
public enum FontDrawFlags
{
    /// <summary>
    /// Align the text to the bottom.
    /// </summary>
    Bottom = 8,
    /// <summary>
    /// Align the text to the center.
    /// </summary>
    Center = 1,
    /// <summary>
    /// Expand tab characters.
    /// </summary>
    ExpandTabs = 0x40,
    /// <summary>
    /// Align the text to the left.
    /// </summary>
    Left = 0,
    /// <summary>
    /// Don't clip the text.
    /// </summary>
    NoClip = 0x100,
    /// <summary>
    /// Align the text to the right.
    /// </summary>
    Right = 2,
    /// <summary>
    /// Rendering the text in right-to-left reading order.
    /// </summary>
    RtlReading = 0x20000,
    /// <summary>
    /// Force all text to a single line.
    /// </summary>
    SingleLine = 0x20,
    /// <summary>
    /// Align the text to the top.
    /// </summary>
    Top = 0,
    /// <summary>
    /// Vertically align the text to the center.
    /// </summary>
    VerticalCenter = 4,
    /// <summary>
    /// Allow word breaks.
    /// </summary>
    WordBreak = 0x10
}