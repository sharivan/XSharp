using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public class NAudioWaveStream : WaveStream, IWaveStream
{
    private WaveStream stream;

    public override long Position
    { 
        get => stream.Position;
        set => stream.Position = value;
    }

    public override long Length => stream.Length;

    public override WaveFormat WaveFormat => stream.WaveFormat;

    public NAudioWaveStream(WaveStream stream)
    {
        this.stream = stream;
    }

    public new void Dispose()
    {
        stream?.Dispose();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return stream.Read(buffer, offset, count);
    }
}