using System;
using System.Collections.Generic;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public class PrecachedSound : IDisposable
{
    private HashSet<string> names;

    public IReadOnlySet<string> Names => names;

    public string Path
    {
        get;
    }

    public WaveStream Stream
    {
        get;
        internal set;
    }

    public PrecachedSound(string name, string path, WaveStream stream = null)
    {
        Path = path;
        Stream = stream;

        names = new HashSet<string>
        {
            name
        };
    }

    public PrecachedSound(string path, params string[] names)
        : this(path, null, names)
    {
    }

    public PrecachedSound(string path, WaveStream stream = null, params string[] names)
    {
        Path = path;
        Stream = stream;

        if (names != null && names.Length > 0)
            this.names = new HashSet<string>(names);
        else
            this.names = new HashSet<string>();
    }

    internal bool AddName(string name)
    {
        return names.Add(name);
    }

    internal bool RemoveName(string name)
    {
        return names.Remove(name);
    }

    public void Dispose()
    {
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Deconstruct(out IReadOnlySet<string> names, out string path)
    {
        names = Names;
        path = Path;
    }

    public void Deconstruct(out IReadOnlySet<string> names, out string path, out WaveStream stream)
    {
        names = Names;
        path = Path;
        stream = Stream;
    }

    public override string ToString()
    {
        return $"{{Path={Path} Names=[{names}]}}";
    }
}