using System;
using System.IO;
using XSharp.Engine.World;
using XSharp.Graphics;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Graphics;

public class Tileset : IDisposable
{
    private Tile[,] tiles;

    public string Name
    {
        get;
    } 

    public Palette Palette
    {
        get;
        set;
    }

    public ITexture Texture
    {
        get;
    }

    public int Rows => Texture.Height / TILE_SIZE;

    public int Cols => Texture.Width / TILE_SIZE;

    public Tile this[int row, int col] => tiles[row, col];

    public Tileset(string name, int rows, int cols) : this(name, null, rows, cols)
    {
    }

    public Tileset(string name, Palette palette, int rows, int cols)
    {
        Name = name;
        Palette = palette;
        
        Texture = BaseEngine.Engine.CreateEmptyTexture(
            cols * TILE_SIZE,
            rows * TILE_SIZE
        );

        tiles = new Tile[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var tile = new Tile(row * cols + col);
                tiles[row, col] = tile;
            }
        }
    }

    public void Dispose()
    {
        Texture.Dispose();
        GC.SuppressFinalize(this);
    }

    public void SetTile(int row, int col, byte[] data)
    {
        var tile = tiles[row, col];
        tile.data = data;

        var rectangle = Texture.LockRectangle();
        try
        {
            using var stream = BaseEngine.Engine.CreateDataStream(rectangle.DataPointer, Texture.Width * Texture.Height * sizeof(byte), true, true);
            stream.Seek((row * Cols + col) * TILE_SIZE * TILE_SIZE, SeekOrigin.Begin);
            stream.Write(data, 0, data.Length);
        }
        finally
        {
            Texture.UnlockRectangle();
        }
    }

    public Tile GetTileByID(int tileID)
    {
        int row = tileID / Cols;
        int col = tileID % Cols;
        return tiles[row, col];
    }
}