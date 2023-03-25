namespace XSharp.Engine.Entities.Objects;

public interface IControllable
{
    bool Paused
    {
        get;
    }

    void Pause();

    void Resume();
}