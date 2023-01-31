using MMX.Engine.Entities;
using MMX.Geometry;
using MMX.Math;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using static MMX.Engine.Consts;
using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.World
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

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount) :
            this(engine, sceneRowCount, sceneColCount, sceneRowCount, sceneColCount)
        {
        }

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount, int backgroundSceneRowCount, int backgroundSceneColCount)
        {
            Engine = engine;

            SceneRowCount = sceneRowCount;
            SceneColCount = sceneColCount;
            this.backgroundSceneRowCount = backgroundSceneRowCount;
            this.backgroundSceneColCount = backgroundSceneColCount;

            Camera = new Camera(this, SCREEN_WIDTH, SCREEN_HEIGHT);

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
        }

        public GameEngine Engine
        {
            get;
        }

        public Device Device => Engine.Device;

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

        public Map AddMap(CollisionData collisionData = CollisionData.BACKGROUND, bool background = false)
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

        public Map AddMap(Vector pos, CollisionData collisionData = CollisionData.BACKGROUND, bool background = false)
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
                        Engine.RenderVertexBuffer(scene.layers[layer], GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, BackgroundTilemap, BackgroundPalette, sceneBox + screenDelta);
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
                        Engine.RenderVertexBuffer(scene.layers[layer], GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, ForegroundTilemap, ForegroundPalette, sceneBox);
                    }
                }
            }
        }

        public void OnFrame()
        {
            Camera.OnFrame();
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

        public static bool IsSolidBlock(CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.MUD => true,
                CollisionData.TOP_MUD => true,
                CollisionData.LAVA => true,
                CollisionData.SOLID2 => true,
                CollisionData.SOLID3 => true,
                CollisionData.UNCLIMBABLE_SOLID => true,
                CollisionData.LEFT_CONVEYOR => true,
                CollisionData.RIGHT_CONVEYOR => true,
                CollisionData.UP_SLOPE_BASE => true,
                CollisionData.DOWN_SLOPE_BASE => true,
                CollisionData.SOLID => true,
                CollisionData.BREAKABLE => true,
                CollisionData.NON_LETHAL_SPIKE => true,
                CollisionData.LETHAL_SPIKE => true,
                CollisionData.SLIPPERY_SLOPE_BASE => true,
                CollisionData.SLIPPERY => true,
                CollisionData.DOOR => true,
                _ => false,
            };
        }

        public static bool IsSlope(CollisionData collisionData)
        {
            return collisionData is >= CollisionData.SLOPE_16_8 and <= CollisionData.SLOPE_0_4 or
                >= CollisionData.LEFT_CONVEYOR_SLOPE_16_12 and <= CollisionData.RIGHT_CONVEYOR_SLOPE_0_4 or
                >= CollisionData.SLIPPERY_SLOPE_16_8 and <= CollisionData.SLIPPERY_SLOPE_0_4;
        }

        internal static RightTriangle MakeSlopeTriangle(int left, int right)
        {
            return left < right
                ? new RightTriangle(new Vector(0, right), MAP_SIZE, left - right)
                : new RightTriangle(new Vector(MAP_SIZE, left), -MAP_SIZE, right - left);
        }

        internal static RightTriangle MakeSlopeTriangle(CollisionData collisionData)
        {
            return collisionData switch
            {
                CollisionData.SLOPE_16_8 => MakeSlopeTriangle(16, 8),
                CollisionData.SLOPE_8_0 => MakeSlopeTriangle(8, 0),
                CollisionData.SLOPE_8_16 => MakeSlopeTriangle(8, 16),
                CollisionData.SLOPE_0_8 => MakeSlopeTriangle(0, 8),
                CollisionData.SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.SLOPE_4_0 => MakeSlopeTriangle(4, 0),
                CollisionData.SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                CollisionData.LEFT_CONVEYOR_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.LEFT_CONVEYOR_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.LEFT_CONVEYOR_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.LEFT_CONVEYOR_SLOPE_4_0 => MakeSlopeTriangle(4, 0),

                CollisionData.RIGHT_CONVEYOR_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.RIGHT_CONVEYOR_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.RIGHT_CONVEYOR_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.RIGHT_CONVEYOR_SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                CollisionData.SLIPPERY_SLOPE_16_8 => MakeSlopeTriangle(16, 8),
                CollisionData.SLIPPERY_SLOPE_8_0 => MakeSlopeTriangle(8, 0),
                CollisionData.SLIPPERY_SLOPE_8_16 => MakeSlopeTriangle(8, 16),
                CollisionData.SLIPPERY_SLOPE_0_8 => MakeSlopeTriangle(0, 8),
                CollisionData.SLIPPERY_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
                CollisionData.SLIPPERY_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
                CollisionData.SLIPPERY_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
                CollisionData.SLIPPERY_SLOPE_4_0 => MakeSlopeTriangle(4, 0),
                CollisionData.SLIPPERY_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
                CollisionData.SLIPPERY_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
                CollisionData.SLIPPERY_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
                CollisionData.SLIPPERY_SLOPE_0_4 => MakeSlopeTriangle(0, 4),

                _ => RightTriangle.EMPTY,
            };
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox, null, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox, placements, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox, null, out slopeTriangle, ignore, preciseCollisionCheck);
        }

        public static bool HasIntersection(MMXBox box1, MMXBox box2)
        {
            return (box1 & box2).IsValid(EPSLON);
        }

        public static bool HasIntersection(MMXBox box, RightTriangle slope)
        {
            return slope.HasIntersectionWith(box, EPSLON, true);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            slopeTriangle = RightTriangle.EMPTY;

            Cell start = GetMapCellFromPos(collisionBox.LeftTop);
            Cell end = GetMapCellFromPos(collisionBox.RightBottom);

            int startRow = start.Row;
            int startCol = start.Col;

            if (startRow < 0)
                startRow = 0;

            if (startRow >= MapRowCount)
                startRow = MapRowCount - 1;

            if (startCol < 0)
                startCol = 0;

            if (startCol >= MapColCount)
                startCol = MapColCount - 1;

            int endRow = end.Row;
            int endCol = end.Col;

            if (endRow < 0)
                endRow = 0;

            if (endRow >= MapRowCount)
                endRow = MapRowCount - 1;

            if (endCol < 0)
                endCol = 0;

            if (endCol >= MapColCount)
                endCol = MapColCount - 1;

            CollisionFlags result = CollisionFlags.NONE;
            for (int row = startRow; row <= endRow; row++)
                for (int col = startCol; col <= endCol; col++)
                {
                    var v = new Vector(col * MAP_SIZE, row * MAP_SIZE);
                    Map map = GetMapFrom(v);
                    if (map != null)
                    {
                        MMXBox mapBox = GetMapBoundingBox(row, col);
                        CollisionData collisionData = map.CollisionData;

                        if (collisionData == CollisionData.BACKGROUND || !HasIntersection(mapBox, collisionBox))
                            continue;

                        bool hasIntersection = HasIntersection(collisionBox, mapBox);
                        if (IsSolidBlock(collisionData) && hasIntersection && !ignore.HasFlag(CollisionFlags.BLOCK))
                        {
                            if (collisionData == CollisionData.UNCLIMBABLE_SOLID)
                            {
                                if (!ignore.HasFlag(CollisionFlags.UNCLIMBABLE))
                                {
                                    placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));
                                    result |= CollisionFlags.BLOCK | CollisionFlags.UNCLIMBABLE;
                                }
                            }
                            else
                            {
                                placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));
                                result |= CollisionFlags.BLOCK;
                            }
                        }
                        else if (collisionData == CollisionData.LADDER && hasIntersection && !ignore.HasFlag(CollisionFlags.LADDER))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.LADDER, row, col, map));

                            result |= CollisionFlags.LADDER;
                        }
                        else if (collisionData == CollisionData.TOP_LADDER && hasIntersection && !ignore.HasFlag(CollisionFlags.TOP_LADDER))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.TOP_LADDER, row, col, map));

                            result |= CollisionFlags.TOP_LADDER;
                        }
                        else if (collisionData == CollisionData.WATER && hasIntersection && !ignore.HasFlag(CollisionFlags.WATER))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.WATER, row, col, map));

                            result |= CollisionFlags.WATER;
                        }
                        else if (collisionData == CollisionData.WATER_SURFACE && hasIntersection && !ignore.HasFlag(CollisionFlags.WATER_SURFACE))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.WATER_SURFACE, row, col, map));

                            result |= CollisionFlags.WATER_SURFACE;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.SLOPE) && IsSlope(collisionData))
                        {
                            RightTriangle st = MakeSlopeTriangle(collisionData) + v;
                            if (preciseCollisionCheck)
                            {
                                if (HasIntersection(collisionBox, st))
                                {
                                    placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                                    slopeTriangle = st;
                                    result |= CollisionFlags.SLOPE;
                                }
                            }
                            else if (hasIntersection)
                            {
                                placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                                slopeTriangle = st;
                                result |= CollisionFlags.SLOPE;
                            }
                        }
                    }
                }

            return result;
        }

        public CollisionFlags ComputedLandedState(MMXBox box, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return ComputedLandedState(box, placements, out RightTriangle slope, MASK_SIZE, ignore);
        }

        public CollisionFlags ComputedLandedState(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return ComputedLandedState(box, null, out slope, MASK_SIZE, ignore);
        }

        public CollisionFlags ComputedLandedState(MMXBox box, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return ComputedLandedState(box, null, out slope, maskSize, ignore);
        }

        private readonly List<CollisionPlacement> bottomPlacementsDisplacedHalfLeft = new();
        private readonly List<CollisionPlacement> bottomPlacementsDisplacedHalfRight = new();

        public CollisionFlags ComputedLandedState(MMXBox box, List<CollisionPlacement> placements, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;

            MMXBox bottomMask = box.ClipTop(box.Height - maskSize);
            MMXBox bottomMaskDisplaced = bottomMask + maskSize * Vector.DOWN_VECTOR;

            MMXBox bottomMaskDisplacedHalfLeft = bottomMaskDisplaced.HalfLeft();
            MMXBox bottomMaskDisplacedHalfRight = bottomMaskDisplaced.HalfRight();

            if (placements != null)
            {
                bottomPlacementsDisplacedHalfLeft.Clear();
                bottomPlacementsDisplacedHalfRight.Clear();
            }

            CollisionFlags bottomLeftDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfLeft, bottomPlacementsDisplacedHalfLeft, out RightTriangle leftDisplaceSlope, ignore, true);
            CollisionFlags bottomRightDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfRight, bottomPlacementsDisplacedHalfRight, out RightTriangle rightDisplaceSlope, ignore, true);

            if (!CanBlockTheMove(bottomLeftDisplacedCollisionFlags) && !CanBlockTheMove(bottomRightDisplacedCollisionFlags))
                return CollisionFlags.NONE;

            if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (!CanBlockTheMove(bottomLeftDisplacedCollisionFlags) && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    placements?.AddRange(bottomPlacementsDisplacedHalfRight);

                    return bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && !CanBlockTheMove(bottomRightDisplacedCollisionFlags))
                {
                    placements?.AddRange(bottomPlacementsDisplacedHalfLeft);

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                    ;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    if (placements != null)
                    {
                        placements.AddRange(bottomPlacementsDisplacedHalfLeft);
                        placements.AddRange(bottomPlacementsDisplacedHalfRight);
                    }

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }
            }
            else
            {
                MMXBox bottomMaskHalfLeft = bottomMask.HalfLeft();
                MMXBox bottomMaskHalfRight = bottomMask.HalfRight();

                CollisionFlags bottomLeftCollisionFlags = GetCollisionFlags(bottomMaskHalfLeft, out RightTriangle leftSlope, ignore, true);
                CollisionFlags bottomRightCollisionFlags = GetCollisionFlags(bottomMaskHalfRight, out RightTriangle rightSlope, ignore, true);

                if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK))
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfLeft);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER))
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfLeft);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (rightDisplaceSlope.HCathetusSign > 0)
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfRight);

                        slope = rightDisplaceSlope;
                        return CollisionFlags.SLOPE;
                    }

                    return CollisionFlags.NONE;
                }

                if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.BLOCK)
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfRight);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.TOP_LADDER)
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfRight);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (leftDisplaceSlope.HCathetusSign < 0)
                    {
                        placements?.AddRange(bottomPlacementsDisplacedHalfLeft);

                        slope = leftDisplaceSlope;
                        return CollisionFlags.SLOPE;
                    }

                    return CollisionFlags.NONE;
                }

                if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (placements != null)
                    {
                        placements.AddRange(bottomPlacementsDisplacedHalfLeft);
                        placements.AddRange(bottomPlacementsDisplacedHalfRight);
                    }

                    slope = leftDisplaceSlope;
                    return CollisionFlags.SLOPE;
                }
            }

            return CollisionFlags.NONE;
        }

        public MMXBox MoveContactFloor(MMXBox box, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactFloor(box, out RightTriangle slope, QUERY_MAX_DISTANCE, maskSize, ignore);
        }

        public MMXBox MoveContactFloor(MMXBox box, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactFloor(box, out RightTriangle slope, maxDistance, maskSize, ignore);
        }

        public MMXBox MoveContactFloor(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactFloor(box, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public MMXBox MoveContactFloor(MMXBox box, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_DOWN_VECTOR)
            {
                if (CanBlockTheMove(ComputedLandedState(box, out slope, maskSize, ignore)))
                    break;
            }

            return box;
        }

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactFloor(box, out newBox, out RightTriangle slope, maxDistance, maskSize, ignore);
        }

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactFloor(box, out newBox, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;
            newBox = MMXBox.EMPTY_BOX;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_DOWN_VECTOR)
            {
                if (CanBlockTheMove(ComputedLandedState(box, out slope, maskSize, ignore)))
                {
                    newBox = box;
                    return true;
                }
            }

            return false;
        }

        public bool TryMoveContactSlope(MMXBox box, out MMXBox newBox, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactSlope(box, out newBox, out RightTriangle slope, maxDistance, maskSize, ignore);
        }

        public bool TryMoveContactSlope(MMXBox box, out MMXBox newBox, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return TryMoveContactSlope(box, out newBox, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public bool TryMoveContactSlope(MMXBox box, out MMXBox newBox, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;
            newBox = MMXBox.EMPTY_BOX;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_DOWN_VECTOR)
            {
                if (ComputedLandedState(box, out slope, maskSize, ignore).HasFlag(CollisionFlags.SLOPE))
                {
                    newBox = box;
                    return true;
                }
            }

            return false;
        }

        public MMXBox AdjustOnTheFloor(MMXBox box, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return AdjustOnTheFloor(box, out RightTriangle slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public MMXBox AdjustOnTheFloor(MMXBox box, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return AdjustOnTheFloor(box, out RightTriangle slope, maxDistance, maskSize, ignore);
        }

        public MMXBox AdjustOnTheFloor(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return AdjustOnTheFloor(box, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public MMXBox AdjustOnTheFloor(MMXBox box, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (!CanBlockTheMove(ComputedLandedState(box, out slope, maskSize, ignore)))
                return box;

            slope = RightTriangle.EMPTY;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_UP_VECTOR)
            {
                if (!CanBlockTheMove(ComputedLandedState(box + STEP_UP_VECTOR, out RightTriangle slope2, maskSize, ignore)))
                    break;

                slope = slope2;
            }

            return box;
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveUntilIntersect(box, dir, null, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveUntilIntersect(box, dir, null, maxDistance, maskSize, ignore);
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveUntilIntersect(box, dir, placements, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, List<CollisionPlacement> placements, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            Vector deltaDir = GetStepVector(dir);
            FixedSingle step = deltaDir.X == 0 ? deltaDir.Y.Abs : deltaDir.X.Abs;
            MMXBox lastBox = box;
            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += step, box += deltaDir)
            {
                if (CanBlockTheMove(GetCollisionFlags(box, placements, ignore, true)))
                    break;

                lastBox = box;
            }

            return box;
        }

        private static Vector GetStepVector(Vector dir)
        {
            if (dir.X == 0)
                return dir.Y > 0 ? STEP_DOWN_VECTOR : dir.Y < 0 ? STEP_UP_VECTOR : Vector.NULL_VECTOR;

            if (dir.Y == 0)
                return dir.X > 0 ? STEP_RIGHT_VECTOR : dir.X < 0 ? STEP_LEFT_VECTOR : Vector.NULL_VECTOR;

            FixedSingle x = dir.X;
            FixedSingle xm = x.Abs;
            FixedSingle y = dir.Y;

            return new Vector(x.Signal * STEP_SIZE, y / xm * STEP_SIZE);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, out slopeTriangle, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, placements, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, placements, out slopeTriangle, ignore, preciseCollisionCheck);
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
    }
}