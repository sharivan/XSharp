using System.IO;
using System.IO.Compression;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using XSharp.Exporter.JSON;
using XSharp.Exporter.Map;
using XSharp.Exporter.MegaEDX;
using XSharp.Graphics;
using XSharp.Math.Fixed.Geometry;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XSharp.Exporter;

public class LevelWriter
{
    private MMXCoreLoader loader;

    private List<PaletteProperties> palettes;
    private Dictionary<string, PaletteProperties> palettesByName;
    private List<TilesetProperties> tilesets;
    private Dictionary<string, TilesetProperties> tilesetsByName;
    private List<TilemapProperties> tilemaps;
    private Dictionary<string, TilemapProperties> tilemapsByName;
    private List<BlockProperties> blocks;
    private List<SceneProperties> scenes;
    private List<LayoutProperties> layouts;
    private List<EntityProperties> entities;

    public LevelWriter()
    {
        loader = new MMXCoreLoader(this);
        palettes = [];
        palettesByName = [];
        tilesets = [];
        tilesetsByName = [];
        tilemaps = [];
        tilemapsByName = [];
        blocks = [];
        scenes = [];
        layouts = [];
        entities = [];
    }

    internal PaletteProperties AddPalette(string name, Color[] colors)
    {
        var palette = new PaletteProperties(this, name, colors);
        palettes.Add(palette);
        palettesByName.Add(name, palette);
        return palette;
    }

    internal TilesetProperties AddTileset(string name, string paletteName)
    {
        var tileset = new TilesetProperties(this, name, paletteName);
        tilesets.Add(tileset);
        tilesetsByName.Add(name, tileset);
        return tileset;
    }

    internal TileProperties AddTile(string tilesetName, byte[] data)
    {
        var tileset = GetTilesetByName(tilesetName);
        tileset ??= AddTileset(tilesetName, tilesetName == "BackgroundTileset" ? "BackgroundPalette" : "ForegroundPalette");

        return tileset.AddTile(data);
    }

    internal TilemapProperties AddTilemap(string name, string tilesetName)
    {
        var tileset = GetTilesetByName(tilesetName);
        if (tileset == null)
            AddTileset(tilesetName, tilesetName == "BackgroundTileset" ? "BackgroundPalette" : "ForegroundPalette");

        var tilemap = new TilemapProperties(this, name, tilesetName);
        tilemaps.Add(tilemap);
        tilemapsByName.Add(name, tilemap);
        return tilemap;
    }

    internal BlockProperties AddBlock(string tilemapName)
    {
        int id = blocks.Count;
        var result = new BlockProperties(this, id, tilemapName);
        blocks.Add(result);

        return result;
    }

    internal SceneProperties AddScene(string name = null)
    {
        int id = scenes.Count;
        var result = new SceneProperties(this, id, name);
        scenes.Add(result);

        return result;
    }

    internal LayoutProperties AddLayout(string layoutName, int width, int height)
    {
        var layout = new LayoutProperties(this, layoutName, width, height);
        layouts.Add(layout);
        return layout;
    }

    internal EntityProperties AddEntity(string entityClass, dynamic properties)
    {
        var entity = new EntityProperties(entityClass, properties);
        entities.Add(entity);
        return entity;
    }

    internal void AddCheckpoint(ushort point, Box box, Vector vector1, Vector vector2, Vector vector3, Vector vector4, byte v)
    {
    }

    internal PaletteProperties GetPaletteByID(int id)
    {
        return palettes[id];
    }

    internal PaletteProperties GetPaletteByName(string name)
    {
        return palettesByName.TryGetValue(name, out var palette) ? palette : null;
    }

    internal TilesetProperties GetTilesetByID(int id)
    {
        return tilesets[id];
    }

    internal TilesetProperties GetTilesetByName(string name)
    {
        return tilesetsByName.TryGetValue(name, out var tileset) ? tileset : null;
    }

