using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using XSharp.Engine.Entities;
using XSharp.Geometry;
using static XSharp.Engine.Consts;
using MMXBox = XSharp.Geometry.Box;

namespace XSharp.Engine.World
{
    public class World : IDisposable
    {
        public const int TILEMAP_WIDTH = 32 * MAP_SIZE;
        public const int TILEMAP_HEIGHT = 32 * MAP_SIZE;

        public const float TILE_FRAC_SIZE = 1f / 64;

        public static readonly Vector TILE_SIZE_VECTOR = new(TILE_SIZE, TILE_SIZE);
        public static readonly Vector TILE_FRAC_SIZE_VECTOR = new(TILE_FRAC_SIZE, TILE_FRAC_SIZE);
        private int backgroundSceneRowCount;
        private int backgroundSceneColCount;

        private readonly List<Tile> tileList;
        private readonly List<Tile> backgroundTileList;
        private readonly List<Map> mapList;
        private readonly List<Map> backgroundMapList;
        private readonly List<Block> blockList;
        private readonly List<Block> backgroundBlockList;
        private readonly List<Scene> sceneList;
        private readonly List<Scene> backgroundSceneList;

        private Scene[,] scenes;
        private Scene[,] backgroundScenes;

        private CollisionChecker collisionChecker;

        public FadingControl FadingSettings
        {
            get;
        }

        internal World(int sceneRowCount, int sceneColCount)
            : this(sceneRowCount, sceneColCount, sceneRowCount, sceneColCount)
        {
        }

        internal World(int sceneRowCount, int sceneColCount, int backgroundSceneRowCount, int backgroundSceneColCount)
        {
            SceneRowCount = sceneRowCount;
            SceneColCount = sceneColCount;
            this.backgroundSceneRowCount = backgroundSceneRowCount;
            this.backgroundSceneColCount = backgroundSceneColCount;

            Camera = new Camera()
            {
                Width = SCREEN_WIDTH,
                Height = SCREEN_HEIGHT
            };

            FadingSettings = new FadingControl();

            tileList = new List<Tile>();
            backgroundTileList = new List<Tile>();
            mapList = new List<Map>();
            backgroundMapList = new List<Map>();
            blockList = new List<Block>();
            backgroundBlockList = new List<Block>();
            sceneList = new List<Scene>();
            backgroundSceneList = new List<Scene>();

            scenes = new Scene[sceneRowCount, sceneColCount];
            backgroundScenes = new Scene[backgroundSceneRowCount, backgroundSceneColCount];

            collisionChecker = new CollisionChecker();
        }

        public GameEngine Engine => GameEngine.Engine;

        public Device Device => GameEngine.Engine.Device;

        public int Width => SceneColCount * SCENE_SIZE;

        public int Height => SceneRowCount * SCENE_SIZE;

        public int BackgroundWidth => backgroundSceneColCount * SCENE_SIZE;

        public int BackgroundHeight => backgroundSceneRowCount * SCENE_SIZE;

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

        public MMXBox BoundingBox => new(0, 0, Width, Height);

        public Vector Size => new(Width, Height);

        public Vector LayoutSize => new(SceneRowCount, SceneColCount);

        public Vector LayoutBackgroundtSize => new(backgroundSceneRowCount, backgroundSceneColCount);

        public Camera Camera
        {
            get;
        }

        public Texture ForegroundPalette => Engine.ForegroundPalette;

        public Texture BackgroundPalette => Engine.BackgroundPalette;

        public Texture ForegroundTilemap => Engine.ForegroundTilemap;

        public Texture BackgroundTilemap => Engine.BackgroundTilemap;

        public Tile AddTile(bool background = false)
        {
            int id = background ? backgroundTileList.Count : tileList.Count;
            var result = new Tile(this, id);

            if (background)
                backgroundTileList.Add(result);
            else
                tileList.Add(result);

            return result;
        }

