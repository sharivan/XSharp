using SharpDX.Direct3D9;
using System;

namespace XSharp.Engine.Graphics
{
    public class Palette : IDisposable
    {
        internal string name;

        public int Index
        {
            get;
            internal set;
        }

        public string Name
        {
            get => name;
            set => GameEngine.Engine.UpdatePaletteName(this, value);
        }

        public Texture Texture
        {
            get;
        }

        public Palette()
        {
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }
    }
}