using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Engine.World;
using XSharp.Exporter.JSON;
using XSharp.Exporter.Util;
using XSharp.Math.Fixed.Geometry;
using static XSharp.Engine.Consts;
using static XSharp.Engine.Functions;

namespace XSharp.Exporter.Map;

internal class BlockProperties(LevelWriter writer, int id, string tilemapName)
{
    [JsonIgnore]
    public LevelWriter Writer
    {
        get;
    } = writer;

    [JsonPropertyName("id")]
    public int ID
    {
        get;
    } = id;

    [JsonPropertyName("tilemap")]
    public string TileMapName
    {
        get;
    } = tilemapName;

    [JsonPropertyName("maps")]
    [JsonConverter(typeof(MatrixConverter<int>))]
    public int[,] Maps
    {
        get;
    } = ArrayUtil.CreateFilledArray(SIDE_MAPS_PER_BLOCK, SIDE_MAPS_PER_BLOCK, -1);

    internal MapProperties GetMap(int row, int col)
    {
        int mapID = Maps[row, col];
        if (mapID < 0)
            return null;

        return Writer.GetMap(TileMapName, mapID);
    }

    internal void SetMap(Vector pos, MapProperties map)
    {
        Cell cell = GetMapCellFromPos(pos);
        Maps[cell.Row, cell.Col] = map.ID;
    }
}