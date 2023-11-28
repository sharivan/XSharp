using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Input;

/// <summary>	
/// Keyboard device constants.	
/// </summary>	
/// <remarks>	
/// <p> The following alternate names are available: </p> <table> <tr><th> Alternate name </th><th> Regular name </th><th> Note </th></tr> <tr><td> DIK_BACKSPACE				 </td><td> <see cref="SharpDX.DirectInput.Key.Back"/> </td><td> BACKSPACE </td></tr> <tr><td> DIK_CAPSLOCK </td><td> <see cref="SharpDX.DirectInput.Key.Capital"/> </td><td> CAPS LOCK </td></tr> <tr><td> DIK_CIRCUMFLEX </td><td> <see cref="SharpDX.DirectInput.Key.PreviousTrack"/> </td><td> On Japanese keyboard </td></tr> <tr><td> DIK_DOWNARROW </td><td> <see cref="SharpDX.DirectInput.Key.Down"/> </td><td> On arrow keypad </td></tr> <tr><td> DIK_LALT </td><td> <see cref="SharpDX.DirectInput.Key.LeftAlt"/> </td><td> Left ALT </td></tr> <tr><td> DIK_LEFTARROW </td><td> <see cref="SharpDX.DirectInput.Key.Left"/> </td><td> On arrow keypad </td></tr> <tr><td> DIK_NUMPADMINUS </td><td> DIK__SUBTRACT </td><td> MINUS SIGN (-) on numeric keypad  </td></tr> <tr><td> DIK_NUMPADPERIOD </td><td> <see cref="SharpDX.DirectInput.Key.Decimal"/> </td><td> PERIOD (decimal point) on numeric keypad </td></tr> <tr><td> DIK_NUMPADPLUS </td><td> <see cref="SharpDX.DirectInput.Key.Add"/> </td><td> PLUS SIGN (+) on numeric keypad  </td></tr> <tr><td> DIK_NUMPADSLASH </td><td> DIK__DIVIDE </td><td> Forward slash (/) on numeric keypad </td></tr> <tr><td> DIK_NUMPADSTAR </td><td> <see cref="SharpDX.DirectInput.Key.Multiply"/> </td><td> Asterisk (*) on numeric keypad </td></tr> <tr><td> DIK_PGDN </td><td> <see cref="SharpDX.DirectInput.Key.PageDown"/> </td><td> On arrow keypad </td></tr> <tr><td> DIK_PGUP </td><td> <see cref="SharpDX.DirectInput.Key.PageUp"/> </td><td> On arrow keypad </td></tr> <tr><td> DIK_RALT </td><td> <see cref="SharpDX.DirectInput.Key.RightAlt"/> </td><td> Right ALT </td></tr> <tr><td> DIK_RIGHTARROW </td><td> <see cref="SharpDX.DirectInput.Key.Right"/> </td><td> On arrow keypad </td></tr> <tr><td> DIK_UPARROW </td><td> <see cref="SharpDX.DirectInput.Key.Up"/> </td><td> On arrow keypad </td></tr> </table> <p> For information about Japanese keyboards, see DirectInput and Japanese Keyboards. </p> <p>The data at a given offset is associated with a keyboard key. Typically, these values are used in the dwOfs member of the <see cref="SharpDX.DirectInput.ObjectData"/>, DIOBJECTDATAFORMAT or <see cref="SharpDX.DirectInput.DeviceObjectInstance"/> structures, or as indices when accessing data within the array using array notation.</p>	
/// </remarks>	
public enum Key : int
{
    Escape = unchecked((int) 1),
    D1 = unchecked((int) 2),	
    D2 = unchecked((int) 3),	
    D3 = unchecked((int) 4),	
    D4 = unchecked((int) 5),	
    D5 = unchecked((int) 6),
    D6 = unchecked((int) 7),	
    D7 = unchecked((int) 8),
    D8 = unchecked((int) 9),
    D9 = unchecked((int) 10),
    D0 = unchecked((int) 11),
    Minus = unchecked((int) 12),
    Equals = unchecked((int) 13),
    Back = unchecked((int) 14),	
    Tab = unchecked((int) 15),
    Q = unchecked((int) 16),
    W = unchecked((int) 17),
    E = unchecked((int) 18),
    R = unchecked((int) 19),
    T = unchecked((int) 20),
    Y = unchecked((int) 21),	
    U = unchecked((int) 22),
    I = unchecked((int) 23),
    O = unchecked((int) 24),
    P = unchecked((int) 25),
    LeftBracket = unchecked((int) 26),
    RightBracket = unchecked((int) 27),
    Return = unchecked((int) 28),
    LeftControl = unchecked((int) 29),
    A = unchecked((int) 30),	
    S = unchecked((int) 31),
    D = unchecked((int) 32),
    F = unchecked((int) 33),
    G = unchecked((int) 34),	
    H = unchecked((int) 35),
    J = unchecked((int) 36),
    K = unchecked((int) 37),
    L = unchecked((int) 38),	
    Semicolon = unchecked((int) 39),
    Apostrophe = unchecked((int) 40),
    Grave = unchecked((int) 41),
    LeftShift = unchecked((int) 42),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_BACKSLASH']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_BACKSLASH</unmanaged>	
    /// <unmanaged-short>DIK_BACKSLASH</unmanaged-short>	
    Backslash = unchecked((int) 43),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_Z']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_Z</unmanaged>	
    /// <unmanaged-short>DIK_Z</unmanaged-short>	
    Z = unchecked((int) 44),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_X']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_X</unmanaged>	
    /// <unmanaged-short>DIK_X</unmanaged-short>	
    X = unchecked((int) 45),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_C']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_C</unmanaged>	
    /// <unmanaged-short>DIK_C</unmanaged-short>	
    C = unchecked((int) 46),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_V']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_V</unmanaged>	
    /// <unmanaged-short>DIK_V</unmanaged-short>	
    V = unchecked((int) 47),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_B']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_B</unmanaged>	
    /// <unmanaged-short>DIK_B</unmanaged-short>	
    B = unchecked((int) 48),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_N']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_N</unmanaged>	
    /// <unmanaged-short>DIK_N</unmanaged-short>	
    N = unchecked((int) 49),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_M']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_M</unmanaged>	
    /// <unmanaged-short>DIK_M</unmanaged-short>	
    M = unchecked((int) 50),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_COMMA']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_COMMA</unmanaged>	
    /// <unmanaged-short>DIK_COMMA</unmanaged-short>	
    Comma = unchecked((int) 51),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_PERIOD']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_PERIOD</unmanaged>	
    /// <unmanaged-short>DIK_PERIOD</unmanaged-short>	
    Period = unchecked((int) 52),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SLASH']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SLASH</unmanaged>	
    /// <unmanaged-short>DIK_SLASH</unmanaged-short>	
    Slash = unchecked((int) 53),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_RSHIFT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_RSHIFT</unmanaged>	
    /// <unmanaged-short>DIK_RSHIFT</unmanaged-short>	
    RightShift = unchecked((int) 54),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MULTIPLY']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MULTIPLY</unmanaged>	
    /// <unmanaged-short>DIK_MULTIPLY</unmanaged-short>	
    Multiply = unchecked((int) 55),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_LMENU']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_LMENU</unmanaged>	
    /// <unmanaged-short>DIK_LMENU</unmanaged-short>	
    LeftAlt = unchecked((int) 56),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SPACE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SPACE</unmanaged>	
    /// <unmanaged-short>DIK_SPACE</unmanaged-short>	
    Space = unchecked((int) 57),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_CAPITAL']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_CAPITAL</unmanaged>	
    /// <unmanaged-short>DIK_CAPITAL</unmanaged-short>	
    Capital = unchecked((int) 58),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F1']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F1</unmanaged>	
    /// <unmanaged-short>DIK_F1</unmanaged-short>	
    F1 = unchecked((int) 59),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F2']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F2</unmanaged>	
    /// <unmanaged-short>DIK_F2</unmanaged-short>	
    F2 = unchecked((int) 60),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F3']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F3</unmanaged>	
    /// <unmanaged-short>DIK_F3</unmanaged-short>	
    F3 = unchecked((int) 61),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F4']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F4</unmanaged>	
    /// <unmanaged-short>DIK_F4</unmanaged-short>	
    F4 = unchecked((int) 62),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F5']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F5</unmanaged>	
    /// <unmanaged-short>DIK_F5</unmanaged-short>	
    F5 = unchecked((int) 63),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F6']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F6</unmanaged>	
    /// <unmanaged-short>DIK_F6</unmanaged-short>	
    F6 = unchecked((int) 64),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F7']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F7</unmanaged>	
    /// <unmanaged-short>DIK_F7</unmanaged-short>	
    F7 = unchecked((int) 65),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F8']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F8</unmanaged>	
    /// <unmanaged-short>DIK_F8</unmanaged-short>	
    F8 = unchecked((int) 66),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F9']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F9</unmanaged>	
    /// <unmanaged-short>DIK_F9</unmanaged-short>	
    F9 = unchecked((int) 67),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F10']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F10</unmanaged>	
    /// <unmanaged-short>DIK_F10</unmanaged-short>	
    F10 = unchecked((int) 68),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMLOCK']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMLOCK</unmanaged>	
    /// <unmanaged-short>DIK_NUMLOCK</unmanaged-short>	
    NumberLock = unchecked((int) 69),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SCROLL']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SCROLL</unmanaged>	
    /// <unmanaged-short>DIK_SCROLL</unmanaged-short>	
    ScrollLock = unchecked((int) 70),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD7']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD7</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD7</unmanaged-short>	
    NumberPad7 = unchecked((int) 71),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD8']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD8</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD8</unmanaged-short>	
    NumberPad8 = unchecked((int) 72),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD9']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD9</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD9</unmanaged-short>	
    NumberPad9 = unchecked((int) 73),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SUBTRACT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SUBTRACT</unmanaged>	
    /// <unmanaged-short>DIK_SUBTRACT</unmanaged-short>	
    Subtract = unchecked((int) 74),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD4']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD4</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD4</unmanaged-short>	
    NumberPad4 = unchecked((int) 75),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD5']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD5</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD5</unmanaged-short>	
    NumberPad5 = unchecked((int) 76),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD6']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD6</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD6</unmanaged-short>	
    NumberPad6 = unchecked((int) 77),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_ADD']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_ADD</unmanaged>	
    /// <unmanaged-short>DIK_ADD</unmanaged-short>	
    Add = unchecked((int) 78),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD1']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD1</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD1</unmanaged-short>	
    NumberPad1 = unchecked((int) 79),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD2']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD2</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD2</unmanaged-short>	
    NumberPad2 = unchecked((int) 80),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD3']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD3</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD3</unmanaged-short>	
    NumberPad3 = unchecked((int) 81),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPAD0']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPAD0</unmanaged>	
    /// <unmanaged-short>DIK_NUMPAD0</unmanaged-short>	
    NumberPad0 = unchecked((int) 82),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_DECIMAL']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_DECIMAL</unmanaged>	
    /// <unmanaged-short>DIK_DECIMAL</unmanaged-short>	
    Decimal = unchecked((int) 83),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_OEM_102']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_OEM_102</unmanaged>	
    /// <unmanaged-short>DIK_OEM_102</unmanaged-short>	
    Oem102 = unchecked((int) 86),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F11']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F11</unmanaged>	
    /// <unmanaged-short>DIK_F11</unmanaged-short>	
    F11 = unchecked((int) 87),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F12']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F12</unmanaged>	
    /// <unmanaged-short>DIK_F12</unmanaged-short>	
    F12 = unchecked((int) 88),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F13']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F13</unmanaged>	
    /// <unmanaged-short>DIK_F13</unmanaged-short>	
    F13 = unchecked((int) 100),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F14']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F14</unmanaged>	
    /// <unmanaged-short>DIK_F14</unmanaged-short>	
    F14 = unchecked((int) 101),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_F15']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_F15</unmanaged>	
    /// <unmanaged-short>DIK_F15</unmanaged-short>	
    F15 = unchecked((int) 102),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_KANA']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_KANA</unmanaged>	
    /// <unmanaged-short>DIK_KANA</unmanaged-short>	
    Kana = unchecked((int) 112),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_ABNT_C1']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_ABNT_C1</unmanaged>	
    /// <unmanaged-short>DIK_ABNT_C1</unmanaged-short>	
    AbntC1 = unchecked((int) 115),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_CONVERT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_CONVERT</unmanaged>	
    /// <unmanaged-short>DIK_CONVERT</unmanaged-short>	
    Convert = unchecked((int) 121),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NOCONVERT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NOCONVERT</unmanaged>	
    /// <unmanaged-short>DIK_NOCONVERT</unmanaged-short>	
    NoConvert = unchecked((int) 123),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_YEN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_YEN</unmanaged>	
    /// <unmanaged-short>DIK_YEN</unmanaged-short>	
    Yen = unchecked((int) 125),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_ABNT_C2']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_ABNT_C2</unmanaged>	
    /// <unmanaged-short>DIK_ABNT_C2</unmanaged-short>	
    AbntC2 = unchecked((int) 126),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPADEQUALS']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPADEQUALS</unmanaged>	
    /// <unmanaged-short>DIK_NUMPADEQUALS</unmanaged-short>	
    NumberPadEquals = unchecked((int) 141),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_PREVTRACK']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_PREVTRACK</unmanaged>	
    /// <unmanaged-short>DIK_PREVTRACK</unmanaged-short>	
    PreviousTrack = unchecked((int) 144),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_AT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_AT</unmanaged>	
    /// <unmanaged-short>DIK_AT</unmanaged-short>	
    AT = unchecked((int) 145),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_COLON']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_COLON</unmanaged>	
    /// <unmanaged-short>DIK_COLON</unmanaged-short>	
    Colon = unchecked((int) 146),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_UNDERLINE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_UNDERLINE</unmanaged>	
    /// <unmanaged-short>DIK_UNDERLINE</unmanaged-short>	
    Underline = unchecked((int) 147),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_KANJI']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_KANJI</unmanaged>	
    /// <unmanaged-short>DIK_KANJI</unmanaged-short>	
    Kanji = unchecked((int) 148),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_STOP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_STOP</unmanaged>	
    /// <unmanaged-short>DIK_STOP</unmanaged-short>	
    Stop = unchecked((int) 149),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_AX']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_AX</unmanaged>	
    /// <unmanaged-short>DIK_AX</unmanaged-short>	
    AX = unchecked((int) 150),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_UNLABELED']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_UNLABELED</unmanaged>	
    /// <unmanaged-short>DIK_UNLABELED</unmanaged-short>	
    Unlabeled = unchecked((int) 151),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NEXTTRACK']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NEXTTRACK</unmanaged>	
    /// <unmanaged-short>DIK_NEXTTRACK</unmanaged-short>	
    NextTrack = unchecked((int) 153),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPADENTER']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPADENTER</unmanaged>	
    /// <unmanaged-short>DIK_NUMPADENTER</unmanaged-short>	
    NumberPadEnter = unchecked((int) 156),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_RCONTROL']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_RCONTROL</unmanaged>	
    /// <unmanaged-short>DIK_RCONTROL</unmanaged-short>	
    RightControl = unchecked((int) 157),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MUTE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MUTE</unmanaged>	
    /// <unmanaged-short>DIK_MUTE</unmanaged-short>	
    Mute = unchecked((int) 160),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_CALCULATOR']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_CALCULATOR</unmanaged>	
    /// <unmanaged-short>DIK_CALCULATOR</unmanaged-short>	
    Calculator = unchecked((int) 161),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_PLAYPAUSE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_PLAYPAUSE</unmanaged>	
    /// <unmanaged-short>DIK_PLAYPAUSE</unmanaged-short>	
    PlayPause = unchecked((int) 162),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MEDIASTOP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MEDIASTOP</unmanaged>	
    /// <unmanaged-short>DIK_MEDIASTOP</unmanaged-short>	
    MediaStop = unchecked((int) 164),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_VOLUMEDOWN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_VOLUMEDOWN</unmanaged>	
    /// <unmanaged-short>DIK_VOLUMEDOWN</unmanaged-short>	
    VolumeDown = unchecked((int) 174),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_VOLUMEUP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_VOLUMEUP</unmanaged>	
    /// <unmanaged-short>DIK_VOLUMEUP</unmanaged-short>	
    VolumeUp = unchecked((int) 176),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBHOME']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBHOME</unmanaged>	
    /// <unmanaged-short>DIK_WEBHOME</unmanaged-short>	
    WebHome = unchecked((int) 178),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NUMPADCOMMA']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NUMPADCOMMA</unmanaged>	
    /// <unmanaged-short>DIK_NUMPADCOMMA</unmanaged-short>	
    NumberPadComma = unchecked((int) 179),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_DIVIDE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_DIVIDE</unmanaged>	
    /// <unmanaged-short>DIK_DIVIDE</unmanaged-short>	
    Divide = unchecked((int) 181),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SYSRQ']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SYSRQ</unmanaged>	
    /// <unmanaged-short>DIK_SYSRQ</unmanaged-short>	
    PrintScreen = unchecked((int) 183),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_RMENU']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_RMENU</unmanaged>	
    /// <unmanaged-short>DIK_RMENU</unmanaged-short>	
    RightAlt = unchecked((int) 184),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_PAUSE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_PAUSE</unmanaged>	
    /// <unmanaged-short>DIK_PAUSE</unmanaged-short>	
    Pause = unchecked((int) 197),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_HOME']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_HOME</unmanaged>	
    /// <unmanaged-short>DIK_HOME</unmanaged-short>	
    Home = unchecked((int) 199),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_UP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_UP</unmanaged>	
    /// <unmanaged-short>DIK_UP</unmanaged-short>	
    Up = unchecked((int) 200),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_PRIOR']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_PRIOR</unmanaged>	
    /// <unmanaged-short>DIK_PRIOR</unmanaged-short>	
    PageUp = unchecked((int) 201),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_LEFT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_LEFT</unmanaged>	
    /// <unmanaged-short>DIK_LEFT</unmanaged-short>	
    Left = unchecked((int) 203),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_RIGHT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_RIGHT</unmanaged>	
    /// <unmanaged-short>DIK_RIGHT</unmanaged-short>	
    Right = unchecked((int) 205),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_END']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_END</unmanaged>	
    /// <unmanaged-short>DIK_END</unmanaged-short>	
    End = unchecked((int) 207),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_DOWN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_DOWN</unmanaged>	
    /// <unmanaged-short>DIK_DOWN</unmanaged-short>	
    Down = unchecked((int) 208),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_NEXT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_NEXT</unmanaged>	
    /// <unmanaged-short>DIK_NEXT</unmanaged-short>	
    PageDown = unchecked((int) 209),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_INSERT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_INSERT</unmanaged>	
    /// <unmanaged-short>DIK_INSERT</unmanaged-short>	
    Insert = unchecked((int) 210),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_DELETE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_DELETE</unmanaged>	
    /// <unmanaged-short>DIK_DELETE</unmanaged-short>	
    Delete = unchecked((int) 211),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_LWIN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_LWIN</unmanaged>	
    /// <unmanaged-short>DIK_LWIN</unmanaged-short>	
    LeftWindowsKey = unchecked((int) 219),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_RWIN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_RWIN</unmanaged>	
    /// <unmanaged-short>DIK_RWIN</unmanaged-short>	
    RightWindowsKey = unchecked((int) 220),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_APPS']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_APPS</unmanaged>	
    /// <unmanaged-short>DIK_APPS</unmanaged-short>	
    Applications = unchecked((int) 221),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_POWER']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_POWER</unmanaged>	
    /// <unmanaged-short>DIK_POWER</unmanaged-short>	
    Power = unchecked((int) 222),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_SLEEP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_SLEEP</unmanaged>	
    /// <unmanaged-short>DIK_SLEEP</unmanaged-short>	
    Sleep = unchecked((int) 223),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WAKE']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WAKE</unmanaged>	
    /// <unmanaged-short>DIK_WAKE</unmanaged-short>	
    Wake = unchecked((int) 227),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBSEARCH']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBSEARCH</unmanaged>	
    /// <unmanaged-short>DIK_WEBSEARCH</unmanaged-short>	
    WebSearch = unchecked((int) 229),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBFAVORITES']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBFAVORITES</unmanaged>	
    /// <unmanaged-short>DIK_WEBFAVORITES</unmanaged-short>	
    WebFavorites = unchecked((int) 230),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBREFRESH']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBREFRESH</unmanaged>	
    /// <unmanaged-short>DIK_WEBREFRESH</unmanaged-short>	
    WebRefresh = unchecked((int) 231),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBSTOP']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBSTOP</unmanaged>	
    /// <unmanaged-short>DIK_WEBSTOP</unmanaged-short>	
    WebStop = unchecked((int) 232),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBFORWARD']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBFORWARD</unmanaged>	
    /// <unmanaged-short>DIK_WEBFORWARD</unmanaged-short>	
    WebForward = unchecked((int) 233),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_WEBBACK']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_WEBBACK</unmanaged>	
    /// <unmanaged-short>DIK_WEBBACK</unmanaged-short>	
    WebBack = unchecked((int) 234),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MYCOMPUTER']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MYCOMPUTER</unmanaged>	
    /// <unmanaged-short>DIK_MYCOMPUTER</unmanaged-short>	
    MyComputer = unchecked((int) 235),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MAIL']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MAIL</unmanaged>	
    /// <unmanaged-short>DIK_MAIL</unmanaged-short>	
    Mail = unchecked((int) 236),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_MEDIASELECT']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_MEDIASELECT</unmanaged>	
    /// <unmanaged-short>DIK_MEDIASELECT</unmanaged-short>	
    MediaSelect = unchecked((int) 237),

    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <include file='.\..\Documentation\CodeComments.xml' path="/comments/comment[@id='DIK_UNKNOWN']/*"/>	
    /// <msdn-id>ee418641</msdn-id>	
    /// <unmanaged>DIK_UNKNOWN</unmanaged>	
    /// <unmanaged-short>DIK_UNKNOWN</unmanaged-short>	
    Unknown = unchecked((int) 0),
}