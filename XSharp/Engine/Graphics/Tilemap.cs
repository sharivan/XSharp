using System;
using System.Xml.Linq;
using XSharp.Engine.World;
using XSharp.Graphics;

namespace XSharp.Engine.Graphics;

public class Tilemap
{
    private Map[,] maps;

    public string Name
    {
        get;
    }

    public Tileset Tileset
    {
        get;
        set;
    }

    public int Rows
    {
        get;
    }

    public int Cols
    {
        get;
    }

    public Map this[int row, int col] => maps[row, col];

    public Tilemap(string name, int rows, int cols, bool fill = true) : this(name, null, rows, cols, fill)
    {
    }

    public Tilemap(string name, Tileset tileset, int rows, int cols, bool fill = true)
    {
        Name = name;
        Tileset = tileset;
        Rows = rows;
        Cols = cols;

        maps = new Map[rows, cols];
       
        if (fill)
        {
            int id = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var map = new Map(id++, Collision.CollisionData.NONE, true);
                    maps[row, col] = map;
                }
            }
        }
    }

    public Map AddMap(int row, int col)
    {
        var map = maps[row, col];
        if (map != null)
            return map;

        map = new Map(row * Cols + col);
        maps[row, col] = map;
        return map;
    }
}