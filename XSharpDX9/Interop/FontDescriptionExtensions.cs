using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FontDescription = XSharp.Graphics.FontDescription;
using FontCharacterSet = XSharp.Graphics.FontCharacterSet;
using FontDrawFlags = XSharp.Graphics.FontDrawFlags;
using FontPitchAndFamily = XSharp.Graphics.FontPitchAndFamily;
using FontPrecision = XSharp.Graphics.FontPrecision;
using FontQuality = XSharp.Graphics.FontQuality;
using FontWeight = XSharp.Graphics.FontWeight;
using DX9FontDescription = SharpDX.Direct3D9.FontDescription;
using DX9FontCharacterSet = SharpDX.Direct3D9.FontCharacterSet;
using DX9FontDrawFlags = SharpDX.Direct3D9.FontDrawFlags;
using DX9FontPitchAndFamily = SharpDX.Direct3D9.FontPitchAndFamily;
using DX9FontPrecision = SharpDX.Direct3D9.FontPrecision;
using DX9FontQuality = SharpDX.Direct3D9.FontQuality;
using DX9FontWeight = SharpDX.Direct3D9.FontWeight;

namespace XSharp.Interop;

public static class FontDescriptionExtensions
{
    public static DX9FontDescription ToDX9FontDescription(this FontDescription description)
    {
        var result = new DX9FontDescription
        {
            Width = description.Width,
            Height = description.Height,
            Weight = (DX9FontWeight) description.Weight,
            MipLevels = 0,
            CharacterSet = (DX9FontCharacterSet) description.CharacterSet,
            FaceName = description.FaceName,
            Italic = description.Italic,
            OutputPrecision = (DX9FontPrecision) description.OutputPrecision,
            PitchAndFamily = (DX9FontPitchAndFamily) description.PitchAndFamily,
            Quality = (DX9FontQuality) description.Quality
        };
        return result;
    }

    public static FontDescription ToFontDescription(this DX9FontDescription description)
    {
        var result = new FontDescription
        {
            Width = description.Width,
            Height = description.Height,
            Weight = (FontWeight) description.Weight,
            CharacterSet = (FontCharacterSet) description.CharacterSet,
            FaceName = description.FaceName,
            Italic = description.Italic,
            OutputPrecision = (FontPrecision) description.OutputPrecision,
            PitchAndFamily = (FontPitchAndFamily) description.PitchAndFamily,
            Quality = (FontQuality) description.Quality
        };
        return result;
    }

    public static DX9FontCharacterSet ToDX9FontCharacterSet(this FontCharacterSet flags)
    {
        return (DX9FontCharacterSet) flags;
    }

    public static FontCharacterSet ToFontCharacterSet(this DX9FontCharacterSet flags)
    {
        return (FontCharacterSet) flags;
    }

    public static DX9FontDrawFlags ToDX9FontDrawFlags(this FontDrawFlags flags)
    {
        return (DX9FontDrawFlags) flags;
    }

    public static FontDrawFlags ToFontDrawFlags(this DX9FontDrawFlags flags)
    {
        return (FontDrawFlags) flags;
    }

    public static DX9FontPitchAndFamily ToDX9FontPitchAndFamily(this FontPitchAndFamily flags)
    {
        return (DX9FontPitchAndFamily) flags;
    }

    public static FontPitchAndFamily ToFontPitchAndFamily(this DX9FontDrawFlags flags)
    {
        return (FontPitchAndFamily) flags;
    }

    public static DX9FontPrecision ToDX9FontPrecision(this FontPrecision flags)
    {
        return (DX9FontPrecision) flags;
    }

    public static FontPrecision ToFontPrecision(this DX9FontPrecision flags)
    {
        return (FontPrecision) flags;
    }

    public static DX9FontQuality ToDX9FontDrawFlags(this FontQuality flags)
    {
        return (DX9FontQuality) flags;
    }

    public static FontQuality ToFontQuality(this DX9FontQuality flags)
    {
        return (FontQuality) flags;
    }

    public static DX9FontWeight ToDX9FontWeight(this FontWeight flags)
    {
        return (DX9FontWeight) flags;
    }

    public static FontWeight ToFontWeight(this DX9FontWeight flags)
    {
        return (FontWeight) flags;
    }
}