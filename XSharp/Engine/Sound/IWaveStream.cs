using System;

namespace XSharp.Engine.Sound;

public interface IWaveStream : IDisposable
{
    public long Position
    {
        get;
        set;
    }

    public long Length
    {
        get;
    }

    public int Read(byte[] buffer, int offset, int count);
}