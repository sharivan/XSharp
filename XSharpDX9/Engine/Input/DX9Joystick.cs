using SharpDX.DirectInput;

namespace XSharp.Engine.Input;

public class DX9Joystick(Joystick joystick) : IJoystick
{
    private Joystick joystick = joystick;
    private DX9Capabilities capabilities = null;
    private DX9JoystickState state = null;

    public ICapabilities Capabilities => GetCapabilities();

    public void Dispose()
    {
        joystick?.Dispose();
    }

    public IJoystickState GetCurrentState()
    {
        if (state == null)
        {
            state = new DX9JoystickState(joystick.GetCurrentState());
            return state;
        }

        var currState = joystick.GetCurrentState();
        if (state.state != currState)
            state.state = currState;

        return state;
    }

    public void Poll()
    {
        joystick.Poll();
    }

    private DX9Capabilities GetCapabilities()
    {
        if (capabilities == null)
        {
            capabilities = new DX9Capabilities(joystick.Capabilities);
            return capabilities;
        }

        var currCapabilities = joystick.Capabilities;
        if (capabilities.capabilities != currCapabilities)
            capabilities.capabilities = currCapabilities;

        return capabilities;
    }
}