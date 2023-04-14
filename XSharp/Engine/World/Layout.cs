using System;
using System.Collections;
using System.Collections.Generic;

using SharpDX.Direct3D9;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public abstract class Layout : IRenderable, IEnumerable<Scene>, IDisposable
{
    public static BaseEngine Engine => BaseEngine.Engine;

    public static World World => Engine.World;

    private class LayoutEnumerator : IEnumerator<Scene>
    {
        private Layout layout;
        private int row = -1;
        private int col = -1;

        public Scene Current => row >= 0 && row < layout.SceneRowCount && col >= 0 && col < layout.SceneColCount ? layout.scenes[row, col] : null;

        object IEnumerator.Current => Current;

        public LayoutEnumerator(Layout layout)
        {
            this.layout = layout;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (row == -1)
            {
                row = 0;
                col = 0;
            }
            else if (col == layout.SceneColCount)
            {
                row++;
                col = 0;
            }
            else
                col++;

            return row < layout.SceneRowCount;
        }

        public void Reset()
        {
            row = -1;
            col = -1;
        }
    }

    private readonly List<Tile> tileList;
    private readonly List<Map> mapList;
    private readonly List<Block> blockList;
    private readonly List<Scene> sceneList;

    protected Scene[,] scenes;

    public Scene this[int row, int col] => scenes[row, col];

    public int TileRowCount => Height / TILE_SIZE;

    public int TileColCount => Width / TILE_SIZE;

    public int MapRowCount => Height / MAP_SIZE;

    public int MapColCount => Width / MAP_SIZE;

    public int BlockRowCount => Height / BLOCK_SIZE;

    public int BlockColCount => Width / BLOCK_SIZE;

    public int SceneRowCount
    {
        get;
        private set;
    }

    public int SceneColCount
    {
        get;
        private set;
    }

    public int Width => SceneColCount * SCENE_SIZE;

    public int Height => SceneRowCount * SCENE_SIZE;

    public Box BoundingBox => new(0, 0, Width, Height);

    public Vector Size => new(Width, Height);

    public Vector LayoutSize => new(SceneRowCount, SceneColCount);

    public FadingControl FadingControl
    {
        get;
        private set;
    }

    public abstract Palette Palette
    {
        get;
    }

    public abstract Texture Tilemap
    {
        get;
    }

    protected Layout(int sceneRowCount, int sceneColCount)
    {
        SceneRowCount = sceneRowCount;
        SceneColCount = sceneColCount;

        tileList = new List<Tile>();
        mapList = new List<Map>();
        blockList = new List<Block>();
        sceneList = new List<Scene>();

        scenes = new Scene[sceneRowCount, sceneColCount];

        FadingControl = new FadingControl();
    }

    public Tile GetTileFrom(Vector pos)
    {
        Cell tsp = GetSceneCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
            return null;

        Scene scene = scenes[row, col];
        return scene?.GetTileFrom(pos - GetSceneLeftTop(row, col));
    }

    public Map GetMapFrom(Vector pos)
    {
        Cell tsp = GetSceneCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
            return null;

        Scene scene = scenes[row, col];
        return scene?.GetMapFrom(pos - GetSceneLeftTop(row, col));
    }

    public Block GetBlockFrom(Vector pos)
    {
        Cell tsp = GetSceneCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
            return null;

        Scene scene = scenes[row, col];
        return scene?.GetBlockFrom(pos - GetSceneLeftTop(row, col));
    }

    public Scene GetSceneFrom(Vector pos)
    {
        Cell tsp = GetSceneCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        return row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount
            ? null
            : scenes[row, col];
    }

    public Scene GetSceneFrom(Cell cell)
    {
        return GetSceneFrom(cell.Row, cell.Col);
    }

    public Scene GetSceneFrom(int row, int col)
    {
        return row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount
            ? null
            : scenes[row, col];
    }

    public Tile AddTile()
    {
        int id = tileList.Count;
        var result = new Tile(id);
        tileList.Add(result);

        return result;
    }

    public Tile AddTile(byte[] source)
    {
        int id = tileList.Count;
        var result = new Tile(id, source);
        tileList.Add(result);

        return result;
    }

    public Map AddMap(CollisionData collisionData = CollisionData.NONE)
    {
        int id = mapList.Count;
        var result = new Map(id, collisionData);
        mapList.Add(result);

        return result;
    }

    public Block AddBlock()
    {
        int id = blockList.Count;
        var result = new Block(id);
        blockList.Add(result);

        return result;
    }

    public Scene AddScene()
    {
        int id = sceneList.Count;
        var result = new Scene(id);
        sceneList.Add(result);

        return result;
    }

    public Scene AddScene(int row, int col)
    {
        Scene result = AddScene();
        scenes[row, col] = result;

        return result;
    }

    public Scene AddScene(Vector pos)
    {
        Scene result = AddScene();

        Cell cell = GetSceneCellFromPos(pos);
        scenes[cell.Row, cell.Col] = result;

        return result;
    }

    public Map AddMap(Vector pos, CollisionData collisionData = CollisionData.NONE)
    {
        Map result = AddMap(collisionData);
        SetMap(pos, result);

        return result;
    }

    public void SetMap(Vector pos, Map map)
    {
        Cell cell = GetSceneCellFromPos(pos);
        Scene scene = scenes[cell.Row, cell.Col];
        scene ??= AddScene(pos);

        scene.SetMap(pos - GetSceneLeftTop(cell), map);
    }

    public void SetBlock(Vector pos, Block block)
    {
        Cell cell = GetSceneCellFromPos(pos);
        Scene scene = scenes[cell.Row, cell.Col];
        scene ??= AddScene(pos);

        scene.SetBlock(pos - GetSceneLeftTop(cell), block);
    }

    public void SetScene(Vector pos, Scene scene)
    {
        Cell cell = GetSceneCellFromPos(pos);
        scenes[cell.Row, cell.Col] = scene;
    }

    public Tile GetTileByID(int id)
    {
        return id < 0 || id >= tileList.Count ? null : tileList[id];
    }

    public Map GetMapByID(int id)
    {
        return id < 0 || id >= mapList.Count ? null : mapList[id];
    }

    public Block GetBlockByID(int id)
    {
        return id < 0 || id >= blockList.Count ? null : blockList[id];
    }

    public Scene GetSceneByID(int id)
    {
        return id < 0 || id >= sceneList.Count ? null : sceneList[id];
    }

    public void RemoveTile(Tile tile)
    {
        if (tile == null)
            return;

        tileList.Remove(tile);

        foreach (Map map in mapList)
            map.RemoveTile(tile);
    }

    public void RemoveMap(Map map)
    {
        if (map == null)
            return;

        mapList.Remove(map);

        foreach (Block block in blockList)
            block.RemoveMap(map);
    }

    public void RemoveBlock(Block block)
    {
        if (block == null)
            return;

        blockList.Remove(block);

        foreach (Scene scene in sceneList)
            scene.RemoveBlock(block);
    }

    public void RemoveScene(Scene scene)
    {
        if (scene == null)
            return;

        sceneList.Remove(scene);

        for (int col = 0; col < SceneColCount; col++)
        {
            for (int row = 0; row < SceneRowCount; row++)
            {
                if (scenes[row, col] == scene)
                    scenes[row, col] = null;
            }
        }
    }

    public void Resize(int rowCount, int colCount)
    {
        if (rowCount == SceneRowCount && colCount == SceneColCount)
            return;

        var newScenes = new Scene[rowCount, colCount];

        int minRows = System.Math.Min(rowCount, SceneRowCount);
        int minCols = System.Math.Min(colCount, SceneColCount);

        for (int col = 0; col < minCols; col++)
        {
            for (int row = 0; row < minRows; row++)
                newScenes[row, col] = scenes[row, col];
        }

        SceneRowCount = rowCount;
        SceneColCount = colCount;

        scenes = newScenes;
    }

    public IEnumerator<Scene> GetEnumerator()
    {
        return new LayoutEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new LayoutEnumerator(this);
    }

    public abstract void Render(IRenderTarget target);

    internal void Tesselate()
    {
        foreach (Scene scene in sceneList)
            scene.Tessellate();
    }

    internal void OnDisposeDevice()
    {
        foreach (Scene scene in sceneList)
            scene.OnDisposeDevice();
    }

    public void Clear()
    {
        for (int col = 0; col < SceneColCount; col++)
        {
            for (int row = 0; row < SceneRowCount; row++)
                scenes[row, col] = null;
        }

        tileList.Clear();
        blockList.Clear();
        mapList.Clear();

        foreach (Scene scene in sceneList)
            scene.Dispose();

        sceneList.Clear();
    }

    public void FillRectangle(Box box, Map map)
    {
        Vector boxLT = box.LeftTop;
        Vector boxSize = box.DiagonalVector;

        Cell cell = GetMapCellFromPos(boxLT);
        int col = cell.Col;
        int row = cell.Row;
        int cols = (int) (boxSize.X / MAP_SIZE);
        int rows = (int) (boxSize.Y / MAP_SIZE);

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
                SetMap(GetMapLeftTop(row + r, col + c), map);
        }
    }

    public void FillRectangle(Box box, Block block)
    {
        Vector boxLT = box.LeftTop;
        Vector boxSize = box.DiagonalVector;

        Cell cell = GetBlockCellFromPos(boxLT);
        int col = cell.Col;
        int row = cell.Row;
        int cols = (int) (boxSize.X / BLOCK_SIZE);
        int rows = (int) (boxSize.Y / BLOCK_SIZE);

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
                SetBlock(GetBlockLeftTop(row + r, col + c), block);
        }
    }

    public void FillRectangle(Box box, Scene scene)
    {
        Vector boxLT = box.LeftTop;
        Vector boxSize = box.DiagonalVector;

        Cell cell = GetSceneCellFromPos(boxLT);
        int col = cell.Col;
        int row = cell.Row;
        int cols = (int) (boxSize.X / SCENE_SIZE);
        int rows = (int) (boxSize.Y / SCENE_SIZE);

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
                SetScene(GetSceneLeftTop(row + r, col + c), scene);
        }
    }

    public void Dispose()
    {
        try
        {
            Clear();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    internal void OnFrame()
    {
        FadingControl.DoFrame();
    }
}