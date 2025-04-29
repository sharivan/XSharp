using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Exporter.JSON;
using static XSharp.Engine.Consts;

namespace XSharp.Exporter.Map;

internal class TileProperties
{
    [JsonIgnore]
    public TilesetProperties Tileset
    {
        get;
    }

    [JsonPropertyName("id")]
    public int ID
    {
        get;
    }

    [JsonPropertyName("data")]
    [JsonConverter(typeof(MatrixConverter<byte>))]
    public byte[,] Data
    {
        get;
    }

    public TileProperties(TilesetProperties tileset, int id, byte[,] data)
    {
        Tileset = tileset;
        ID = id;
        Data = data;
    }

    public TileProperties(TilesetProperties tileset, int id, byte[] data)
    {
        Tileset = tileset;
        ID = id;
        Data = new byte[TILE_SIZE, TILE_SIZE];

        for (int row = 0; row < TILE_SIZE; row++)
            for (int col = 0; col < TILE_SIZE; col++)
                Data[row, col] = data[row * TILE_SIZE + col];
    }
}