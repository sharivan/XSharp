using System;
using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;

using static MMX.Engine.Consts;
using System.Runtime.InteropServices;

namespace MMX.Engine
{
    public class Tile
    {
        private readonly World world;
        private readonly int id;

        internal byte[] data;

        public World World
        {
            get
            {
                return world;
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public byte[] Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        internal Tile(World world, int id) :
            this(world, id, null)
        {
        }

        internal Tile(World world, int id, byte[] data)
        {
            this.world = world;
            this.id = id;
            this.data = data;
        }
    }
}
