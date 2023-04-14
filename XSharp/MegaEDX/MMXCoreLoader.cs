using System;
using System.Collections.Generic;
using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities.Triggers;
using XSharp.Engine.Graphics;
using XSharp.Engine.World;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.MegaEDX;

public class MMXCoreLoader : MMXCore
{
    public static Engine.BaseEngine Engine => XSharp.Engine.BaseEngine.Engine;

    public static Device Device => Engine.Device;

    public static World World => Engine.World;

    private Map[] maps;

    new public Vector BackgroundPos
    {
        get
        {
            var pos = base.BackgroundPos;
            return new Vector(pos.X, pos.Y);
        }
    }

    new public Vector CameraPos
    {
        get
        {
            var pos = base.CameraPos;
            return new Vector(pos.X, pos.Y);
        }
    }

    new public Vector CharacterPos
    {
        get
        {
            var pos = base.CharacterPos;
            return new Vector(pos.X, pos.Y);
        }
    }

    new public Vector MinCharacterPos
    {
        get
        {
            var pos = base.MinCharacterPos;
            return new Vector(pos.X, pos.Y);
        }
    }

    new public Vector MaxCharacterPos
    {
        get
        {
            var pos = base.MaxCharacterPos;
            return new Vector(pos.X, pos.Y);
        }
    }

    public MMXCoreLoader()
    {
        maps = new Map[0x400];
    }

