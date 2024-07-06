using System;

namespace XSharp.Engine.Input;

public interface IKeyboard : IDisposable
{
    public void Poll();

    public IKeyboardState GetCurrentState();
}