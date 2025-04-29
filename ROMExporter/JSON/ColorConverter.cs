using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Exporter.JSON;

internal class ColorConverter : JsonConverter<Color>
{
    private int ParseInt(string valueStr)
    {
        valueStr = valueStr.Trim();

        if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(valueStr[2..], System.Globalization.NumberStyles.HexNumber);

        if (valueStr.StartsWith("#"))
            return int.Parse(valueStr[1..], System.Globalization.NumberStyles.HexNumber);

        return int.Parse(valueStr);
    }

    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueStr = reader.GetString().Trim();

        string[] parts;
        int r;
        int g;
        int b;
        int a;

        try
        {
            int rgba = ParseInt(valueStr);
        }
        catch (FormatException)
        {
        }

        if (valueStr.StartsWith('('))
        {
            if (!valueStr.EndsWith(')'))
                throw new JsonException("Invalid color format. Expected closing parenthesis.");

            valueStr = valueStr[..^1];
            parts = valueStr.Split(',');

            if (parts.Length is < 3 or > 4)
                throw new JsonException("Invalid color format. Expected 3 or 4 components.");

            r = ParseInt(parts[0]);
            g = ParseInt(parts[1]);
            b = ParseInt(parts[2]);
            a = parts.Length == 4 ? ParseInt(parts[3]) : 255; // Default alpha to 255 if not provided

            return new Color(r, g, b, a);
        }

        parts = valueStr.Split(' ');

        r = 0;
        g = 0;
        b = 0;
        a = 255;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i].Trim();
            if (part.StartsWith("R:"))
            {
                part = part[2..];
                r = ParseInt(part);
            }
            else if (part.StartsWith("G:"))
            {
                part = part[2..];
                g = ParseInt(part);
            }
            else if (part.StartsWith("B:"))
            {
                part = part[2..];
                b = ParseInt(part);
            }
            else if (part.StartsWith("A:"))
            {
                part = part[2..];
                a = ParseInt(part);
            }
            else
            {
                throw new JsonException($"Invalid color format. Unknown component: {part}");
            }
        }

        return new Color(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue("#" + value.ToRgba().ToString("X8"));
    }
}