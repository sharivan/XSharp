using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Exporter.JSON;

namespace XSharp.Exporter.Map;

internal class TilesetProperties(LevelWriter writer, string name, string paletteName)
{
    [JsonInclude]
    [JsonPropertyName("tiles")]
    [JsonConverter(typeof(ListConverter<TileProperties>))]
    private List<TileProperties> tiles = [];

    [JsonIgnore]
    public LevelWriter Writer
    {
        get;
    } = writer;

    [JsonPropertyName("name")]
    public string Name
    {
        get;
    } = name;

    [JsonPropertyName("palette")]
    public string PaletteName
    {
        get;
    } = paletteName;

    public TileProperties this[int index] => tiles[index];

    public TileProperties AddTile(byte[] data)
    {
        var tile = new TileProperties(this, tiles.Count, data);
        tiles.Add(tile);
        return tile;
    }
}