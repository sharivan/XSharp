using System;
using System.IO;
using System.IO.Compression;

namespace XSharp.Engine.IO;

public class LevelReader : StreamReader
{
    private ZipArchive archive;

    public LevelReader(Stream stream) : base(stream)
    {
        archive = new ZipArchive(stream);
    }

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