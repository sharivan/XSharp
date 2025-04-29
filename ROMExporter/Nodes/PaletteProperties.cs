using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Graphics;

namespace XSharp.Exporter.Map;

internal class PaletteProperties
{
    [JsonIgnore]
    public LevelWriter Writer
    {
        get;
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get;
    }

    [JsonPropertyName("colors")]
    public Color[] Colors
    {
        get;
    }

    public PaletteProperties(LevelWriter writer, string name, Color[] colors)
    {
        Writer = writer;
        Name = name;
        Colors = colors;
    }

    public PaletteProperties(LevelWriter writer, string name, int colorCount)
    {
        Writer = writer;
        Name = name;
        Colors = new Color[colorCount];
    }
}