    internal TileProperties GetTile(string tilesetName, int tileID)
    {
        var tileset = GetTilesetByName(tilesetName);
        if (tileset == null)
            return null;

        return tileset[tileID];
    }

    internal TilemapProperties GetTilemapByID(int id)
    {
        return tilemaps[id];
    }

    internal TilemapProperties GetTilemapByName(string name)
    {
        return tilemapsByName.TryGetValue(name, out var tilemap) ? tilemap : null;
    }

    internal MapProperties GetMap(string tilemapName, int mapID)
    {
        var tilemap = GetTilemapByName(tilemapName);
        if (tilemap == null)
            return null;

        return tilemap[mapID];
    }

    internal BlockProperties GetBlock(int blockID)
    {
        return blocks[blockID];
    }

    internal SceneProperties GetScene(int sceneID)
    {
        return scenes[sceneID];
    }

    private void Clear()
    {
        palettes.Clear();
        palettesByName.Clear();
        tilesets.Clear();
        tilesetsByName.Clear();
        tilemaps.Clear();
        tilemapsByName.Clear();
        blocks.Clear();
        scenes.Clear();
        layouts.Clear();
        entities.Clear();
    }

    public void Load(string romFilePath, int level)
    {
        Clear();

        loader.LoadNewRom(romFilePath);
        loader.Init();

        if (loader.CheckROM() == 0)
            throw new IOException($"The format of rom \"filePath\" is not a valid Mega Man X, X2 or X3 rom.");

        loader.LoadFont();
        loader.LoadProperties();

        loader.SetLevel((ushort) level, 0);

        loader.LoadLevel();
        loader.LoadEventsToEngine();
        loader.LoadToWorld(false);

        loader.LoadBackground();
        loader.LoadToWorld(true);

        loader.UpdateVRAMCache();
    }

    private void SavePalette(ZipArchive archive, PaletteProperties palette)
    {
        var entry = archive.CreateEntry("palettes/" + palette.Name + ".json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(palette, options);

        writter.Write(jsonString);
    }

    private void SaveTileset(ZipArchive archive, TilesetProperties tileset)
    {
        var entry = archive.CreateEntry("tilesets/" + tileset.Name + ".json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(tileset, options);

        writter.Write(jsonString);
    }

    private void SaveTilemap(ZipArchive archive, TilemapProperties tilemap)
    {
        var entry = archive.CreateEntry("tilemaps/" + tilemap.Name + ".json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(tilemap, options);

        writter.Write(jsonString);
    }

    private void SaveBlocks(ZipArchive archive)
    {
        var entry = archive.CreateEntry("blocks/blocks.json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(blocks.ToArray(), options);

        writter.Write(jsonString);
    }

    private void SaveScenes(ZipArchive archive)
    {
        var entry = archive.CreateEntry("scenes/scenes.json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(scenes.ToArray(), options);

        writter.Write(jsonString);
    }

    private void SaveLayouts(ZipArchive archive)
    {
        var entry = archive.CreateEntry("layouts/layouts.json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(layouts.ToArray(), options);

        writter.Write(jsonString);
    }

    private void SaveEntities(ZipArchive archive)
    {
        var entry = archive.CreateEntry("entities/entities.json");
        using var stream = entry.Open();
        using var writter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new FixedSingleConverter());
        options.Converters.Add(new VectorConverter());
        options.Converters.Add(new BoxConverter());
        var jsonString = JsonSerializer.Serialize(entities.ToArray(), options);

        writter.Write(jsonString);
    }

    public void Save(string path)
    {
        using var stream = File.Open(path, FileMode.Create);
        Save(stream);
    }

    public void Save(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Update, false);

        foreach (var palette in palettes)
            SavePalette(archive, palette);

        foreach (var tileset in tilesets)
            SaveTileset(archive, tileset);

        foreach (var tilemap in tilemaps)
            SaveTilemap(archive, tilemap);

        SaveBlocks(archive);
        SaveScenes(archive);
        SaveLayouts(archive);
        SaveEntities(archive);
    }
}