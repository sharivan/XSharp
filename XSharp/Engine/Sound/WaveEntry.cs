using System;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public class WaveEntry : IDisposable
{
    public string Name
    {
        get;
    }

    public string Path
    {
        get;
    }

    public SoundFormat Format
    {
        get;
    }

    public WaveStream Stream
    {
        get;
        internal set;
    }

    public WaveEntry(string name, string path, SoundFormat format, WaveStream stream = null)
    {
        Name = name;
        Path = path;
        Format = format;
        Stream = stream;
    }

    public void Dispose()
    {
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Deconstruct(out string name, out string path, out SoundFormat format)
    {
        name = Name;
        path = Path;
        format = Format;
    }

    public void Deconstruct(out string name, out string path, out SoundFormat format, out WaveStream stream)
    {
        name = Name;
        path = Path;
        format = Format;
        stream = Stream;
    }
}