        public Tile AddTile(byte[] source, bool background = false)
        {
            int id = background ? backgroundTileList.Count : tileList.Count;
            var result = new Tile(this, id, source);

            if (background)
                backgroundTileList.Add(result);
            else
                tileList.Add(result);

            return result;
        }

        public Map AddMap(CollisionData collisionData = CollisionData.NONE, bool background = false)
        {
            int id = background ? backgroundMapList.Count : mapList.Count;
            var result = new Map(this, id, collisionData);

            if (background)
                backgroundMapList.Add(result);
            else
                mapList.Add(result);

            return result;
        }

        public Block AddBlock(bool background = false)
        {
            int id = background ? backgroundBlockList.Count : blockList.Count;
            var result = new Block(this, id);

            if (background)
                backgroundBlockList.Add(result);
            else
                blockList.Add(result);

            return result;
        }

        public Scene AddScene(bool background = false)
        {
            int id = background ? backgroundSceneList.Count : sceneList.Count;
            var result = new Scene(this, id);

            if (background)
                backgroundSceneList.Add(result);
            else
                sceneList.Add(result);

            return result;
        }

        public Scene AddScene(int row, int col, bool background = false)
        {
            Scene result = AddScene(background);

            if (background)
                backgroundScenes[row, col] = result;
            else
                scenes[row, col] = result;

            return result;
        }

        public Scene AddScene(Vector pos, bool background = false)
        {
            Scene result = AddScene(background);

            Cell cell = GetSceneCellFromPos(pos);

            if (background)
                backgroundScenes[cell.Row, cell.Col] = result;
            else
                scenes[cell.Row, cell.Col] = result;

            return result;
        }

        public Map AddMap(Vector pos, CollisionData collisionData = CollisionData.NONE, bool background = false)
        {
            Map result = AddMap(collisionData, background);
            SetMap(pos, result, background);
            return result;
        }

        public void SetMap(Vector pos, Map map, bool background = false)
        {
            Cell cell = GetSceneCellFromPos(pos);
            Scene scene = background ? backgroundScenes[cell.Row, cell.Col] : scenes[cell.Row, cell.Col];
            scene ??= AddScene(pos, background);

            scene.SetMap(pos - new Vector(cell.Col * SCENE_SIZE, cell.Row * SCENE_SIZE), map);
        }

        public void SetBlock(Vector pos, Block block, bool background = false)
        {
            Cell cell = GetSceneCellFromPos(pos);
            Scene scene = background ? backgroundScenes[cell.Row, cell.Col] : scenes[cell.Row, cell.Col];
            scene ??= AddScene(pos, background);

            scene.SetBlock(pos - new Vector(cell.Col * SCENE_SIZE, cell.Row * SCENE_SIZE), block);
        }

        public void SetScene(Vector pos, Scene scene, bool background = false)
        {
            Cell cell = GetSceneCellFromPos(pos);

            if (background)
                backgroundScenes[cell.Row, cell.Col] = scene;
            else
                scenes[cell.Row, cell.Col] = scene;
        }

        public Tile GetTileByID(int id, bool background = false)
        {
            return background
                ? id < 0 || id >= backgroundTileList.Count ? null : backgroundTileList[id]
                : id < 0 || id >= tileList.Count ? null : tileList[id];
        }

        public Map GetMapByID(int id, bool background = false)
        {
            return background
                ? id < 0 || id >= backgroundMapList.Count ? null : backgroundMapList[id]
                : id < 0 || id >= mapList.Count ? null : mapList[id];
        }

        public Block GetBlockByID(int id, bool background = false)
        {
            return background
                ? id < 0 || id >= backgroundBlockList.Count ? null : backgroundBlockList[id]
                : id < 0 || id >= blockList.Count ? null : blockList[id];
        }

        public Scene GetSceneByID(int id, bool background = false)
        {
            return background
                ? id < 0 || id >= backgroundSceneList.Count ? null : backgroundSceneList[id]
                : id < 0 || id >= sceneList.Count ? null : sceneList[id];
        }

