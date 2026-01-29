using System;
using System.Dynamic;
using XSharp.Engine.Collision;
using XSharp.Exporter.Map;
using XSharp.Graphics;
using XSharp.Math.Fixed.Geometry;
using XSharp.MegaEDX;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Fixed.Geometry.Box;

namespace XSharp.Exporter.MegaEDX;

public class MMXCoreLoader(LevelWriter writer) : MMXCore
{
    private LevelWriter writer = writer;

    private MapProperties[] maps = new MapProperties[0x400];

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

    private static int Transform(int color, bool notTransparent)
    {
        return !notTransparent ? 0 : (int) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9) | 0xFF000000);
    }

    private TileProperties AddTile(uint tile, bool transparent = false, bool background = false)
    {
        uint image = (tile & 0x3FF) << 6;

        int count = TILE_SIZE * TILE_SIZE;
        byte[] imageData = new byte[count * sizeof(byte)];
        bool notNull = false;
        using (var ms = new MemoryStream(imageData))
        {
            using var writer = new BinaryWriter(ms);
            for (int i = 0; i < count; i++, image++)
            {
                var v = vramCache[image];
                bool notTransparent = v != 0 || !transparent;
                notNull |= notTransparent;
                writer.Write(v);
            }
        }

        TileProperties wtile = background ? writer.AddTile("BackgroundTileset", imageData) : writer.AddTile("ForegroundTileset", imageData);
        return wtile;
    }

    public void RefreshMapCache(bool background = false)
    {
        string tilesetName = background ? "BackgroundTileset" : "ForegroundTileset";
        TilemapProperties tilemap = background
            ? writer.AddTilemap("BackgroundTilemap", tilesetName)
            : writer.AddTilemap("ForegroundTilemap", tilesetName);

        Array.Clear(maps);

        uint pMap = pMaps;
        /* I didn't write this function, but basically the above loses a lot of data because size of a WORD is max 65535 and pMaps is a DWORD */
        for (int i = 0; i < 0x400; i++)
        {
            MapProperties map = tilemap.AddMap();

            byte colisionByte = rom[pCollisions + i];
            map.CollisionData = (CollisionData) colisionByte;

            uint tileData = ReadWord(pMap);           
            byte palette = (byte) ((tileData >> 10) & 7);
            bool flipped = (tileData & 0x8000) != 0;
            bool mirrored = (tileData & 0x4000) != 0;
            bool upLayer = (tileData & 0x2000) != 0;
            pMap += 2;
            var tile = AddTile(tileData, true, background);
            map.SetCell(0, 0, tile, palette, flipped, mirrored, upLayer);

            tileData = ReadWord(pMap);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            pMap += 2;
            tile = AddTile(tileData, true, background);
            map.SetCell(1, 0, tile, palette, flipped, mirrored, upLayer);

            tileData = ReadWord(pMap);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            pMap += 2;
            tile = AddTile(tileData, true, background);
            map.SetCell(0, 1, tile, palette, flipped, mirrored, upLayer);


            tileData = ReadWord(pMap);
            palette = (byte) ((tileData >> 10) & 7);
            flipped = (tileData & 0x8000) != 0;
            mirrored = (tileData & 0x4000) != 0;
            upLayer = (tileData & 0x2000) != 0;
            pMap += 2;
            tile = AddTile(tileData, true, background);
            map.SetCell(1, 1, tile, palette, flipped, mirrored, upLayer);

            maps[i] = map.IsEmpty() ? null : map;
        }
    }

    private void LoadMap(LayoutProperties layout, int x, int y, ushort index)
    {
        if (index < maps.Length)
        {
            var map = maps[index];
            if (map != null)
                layout.SetMap(new Vector(x * MAP_SIZE + WORLD_OFFSET.X, y * MAP_SIZE + WORLD_OFFSET.Y), map);
        }
    }

    private void LoadSceneEx(LayoutProperties layout, int x, int y, ushort index)
    {
        x <<= 4;
        y <<= 4;
        uint pmap = (uint) (index << 8);
        for (int iy = 0; iy < 16; iy++)
        {
            for (int ix = 0; ix < 16; ix++)
            {
                LoadMap(layout, x + ix, y + iy, mapping[pmap]);
                pmap++;
            }
        }
    }

    internal void LoadPalette(bool background = false)
    {
        Color[] colors = new Color[256];
        int k = 0;

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
                colors[k++] = Color.FromBgra(Transform(palCache[(i << 4) | j], j != 0));
        }

        writer.AddPalette((background ? "BackgroundPalette" : "ForegroundPalette") + PalLoad, colors);
    }

    public void LoadToWorld(bool background = false)
    {
        LoadPalette(background);

        var layout = writer.AddLayout(background ? "BackgroundLayout" : "ForegroundLayout", levelWidth, levelHeight);

        RefreshMapCache(background);

        uint tmpLayout = 0;
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
                LoadSceneEx(layout, x, y, sceneLayout[tmpLayout++]);
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
            writer.AddCheckpoint(
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
                dynamic props = new ExpandoObject();

                switch (info.type)
                {
                    case 0x00:
                    {
                        props.origin = new Vector(info.xpos, info.ypos);

                        switch (info.eventId)
                        {
                            case 0x01:
                                if (info.eventSubId == 0x80)
                                    writer.AddEntity("big_ammo_recover", props);
                                else
                                    writer.AddEntity("small_ammo_recover", props);

                                break;

                            case 0x02:
                                if (info.eventSubId == 0x80)
                                    writer.AddEntity("big_health_recover", props);
                                else
                                    writer.AddEntity("small_health_recover", props);

                                break;

                            case 0x04:
                                writer.AddEntity("life_up", props);
                                break;

                            case 0x05:
                                writer.AddEntity("sub_tank", props);
                                break;

                            case 0x07:
                                props.secondDoor = (info.eventSubId & 0x80) != 0;
                                writer.AddEntity("boss_door", props);
                                break;

                            case 0x0B:
                                writer.AddEntity("heart_tank", props);
                                break;
                        }

                        break;
                    }

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
                                    ushort camOffset;
                                    ushort camValue;

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

                                props.boudingBox = boudingBox;
                                props.extensions = extensions;
                                writer.AddEntity("trigger_camera_lock", props);
                                break;
                            }

                            case 0x02:
                            case 0x0B: // checkpoint trigger
                            {
                                props.id = (ushort) (info.eventSubId & 0xf);
                                props.origin = new Vector(info.xpos, info.ypos);
                                writer.AddEntity("trigger_checkpoint", props);
                                break;
                            }

                            case 0x15: // dynamic change object/enemy tiles (vertical)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "objectTile";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "vertical";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }

                            case 0x16: // dynamic change background tiles tiles (vertical)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "backgroundTile";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "vertical";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }

                            case 0x17: // dynamic change palette (vertical)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "palette";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "vertical";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }

                            case 0x18: // dynamic change object/enemy tiles (horizontal)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "objectTile";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "horizontal";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }

                            case 0x19: // dynamic change background tiles tiles (horizontal)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "backgroundTile";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "horizontal";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }

                            case 0x1A: // dynamic change palette (horizontal)
                            {
                                props.origin = new Vector(info.xpos, info.ypos);
                                props.prop = "palette";
                                props.forward = info.eventSubId & 0xf;
                                props.backward = (info.eventSubId >> 4) & 0xf;
                                props.orientation = "horizontal";
                                writer.AddEntity("trigger_change_dynamic_property", props);
                                break;
                            }
                        }

                        break;

                    case 0x03:
                        props.origin = new Vector(info.xpos, info.ypos);

                        switch (info.eventId)
                        {
                            case 0x01 when Type == 0:
                                writer.AddEntity("enemy_hoganmer", props);
                                break;

                            case 0x02 when Type == 0:
                                writer.AddEntity("boss_chill_penguin", props);
                                break;

                            case 0x03 when Type == 0:
                                writer.AddEntity("miniboss_thunder_slimer", props);
                                break;

                            case 0x04 when Type == 0:
                                writer.AddEntity("enemy_flammingle", props);
                                break;

                            case 0x05 when Type == 0:
                                writer.AddEntity("boss_boomer_kuwanger", props);
                                break;

                            case 0x06 when Type == 0:
                                writer.AddEntity("enemy_planty", props);
                                break;

                            case 0x07 when Type == 0:
                                writer.AddEntity("boss_launch_octupus", props);
                                break;

                            case 0x09 when Type == 1:
                                writer.AddEntity("enemy_scriver", props);
                                break;

                            case 0x0A when Type == 0:
                                writer.AddEntity("boss_sting_chameleon", props);
                                break;

                            case 0x0B when Type == 0:
                                writer.AddEntity("enemy_axe_max", props);
                                break;

                            case 0x0C when Type == 0:
                                writer.AddEntity("boss_flame_mammoth", props);
                                break;

                            case 0x0D when Type == 0:
                                writer.AddEntity("enemy_rush_roader", props);
                                break;

                            case 0x0F when Type == 0:
                                writer.AddEntity("enemy_crusher", props);
                                break;

                            case 0x11 when Type == 0:
                                writer.AddEntity("enemy_road_attacker", props);
                                break;

                            case 0x13 when Type == 0:
                                writer.AddEntity("enemy_dodge_blaster", props);
                                break;

                            case 0x14 when Type == 0:
                                writer.AddEntity("boss_armored_armadillo", props);
                                break;

                            case 0x15 when Type == 0:
                                writer.AddEntity("enemy_spiky", props);
                                break;

                            case 0x16 when Type == 0:
                                writer.AddEntity("prop_hover_platform", props);
                                break;

                            case 0x17 when Type == 0:
                                writer.AddEntity("enemy_turn_cannon", props);
                                break;

                            case 0x19 when Type == 0:
                                writer.AddEntity("enemy_bomb_been", props);
                                break;

                            case 0x1D when Type == 0:
                                writer.AddEntity("enemy_gulpfer", props);
                                break;

                            case 0x1E when Type == 0:
                                writer.AddEntity("enemy_mad_pecker", props);
                                break;

                            case 0x20 when Type == 0:
                                writer.AddEntity("enemy_amenhopper", props);
                                break;

                            case 0x22 when Type == 0:
                                writer.AddEntity("enemy_bee_blader", props);
                                break;

                            case 0x27 when Type == 0:
                                writer.AddEntity("enemy_ball_devoux", props);
                                break;

                            case 0x29 when Type == 0:
                                writer.AddEntity("enemy_gunvolt", props);
                                break;

                            case 0x2B when Type == 0:
                                writer.AddEntity("vehicle_minecart", props);
                                break;

                            case 0x2C when Type == 0:
                                writer.AddEntity("enemy_mole_borer", props);
                                break;

                            case 0x2C when Type == 1:
                                writer.AddEntity("prop_probe8201u", props);
                                break;

                            case 0x2D when Type == 0:
                                writer.AddEntity("enemy_batton_bone_g", props);
                                break;

                            case 0x2E when Type == 0:
                                writer.AddEntity("enemy_metall_c15", props);
                                break;

                            case 0x2F:
                                writer.AddEntity("enemy_armor_soldier", props);
                                break;

                            case 0x30 when Type == 0:
                                writer.AddEntity("enemy_dig_labour", props);
                                break;

                            case 0x31 when Type == 0:
                                writer.AddEntity("boss_spirk_mandrill", props);
                                break;

                            case 0x36 when Type == 0:
                                writer.AddEntity("enemy_kamminger", props);
                                break;

                            case 0x37 when Type == 0:
                                writer.AddEntity("enemy_hotarion", props);
                                break;

                            case 0x39 when Type == 0:
                                writer.AddEntity("enemy_compressor", props);
                                break;

                            case 0x3A when Type == 0:
                                writer.AddEntity("enemy_tombot", props);
                                break;

                            case 0x3B when Type == 0:
                                writer.AddEntity("enemy_ladder_yadder", props);
                                break;

                            case 0x3D when Type == 0:
                                writer.AddEntity("func_bk_elevator", props);
                                break;

                            case 0x40 when Type == 0:
                                writer.AddEntity("func_coil", props);
                                break;

                            case 0x42 when Type == 0:
                                writer.AddEntity("enemy_ray_field", props);
                                break;

                            case 0x44 when Type == 0:
                                writer.AddEntity("enemy_ray_trap", props);
                                break;

                            case 0x46 when Type == 0:
                                writer.AddEntity("enemy_missiles", props);
                                break;

                            case 0x47 when Type == 0:
                                writer.AddEntity("enemy_flame_pillar", props);
                                break;

                            case 0x49 when Type == 0:
                                writer.AddEntity("enemy_sky_claw", props);
                                break;

                            case 0x4C when Type == 0:
                                writer.AddEntity("enemy_dripping_lava", props);
                                break;

                            case 0x4D:
                                writer.AddEntity("func_capsule", props);
                                break;

                            case 0x4F when Type == 0:
                                writer.AddEntity("enemy_rolling_gabyoall", props);
                                break;

                            case 0x50 when Type == 0:
                                writer.AddEntity("enemy_death_rogumer_cannon", props);
                                break;

                            case 0x50 when Type == 1:
                                writer.AddEntity("enemy_batton_bone_g", props);
                                break;


                            case 0x51 when Type == 0:
                                writer.AddEntity("enemy_raybit", props);
                                break;

                            case 0x53 when Type == 0:
                                writer.AddEntity("enemy_snow_shooter", props);
                                break;

                            case 0x54 when Type == 0:
                                writer.AddEntity("enemy_snow_ball", props);
                                break;

                            case 0x57 when Type == 0:
                                writer.AddEntity("func_igloo", props);
                                break;

                            case 0x59 when Type == 0:
                                writer.AddEntity("enemy_lift_cannon", props);
                                break;

                            case 0x5B when Type == 0:
                                writer.AddEntity("enemy_mega_tortoise", props);
                                break;

                            case 0x5D when Type == 0:
                                writer.AddEntity("boss_rangda_bangda", props);
                                break;

                            case 0x62 when Type == 0:
                                writer.AddEntity("boss_drex", props);
                                break;

                            case 0x63 when Type == 0:
                                writer.AddEntity("boss_bosspider", props);
                                break;

                            case 0x64 when Type == 0:
                                writer.AddEntity("prop_prision_capsule", props);
                                break;

                            case 0x65 when Type == 0:
                                writer.AddEntity("boss_jedi_sigma", props);
                                break;

                            case 0x66 when Type == 0:
                                writer.AddEntity("boss_vile", props);
                                break;

                            case 0x67 when Type == 0:
                                writer.AddEntity("boss_ride_armor_vile", props);
                                break;

                            default:
                                props.eventID = info.eventId;
                                props.eventSubID = info.eventSubId;
                                writer.AddEntity("unknow_entity", props);
                                break;
                        }

                        break;
                }
            }
        }
    }
}