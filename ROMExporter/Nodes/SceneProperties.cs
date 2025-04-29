using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Engine.World;
using XSharp.Exporter.JSON;
using XSharp.Exporter.Util;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.Functions;

namespace XSharp.Exporter.Map;

internal class SceneProperties(LevelWriter writer, int id, string name = null)
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

    [JsonPropertyName("name")]
    public string Name
    {
        get;
    } = name;

    [JsonPropertyName("blocks")]
    [JsonConverter(typeof(MatrixConverter<int>))]
    public int[,] Blocks
    {
        get;
    } = ArrayUtil.CreateFilledArray(SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE, -1);

    internal BlockProperties GetBlock(int row, int col)
    {
        int blockID = Blocks[row, col];
        if (blockID < 0)
            return null;

        return Writer.GetBlock(blockID);
    }

    internal void SetMap(Vector pos, MapProperties map)
    {
        Cell cell = GetBlockCellFromPos(pos);
        BlockProperties block = GetBlock(cell.Row, cell.Col);
        if (block == null)
        {
            block = Writer.AddBlock(map.Tilemap.Name);
            Blocks[cell.Row, cell.Col] = block.ID;
        }

        block.SetMap(pos - new Vector(cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE), map);
    }
}