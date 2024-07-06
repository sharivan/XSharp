using System;

namespace XSharp.Engine.Input;

public interface IJoystick : IDisposable
{
    public ICapabilities Capabilities
    {
        get;
    }

    public void Poll();

    public IJoystickState GetCurrentState();
}