using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XSharp.Exporter.Map;

internal class MapCellProperties(MapProperties map, int tileID, int palette, bool flipped, bool mirrored, bool upLayer)
{
    [JsonIgnore]
    public MapProperties Map
    {
        get;
    } = map;

    [JsonPropertyName("tile")]
    public int TileID
    {
        get;
        internal set;
    } = tileID;

    [JsonPropertyName("palette")]
    public int Palette
    {
        get;
        internal set;
    } = palette;

    [JsonPropertyName("flipped")]
    public bool Flipped
    {
        get;
        internal set;
    } = flipped;

    [JsonPropertyName("mirrored")]
    public bool Mirrored
    {
        get;
        internal set;
    } = mirrored;

    [JsonPropertyName("upLayer")]
    public bool UpLayer
    {
        get;
        internal set;
    } = upLayer;
}