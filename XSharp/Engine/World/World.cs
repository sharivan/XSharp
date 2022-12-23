﻿using System;
using System.Collections.Generic;
using SharpDX.Direct3D9;

using MMX.Geometry;
using MMX.Math;

using MMXBox = MMX.Geometry.Box;

using static MMX.Engine.Consts;

namespace MMX.Engine.World
{
    public class World : IDisposable
    {
        public const int TILEMAP_WIDTH = 32 * MAP_SIZE;
        public const int TILEMAP_HEIGHT = 32 * MAP_SIZE;

        public const float TILE_FRAC_SIZE = 1f / 64;// (float) TILE_SIZE / TILEMAP_WIDTH;

        public static readonly Vector TILE_SIZE_VECTOR = new(TILE_SIZE, TILE_SIZE);
        public static readonly Vector TILE_FRAC_SIZE_VECTOR = new(TILE_FRAC_SIZE, TILE_FRAC_SIZE);
        private int backgroundSceneRowCount;
        private int backgroundSceneColCount;

        private readonly List<Tile> tileList;
        private readonly List<Map> mapList;
        private readonly List<Block> blockList;
        private readonly List<Scene> sceneList;

        private Scene[,] scenes;
        private Scene[,] backgroundScenes;
        internal Texture foregroundTilemap;
        internal Texture backgroundTilemap;

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount) :
            this(engine, sceneRowCount, sceneColCount, sceneRowCount, sceneColCount)
        {
        }

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount, int backgroundSceneRowCount, int backgroundSceneColCount)
        {
            this.Engine = engine;

            this.SceneRowCount = sceneRowCount;
            this.SceneColCount = sceneColCount;
            this.backgroundSceneRowCount = backgroundSceneRowCount;
            this.backgroundSceneColCount = backgroundSceneColCount;

            Screen = new Screen(this, SCREEN_WIDTH, SCREEN_HEIGHT);

            tileList = new List<Tile>();
            mapList = new List<Map>();
            blockList = new List<Block>();
            sceneList = new List<Scene>();

            scenes = new Scene[sceneRowCount, sceneColCount];
            backgroundScenes = new Scene[backgroundSceneRowCount, backgroundSceneColCount];
        }

        public GameEngine Engine { get; }

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

        public int SceneRowCount { get;
            private set;
        }

        public int SceneColCount { get;
            private set;
        }

        public MMXBox BoundingBox => new(0, 0, Width, Height);

        public Vector Size => new(Width, Height);

        public Vector LayoutSize => new(SceneRowCount, SceneColCount);

        public Vector LayoutBackgroundtSize => new(backgroundSceneRowCount, backgroundSceneColCount);

        public Screen Screen { get; }

        public Texture ForegroundPalette { get;
            set; }

        public Texture BackgroundPalette { get;
            set; }

        public Tile AddTile()
        {
            var result = new Tile(this, tileList.Count);
            tileList.Add(result);
            return result;
        }

        public Tile AddTile(byte[] source)
        {
            var result = new Tile(this, tileList.Count, source);
            tileList.Add(result);
            return result;
        }

        public Map AddMap(CollisionData collisionData = CollisionData.NONE)
        {
            var result = new Map(this, mapList.Count, collisionData);
            mapList.Add(result);
            return result;
        }

        public Block AddBlock()
        {
            var result = new Block(this, blockList.Count);
            blockList.Add(result);
            return result;
        }

        public Scene AddScene()
        {
            var result = new Scene(this, sceneList.Count);

            sceneList.Add(result);
            return result;
        }

        public Scene AddScene(int row, int col, bool background = false)
        {
            Scene result = AddScene();

            if (background)
                backgroundScenes[row, col] = result;
            else
                scenes[row, col] = result;

            return result;
        }

        public Scene AddScene(Vector pos, bool background = false)
        {
            Scene result = AddScene();

            Cell cell = GetSceneCellFromPos(pos);

            if (background)
                backgroundScenes[cell.Row, cell.Col] = result;
            else
                scenes[cell.Row, cell.Col] = result;

            return result;
        }

        public Map AddMap(Vector pos, CollisionData collisionData = CollisionData.NONE, bool background = false)
        {
            Map result = AddMap(collisionData);
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

        public Tile GetTileByID(int id) => id < 0 || id >= tileList.Count ? null : tileList[id];

        public Map GetMapByID(int id) => id < 0 || id >= mapList.Count ? null : mapList[id];

        public Block GetBlockByID(int id) => id < 0 || id >= blockList.Count ? null : blockList[id];

        public Scene GetSceneByID(int id) => id < 0 || id >= sceneList.Count ? null : sceneList[id];

        public void RemoveTile(Tile tile)
        {
            tileList.Remove(tile);

            foreach (Map map in mapList)
                map.RemoveTile(tile);
        }

        public void RemoveMap(Map map)
        {
            mapList.Remove(map);

            foreach (Block block in blockList)
                block.RemoveMap(map);
        }

        public void RemoveBlock(Block block)
        {
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
            blockList.Clear();
            mapList.Clear();

            foreach (Scene scene in sceneList)
                scene.Dispose();

            sceneList.Clear();
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

        public void Dispose() => Clear();

        public void RenderBackground(bool upLayer)
        {
            Checkpoint checkpoint = Engine.CurrentCheckpoint;
            if (checkpoint == null)
                return;

            Device device = Device;

            MMXBox screenBox = Screen.BoudingBox;
            Vector screenLT = Screen.LeftTop;

            Vector backgroundPos = checkpoint.BackgroundPos;
            MMXBox checkpointBox = checkpoint.BoundingBox;
            Vector checkpointPos = checkpointBox.LeftTop;
            FixedSingle factorX = (float) BackgroundWidth / Width;
            FixedSingle factorY = (float) BackgroundHeight / Height;

            Vector delta = checkpointPos - backgroundPos;
            var backgroundBox = new MMXBox(backgroundPos.X, backgroundPos.Y, checkpointBox.Width, checkpointBox.Height);
            var screenDelta = new Vector(factorX * (screenLT.X - checkpointPos.X), checkpoint.Scroll != 0 ? factorY * (screenLT.Y - checkpointPos.Y) : 0);
            backgroundBox &= screenBox - delta - screenDelta;

            Cell start = GetSceneCellFromPos(backgroundBox.LeftTop);
            Cell end = GetSceneCellFromPos(backgroundBox.RightBottom);

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0 || col >= backgroundSceneColCount)
                    continue;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= backgroundSceneRowCount)
                        continue;

                    var sceneLT = new Vector(col * SCENE_SIZE, row * SCENE_SIZE);
                    Scene scene = backgroundScenes[row, col];
                    if (scene != null)
                    {
                        MMXBox sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                        Engine.RenderVertexBuffer(upLayer ? scene.upLayerVB : scene.downLayerVB, GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, foregroundTilemap, ForegroundPalette, sceneBox + delta + screenDelta);
                    }
                }
            }
        }

        public void RenderForeground(bool upLayer)
        {
            MMXBox screenBox = Screen.BoudingBox;
            Vector screenLT = Screen.LeftTop;
            Vector screenRB = Screen.RightBottom;

            Cell start = GetSceneCellFromPos(screenLT);
            Cell end = GetSceneCellFromPos(screenRB);

            Device device = Device;

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0 || col >= SceneColCount)
                    continue;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= SceneRowCount)
                        continue;

                    var sceneLT = new Vector(col * SCENE_SIZE, row * SCENE_SIZE);
                    Scene scene = scenes[row, col];
                    if (scene != null)
                        Engine.RenderVertexBuffer(upLayer ? scene.upLayerVB : scene.downLayerVB, GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, foregroundTilemap, ForegroundPalette, new MMXBox(sceneLT.X, sceneLT.Y, SCENE_SIZE, SCENE_SIZE));
                }
            }
        }

        public void OnFrame() => Screen.OnFrame();

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

        public static MMXBox GetTileBoundingBox(int row, int col) => GetTileBoundingBox(new Cell(row, col));

        public static MMXBox GetMapBoundingBox(int row, int col) => GetMapBoundingBox(new Cell(row, col));

        public static MMXBox GetBlockBoundingBox(int row, int col) => GetBlockBoundingBox(new Cell(row, col));

        public static MMXBox GetSceneBoundingBox(int row, int col) => GetSceneBoundingBox(new Cell(row, col));

        public static MMXBox GetTileBoundingBox(Cell pos) => new(pos.Col * TILE_SIZE, pos.Row * TILE_SIZE, TILE_SIZE, TILE_SIZE);

        public static MMXBox GetMapBoundingBox(Cell pos) => new(pos.Col * MAP_SIZE, pos.Row * MAP_SIZE, MAP_SIZE, MAP_SIZE);

        public static MMXBox GetBlockBoundingBox(Cell pos) => new(pos.Col * BLOCK_SIZE, pos.Row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
        public static MMXBox GetSceneBoundingBox(Cell pos) => new(pos.Col * SCENE_SIZE, pos.Row * SCENE_SIZE, SCENE_SIZE, SCENE_SIZE);

        public static MMXBox GetTileBoundingBoxFromPos(Vector pos) => GetTileBoundingBox(GetTileCellFromPos(pos));

        public static MMXBox GetMapBoundingBoxFromPos(Vector pos) => GetMapBoundingBox(GetMapCellFromPos(pos));

        public static MMXBox GetBlockBoundingBoxFromPos(Vector pos) => GetBlockBoundingBox(GetBlockCellFromPos(pos));

        public static MMXBox GetSceneBoundingBoxFromPos(Vector pos) => GetSceneBoundingBox(GetSceneCellFromPos(pos));

        public static bool IsSolidBlock(CollisionData collisionData) => collisionData switch
        {
            CollisionData.LAVA => true,
            CollisionData.UNKNOW34 => true,
            CollisionData.UNKNOW35 => true,
            CollisionData.UNCLIMBABLE_SOLID => true,
            CollisionData.LEFT_TREADMILL => true,
            CollisionData.RIGHT_TREADMILL => true,
            CollisionData.UP_SLOPE_BASE => true,
            CollisionData.DOWN_SLOPE_BASE => true,
            CollisionData.SOLID => true,
            CollisionData.BREAKABLE => true,
            CollisionData.NON_LETHAL_SPIKE => true,
            CollisionData.LETHAL_SPIKE => true,
            CollisionData.ICE => true,
            _ => false,
        };

        public static bool IsSlope(CollisionData collisionData) => collisionData is >= CollisionData.SLOPE_16_8 and <= CollisionData.SLOPE_0_4 or
                >= CollisionData.LEFT_TREADMILL_SLOPE_16_12 and <= CollisionData.RIGHT_TREADMILL_SLOPE_0_4 or
                >= CollisionData.ICE_SLOPE_16_12 and <= CollisionData.ICE_SLOPE_0_4;

        internal static RightTriangle MakeSlopeTriangle(int left, int right) => left < right
                ? new RightTriangle(new Vector(0, right), MAP_SIZE, left - right)
                : new RightTriangle(new Vector(MAP_SIZE, left), -MAP_SIZE, right - left);

        internal static RightTriangle MakeSlopeTriangle(CollisionData collisionData) => collisionData switch
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
            CollisionData.LEFT_TREADMILL_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
            CollisionData.LEFT_TREADMILL_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
            CollisionData.LEFT_TREADMILL_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
            CollisionData.LEFT_TREADMILL_SLOPE_4_0 => MakeSlopeTriangle(4, 0),
            CollisionData.RIGHT_TREADMILL_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
            CollisionData.RIGHT_TREADMILL_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
            CollisionData.RIGHT_TREADMILL_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
            CollisionData.RIGHT_TREADMILL_SLOPE_0_4 => MakeSlopeTriangle(0, 4),
            CollisionData.ICE_SLOPE_12_16 => MakeSlopeTriangle(12, 16),
            CollisionData.ICE_SLOPE_8_12 => MakeSlopeTriangle(8, 12),
            CollisionData.ICE_SLOPE_4_8 => MakeSlopeTriangle(4, 8),
            CollisionData.ICE_SLOPE_0_4 => MakeSlopeTriangle(0, 4),
            CollisionData.ICE_SLOPE_4_0 => MakeSlopeTriangle(4, 0),
            CollisionData.ICE_SLOPE_8_4 => MakeSlopeTriangle(8, 4),
            CollisionData.ICE_SLOPE_12_8 => MakeSlopeTriangle(12, 8),
            CollisionData.ICE_SLOPE_16_12 => MakeSlopeTriangle(16, 12),
            _ => RightTriangle.EMPTY,
        };

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER) => GetCollisionFlags(collisionBox, null, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck, side);

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER) => GetCollisionFlags(collisionBox, placements, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck, side);

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER) => GetCollisionFlags(collisionBox, null, out slopeTriangle, ignore, preciseCollisionCheck, side);

        private bool HasIntersection(MMXBox box1, MMXBox box2, CollisionSide side) =>
            /*if (side.HasFlag(CollisionSide.FLOOR) && (box1 & box2.TopSegment).Length > 0)
return true;

if (side.HasFlag(CollisionSide.CEIL) && (box1 & box2.BottomSegment).Length > 0)
return true;

if (side.HasFlag(CollisionSide.LEFT_WALL) && (box1 & box2.RightSegment).Length > 0)
return true;

if (side.HasFlag(CollisionSide.RIGHT_WALL) && (box1 & box2.LeftSegment).Length > 0)
return true;

if (side.HasFlag(CollisionSide.INNER) && (box1 & box2).Area > 0)
return true;

return false;*/

            (box1 & box2).Area > 0;

        private bool HasIntersection(MMXBox box, RightTriangle slope, CollisionSide side) =>
            /*if (side.HasFlag(CollisionSide.FLOOR) && (box & slope.HypotenuseLine).Length > 0)
return true;

if (side.HasFlag(CollisionSide.CEIL) && (box & slope.HCathetusLine).Length > 0)
return true;

if (side.HasFlag(CollisionSide.LEFT_WALL) && (box & (slope.HCathetusSign > 0 ? slope.VCathetusLine : slope.HypotenuseLine)).Length > 0)
return true;

if (side.HasFlag(CollisionSide.RIGHT_WALL) && (box & (slope.HCathetusSign < 0 ? slope.VCathetusLine : slope.HypotenuseLine)).Length > 0)
return true;

if (side.HasFlag(CollisionSide.INNER) && slope.HasIntersectionWith(box, true))
return true;

return false;*/

            slope.HasIntersectionWith(box, true);

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
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

                        if (collisionData == CollisionData.NONE || (mapBox & collisionBox).Area == 0)
                            continue;

                        if (!ignore.HasFlag(CollisionFlags.BLOCK) && IsSolidBlock(collisionData) && HasIntersection(collisionBox, mapBox, side))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                            result |= CollisionFlags.BLOCK;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.LADDER) && collisionData == CollisionData.LADDER && HasIntersection(collisionBox, mapBox, side))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.LADDER, row, col, map));

                            result |= CollisionFlags.LADDER;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.TOP_LADDER) && collisionData == CollisionData.TOP_LADDER && HasIntersection(collisionBox, mapBox, side))
                        {
                            placements?.Add(new CollisionPlacement(this, CollisionFlags.TOP_LADDER, row, col, map));

                            result |= CollisionFlags.TOP_LADDER;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.SLOPE) && IsSlope(collisionData))
                        {
                            RightTriangle st = MakeSlopeTriangle(collisionData) + v;
                            if (preciseCollisionCheck)
                            {
                                if (HasIntersection(collisionBox, st, side))
                                {
                                    placements?.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                                    slopeTriangle = st;
                                    result |= CollisionFlags.SLOPE;
                                }
                            }
                            else if (HasIntersection(collisionBox, mapBox, side))
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

        public CollisionFlags ComputedLandedState(MMXBox box, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE) => ComputedLandedState(box, placements, out RightTriangle slope, MASK_SIZE, ignore);

        public CollisionFlags ComputedLandedState(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE) => ComputedLandedState(box, null, out slope, MASK_SIZE, ignore);

        public CollisionFlags ComputedLandedState(MMXBox box, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => ComputedLandedState(box, null, out slope, maskSize, ignore);

        private static readonly List<CollisionPlacement> bottomPlacementsDisplacedHalfLeft = new();
        private static readonly List<CollisionPlacement> bottomPlacementsDisplacedHalfRight = new();

        /*public CollisionFlags ComputedLandedState(Box box, List<CollisionPlacement> placements, out RightTriangle slope, Fixed maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;

            Box bottomMask = box.ClipTop(box.Height - maskSize);
            Box bottomMaskDisplaced = bottomMask + maskSize * Vector.DOWN_VECTOR;

            Box bottomMaskDisplacedHalfLeft = bottomMaskDisplaced.HalfLeft();
            Box bottomMaskDisplacedHalfRight = bottomMaskDisplaced.HalfRight();

            if (placements != null)
            {
                bottomPlacementsDisplacedHalfLeft.Clear();
                bottomPlacementsDisplacedHalfRight.Clear();
            }

            CollisionFlags bottomLeftDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfLeft, bottomPlacementsDisplacedHalfLeft, out RightTriangle leftDisplaceSlope, ignore, true, CollisionSide.INNER);
            CollisionFlags bottomRightDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfRight, bottomPlacementsDisplacedHalfRight, out RightTriangle rightDisplaceSlope, ignore, true, CollisionSide.INNER);

            if (bottomLeftDisplacedCollisionFlags == CollisionFlags.NONE && bottomRightDisplacedCollisionFlags == CollisionFlags.NONE)
                return CollisionFlags.NONE;

            if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (bottomLeftDisplacedCollisionFlags == CollisionFlags.NONE && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    if (placements != null)
                        placements.AddRange(bottomPlacementsDisplacedHalfRight);

                    return bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && bottomRightDisplacedCollisionFlags == CollisionFlags.NONE)
                {
                    if (placements != null)
                        placements.AddRange(bottomPlacementsDisplacedHalfLeft);

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER; ;
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
                Box bottomMaskHalfLeft = bottomMask.HalfLeft();
                Box bottomMaskHalfRight = bottomMask.HalfRight();

                CollisionFlags bottomLeftCollisionFlags = GetCollisionFlags(bottomMaskHalfLeft, out RightTriangle leftSlope, ignore, true, CollisionSide.INNER);
                CollisionFlags bottomRightCollisionFlags = GetCollisionFlags(bottomMaskHalfRight, out RightTriangle rightSlope, ignore, true, CollisionSide.INNER);

                if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK))
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfLeft);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER))
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfLeft);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (rightDisplaceSlope.HCathetusSign > 0)
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfRight);

                        slope = rightDisplaceSlope;
                        return CollisionFlags.SLOPE;
                    }

                    return CollisionFlags.NONE;
                }

                if (bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
                {
                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.BLOCK)
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfRight);

                        return CollisionFlags.BLOCK;
                    }

                    if (bottomRightDisplacedCollisionFlags == CollisionFlags.TOP_LADDER)
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfRight);

                        return CollisionFlags.TOP_LADDER;
                    }

                    if (leftDisplaceSlope.HCathetusSign < 0)
                    {
                        if (placements != null)
                            placements.AddRange(bottomPlacementsDisplacedHalfLeft);

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
        }*/

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

            CollisionFlags bottomLeftDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfLeft, bottomPlacementsDisplacedHalfLeft, out RightTriangle leftDisplaceSlope, ignore, true, CollisionSide.INNER);
            CollisionFlags bottomRightDisplacedCollisionFlags = GetCollisionFlags(bottomMaskDisplacedHalfRight, bottomPlacementsDisplacedHalfRight, out RightTriangle rightDisplaceSlope, ignore, true, CollisionSide.INNER);

            if (bottomLeftDisplacedCollisionFlags == CollisionFlags.NONE && bottomRightDisplacedCollisionFlags == CollisionFlags.NONE)
                return CollisionFlags.NONE;

            if (!bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE) && !bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (bottomLeftDisplacedCollisionFlags == CollisionFlags.NONE && (bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)))
                {
                    placements?.AddRange(bottomPlacementsDisplacedHalfRight);

                    return bottomRightDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER;
                }

                if ((bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) || bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.TOP_LADDER)) && bottomRightDisplacedCollisionFlags == CollisionFlags.NONE)
                {
                    placements?.AddRange(bottomPlacementsDisplacedHalfLeft);

                    return bottomLeftDisplacedCollisionFlags.HasFlag(CollisionFlags.BLOCK) ? CollisionFlags.BLOCK : CollisionFlags.TOP_LADDER; ;
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

                CollisionFlags bottomLeftCollisionFlags = GetCollisionFlags(bottomMaskHalfLeft, out RightTriangle leftSlope, ignore, true, CollisionSide.INNER);
                CollisionFlags bottomRightCollisionFlags = GetCollisionFlags(bottomMaskHalfRight, out RightTriangle rightSlope, ignore, true, CollisionSide.INNER);

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

        public MMXBox MoveContactFloor(MMXBox box, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => MoveContactFloor(box, out RightTriangle slope, QUERY_MAX_DISTANCE, maskSize, ignore);

        public MMXBox MoveContactFloor(MMXBox box, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => MoveContactFloor(box, out RightTriangle slope, maxDistance, maskSize, ignore);

        public MMXBox MoveContactFloor(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE) => MoveContactFloor(box, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);

        public MMXBox MoveContactFloor(MMXBox box, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_DOWN_VECTOR)
            {
                if (ComputedLandedState(box, out slope, maskSize, ignore) != CollisionFlags.NONE)
                    break;
            }

            return box;
        }

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => TryMoveContactFloor(box, out newBox, out RightTriangle slope, maxDistance, maskSize, ignore);

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE) => TryMoveContactFloor(box, out newBox, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);

        public bool TryMoveContactFloor(MMXBox box, out MMXBox newBox, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;
            newBox = MMXBox.EMPTY_BOX;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_DOWN_VECTOR)
            {
                if (ComputedLandedState(box, out slope, maskSize, ignore) != CollisionFlags.NONE)
                {
                    newBox = box;
                    return true;
                }
            }

            return false;
        }

        public bool TryMoveContactSlope(MMXBox box, out MMXBox newBox, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => TryMoveContactSlope(box, out newBox, out RightTriangle slope, maxDistance, maskSize, ignore);

        public bool TryMoveContactSlope(MMXBox box, out MMXBox newBox, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE) => TryMoveContactSlope(box, out newBox, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);

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

        public MMXBox AdjustOnTheFloor(MMXBox box, CollisionFlags ignore = CollisionFlags.NONE) => AdjustOnTheFloor(box, out RightTriangle slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);

        public MMXBox AdjustOnTheFloor(MMXBox box, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => AdjustOnTheFloor(box, out RightTriangle slope, maxDistance, maskSize, ignore);

        public MMXBox AdjustOnTheFloor(MMXBox box, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE) => AdjustOnTheFloor(box, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);

        public MMXBox AdjustOnTheFloor(MMXBox box, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            if (ComputedLandedState(box, out slope, maskSize, ignore) == CollisionFlags.NONE)
                return box;

            slope = RightTriangle.EMPTY;

            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += STEP_SIZE, box += STEP_UP_VECTOR)
            {
                if (ComputedLandedState(box + STEP_UP_VECTOR, out RightTriangle slope2, maskSize, ignore) == CollisionFlags.NONE)
                    break;

                slope = slope2;
            }

            return box;
        }

        /*public Box MoveContactSolid(Box box, Vector dir, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactSolid(box, dir, out RightTriangle slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public Box MoveContactSolid(Box box, Vector dir, Fixed maxDistance, Fixed maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactSolid(box, dir, out RightTriangle slope, maxDistance, maskSize, ignore);
        }
        public Box MoveContactSolid(Box box, Vector dir, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return MoveContactSolid(box, dir, out slope, QUERY_MAX_DISTANCE, MASK_SIZE, ignore);
        }

        public Box MoveContactSolid(Box box, Vector dir, out RightTriangle slope, Fixed maxDistance, Fixed maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            slope = RightTriangle.EMPTY;

            Vector deltaDir = GetStepVector(dir);
            Vector dx = deltaDir.XVector;
            Vector dy = deltaDir.YVector;
            Fixed step = deltaDir.X == 0 ? deltaDir.Y.Abs : deltaDir.X.Abs;
            CollisionSide sideX = dir.X > 0 ? CollisionSide.RIGHT_WALL : dir.X < 0 ? CollisionSide.LEFT_WALL : CollisionSide.NONE;
            for (Fixed distance = Fixed.ZERO; distance < maxDistance; distance += step, box += deltaDir)
            {
                if (GetCollisionFlags(box + dx, ignore, true, sideX) != CollisionFlags.NONE ||
                    (dy.Y > 0 ? ComputedLandedState(box, out slope, maskSize, ignore) : GetCollisionFlags(box + dy, ignore, true, CollisionSide.CEIL)) != CollisionFlags.NONE)
                    break;
            }

            return box;
        }*/

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER) => MoveUntilIntersect(box, dir, null, QUERY_MAX_DISTANCE, MASK_SIZE, ignore, side);

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER) => MoveUntilIntersect(box, dir, null, maxDistance, maskSize, ignore, side);

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER) => MoveUntilIntersect(box, dir, placements, QUERY_MAX_DISTANCE, MASK_SIZE, ignore, side);

        private CollisionSide GetCollisionSide(Vector dir)
        {
            CollisionSide result = CollisionSide.NONE;
            if (dir.X > 0)
                result |= CollisionSide.RIGHT_WALL;
            else if (dir.X < 0)
                result |= CollisionSide.LEFT_WALL;

            if (dir.Y > 0)
                result |= CollisionSide.FLOOR;
            else if (dir.Y < 0)
                result |= CollisionSide.CEIL;

            return result;
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, List<CollisionPlacement> placements, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER)
        {
            Vector deltaDir = GetStepVector(dir);
            FixedSingle step = deltaDir.X == 0 ? deltaDir.Y.Abs : deltaDir.X.Abs; // FixedSingle.Max(deltaDir.X.Abs, deltaDir.Y.Abs);
            MMXBox lastBox = box;
            for (FixedSingle distance = FixedSingle.ZERO; distance < maxDistance; distance += step, box += deltaDir)
            {
                if (GetCollisionFlags(box, placements, ignore, true, side) != CollisionFlags.NONE)
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
            //FixedSingle ym = y.Abs;

            //if (xm > ym)
            //    return new Vector(x / ym * STEP_SIZE, y.Signal * STEP_SIZE);

            return new Vector(x.Signal * STEP_SIZE, y / xm * STEP_SIZE);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true) => GetCollisionFlags(collisionBox + dir, ignore, preciseCollisionCheck, GetCollisionSide(dir));

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true) => GetCollisionFlags(collisionBox + dir, out slopeTriangle, ignore, preciseCollisionCheck, GetCollisionSide(dir));

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true) => GetCollisionFlags(collisionBox + dir, placements, ignore, preciseCollisionCheck, GetCollisionSide(dir));

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true) => GetCollisionFlags(collisionBox + dir, placements, out slopeTriangle, ignore, preciseCollisionCheck, GetCollisionSide(dir));

        internal void Tessellate()
        {
            foreach (Scene scene in sceneList)
                scene.Tessellate();
        }
    }
}