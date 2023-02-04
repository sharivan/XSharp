namespace XSharp.Engine.World
{
    public class Tile
    {
        internal byte[] data;

        public World World
        {
            get;
        }

        public int ID
        {
            get;
        }

        public byte[] Data
        {
            get => data;
            set => data = value;
        }

        internal Tile(World world, int id)
            : this(world, id, null)
        {
        }

        internal Tile(World world, int id, byte[] data)
        {
            World = world;
            ID = id;
            this.data = data;
        }
    }
}