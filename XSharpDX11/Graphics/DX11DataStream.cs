using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using DataStreamImpl = SharpDX.DataStream;

namespace XSharp.Graphics;

public class DX11DataStream : DataStream
{
    public unsafe static DX11DataStream Create<T>(T[] userBuffer, bool canRead, bool canWrite, int index = 0, bool pinBuffer = true) where T : struct
    {
        return DataStreamImpl.Create(userBuffer, canRead, canWrite, index, pinBuffer);
    }

    private DataStreamImpl impl;
    private bool canDispose = true;

    public override bool CanRead => impl.CanRead;

    public override bool CanWrite => impl.CanRead;

    public override bool CanSeek => impl.CanSeek;

    public unsafe override IntPtr DataPointer => impl.DataPointer;

    public override long Length => impl.Length;

    public override long Position
    {
        get => impl.Position;
        set => impl.Position = value;
    }

    public unsafe override IntPtr PositionPointer => impl.PositionPointer;

    public override long RemainingLength => impl.RemainingLength;

    public DX11DataStream(DataStreamImpl impl, bool canDispose = true)
    {
        this.impl = impl;
        this.canDispose = canDispose;
    }

    public unsafe DX11DataStream(Blob buffer)
    {
        impl = new DataStreamImpl(buffer);
        canDispose = true;
    }

    public unsafe DX11DataStream(int sizeInBytes, bool canRead, bool canWrite)
    {
        impl = new DataStreamImpl(sizeInBytes, canRead, canWrite);
        canDispose = true;
    }

    public DX11DataStream(DataPointer dataPointer)
    {
        impl = new DataStreamImpl(dataPointer);
        canDispose = true;
    }

    public unsafe DX11DataStream(IntPtr userBuffer, long sizeInBytes, bool canRead, bool canWrite)
    {
        impl = new DataStreamImpl(userBuffer, sizeInBytes, canRead, canWrite);
        canDispose = true;
    }

    protected unsafe override void Dispose(bool disposing)
    {
        if (canDispose)
            impl.Dispose();
    }

    public override void Flush()
    {
        impl.Flush();
    }

    public unsafe override T Read<T>() where T : struct
    {
        return impl.Read<T>();
    }

    public unsafe override int ReadByte()
    {      
        return impl.ReadByte();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return impl.Read(buffer, offset, count);
    }

    public unsafe override void Read(IntPtr buffer, int offset, int count)
    {
        impl.Read(buffer, offset, count);
    }

    public unsafe override T[] ReadRange<T>(int count) where T : struct
    {
        return impl.ReadRange<T>(count);
    }

    public unsafe override int ReadRange<T>(T[] buffer, int offset, int count) where T : struct
    {
        return impl.ReadRange(buffer, offset, count);
    }

    public override void SetLength(long value)
    {
        impl.SetLength(value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return impl.Seek(offset, origin);
    }

    public unsafe override void Write<T>(T value) where T : struct
    {
        impl.Write(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        impl.Write(buffer, offset, count);
    }

    public unsafe override void Write(IntPtr buffer, int offset, int count)
    {
        impl.Write(buffer, offset, count);
    }

    public unsafe override void WriteRange(IntPtr source, long count)
    {
        impl.WriteRange(source, count);
    }

    public unsafe override void WriteRange<T>(T[] data, int offset, int count) where T : struct
    {
        impl.WriteRange(data, offset, count);
    }

    public static implicit operator DataPointer(DX11DataStream from)
    {
        return new DataPointer(from.PositionPointer, (int) from.RemainingLength);
    }

    public static implicit operator DataStreamImpl(DX11DataStream from)
    {
        return from?.impl;
    }

    public static implicit operator DX11DataStream(DataStreamImpl from)
    {
        return new DX11DataStream(from);
    }
}