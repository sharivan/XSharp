using SharpDX.DirectInput;

namespace XSharp.Engine.Input;

public class DX9Capabilities : ICapabilities
{
    internal Capabilities capabilities;

    public int ButtonCount => capabilities.ButtonCount;

    public DX9Capabilities(Capabilities capabilities)
    {
        this.capabilities = capabilities;
    }
}