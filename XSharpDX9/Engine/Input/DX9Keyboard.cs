using SharpDX.DirectInput;

namespace XSharp.Engine.Input;

public class DX9Keyboard(Keyboard keyboard) : IKeyboard
{
    private Keyboard keyboard = keyboard;
    private DX9KeyboardState state = null;

    public void Dispose()
    {
        keyboard?.Dispose();
    }

    public IKeyboardState GetCurrentState()
    {
        if (state == null)
        {
            state = new DX9KeyboardState(keyboard.GetCurrentState());
            return state;
        }

        var currState = keyboard.GetCurrentState();
        if (state.state != currState)
            state.state = currState;

        return state;
    }

    public void Poll()
    {
        keyboard.Poll();
    }
}