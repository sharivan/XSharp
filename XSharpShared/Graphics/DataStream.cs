using System;
using System.IO;

namespace XSharp.Graphics;

public abstract class DataStream : Stream
{
    public unsafe abstract IntPtr DataPointer
    {
        get;
    }

    public unsafe abstract IntPtr PositionPointer
    {
        get;
    }

    public abstract long RemainingLength
    {
        get;
    }

    protected DataStream()
    {
    }

    ~DataStream()
    {
        Dispose(disposing: false);
    }

    public unsafe abstract T Read<T>() where T : struct;

    public unsafe abstract void Read(IntPtr buffer, int offset, int count);

    public unsafe abstract T[] ReadRange<T>(int count) where T : struct;

    public unsafe abstract int ReadRange<T>(T[] buffer, int offset, int count) where T : struct;

    public unsafe abstract void Write<T>(T value) where T : struct;

    public unsafe abstract void Write(IntPtr buffer, int offset, int count);

    public void WriteRange<T>(T[] data) where T : struct
    {
        WriteRange(data, 0, data.Length);
    }

    public unsafe abstract void WriteRange(IntPtr source, long count);

    public unsafe abstract void WriteRange<T>(T[] data, int offset, int count) where T : struct;
}