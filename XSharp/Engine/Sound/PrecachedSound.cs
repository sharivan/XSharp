using System;
using System.Collections.Generic;

namespace XSharp.Engine.Sound;

public class PrecachedSound : IDisposable
{
    private HashSet<string> names;

    public IReadOnlySet<string> Names => names;

    public string RelativePath
    {
        get;
    }

    public IWaveStream Stream
    {
        get;
        internal set;
    }

    public PrecachedSound(string name, string relativePath, IWaveStream stream = null)
    {
        RelativePath = relativePath;
        Stream = stream;

        names =
        [
            name
        ];
    }

    public PrecachedSound(string path, params string[] names)
        : this(path, null, names)
    {
    }

    public PrecachedSound(string path, IWaveStream stream = null, params string[] names)
    {
        RelativePath = path;
        Stream = stream;

        this.names = names != null && names.Length > 0 ? new HashSet<string>(names) : [];
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
        path = RelativePath;
    }

    public void Deconstruct(out IReadOnlySet<string> names, out string path, out IWaveStream stream)
    {
        names = Names;
        path = RelativePath;
        stream = Stream;
    }

    public override string ToString()
    {
        return $"{{Relative Path={RelativePath} Names=[{names}]}}";
    }
}