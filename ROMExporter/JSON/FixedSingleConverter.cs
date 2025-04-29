using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Math;

namespace XSharp.Exporter.JSON;

internal class FixedSingleConverter : JsonConverter<FixedSingle>
{
    public override FixedSingle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueStr = reader.GetString();
        if (valueStr.Contains('_'))
        {
            var parts = valueStr.Split('_');
            int pixel = int.Parse(parts[0]);
            double subPixel = double.Parse(parts[1]);
            return new FixedSingle(pixel, subPixel);
        }

        return new FixedSingle(double.Parse(valueStr));
    }

    public override void Write(Utf8JsonWriter writer, FixedSingle value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(FixedSingleStringFormat.PIXEL_SUBPIXEL));
    }
}