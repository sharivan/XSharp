using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.World;

public class MapCell
{
    public Tile Tile
    {
        get;
        set;
    }

    public int Palette
    {
        get;
        set;
    }

    public bool Flipped
    {
        get;
        set;
    }

    public bool Mirrored
    {
        get;
        set;
    }

    public bool UpLayer
    {
        get;
        set;
    }

    public MapCell()
    {
    }
}