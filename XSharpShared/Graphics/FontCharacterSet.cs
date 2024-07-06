namespace XSharp.Graphics;

/// <summary>
/// Defines possible character sets for fonts.
/// </summary>
/// <unmanaged>CHARSET</unmanaged>
public enum FontCharacterSet : byte
{
    /// <summary>
    /// The ANSI character set.
    /// </summary>
    Ansi = 0,
    /// <summary>
    /// The Arabic character set.
    /// </summary>
    Arabic = 0xb2,
    /// <summary>
    /// The Baltic character set.
    /// </summary>
    Baltic = 0xba,
    /// <summary>
    /// The Chinese character set.
    /// </summary>
    ChineseBig5 = 0x88,
    /// <summary>
    /// The default system character set.
    /// </summary>
    Default = 1,
    /// <summary>
    /// The East Europe character set.
    /// </summary>
    EastEurope = 0xee,
    /// <summary>
    /// The GB2312 character set.
    /// </summary>
    GB2312 = 0x86,
    /// <summary>
    /// The Greek character set.
    /// </summary>
    Greek = 0xa1,
    /// <summary>
    /// The Hangul character set.
    /// </summary>
    Hangul = 0x81,
    /// <summary>
    /// The Hebrew character set.
    /// </summary>
    Hebrew = 0xb1,
    /// <summary>
    /// The Johab character set.
    /// </summary>
    Johab = 130,
    /// <summary>
    /// The Mac character set.
    /// </summary>
    Mac = 0x4d,
    /// <summary>
    /// The OEM character set.
    /// </summary>
    Oem = 0xff,
    /// <summary>
    /// The Russian character set.
    /// </summary>
    Russian = 0xcc,
    /// <summary>
    /// The ShiftJIS character set.
    /// </summary>
    ShiftJIS = 0x80,
    /// <summary>
    /// The symbol character set.
    /// </summary>
    Symbol = 2,
    /// <summary>
    /// The Thai character set.
    /// </summary>
    Thai = 0xde,
    /// <summary>
    /// The Turkish character set.
    /// </summary>
    Turkish = 0xa2,
    /// <summary>
    /// The Vietnamese character set.
    /// </summary>
    Vietnamese = 0xa3
}