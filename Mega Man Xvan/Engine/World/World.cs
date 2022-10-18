﻿using System;
using System.Collections.Generic;

using SharpDX;
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

        public static readonly Vector TILE_SIZE_VECTOR = new Vector(TILE_SIZE, TILE_SIZE);
        public static readonly Vector TILE_FRAC_SIZE_VECTOR = new Vector(TILE_FRAC_SIZE, TILE_FRAC_SIZE);

        private GameEngine engine;

        private Screen screen;

        private int sceneRowCount;
        private int sceneColCount;
        private int backgroundSceneRowCount;
        private int backgroundSceneColCount;

        private List<Tile> tileList;
        private List<Map> mapList;
        private List<Block> blockList;
        private List<Scene> sceneList;

        private Scene[,] scenes;
        private Scene[,] backgroundScenes;

        private Texture foregroundPalette;
        private Texture backgroundPalette;

        internal Texture foregroundTilemap;
        internal Texture backgroundTilemap;

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount) :
            this(engine, sceneRowCount, sceneColCount, sceneRowCount, sceneColCount)
        {
        }

        internal World(GameEngine engine, int sceneRowCount, int sceneColCount, int backgroundSceneRowCount, int backgroundSceneColCount)
        {
            this.engine = engine;

            this.sceneRowCount = sceneRowCount;
            this.sceneColCount = sceneColCount;
            this.backgroundSceneRowCount = backgroundSceneRowCount;
            this.backgroundSceneColCount = backgroundSceneColCount;

            screen = new Screen(this, SCREEN_WIDTH, SCREEN_HEIGHT);

            tileList = new List<Tile>();
            mapList = new List<Map>();
            blockList = new List<Block>();
            sceneList = new List<Scene>();

            scenes = new Scene[sceneRowCount, sceneColCount];
            backgroundScenes = new Scene[backgroundSceneRowCount, backgroundSceneColCount];
        }

        public GameEngine Engine
        {
            get
            {
                return engine;
            }
        }

        public Device Device
        {
            get
            {
                return engine.Device;
            }
        }

        public int Width
        {
            get
            {
                return sceneColCount * SCENE_SIZE;
            }
        }

        public int Height
        {
            get
            {
                return sceneRowCount * SCENE_SIZE;
            }
        }

        public int BackgroundWidth
        {
            get
            {
                return backgroundSceneColCount * SCENE_SIZE;
            }
        }

        public int BackgroundHeight
        {
            get
            {
                return backgroundSceneRowCount * SCENE_SIZE;
            }
        }

        public int TileRowCount
        {
            get
            {
                return Height / TILE_SIZE;
            }
        }

        public int TileColCount
        {
            get
            {
                return Width / TILE_SIZE;
            }
        }

        public int MapRowCount
        {
            get
            {
                return Height / MAP_SIZE;
            }
        }

        public int MapColCount
        {
            get
            {
                return Width / MAP_SIZE;
            }
        }

        public int BlockRowCount
        {
            get
            {
                return Height / BLOCK_SIZE;
            }
        }

        public int BlockColCount
        {
            get
            {
                return Width / BLOCK_SIZE;
            }
        }

        public int SceneRowCount
        {
            get
            {
                return sceneRowCount;
            }
        }

        public int SceneColCount
        {
            get
            {
                return sceneColCount;
            }
        }

        public MMXBox BoundingBox
        {
            get
            {
                return new MMXBox(0, 0, Width, Height);
            }
        }

        public Vector Size
        {
            get
            {
                return new Vector(Width, Height);
            }
        }

        public Vector LayoutSize
        {
            get
            {
                return new Vector(sceneRowCount, sceneColCount);
            }
        }

        public Vector LayoutBackgroundtSize
        {
            get
            {
                return new Vector(backgroundSceneRowCount, backgroundSceneColCount);
            }
        }

        public Screen Screen
        {
            get
            {
                return screen;
            }
        }

        public Texture ForegroundPalette
        {
            get
            {
                return foregroundPalette;
            }

            set
            {
                foregroundPalette = value;
            }
        }

        public Texture BackgroundPalette
        {
            get
            {
                return backgroundPalette;
            }

            set
            {
                backgroundPalette = value;
            }
        }

        public Tile AddTile()
        {
            Tile result = new Tile(this, tileList.Count);
            tileList.Add(result);
            return result;
        }

        public Tile AddTile(byte[] source)
        {
            Tile result = new Tile(this, tileList.Count, source);
            tileList.Add(result);
            return result;
        }

        public Map AddMap(CollisionData collisionData = CollisionData.NONE)
        {
            Map result = new Map(this, mapList.Count, collisionData);
            mapList.Add(result);
            return result;
        }

        public Block AddBlock()
        {
            Block result = new Block(this, blockList.Count);
            blockList.Add(result);
            return result;
        }

        public Scene AddScene()
        {
            Scene result = new Scene(this, sceneList.Count);

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
            if (scene == null)
                scene = AddScene(pos, background);

            scene.SetMap(pos - new Vector(cell.Col * SCENE_SIZE, cell.Row * SCENE_SIZE), map);
        }

        public void SetBlock(Vector pos, Block block, bool background = false)
        {
            Cell cell = GetSceneCellFromPos(pos);
            Scene scene = background ? backgroundScenes[cell.Row, cell.Col] : scenes[cell.Row, cell.Col];
            if (scene == null)
                scene = AddScene(pos, background);

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

        public Tile GetTileByID(int id)
        {
            if (id < 0 || id >= tileList.Count)
                return null;

            return tileList[id];
        }

        public Map GetMapByID(int id)
        {
            if (id < 0 || id >= mapList.Count)
                return null;

            return mapList[id];
        }

        public Block GetBlockByID(int id)
        {
            if (id < 0 || id >= blockList.Count)
                return null;

            return blockList[id];
        }

        public Scene GetSceneByID(int id)
        {
            if (id < 0 || id >= sceneList.Count)
                return null;

            return sceneList[id];
        }

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

            for (int col = 0; col < sceneColCount; col++)
                for (int row = 0; row < sceneRowCount; row++)
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
            if (rowCount == sceneRowCount && colCount == sceneColCount)
                return;

            Scene[,] newScenes = new Scene[rowCount, colCount];

            int minRows = System.Math.Min(rowCount, sceneRowCount);
            int minCols = System.Math.Min(colCount, sceneColCount);

            for (int col = 0; col < minCols; col++)
                for (int row = 0; row < minRows; row++)
                    newScenes[row, col] = scenes[row, col];

            sceneRowCount = rowCount;
            sceneColCount = colCount;

            scenes = newScenes;
        }

        public void ResizeBackground(int rowCount, int colCount)
        {
            if (rowCount == backgroundSceneRowCount && colCount == backgroundSceneColCount)
                return;

            Scene[,] newScenes = new Scene[rowCount, colCount];

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
            for (int col = 0; col < sceneColCount; col++)
                for (int row = 0; row < sceneRowCount; row++)
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

        public void Dispose()
        {
            Clear();
        }

        public void RenderBackground(bool upLayer)
        {
            Checkpoint checkpoint = engine.CurrentCheckpoint;
            if (checkpoint == null)
                return;

            Device device = Device;

            MMXBox screenBox = screen.BoudingBox;
            Vector screenLT = screen.LeftTop;

            Vector backgroundPos = checkpoint.BackgroundPos;
            MMXBox checkpointBox = checkpoint.BoundingBox;
            Vector checkpointPos = checkpointBox.LeftTop;
            FixedSingle factorX = (float) BackgroundWidth / Width;
            FixedSingle factorY = (float) BackgroundHeight / Height;

            Vector delta = checkpointPos - backgroundPos;
            MMXBox backgroundBox = new MMXBox(backgroundPos.X, backgroundPos.Y, checkpointBox.Width, checkpointBox.Height);
            Vector screenDelta = new Vector(factorX * (screenLT.X - checkpointPos.X), checkpoint.Scroll != 0 ? factorY * (screenLT.Y - checkpointPos.Y) : 0);
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

                    Vector sceneLT = new Vector(col * SCENE_SIZE, row * SCENE_SIZE);
                    Scene scene = backgroundScenes[row, col];
                    if (scene != null)
                    {
                        MMXBox sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                        engine.RenderVertexBuffer(upLayer ? scene.upLayerVB : scene.downLayerVB, GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, foregroundTilemap, foregroundPalette, sceneBox + delta + screenDelta);
                    }
                }
            }
        }

        public void RenderForeground(bool upLayer)
        {
            MMXBox screenBox = screen.BoudingBox;
            Vector screenLT = screen.LeftTop;
            Vector screenRB = screen.RightBottom;

            Cell start = GetSceneCellFromPos(screenLT);
            Cell end = GetSceneCellFromPos(screenRB);

            Device device = Device;

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0 || col >= sceneColCount)
                    continue;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= sceneRowCount)
                        continue;

                    Vector sceneLT = new Vector(col * SCENE_SIZE, row * SCENE_SIZE);
                    Scene scene = scenes[row, col];
                    if (scene != null)
                        engine.RenderVertexBuffer(upLayer ? scene.upLayerVB : scene.downLayerVB, GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, foregroundTilemap, foregroundPalette, new MMXBox(sceneLT.X, sceneLT.Y, SCENE_SIZE, SCENE_SIZE));
                }
            }
        }

        public void OnFrame()
        {
            screen.OnFrame();
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

            if (row < 0 || col < 0 || row >= sceneRowCount || col >= sceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            if (scene == null)
                return null;

            return scene.GetTileFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Map GetMapFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= sceneRowCount || col >= sceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            if (scene == null)
                return null;

            return scene.GetMapFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Block GetBlockFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= sceneRowCount || col >= sceneColCount)
                return null;

            Scene scene = background ? backgroundScenes[row, col] : scenes[row, col];
            if (scene == null)
                return null;

            return scene.GetBlockFrom(pos - new Vector(col * SCENE_SIZE, row * SCENE_SIZE));
        }

        public Scene GetSceneFrom(Vector pos, bool background = false)
        {
            Cell tsp = GetSceneCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= sceneRowCount || col >= sceneColCount)
                return null;

            return background ? backgroundScenes[row, col] : scenes[row, col];
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
            return new MMXBox(pos.Col * TILE_SIZE, pos.Row * TILE_SIZE, TILE_SIZE, TILE_SIZE);
        }

        public static MMXBox GetMapBoundingBox(Cell pos)
        {
            return new MMXBox(pos.Col * MAP_SIZE, pos.Row * MAP_SIZE, MAP_SIZE, MAP_SIZE);
        }

        public static MMXBox GetBlockBoundingBox(Cell pos)
        {
            return new MMXBox(pos.Col * BLOCK_SIZE, pos.Row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
        }
        public static MMXBox GetSceneBoundingBox(Cell pos)
        {
            return new MMXBox(pos.Col * SCENE_SIZE, pos.Row * SCENE_SIZE, SCENE_SIZE, SCENE_SIZE);
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
            switch (collisionData)
            {
                case CollisionData.LAVA:
                    return true;

                case CollisionData.UNKNOW34:
                    return true;

                case CollisionData.UNKNOW35:
                    return true;

                case CollisionData.UNCLIMBABLE_SOLID:
                    return true;

                case CollisionData.LEFT_TREADMILL:
                    return true;

                case CollisionData.RIGHT_TREADMILL:
                    return true;

                case CollisionData.UP_SLOPE_BASE:
                    return true;

                case CollisionData.DOWN_SLOPE_BASE:
                    return true;

                case CollisionData.SOLID:
                    return true;

                case CollisionData.BREAKABLE:
                    return true;

                case CollisionData.NON_LETHAL_SPIKE:
                    return true;

                case CollisionData.LETHAL_SPIKE:
                    return true;

                case CollisionData.ICE:
                    return true;
            }

            return false;
        }

        public static bool IsSlope(CollisionData collisionData)
        {
            return collisionData >= CollisionData.SLOPE_16_8 && collisionData <= CollisionData.SLOPE_0_4 ||
                collisionData >= CollisionData.LEFT_TREADMILL_SLOPE_16_12 && collisionData <= CollisionData.RIGHT_TREADMILL_SLOPE_0_4 ||
                collisionData >= CollisionData.ICE_SLOPE_16_12 && collisionData <= CollisionData.ICE_SLOPE_0_4;
        }

        internal static RightTriangle MakeSlopeTriangle(int left, int right)
        {
            if (left < right)
                return new RightTriangle(new Vector(0, right), MAP_SIZE, left - right);

            return new RightTriangle(new Vector(MAP_SIZE, left), -MAP_SIZE, right - left);
        }

        internal static RightTriangle MakeSlopeTriangle(CollisionData collisionData)
        {
            switch (collisionData)
            {
                case CollisionData.SLOPE_16_8:
                    return MakeSlopeTriangle(16, 8);

                case CollisionData.SLOPE_8_0:
                    return MakeSlopeTriangle(8, 0);

                case CollisionData.SLOPE_8_16:
                    return MakeSlopeTriangle(8, 16);

                case CollisionData.SLOPE_0_8:
                    return MakeSlopeTriangle(0, 8);

                case CollisionData.SLOPE_16_12:
                    return MakeSlopeTriangle(16, 12);

                case CollisionData.SLOPE_12_8:
                    return MakeSlopeTriangle(12, 8);

                case CollisionData.SLOPE_8_4:
                    return MakeSlopeTriangle(8, 4);

                case CollisionData.SLOPE_4_0:
                    return MakeSlopeTriangle(4, 0);

                case CollisionData.SLOPE_12_16:
                    return MakeSlopeTriangle(12, 16);

                case CollisionData.SLOPE_8_12:
                    return MakeSlopeTriangle(8, 12);

                case CollisionData.SLOPE_4_8:
                    return MakeSlopeTriangle(4, 8);

                case CollisionData.SLOPE_0_4:
                    return MakeSlopeTriangle(0, 4);

                case CollisionData.LEFT_TREADMILL_SLOPE_16_12:
                    return MakeSlopeTriangle(16, 12);

                case CollisionData.LEFT_TREADMILL_SLOPE_12_8:
                    return MakeSlopeTriangle(12, 8);

                case CollisionData.LEFT_TREADMILL_SLOPE_8_4:
                    return MakeSlopeTriangle(8, 4);

                case CollisionData.LEFT_TREADMILL_SLOPE_4_0:
                    return MakeSlopeTriangle(4, 0);

                case CollisionData.RIGHT_TREADMILL_SLOPE_12_16:
                    return MakeSlopeTriangle(12, 16);

                case CollisionData.RIGHT_TREADMILL_SLOPE_8_12:
                    return MakeSlopeTriangle(8, 12);

                case CollisionData.RIGHT_TREADMILL_SLOPE_4_8:
                    return MakeSlopeTriangle(4, 8);

                case CollisionData.RIGHT_TREADMILL_SLOPE_0_4:
                    return MakeSlopeTriangle(0, 4);

                case CollisionData.ICE_SLOPE_12_16:
                    return MakeSlopeTriangle(12, 16);

                case CollisionData.ICE_SLOPE_8_12:
                    return MakeSlopeTriangle(8, 12);

                case CollisionData.ICE_SLOPE_4_8:
                    return MakeSlopeTriangle(4, 8);

                case CollisionData.ICE_SLOPE_0_4:
                    return MakeSlopeTriangle(0, 4);

                case CollisionData.ICE_SLOPE_4_0:
                    return MakeSlopeTriangle(4, 0);

                case CollisionData.ICE_SLOPE_8_4:
                    return MakeSlopeTriangle(8, 4);

                case CollisionData.ICE_SLOPE_12_8:
                    return MakeSlopeTriangle(12, 8);

                case CollisionData.ICE_SLOPE_16_12:
                    return MakeSlopeTriangle(16, 12);
            }

            return RightTriangle.EMPTY;
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return GetCollisionFlags(collisionBox, null, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return GetCollisionFlags(collisionBox, placements, out RightTriangle slopeTriangle, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return GetCollisionFlags(collisionBox, null, out slopeTriangle, ignore, preciseCollisionCheck, side);
        }

        private bool HasIntersection(MMXBox box1, MMXBox box2, CollisionSide side)
        {
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

            return (box1 & box2).Area > 0;
        }

        private bool HasIntersection(MMXBox box, RightTriangle slope, CollisionSide side)
        {
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

            return slope.HasIntersectionWith(box, true);
        }

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
                    Vector v = new Vector(col * MAP_SIZE, row * MAP_SIZE);
                    Map map = GetMapFrom(v);
                    if (map != null)
                    {
                        MMXBox mapBox = GetMapBoundingBox(row, col);
                        CollisionData collisionData = map.CollisionData;

                        if (collisionData == CollisionData.NONE || (mapBox & collisionBox).Area == 0)
                            continue;

                        if (!ignore.HasFlag(CollisionFlags.BLOCK) && IsSolidBlock(collisionData) && HasIntersection(collisionBox, mapBox, side))
                        {
                            if (placements != null)
                                placements.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                            result |= CollisionFlags.BLOCK;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.LADDER) && collisionData == CollisionData.LADDER && HasIntersection(collisionBox, mapBox, side))
                        {
                            if (placements != null)
                                placements.Add(new CollisionPlacement(this, CollisionFlags.LADDER, row, col, map));

                            result |= CollisionFlags.LADDER;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.TOP_LADDER) && collisionData == CollisionData.TOP_LADDER && HasIntersection(collisionBox, mapBox, side))
                        {
                            if (placements != null)
                                placements.Add(new CollisionPlacement(this, CollisionFlags.TOP_LADDER, row, col, map));

                            result |= CollisionFlags.TOP_LADDER;
                        }
                        else if (!ignore.HasFlag(CollisionFlags.SLOPE) && IsSlope(collisionData))
                        {
                            RightTriangle st = MakeSlopeTriangle(collisionData) + v;
                            if (preciseCollisionCheck)
                            {
                                if (HasIntersection(collisionBox, st, side))
                                {
                                    if (placements != null)
                                        placements.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

                                    slopeTriangle = st;
                                    result |= CollisionFlags.SLOPE;
                                }
                            }
                            else if (HasIntersection(collisionBox, mapBox, side))
                            {
                                if (placements != null)
                                    placements.Add(new CollisionPlacement(this, CollisionFlags.BLOCK, row, col, map));

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

        private static List<CollisionPlacement> bottomPlacementsDisplacedHalfLeft = new List<CollisionPlacement>();
        private static List<CollisionPlacement> bottomPlacementsDisplacedHalfRight = new List<CollisionPlacement>();

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
                MMXBox bottomMaskHalfLeft = bottomMask.HalfLeft();
                MMXBox bottomMaskHalfRight = bottomMask.HalfRight();

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
                if (ComputedLandedState(box, out slope, maskSize, ignore) != CollisionFlags.NONE)
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
                if (ComputedLandedState(box, out slope, maskSize, ignore) != CollisionFlags.NONE)
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

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER)
        {
            return MoveUntilIntersect(box, dir, null, QUERY_MAX_DISTANCE, MASK_SIZE, ignore, side);
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER)
        {
            return MoveUntilIntersect(box, dir, null, maxDistance, maskSize, ignore, side);
        }

        public MMXBox MoveUntilIntersect(MMXBox box, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, CollisionSide side = CollisionSide.INNER)
        {
            return MoveUntilIntersect(box, dir, placements, QUERY_MAX_DISTANCE, MASK_SIZE, ignore, side);
        }

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

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, ignore, preciseCollisionCheck, GetCollisionSide(dir));
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, out slopeTriangle, ignore, preciseCollisionCheck, GetCollisionSide(dir));
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, placements, ignore, preciseCollisionCheck, GetCollisionSide(dir));
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetCollisionFlags(collisionBox + dir, placements, out slopeTriangle, ignore, preciseCollisionCheck, GetCollisionSide(dir));
        }

        internal void Tessellate()
        {
            foreach (Scene scene in sceneList)
                scene.Tessellate();
        }
    }
}