    private static int Transform(int color, bool notTransparent)
    {
        return !notTransparent ? 0 : (int) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9) | 0xFF000000);
    }

    private Tile AddTile(uint tile, bool transparent = false, bool background = false)
    {
        uint image = (tile & 0x3FF) << 6;

        byte[] imageData = new byte[TILE_SIZE * TILE_SIZE * sizeof(byte)];
        bool notNull = false;
        using (var ms = new MemoryStream(imageData))
        {
            using var writter = new BinaryWriter(ms);
            for (int i = 0; i < TILE_SIZE * TILE_SIZE; i++, image++)
            {
                var v = vramCache[image];
                bool notTransparent = v != 0 || !transparent;
                notNull |= notTransparent;
                writter.Write(v);
            }
        }

        Tile wtile = background ? World.BackgroundLayout.AddTile(imageData) : World.ForegroundLayout.AddTile(imageData);
        return wtile;
    }

    private static void WriteTile(DataRectangle tilemapRect, byte[] data, int mapIndex, int tileRow, int tileCol, int subPalette, bool flipped, bool mirrored)
    {
        int mapRow = mapIndex / 32;
        int mapCol = mapIndex % 32;

        IntPtr ptr = tilemapRect.DataPointer;
        ptr += mapCol * MAP_SIZE * sizeof(byte);
        ptr += World.TILEMAP_WIDTH * mapRow * MAP_SIZE * sizeof(byte);
        ptr += TILE_SIZE * tileCol * sizeof(byte);
        ptr += World.TILEMAP_WIDTH * TILE_SIZE * tileRow * sizeof(byte);

        if (flipped)
            ptr += World.TILEMAP_WIDTH * (TILE_SIZE - 1) * sizeof(byte);

        for (int row = 0; row < TILE_SIZE; row++)
        {
            int dataIndex = row * TILE_SIZE;
            if (mirrored)
                dataIndex += TILE_SIZE - 1;

            using (var stream = new DataStream(ptr, TILE_SIZE * sizeof(byte), true, true))
            {
                for (int col = 0; col < TILE_SIZE; col++)
                {
                    stream.Write((byte) ((subPalette << 4) | (data != null ? data[dataIndex] : 0)));

                    if (mirrored)
                        dataIndex--;
                    else
                        dataIndex++;
                }
            }

            if (flipped)
                ptr -= World.TILEMAP_WIDTH * sizeof(byte);
            else
                ptr += World.TILEMAP_WIDTH * sizeof(byte);
        }
    }

    public void RefreshMapCache(bool background = false)
    {
        var tilemap = new Texture(Device, World.TILEMAP_WIDTH, World.TILEMAP_HEIGHT, 1, Usage.None, Format.L8, Pool.Managed);
        DataRectangle rect = tilemap.LockRectangle(0, LockFlags.Discard);

        Array.Clear(maps);

        uint map = pMaps;
        /* I didn't write this function, but basically the above loses a lot of data because size of a WORD is max 65535 and pMaps is a DWORD */
        for (int i = 0; i < 0x400; i++)
        {
            byte colisionByte = rom[pCollisions + i];
            var collisionData = (CollisionData) colisionByte;
            Map wmap = background ? World.BackgroundLayout.AddMap(collisionData) : World.ForegroundLayout.AddMap(collisionData);

            uint tileData = ReadWord(map);
            byte palette = (byte) ((tileData >> 10) & 7);
            byte subPalette = (byte) ((tileData >> 10) & 7);
            bool flipped = (tileData & 0x8000) != 0;
            bool mirrored = (tileData & 0x4000) != 0;
            bool upLayer = (tileData & 0x2000) != 0;
            map += 2;
            Tile tile = AddTile(tileData, true, background);
            wmap.SetTile(new Vector(0, 0), tile, palette, flipped, mirrored, upLayer);
            WriteTile(rect, tile?.data, i, 0, 0, palette, flipped, mirrored);

            tileData = ReadWord(map);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            map += 2;
            tile = AddTile(tileData, true, background);
            wmap.SetTile(new Vector(TILE_SIZE, 0), tile, palette, flipped, mirrored, upLayer);
            WriteTile(rect, tile?.data, i, 0, 1, palette, flipped, mirrored);

            tileData = ReadWord(map);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            map += 2;
            tile = AddTile(tileData, true, background);
            wmap.SetTile(new Vector(0, TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
            WriteTile(rect, tile?.data, i, 1, 0, palette, flipped, mirrored);

            tileData = ReadWord(map);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            map += 2;
            tile = AddTile(tileData, true, background);
            wmap.SetTile(new Vector(TILE_SIZE, TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
            WriteTile(rect, tile?.data, i, 1, 1, palette, flipped, mirrored);

            maps[i] = wmap.IsNull ? null : wmap;
        }

        tilemap.UnlockRectangle(0);

        if (background)
            Engine.BackgroundTilemap = tilemap;
        else
            Engine.ForegroundTilemap = tilemap;
    }

    private void LoadMap(int x, int y, ushort index, bool background = false)
    {
        if (index < maps.Length)
        {
            Map map = maps[index];
            if (map != null)
            {
                if (background)
                    World.BackgroundLayout.SetMap(new Vector(x * MAP_SIZE + WORLD_OFFSET.X, y * MAP_SIZE + WORLD_OFFSET.Y), map);
                else
                    World.ForegroundLayout.SetMap(new Vector(x * MAP_SIZE + WORLD_OFFSET.X, y * MAP_SIZE + WORLD_OFFSET.Y), map);
            }
        }
    }

    private void LoadSceneEx(int x, int y, ushort index, bool background = false)
    {
        x <<= 4;
        y <<= 4;
        uint pmap = (uint) (index << 8);
        for (int iy = 0; iy < 16; iy++)
        {
            for (int ix = 0; ix < 16; ix++)
            {
                LoadMap(x + ix, y + iy, mapping[pmap], background);
                pmap++;
            }
        }
    }

    internal void LoadPalette(bool background = false)
    {
        var texture = new Texture(Device, 256, 1, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
        DataRectangle rect = texture.LockRectangle(0, LockFlags.Discard);

        using (var stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                    stream.Write(new Color(Transform(palCache[(i << 4) | j], j != 0)).ToRgba());
            }
        }

        texture.UnlockRectangle(0);

        var palette = new Palette()
        {
            Texture = texture,
            Count = 256
        };

        if (background)
            Engine.BackgroundPalette = palette;
        else
            Engine.ForegroundPalette = palette;
    }

    public void LoadToWorld(bool background = false)
    {
        LoadPalette(background);

        if (background)
            World.BackgroundLayout.Resize(levelHeight, levelWidth);
        else
            World.ForegroundLayout.Resize(levelHeight, levelWidth);

        RefreshMapCache(background);

        uint tmpLayout = 0;
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
                LoadSceneEx(x, y, sceneLayout[tmpLayout++], background);
        }
    }

    public void LoadEventsToEngine()
    {
        for (ushort point = 0; point < checkpointInfoTable.Count; point++)
        {
            CheckPointInfo info = checkpointInfoTable[point];

            uint minX = ReadWord(info.minX);
            uint minY = ReadWord(info.minY);
            uint maxX = ReadWord(info.maxX);
            uint maxY = ReadWord(info.maxY);
            Engine.AddCheckpoint(
                point,
                new Box(minX, minY, maxX - minX + SCREEN_WIDTH, maxY - minY + SCREEN_HEIGHT),
                new Vector(ReadWord(info.chX), ReadWord(info.chY)),
                new Vector(ReadWord(info.camX), ReadWord(info.camY)),
                new Vector(ReadWord(info.bkgX), ReadWord(info.bkgY)),
                new Vector(ReadShort(info.forceX), ReadShort(info.forceY)),
                ReadByte(info.scroll)
            );
        }

        foreach (List<EventInfo> list in eventTable)
        {
            foreach (EventInfo info in list)
            {
                switch (info.type)
                {
                    case 0x00:
                        switch (info.eventId)
                        {
                            case 0x01:
                                if (info.eventSubId == 0x80)
                                    Engine.AddBigAmmoRecover((info.xpos, info.ypos));
                                else
                                    Engine.AddSmallAmmoRecover((info.xpos, info.ypos));

                                break;

                            case 0x02:
                                if (info.eventSubId == 0x80)
                                    Engine.AddBigHealthRecover((info.xpos, info.ypos));
                                else
                                    Engine.AddSmallHealthRecover((info.xpos, info.ypos));

                                break;

                            case 0x04:
                                Engine.AddLifeUp((info.xpos, info.ypos));
                                break;

                            case 0x05:
                                Engine.AddSubTank((info.xpos, info.ypos));
                                break;

                            case 0x07:
                                Engine.AddBossDoor(info.eventSubId, (info.xpos, info.ypos));
                                break;

                            case 0x0B:
                                Engine.AddHeartTank((info.xpos, info.ypos));
                                break;
                        }

                        break;

                    case 0x02:
                        switch (info.eventId)
                        {
                            case 0x00: // camera lock
                            {
                                uint pBase;
                                if (expandedROM && expandedROMVersion >= 4)
                                {
                                    pBase = Snes2pc((int) (lockBank << 16) | (0x8000 + Level * 0x800 + info.eventSubId * 0x20));
                                }
                                else
                                {
                                    var borderOffset = ReadWord((int) (Snes2pc(pBorders) + 2 * info.eventSubId));
                                    pBase = Snes2pc(borderOffset | ((pBorders >> 16) << 16));
                                }

                                int right = ReadWord(pBase);
                                pBase += 2;
                                int left = ReadWord(pBase);
                                pBase += 2;
                                int bottom = ReadWord(pBase);
                                pBase += 2;
                                int top = ReadWord(pBase);
                                pBase += 2;

                                var boudingBox = new Box(left, top, right - left, bottom - top);

                                uint lockNum = 0;

                                var extensions = new List<Vector>();
                                while (((expandedROM && expandedROMVersion >= 4) ? ReadWord(pBase) : rom[pBase]) != 0)
                                {
                                    Box lockBox = boudingBox;
                                    ushort camOffset = 0;
                                    ushort camValue = 0;

                                    if (expandedROM && expandedROMVersion >= 4)
                                    {
                                        camOffset = ReadWord(pBase);
                                        pBase += 2;
                                        camValue = ReadWord(pBase);
                                        pBase += 2;
                                    }
                                    else
                                    {
                                        ushort offset = (ushort) ((rom[pBase] - 1) << 2);
                                        camOffset = ReadWord(pLocks + offset + 0x0);
                                        camValue = ReadWord(pLocks + offset + 0x2);
                                        pBase++;
                                    }

                                    int lockX0 = (left + right) / 2;
                                    int lockY0 = (top + bottom) / 2;

                                    int lockLeft = lockX0;
                                    int lockTop = lockY0;
                                    int lockRight = lockX0;
                                    int lockBottom = lockY0;

                                    if (Type > 0)
                                        camOffset -= 0x10;

                                    if (camOffset is 0x1E5E or 0x1E6E or 0x1E68 or 0x1E60)
                                    {
                                        if (camOffset == 0x1E5E)
                                        {
                                            lockLeft = camValue;
                                        }
                                        else if (camOffset == 0x1E6E)
                                        {
                                            lockBottom = camValue + SCREEN_HEIGHT;
                                        }
                                        else if (camOffset == 0x1E68)
                                        {
                                            lockTop = camValue;
                                        }
                                        else if (camOffset == 0x1E60)
                                        {
                                            lockRight = camValue + SCREEN_WIDTH;
                                        }
                                    }

                                    int lockX = lockLeft < lockX0 ? lockLeft : lockRight;
                                    int lockY = lockTop < lockY0 ? lockTop : lockBottom;

                                    extensions.Add((lockX - lockX0, lockY - lockY0));
                                    lockNum++;
                                }

                                Engine.AddCameraLockTrigger(boudingBox, extensions);
                                break;
                            }

                            case 0x02:
                            case 0x0B: // checkpoint trigger
                            {
                                Engine.AddCheckpointTrigger((ushort) (info.eventSubId & 0xf), (info.xpos, info.ypos));
                                break;
                            }

                            case 0x15: // dynamic change object/enemy tiles (vertical)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.OBJECT_TILE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x16: // dynamic change background tiles tiles (vertical)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.BACKGROUND_TILE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x17: // dynamic change palette (vertical)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.PALETTE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x18: // dynamic change object/enemy tiles (horizontal)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.OBJECT_TILE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }

                            case 0x19: // dynamic change background tiles tiles (horizontal)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.BACKGROUND_TILE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }

                            case 0x1A: // dynamic change palette (horizontal)
                            {
                                Engine.AddChangeDynamicPropertyTrigger((info.xpos, info.ypos), DynamicProperty.PALETTE, info.eventSubId & 0xf, (info.eventSubId >> 4) & 0xf, SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }
                        }

                        break;

                    case 0x03:
                        Engine.AddObjectEvent(info.eventId, info.eventSubId, (info.xpos, info.ypos));
                        break;
                }
            }
        }
    }
}