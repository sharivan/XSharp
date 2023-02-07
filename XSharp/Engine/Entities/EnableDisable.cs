using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities
{
    public interface IEnableDisable
    {
        public bool Enabled
        {
            get;
            set;
        }

        public void Enable();

        public void Disable();
    }
}