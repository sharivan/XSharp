using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Exporter.JSON;

internal class BoxConverter : JsonConverter<Box>
{
    public override Box Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var origin = Vector.NULL_VECTOR;
        var mins = Vector.NULL_VECTOR;
        var maxs = Vector.NULL_VECTOR;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Box(origin, mins, maxs);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();

                reader.Read(); // Avança para o valor da propriedade

                switch (propertyName)
                {
                    case "origin":
                        origin = JsonSerializer.Deserialize<Vector>(ref reader, options);
                        break;

                    case "mins":
                        mins = JsonSerializer.Deserialize<Vector>(ref reader, options);
                        break;

                    case "maxs":
                        mins = JsonSerializer.Deserialize<Vector>(ref reader, options);
                        break;

                    default:
                        reader.Skip(); // Pula propriedades desconhecidas
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Box value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("origin");
        JsonSerializer.Serialize(writer, value.Origin, options);
        writer.WritePropertyName("mins");
        JsonSerializer.Serialize(writer, value.Mins, options);
        writer.WritePropertyName("maxs");
        JsonSerializer.Serialize(writer, value.Maxs, options);
        writer.WriteEndObject();
    }
}