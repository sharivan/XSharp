using System.Text.Json;
using System.Text.Json.Serialization;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Exporter.JSON;

internal class VectorConverter : JsonConverter<Vector>
{
    public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        FixedSingle x = 0;
        FixedSingle y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();

                reader.Read(); // Avança para o valor da propriedade

                switch (propertyName)
                {
                    case "x":
                        x = JsonSerializer.Deserialize<FixedSingle>(ref reader, options);
                        break;

                    case "y":
                        y = JsonSerializer.Deserialize<FixedSingle>(ref reader, options);
                        break;

                    default:
                        reader.Skip(); // Pula propriedades desconhecidas
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("x", value.X.ToString(FixedSingleStringFormat.PIXEL_SUBPIXEL));
        writer.WriteString("y", value.Y.ToString(FixedSingleStringFormat.PIXEL_SUBPIXEL));
        writer.WriteEndObject();
    }
}