        public void RemoveTile(Tile tile)
        {
            if (tile == null)
                return;

            tileList.Remove(tile);
            backgroundTileList.Remove(tile);

            foreach (Map map in mapList)
                map.RemoveTile(tile);

            foreach (Map map in backgroundMapList)
                map.RemoveTile(tile);
        }

        public void RemoveMap(Map map)
        {
            if (map == null)
                return;

            mapList.Remove(map);
            backgroundMapList.Remove(map);

            foreach (Block block in blockList)
                block.RemoveMap(map);

            foreach (Block block in backgroundBlockList)
                block.RemoveMap(map);
        }

        public void RemoveBlock(Block block)
        {
            if (block == null)
                return;

            blockList.Remove(block);
            backgroundBlockList.Remove(block);

            foreach (Scene scene in sceneList)
                scene.RemoveBlock(block);

            foreach (Scene scene in backgroundSceneList)
                scene.RemoveBlock(block);
        }

        public void RemoveScene(Scene scene)
        {
            if (scene == null)
                return;

            sceneList.Remove(scene);
            backgroundSceneList.Remove(scene);

            for (int col = 0; col < SceneColCount; col++)
                for (int row = 0; row < SceneRowCount; row++)
                    if (scenes[row, col] == scene)
                        scenes[row, col] = null;

            for (int col = 0; col < backgroundSceneColCount; col++)
                for (int row = 0; row < backgroundSceneRowCount; row++)
                    if (backgroundScenes[row, col] == scene)
                        backgroundScenes[row, col] = null;
        }

        public void Resize(int rowCount, int colCount, bool background = false)
        {
            if (background)
                ResizeBackground(rowCount, colCount);
            else
                ResizeForeground(rowCount, colCount);
        }

        public void ResizeForeground(int rowCount, int colCount)
        {
            if (rowCount == SceneRowCount && colCount == SceneColCount)
                return;

            var newScenes = new Scene[rowCount, colCount];

            int minRows = System.Math.Min(rowCount, SceneRowCount);
            int minCols = System.Math.Min(colCount, SceneColCount);

            for (int col = 0; col < minCols; col++)
                for (int row = 0; row < minRows; row++)
                    newScenes[row, col] = scenes[row, col];

            SceneRowCount = rowCount;
            SceneColCount = colCount;

            scenes = newScenes;
        }

        public void ResizeBackground(int rowCount, int colCount)
        {
            if (rowCount == backgroundSceneRowCount && colCount == backgroundSceneColCount)
                return;

            var newScenes = new Scene[rowCount, colCount];

            int minRows = System.Math.Min(rowCount, backgroundSceneRowCount);
            int minCols = System.Math.Min(colCount, backgroundSceneColCount);

            for (int col = 0; col < minCols; col++)
                for (int row = 0; row < minRows; row++)
                    newScenes[row, col] = backgroundScenes[row, col];

            backgroundSceneRowCount = rowCount;
            backgroundSceneColCount = colCount;

            backgroundScenes = newScenes;
        }

        public void Clear()
        {
            for (int col = 0; col < SceneColCount; col++)
                for (int row = 0; row < SceneRowCount; row++)
                    scenes[row, col] = null;

            for (int col = 0; col < backgroundSceneColCount; col++)
                for (int row = 0; row < backgroundSceneRowCount; row++)
                    backgroundScenes[row, col] = null;

            tileList.Clear();
            backgroundTileList.Clear();
            blockList.Clear();
            backgroundBlockList.Clear();
            mapList.Clear();
            backgroundMapList.Clear();

            foreach (Scene scene in sceneList)
                scene.Dispose();

            foreach (Scene scene in backgroundSceneList)
                scene.Dispose();

            sceneList.Clear();
            backgroundSceneList.Clear();
        }

        public void FillRectangle(MMXBox box, Map map, bool background = false)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / MAP_SIZE);
            int row = (int) (boxLT.Y / MAP_SIZE);
            int cols = (int) (boxSize.X / MAP_SIZE);
            int rows = (int) (boxSize.Y / MAP_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetMap(new Vector((col + c) * MAP_SIZE, (row + r) * MAP_SIZE), map, background);
        }

