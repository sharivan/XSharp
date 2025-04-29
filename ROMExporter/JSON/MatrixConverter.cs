using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XSharp.Exporter.JSON;

internal class MatrixConverter<T> : JsonConverter<T[,]>
{
    public override T[,]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Lê um array de arrays
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var rows = new List<T[]>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Lê uma linha
            var row = JsonSerializer.Deserialize<T[]>(ref reader, options) ?? throw new JsonException();
            rows.Add(row);
        }

        if (rows.Count == 0)
            return new T[0, 0];

        int height = rows.Count;
        int width = rows[0].Length;

        var result = new T[height, width];

        for (int i = 0; i < height; i++)
        {
            if (rows[i].Length != width)
                throw new JsonException("Inconsistent row sizes in 2D array.");

            for (int j = 0; j < width; j++)
                result[i, j] = rows[i][j];
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        int height = value.GetLength(0);
        int width = value.GetLength(1);

        for (int i = 0; i < height; i++)
        {
            writer.WriteStartArray();

            for (int j = 0; j < width; j++)
                JsonSerializer.Serialize(writer, value[i, j], options);
            
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}