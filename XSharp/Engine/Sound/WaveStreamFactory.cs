using System.IO;

namespace XSharp.Engine.Sound;

public abstract class WaveStreamFactory
{
    public abstract IWaveStream FromStream(Stream stream);

    public abstract IWaveStream FromFile(string path);

    public abstract long TimeToBytePosition(IWaveStream stream, double time);
}