        public void FillRectangle(MMXBox box, Block block, bool background = false)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / BLOCK_SIZE);
            int row = (int) (boxLT.Y / BLOCK_SIZE);
            int cols = (int) (boxSize.X / BLOCK_SIZE);
            int rows = (int) (boxSize.Y / BLOCK_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetBlock(new Vector((col + c) * BLOCK_SIZE, (row + r) * BLOCK_SIZE), block, background);
        }

        public void FillRectangle(MMXBox box, Scene scene, bool background = false)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / SCENE_SIZE);
            int row = (int) (boxLT.Y / SCENE_SIZE);
            int cols = (int) (boxSize.X / SCENE_SIZE);
            int rows = (int) (boxSize.Y / SCENE_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetScene(new Vector((col + c) * SCENE_SIZE, (row + r) * SCENE_SIZE), scene, background);
        }

        public void Dispose()
        {
            Clear();
        }

        public void RenderBackground(int layer)
        {
            Checkpoint checkpoint = Engine.CurrentCheckpoint;
            if (checkpoint == null)
                return;

            Vector screenLT = Camera.LeftTop;
            Vector screenRB = Camera.RightBottom;
            Vector backgroundPos = checkpoint.BackgroundPos;

            Vector screenDelta = (checkpoint.Scroll & 0x2) != 0 ? Vector.NULL_VECTOR : (screenLT + checkpoint.CameraPos).Scale(0.5f) - backgroundPos;

            Cell start = GetSceneCellFromPos(screenLT - screenDelta);
            Cell end = GetSceneCellFromPos(screenRB - screenDelta);

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0)
                    continue;

                if ((checkpoint.Scroll & 0x10) == 0 && col >= backgroundSceneColCount)
                    continue;

                int bkgCol = (checkpoint.Scroll & 0x10) != 0 ? col % 2 : col;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= backgroundSceneRowCount)
                        continue;

                    Scene scene = backgroundScenes[row, bkgCol];
                    if (scene != null)
                    {
                        Vector sceneLT = (col * SCENE_SIZE, row * SCENE_SIZE);
                        MMXBox sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                        Engine.RenderVertexBuffer(scene.layers[layer], GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, BackgroundTilemap, BackgroundPalette, FadingSettings, sceneBox + screenDelta);
                    }
                }
            }
        }

        public void RenderForeground(int layer)
        {
            Vector screenLT = Camera.LeftTop;
            Vector screenRB = Camera.RightBottom;

            Cell start = GetSceneCellFromPos(screenLT);
            Cell end = GetSceneCellFromPos(screenRB);

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0 || col >= SceneColCount)
                    continue;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= SceneRowCount)
                        continue;

                    Scene scene = scenes[row, col];
                    if (scene != null)
                    {
                        var sceneLT = new Vector(col * SCENE_SIZE, row * SCENE_SIZE);
                        MMXBox sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                        Engine.RenderVertexBuffer(scene.layers[layer], GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, ForegroundTilemap, ForegroundPalette, FadingSettings, sceneBox);
                    }
                }
            }
        }

        public void OnFrame()
        {
            FadingSettings.OnFrame();
        }

        public static Cell GetTileCellFromPos(Vector pos)
        {
            int col = (int) (pos.X / TILE_SIZE);
            int row = (int) (pos.Y / TILE_SIZE);

            return new Cell(row, col);
        }

        public static Cell GetMapCellFromPos(Vector pos)
        {
            int col = (int) (pos.X / MAP_SIZE);
            int row = (int) (pos.Y / MAP_SIZE);

            return new Cell(row, col);
        }

        public static Cell GetBlockCellFromPos(Vector pos)
        {
            int col = (int) (pos.X / BLOCK_SIZE);
            int row = (int) (pos.Y / BLOCK_SIZE);

            return new Cell(row, col);
        }

        public static Cell GetSceneCellFromPos(Vector pos)
        {
            int col = (int) (pos.X / SCENE_SIZE);
            int row = (int) (pos.Y / SCENE_SIZE);

            return new Cell(row, col);
        }

        public Tile GetTileFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            return scene?.GetTileFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Map GetMapFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            return scene?.GetMapFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Block GetBlockFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            return scene?.GetBlockFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Scene GetSceneFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            return row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount
                ? null
                : background ? backgroundScenes[row, col] : scenes[row, col];
        }

        public Scene GetSceneFrom(Cell cell, bool background = false)
        {
            return GetSceneFrom(cell.Row, cell.Col, background);
        }

        public Scene GetSceneFrom(int row, int col, bool background = false)
        {
            return row < 0 || col < 0 || row >= SceneRowCount || col >= SceneColCount
                ? null
                : background ? backgroundScenes[row, col] : scenes[row, col];
        }

        public static MMXBox GetTileBoundingBox(int row, int col)
        {
            return GetTileBoundingBox(new Cell(row, col));
        }

        public static MMXBox GetMapBoundingBox(int row, int col)
        {
            return GetMapBoundingBox(new Cell(row, col));
        }

        public static MMXBox GetBlockBoundingBox(int row, int col)
        {
            return GetBlockBoundingBox(new Cell(row, col));
        }

        public static MMXBox GetSceneBoundingBox(int row, int col)
        {
            return GetSceneBoundingBox(new Cell(row, col));
        }

        public static MMXBox GetTileBoundingBox(Cell pos)
        {
            return new(pos.Col * TILE_SIZE, pos.Row * TILE_SIZE, TILE_SIZE, TILE_SIZE);
        }

        public static MMXBox GetMapBoundingBox(Cell pos)
        {
            return new(pos.Col * MAP_SIZE, pos.Row * MAP_SIZE, MAP_SIZE, MAP_SIZE);
        }

        public static MMXBox GetBlockBoundingBox(Cell pos)
        {
            return new(pos.Col * BLOCK_SIZE, pos.Row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
        }

        public static MMXBox GetSceneBoundingBox(Cell pos)
        {
            return new(pos.Col * SCENE_SIZE, pos.Row * SCENE_SIZE, SCENE_SIZE, SCENE_SIZE);
        }

        public static MMXBox GetTileBoundingBoxFromPos(Vector pos)
        {
            return GetTileBoundingBox(GetTileCellFromPos(pos));
        }

        public static MMXBox GetMapBoundingBoxFromPos(Vector pos)
        {
            return GetMapBoundingBox(GetMapCellFromPos(pos));
        }

        public static MMXBox GetBlockBoundingBoxFromPos(Vector pos)
        {
            return GetBlockBoundingBox(GetBlockCellFromPos(pos));
        }

        public static MMXBox GetSceneBoundingBoxFromPos(Vector pos)
        {
            return GetSceneBoundingBox(GetSceneCellFromPos(pos));
        }

        internal void Tessellate()
        {
            foreach (Scene scene in sceneList)
                scene.Tessellate();

            foreach (Scene scene in backgroundSceneList)
                scene.Tessellate();
        }

        internal void OnDisposeDevice()
        {
            foreach (Scene scene in sceneList)
                scene.OnDisposeDevice();

            foreach (Scene scene in backgroundSceneList)
                scene.OnDisposeDevice();
        }

        public static bool CanBlockTheMove(CollisionFlags flags)
        {
            return flags is not CollisionFlags.NONE and not CollisionFlags.WATER and not CollisionFlags.WATER_SURFACE;
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, CollisionFlags ignore = CollisionFlags.NONE, Entities.Sprite ignoreSprite = null, bool checkWithWorld = true, bool checknWithSolidSprites = false)
        {
            collisionChecker.Setup(collisionBox, ignore, ignoreSprite, MASK_SIZE, checkWithWorld, checknWithSolidSprites, false, true);
            return collisionChecker.GetCollisionFlags();
        }
    }
}