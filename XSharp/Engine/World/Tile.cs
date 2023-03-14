namespace XSharp.Engine.World;

public class Tile
{
    internal byte[] data;

    public int ID
    {
        get;
    }

    public byte[] Data
    {
        get => data;
        set => data = value;
    }

    internal Tile(int id)
        : this(id, null)
    {
    }

    internal Tile(int id, byte[] data)
    {
        ID = id;
        this.data = data;
    }
}