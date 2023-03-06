namespace XSharp.Engine.Entities;

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