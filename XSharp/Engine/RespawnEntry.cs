using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Math;
using MMX.Geometry;

namespace MMX.Engine
{
    internal class RespawnEntry
    {
        public RespawnEntry(Entity entity, Box box)
        {
            this.Entity = entity;
            this.Box = box;
        }

        public Entity Entity { get; }

        public Box Box
        {
            get;
        }
    }
}
