using SharpDX.DirectInput;

namespace XSharp.Engine.Input;

public class DX9Capabilities(Capabilities capabilities) : ICapabilities
{
    internal Capabilities capabilities = capabilities;

    public int ButtonCount => capabilities.ButtonCount;
}