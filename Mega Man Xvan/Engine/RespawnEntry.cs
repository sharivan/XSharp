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
        private Entity entity;
        private Box box;

        public RespawnEntry(Entity entity, Box box)
        {
            this.entity = entity;
            this.box = box;
        }

        public Entity Entity
        {
            get
            {
                return entity;
            }
        }

        public Box Box
        {
            get
            {
                return box;
            }
        }
    }
}
