using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XSharp.Exporter.JSON;

internal class ListConverter<T> : JsonConverter<List<T>>
{
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonSerializer.Deserialize<T[]>(ref reader, options);
        return [.. arr];
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        var arr = value.ToArray();
        JsonSerializer.Serialize(writer, arr, options);
    }
}