using System;
using System.IO;
using System.IO.Compression;

namespace XSharp.Engine.IO;

public class LevelReader(Stream stream) : StreamReader(stream)
{
    private ZipArchive archive = new(stream);

    public LevelReader(string path)
        : this(File.Open(path, FileMode.Open))
    {
    }

    public void Extract()
    {
        foreach (var entry in archive.Entries)
        {
            entry.Open();
        }
    }

    public override void Close()
    {
        archive.Dispose();
        base.Close();
    }
}