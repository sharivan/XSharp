namespace XSharp.Engine.Input;

public interface IJoystickState
{
    public bool[] Buttons
    {
        get;
    }

    public int[] PointOfViewControllers
    {
        get;
    }
}