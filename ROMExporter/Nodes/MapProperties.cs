using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Engine.Collision;
using XSharp.Exporter.JSON;
using static XSharp.Engine.Consts;

namespace XSharp.Exporter.Map;

internal class MapProperties(TilemapProperties tilemap, int id)
{
    [JsonIgnore]
    public TilemapProperties Tilemap
    {
        get;
    } = tilemap;

    [JsonPropertyName("id")]
    public int ID
    {
        get;
    } = id;

    [JsonPropertyName("cells")]
    [JsonConverter(typeof(MatrixConverter<MapCellProperties>))]
    public MapCellProperties[,] Cells
    {
        get;
    } = new MapCellProperties[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];

    [JsonPropertyName("collision")]
    public CollisionData CollisionData { get; internal set; }

    public bool IsEmpty()
    {
        for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
            {
                if (Cells[row, col] != null)
                    return false;
            }
        }

        return true;
    }

    internal MapCellProperties SetCell(int row, int col, TileProperties tile, byte palette, bool flipped, bool mirrored, bool upLayer)
    {
        int tileID = tile != null ? tile.ID : -1;
        var cell = Cells[row, col];
        if (cell == null)
        {
            cell = new MapCellProperties(this, tileID, palette, flipped, mirrored, upLayer);
            Cells[row, col] = cell;
        }
        else
        {
            cell.TileID = tileID;
            cell.Palette = palette;
            cell.Flipped = flipped;
            cell.Mirrored = mirrored;
            cell.UpLayer = upLayer;
        }

        return cell;
    }
}