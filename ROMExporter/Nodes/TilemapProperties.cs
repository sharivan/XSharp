using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XSharp.Exporter.JSON;

namespace XSharp.Exporter.Map;

internal class TilemapProperties(LevelWriter writer, string name, string tilesetName)
{
    [JsonInclude]
    [JsonPropertyName("maps")]
    [JsonConverter(typeof(ListConverter<MapProperties>))]
    private List<MapProperties> maps = [];

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

    [JsonPropertyName("tileset")]
    public string TilesetName
    {
        get;
    } = tilesetName;

    [JsonIgnore]
    public MapProperties this[int index] => maps[index];

    public MapProperties AddMap()
    {
        int id = maps.Count;
        var map = new MapProperties(this, id);
        maps.Add(map);
        return map;
    }
}