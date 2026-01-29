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

internal class LayoutProperties(LevelWriter writer, string name, int width, int height)
{
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

    [JsonPropertyName("width")]
    public int Width
    {
        get;
    } = width;

    [JsonPropertyName("height")]
    public int Height
    {
        get;
    } = height;

    [JsonPropertyName("scenes")]
    [JsonConverter(typeof(MatrixConverter<int>))]
    public int[,] Scenes
    {
        get;
    } = ArrayUtil.CreateFilledArray(height, width, -1);

    internal SceneProperties AddScene(Vector pos)
    {
        SceneProperties result = Writer.AddScene();

        Cell cell = GetSceneCellFromPos(pos);
        Scenes[cell.Row, cell.Col] = result.ID;

        return result;
    }

    internal void SetMap(Vector pos, MapProperties map)
    {
        Cell cell = GetSceneCellFromPos(pos);
        SceneProperties scene = GetScene(cell);
        scene ??= AddScene(pos);

        scene.SetMap(pos - GetSceneLeftTop(cell), map);
    }

    public SceneProperties GetScene(Cell cell)
    {
        return GetScene(cell.Row, cell.Col);
    }

    public SceneProperties GetScene(int row, int col)
    {
        int sceneID = Scenes[row, col];
        if (sceneID < 0)
            return null;

        return Writer.GetScene(sceneID);
    }
}