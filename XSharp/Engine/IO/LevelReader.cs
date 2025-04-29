using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Graphics;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.IO;

public class LevelReader
{
    private Dictionary<string, Palette> palettes;
    private Dictionary<string, Tileset> tilesets;
    private Dictionary<string, Tilemap> tilemaps;
    private List<ZipArchiveEntry> tilesetsToResolve;
    private List<ZipArchiveEntry> tilemapsToResolve;

    private BaseEngine Engine => BaseEngine.Engine;

    public LevelReader()
    {
        palettes = [];
        tilesets = [];
        tilemaps = [];
        tilesetsToResolve = [];
        tilemapsToResolve = [];
    }

    private void ReadZipEntry(ZipArchiveEntry entry, bool resolving)
    {
        string name = entry.Name;
        string ext = null;
        int idx = name.LastIndexOf('.');

        if (idx >= 0)
        {
            name = name[..idx];
            ext = name[(idx + 1)..];
        }

        using var stream = entry.Open();
        using var reader = new BinaryReader(stream);

        switch (ext)
        {
            case "pal": // pallete
            {
                int version = reader.ReadInt32();

                int capacity = reader.ReadByte();
                int count = reader.ReadByte();

                var colors = new Color[count];
                for (int i = 0; i < count; i++)
                {
                    int rgba = reader.ReadInt32();
                    colors[i] = new Color(rgba);
                }

                var palette = Engine.PrecachePalette(name, colors, capacity);
                palettes.Add(name, palette);
                break;
            }

            case "ts": // tileset
            {
                int version = reader.ReadInt32();

                int cols = reader.ReadInt16();
                int rows = reader.ReadInt16();
                string paletteName = reader.ReadString();

                if (!palettes.TryGetValue(paletteName, out var palette))
                {
                    if (resolving)
                        throw new FileNotFoundException($"Could not found the palette \"{paletteName}\".");

                    palette = null;
                    tilesetsToResolve.Add(entry);
                    return;
                }

                var tileset = Engine.AddTileset(name, palette, rows, cols);
                tilesets.Add(name, tileset);

                for (int row = 0; row < tileset.Rows; row++)
                {
                    for (int col = 0; col < tileset.Cols; col++)
                    {
                        byte[] tile = reader.ReadBytes(TILE_SIZE * TILE_SIZE * sizeof(byte));
                        tileset.SetTile(row, col, tile);
                    }
                }

                break;
            }

            case "tm": // tilemap
            {
                int version = reader.ReadInt32();

                int cols = reader.ReadInt16();
                int rows = reader.ReadInt16();
                string tilesetName = reader.ReadString();

                if (!tilesets.TryGetValue(tilesetName, out var tileset))
                {
                    if (resolving)
                        throw new FileNotFoundException($"Could not found the tileset \"{tilesetName}\".");

                    tilemapsToResolve.Add(entry);
                    return;
                }

                var tilemap = Engine.AddTilemap(name, tileset, rows, cols);
                tilemaps.Add(name, tilemap);

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        var map = tilemap[row, col];
                        map.CollisionData = (CollisionData) reader.ReadByte();

                        for (int mapRow = 0; mapRow < SIDE_TILES_PER_MAP; mapRow++)
                        {
                            for (int mapCol = 0; mapCol < SIDE_TILES_PER_MAP; mapCol++)
                            {
                                var cell = map[mapRow, mapCol];
                                int tileID = reader.ReadInt16();
                                cell.Tile = tileset.GetTileByID(tileID);
                                cell.Palette = reader.ReadByte();
                                cell.Flipped = reader.ReadBoolean();
                                cell.Mirrored = reader.ReadBoolean();
                                cell.UpLayer = reader.ReadBoolean();
                            }
                        }
                    }
                }

                break;
            }

            case "blk": // block
            {
                int version = reader.ReadInt32();

                int cols = reader.ReadInt16();
                int rows = reader.ReadInt16();

                break;
            }

            case "scn": // scenes
            {
                int version = reader.ReadInt32();

                int cols = reader.ReadInt16();
                int rows = reader.ReadInt16();

                break;
            }

            case "lyw": // layouts
            {
                int version = reader.ReadInt32();

                int cols = reader.ReadInt16();
                int rows = reader.ReadInt16();

                break;
            }

            case "obj": // objects
            {
                int version = reader.ReadInt32();

                int count = reader.ReadInt16();

                break;
            }

            case "chk": // checkpoints
            {
                int version = reader.ReadInt32();

                break;
            }

            case "lua": // script
            {
                break;
            }

            case "mp3": // sound
            case "wav":
            {
                break;
            }
        }
    }

    private void Clear()
    {
        palettes.Clear();
        tilesets.Clear();
        tilemaps.Clear();
        tilesetsToResolve.Clear();
        tilemapsToResolve.Clear();
    }

    public void Load(string path)
    {
        using var stream = File.Open(path, FileMode.Open);
        Load(stream);
    }

    public void Load(Stream stream)
    {
        Clear();

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

        foreach (var entry in archive.Entries)
            ReadZipEntry(entry, false);

        foreach (var entry in tilesetsToResolve)
            ReadZipEntry(entry, true);

        foreach (var entry in tilemapsToResolve)
            ReadZipEntry(entry, true);
    }
}